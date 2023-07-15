using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvnetSharp;
using DeepQLearning.DRLAgent;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Utilities;
using HarmonyLib;
using NLog;

namespace ConvenienceBackend.CombatSimulator
{
    internal class GameEnvironment
    {
        private static readonly Logger logger = LogManager.GetLogger("GameEnvironment");

        private static GameEnvironment _environment = new();
        private DeepQLearn brain;

        private static readonly ValueTuple<byte, byte> MinDistance = new(byte.MaxValue, byte.MaxValue);
        private static readonly ValueTuple<byte, byte> MaxDistance = new(byte.MaxValue, byte.MinValue);

        static GameEnvironment()
        {
            var combatConfigItem = Config.CombatConfig.Instance[0];

            MinDistance = new ValueTuple<byte, byte>(combatConfigItem.MinDistance, combatConfigItem.MinDistance);

            for (var i = 1; i < Config.CombatConfig.Instance.Count; i++)
            {
                combatConfigItem = Config.CombatConfig.Instance[i];

                MinDistance = new ValueTuple<byte, byte>(Math.Min(combatConfigItem.MinDistance, MinDistance.Item1), Math.Max(combatConfigItem.MinDistance, MinDistance.Item2));
                MaxDistance = new ValueTuple<byte, byte>(Math.Min(combatConfigItem.MaxDistance, MaxDistance.Item1), Math.Max(combatConfigItem.MaxDistance, MaxDistance.Item2));
            }
        }

        public GameEnvironment()
        {

        }

        public void Reset(DataContext context)
        {
            var consummateLevel = 2;

            var list = DomainManager.Character.GmCmd_GetAllCharacterName().FindAll(x => DomainManager.Character.GetElement_Objects(x.CharId).GetConsummateLevel() == consummateLevel);

            var a = list.GetRandom(context.Random);

            var b = list.GetRandom(context.Random);

            AdaptableLog.Info("开始模拟NPC对战: " + a.Name + "(" + a.CharId + ")" + " VS " + b.Name + "(" + b.CharId + ")");

            if (DomainManager.Combat.IsInCombat())
            {
                DomainManager.Combat.EndNpcCombat(context);
            }

            DomainManager.Combat.StartNpcCombat(context, new int[] { a.CharId, -1, -1, -1 }, new int[] { b.CharId, -1, -1, -1 });

            DomainManager.Combat.SetTimeScale(context, 100f);
            DomainManager.Combat.StartCombat(context);
            DomainManager.Combat.SetTimeScale(context, 100f);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "OnUpdate")]
        public static void OnTickBegin(CombatDomain __instance, DataContext context)
        {
            if (_environment == null) return;

            CombatCharacter selfChar = __instance.GetCombatCharacter(true, false);
            CombatCharacter enemyChar = __instance.GetCombatCharacter(false, false);

            double[] input_array = new double[17];

            // 最小最大距离
            var (currentMinDistance, currentMaxDistance) = __instance.GetDistanceRange();
            input_array[0] = (currentMinDistance - MinDistance.Item1) * 1.0 / (MinDistance.Item2 - MinDistance.Item1);
            input_array[1] = (currentMaxDistance - MaxDistance.Item1) * 1.0 / (MaxDistance.Item2 - MaxDistance.Item1);
            // 当前位置
            input_array[2] = (__instance.GetCurrentDistance() - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            // 伤害槽
            var needDefeatMarkCount = (int)GameData.Domains.Combat.SharedConstValue.NeedDefeatMarkCount[(int)__instance.GetCombatType()];
            input_array[3] = selfChar.GetDefeatMarkCollection().GetTotalCount() / needDefeatMarkCount;
            input_array[4] = enemyChar.GetDefeatMarkCollection().GetTotalCount() / needDefeatMarkCount;
            // 脚力
            input_array[5] = selfChar.GetMobilityValue() / 1000;
            input_array[6] = enemyChar.GetMobilityValue() / 1000;
            // 身法条
            input_array[7] = (selfChar.GetSkillMobility() * 1000.0 / GlobalConfig.Instance.AgileSkillMobility)/1000;
            input_array[8] = (enemyChar.GetSkillMobility() * 1000.0 / GlobalConfig.Instance.AgileSkillMobility) / 1000;
            // 能不能普通攻击
            input_array[9] = __instance.CanNormalAttack(true) ? 1 : 0;
            input_array[10] = __instance.CanNormalAttack(false) ? 1 : 0;
            // 攻击范围
            input_array[11] = (selfChar.GetAttackRange().Outer - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            input_array[12] = (selfChar.GetAttackRange().Inner - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            input_array[13] = (enemyChar.GetAttackRange().Outer - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            input_array[14] = (enemyChar.GetAttackRange().Inner - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            // 状态
            input_array[15] = ((int)(selfChar.StateMachine.GetCurrentState().StateType) + 2.0) /18.0;
            input_array[16] = ((int)(enemyChar.StateMachine.GetCurrentState().StateType) + 2.0) / 18.0;

            __instance.GetDistanceRange();

            Volume input = new(input_array.Length, 1, 1)
            {
                w = input_array
            };

            // get action from brain
            var actionIndex = _environment.brain.Forward(input);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "OnUpdate")]
        public static void OnTickEnd(CombatDomain __instance, DataContext context)
        {
            if (_environment == null) return;
            double reward = 0;
            _environment.brain.Backward(reward);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CombatDomain), "EndCombat")]
        public static void CombatDomain_EndCombat_Postfix(CombatDomain __instance, CombatCharacter failChar, bool flee = false)
        {
            if (_environment == null) return;

        }
    }
}
