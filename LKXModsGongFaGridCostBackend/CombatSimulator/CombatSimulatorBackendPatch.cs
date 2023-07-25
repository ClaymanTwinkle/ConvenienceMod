using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BehTree;
using Config;
using ConvnetSharp;
using DeepQLearning.DRLAgent;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.Mod;
using GameData.Domains.Organization;
using GameData.Domains.SpecialEffect;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;
using Microsoft.VisualBasic;
using NLog;
using TaiwuModdingLib.Core.Utils;
using static GameData.DomainEvents.Events;

namespace ConvenienceBackend.CombatSimulator
{
    // 1. ProcessCombatTeammateBetray
    // 2. PrepareEnemyEquipments
    // 3. PrepareCombat
    // 4. SetAiOptions
    // 5. SetTimeScale
    // 6. GetProactiveSkillList
    // 7. GetCanHealInjuryCount
    // 8. GetCanHealPoisonCount
    // 9. StartCombat
    // 10. PlayMoveStepSound
    // 11. GetWeaponInnerRatio
    // 12. GetWeaponEffects
    // 13. GetCombatResultDisplayData
    // 14. SelectGetItem
    internal class CombatSimulatorBackendPatch : BaseBackendPatch
    {
        private static readonly Logger logger = LogManager.GetLogger("模拟训练");

        private static bool _isStartLearning = false;

        private const int MAX_COMBAT_LEARNING_COUNT = 10000000;
        private static int _currentCombatCount = 0;

        private const bool LIMIT_SINGLE_COMBAT_STEP_COUNT = false;
        private const int MAX_SINGLE_COMBAT_STEP_COUNT = 10000;
        private static int _currentSingleCombatStepCount = 0;

        private static string _netFile = "";
        private static string _analysisFile = "";

        private static GameEnvironment _environment = new(MAX_SINGLE_COMBAT_STEP_COUNT);
        private static DeepQLearn _brain;

