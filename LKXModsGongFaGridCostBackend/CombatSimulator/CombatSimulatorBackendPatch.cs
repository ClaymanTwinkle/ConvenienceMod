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
        private static readonly bool EnableAnalysis = false;


        private static bool _isStartLearning = false;

        private const int MAX_COMBAT_LEARNING_COUNT = 100000;
        private static int _currentCombatCount = 0;

        private const bool LIMIT_SINGLE_COMBAT_STEP_COUNT = false;
        private const int MAX_SINGLE_COMBAT_STEP_COUNT = 100000;
        private static int _currentSingleCombatStepCount = 0;

        private static GameEnvironment _environment = new(MAX_SINGLE_COMBAT_STEP_COUNT);
        private static DeepQLearn _brain;

        public override void OnModSettingUpdate(string modIdStr)
        {

        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
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

            if (EnableAnalysis && _currentSingleCombatStepCount % 100 == 0)
            {
                DeepQLearnManager.SaveAnalysisData(_brain);
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
                logger.Debug("战斗结束，结束训练");
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

            _brain = DeepQLearnManager.LoadOrCreateDeepQLearn();
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
            DeepQLearnManager.SaveLearning(_brain);
        }
    }
}