        public override void OnModSettingUpdate(string modIdStr)
        {

        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
            
            string directoryName = DomainManager.Mod.GetModDirectory(modIdStr);
            _netFile = Path.Combine(directoryName, "learn_data");
            _analysisFile = Path.Combine(directoryName, "analysis_data.txt");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CallMethod")]
        public static bool CombatDomain_CallMethod_Prefix(CombatDomain __instance, Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context, ref int __result)
        {
            if (operation.MethodId == 1970)
            {
                if (_isStartLearning)
                {
                    StopLearning();
                }
                else
                {
                    StartLearning();
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CombatLoop")]
        public static void OnTickBegin(CombatDomain __instance, DataContext context)
        {
            if (_environment == null) return;
            if (_brain == null) return;
            if (!__instance.CanAcceptCommand()) return;

            var state = _environment.Render();
            // get action from brain
            var actionIndex = _brain.forward(state.ToNetInput());
            _environment.Step(actionIndex);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "CombatLoop")]
        public static void OnTickEnd(CombatDomain __instance, DataContext context)
        {
            if (_environment == null) return;
            if (_brain == null) return;

            double reward = _environment.SettleReward();
            _brain.backward(reward);


            if (!__instance.IsInCombat() || __instance.CombatAboutToOver())
            {
                logger.Debug("战斗结束，最终结算奖励");
            }

            if (_currentSingleCombatStepCount % 10 == 0)
            {
                SaveAnalysisData();
            }

            _currentSingleCombatStepCount++;
            if (LIMIT_SINGLE_COMBAT_STEP_COUNT && _currentSingleCombatStepCount > MAX_SINGLE_COMBAT_STEP_COUNT)
            {
                logger.Debug("单局回合数超过" + MAX_SINGLE_COMBAT_STEP_COUNT + "，强制结束游戏");
                _environment.Close();
            }

            // logger.Debug("OnUpdate");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatCharacterStateIdle), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateSelectChangeTrick), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareAttack), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateAttack), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareSkill), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateCastSkill), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareOtherAction), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStatePrepareUseItem), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateUseItem), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateSelectMercy), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateDelaySettlement), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateChangeCharacter), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateTeammateCommand), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateChangeBossPhase), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateAnimalAttack), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateJumpMove), "OnEnter")]
        [HarmonyPatch(typeof(CombatCharacterStateSpecialShow), "OnEnter")]
        public static void CombatCharacterStateBase_OnEnter_Postfix(CombatCharacterStateBase __instance)
        {
            // CombatCharacter CombatChar = (CombatCharacter)__instance.GetFieldValue("CombatChar", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            // AdaptableLog.Info(CombatChar.GetCharacter().GetId() + "进入状态" + __instance.StateType);
        }

        /// <summary>
        /// 修复可能的crash
        /// </summary>
        /// <param name="____combatCharacterDict"></param>
        /// <param name="objectId"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "GetElement_CombatCharacterDict")]
        public static bool CombatDomain_GetElement_CombatCharacterDict_Prefix(Dictionary<int, CombatCharacter> ____combatCharacterDict, int objectId, ref CombatCharacter __result)
        {
            if (!____combatCharacterDict.ContainsKey(objectId))
            {
                __result = null;

                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "CanFlee")]
        public static void CanFlee(ref bool __result)
        {
            if (!_isStartLearning) return;
            __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "NeedShowMercy")]
        public static void NeedShowMercy(CombatCharacter failChar, ref bool __result)
        {
            if (!_isStartLearning) return;
            __result = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "EndCombat")]
        public static void CombatDomain_EndCombat_Prefix(CombatCharacter failChar, DataContext context, bool flee, ref bool playAni)
        {
            if (!_isStartLearning) return;
            playAni = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "EndCombat")]
        public static void CombatDomain_EndCombat_Postfix(CombatCharacter failChar, DataContext context, bool flee, bool playAni)
        {
            if (!_isStartLearning) return;

            if (_currentCombatCount > MAX_COMBAT_LEARNING_COUNT)
            {
                StopLearning();
            }
            else
            {
                logger.Debug("战斗结束，战斗回合数"+ _currentSingleCombatStepCount);
                logger.Debug(_brain.visSelf());
                ConvenienceBackend.PostRunMainAction(delegate () {
                    _currentCombatCount++;
                    _currentSingleCombatStepCount = 0;
                    logger.Debug("准备开始下一把");
                    _environment.Reset();
                }, 100);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CombatSettlement")]
        public static void CombatDomain_CombatSettlement_Prefix(DataContext context, sbyte statusType, ref bool playAni, bool clearAi)
        {
            if (!_isStartLearning) return;
            playAni = false;
        }

        /// <summary>
        /// 开始训练
        /// </summary>
        private static void StartLearning()
        {
            _currentCombatCount = 0;
            _currentSingleCombatStepCount = 0;
            _isStartLearning = true;
            logger.Debug("开始训练");

            _brain = LoadOrCreateDeepQLearn();
            _brain.learning = true;

            RegisterCombatEvents();
            _environment.Reset();
        }

        /// <summary>
        /// 停止训练
        /// </summary>
        private static void StopLearning()
        {
            _isStartLearning = false;
            logger.Debug("停止训练");

            _environment.Close();
            UnRegisterCombatEvents();
            _brain = null;
            _currentCombatCount = 0;
            _currentSingleCombatStepCount = 0;
        }


        private static DeepQLearn BuildDeepQLearn()
        {
            var num_inputs = GameState.MAX_STATE_COUNT; // 9 eyes, each sees 3 numbers (wall, green, red thing proximity)
            var num_actions = GameEnvironment.MAX_ACTION_COUNT; // 5 possible angles agent can turn
            var temporal_window = 4; // amount of temporal memory. 0 = agent lives in-the-moment :)
            var network_size = num_inputs * temporal_window + num_actions * temporal_window + num_inputs;

            var layer_defs = new List<LayerDefinition>
            {
                // the value function network computes a value of taking any of the possible actions
                // given an input state. Here we specify one explicitly the hard way
                // but user could also equivalently instead use opt.hidden_layer_sizes = [20,20]
                // to just insert simple relu hidden layers.
                new LayerDefinition { type = "input", out_sx = 1, out_sy = 1, out_depth = network_size },
                new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" },
                new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" },
                new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" },
                new LayerDefinition { type = "regression", num_neurons = num_actions }
            };

            // options for the Temporal Difference learner that trains the above net
            // by backpropping the temporal difference learning rule.
            //var opt = new Options { method="sgd", learning_rate=0.01, l2_decay=0.001, momentum=0.9, batch_size=10, l1_decay=0.001 };
            var opt = new Options { method = "adadelta", l2_decay = 0.001, batch_size = 10 };

            var tdtrainer_options = new TrainingOptions
            {
                temporal_window = temporal_window,
                experience_size = 300000,
                start_learn_threshold = 10000,
                gamma = 0.7,
                learning_steps_total = 2000000,
                learning_steps_burnin = 30000,
                epsilon_min = 0.05,
                epsilon_test_time = 0.00,
                layer_defs = layer_defs,
                options = opt
            };

            return new DeepQLearn(GameState.MAX_STATE_COUNT, GameEnvironment.MAX_ACTION_COUNT, tdtrainer_options);
        }

        private static void SaveLearning()
        {
            if (_brain == null) return;

            using (FileStream fstream = new(_netFile, FileMode.Create))
            {
                new BinaryFormatter().Serialize(fstream, _brain);
            }
        }

        private static DeepQLearn LoadOrCreateDeepQLearn()
        {
            if (File.Exists(_netFile))
            {
                using (FileStream fstream = new FileStream(_netFile, FileMode.Open))
                {
                    return new BinaryFormatter().Deserialize(fstream) as DeepQLearn;
                }
            }

            return BuildDeepQLearn();
        }

        /// <summary>
        /// 保存分析数据
        /// </summary>
        private static void SaveAnalysisData()
        {
            if (_brain == null) return;

            var currentAnalysisInfo = _brain.GetLineData();

            if (!System.IO.File.Exists(_analysisFile))
            {
                //没有则创建这个文件
                FileStream fs1 = new(_analysisFile, FileMode.Create, FileAccess.Write);//创建写入文件               
                StreamWriter sw = new(fs1);

                sw.WriteLine(currentAnalysisInfo);//开始写入值
                sw.Close();
                fs1.Close();
            }
            else
            {
                using (FileStream fileStream = new FileStream(_analysisFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine(currentAnalysisInfo);
                    }
                }
            }
        }

        private static void RegisterCombatEvents()
        {
            logger.Debug("RegisterCombatEvents");

            Events.RegisterHandler_CombatBegin(OnCombatBegin);
            Events.RegisterHandler_CombatEnd(OnCombatEnd);
        }

        private static void UnRegisterCombatEvents()
        {
            logger.Debug("UnRegisterCombatEvents");

            Events.UnRegisterHandler_CombatBegin(OnCombatBegin);
            Events.UnRegisterHandler_CombatEnd(OnCombatEnd);
        }

        private static void OnCombatBegin(DataContext context)
        {
            logger.Debug("开始第" + _currentCombatCount + "次战斗");
        }

        private static void OnCombatEnd(DataContext context)
        {
            SaveLearning();
        }
    }
}
