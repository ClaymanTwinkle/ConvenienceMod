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
    internal class GameState 
    {
        /// <summary>
        /// 是否游戏结束
        /// </summary>
        public bool IsDone = false;

        /// <summary>
        /// 战斗最小、最大距离
        /// </summary>
        public ValueTuple<byte, byte> DistanceRange;

        /// <summary>
        /// 当前位置
        /// </summary>
        public short CurrentDistance;

        /// <summary>
        /// 战败需要的标记数量
        /// </summary>
        public int NeedDefeatMarkCount;

        /// <summary>
        /// 是否能普通攻击
        /// </summary>
        public ValueTuple<bool, bool> CanNormalAttack;

        /// <summary>
        /// 友方状态
        /// </summary>
        public CombatCharacter SelfChar;

        /// <summary>
        /// 敌方状态
        /// </summary>
        public CombatCharacter EnemyChar;

        private static readonly ValueTuple<byte, byte> MinDistance = new(byte.MaxValue, byte.MaxValue);
        private static readonly ValueTuple<byte, byte> MaxDistance = new(byte.MaxValue, byte.MinValue);

        static GameState()
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

        public Volume ToNetInput()
        {
            double[] input_array = new double[17];

            // 最小最大距离
            var (currentMinDistance, currentMaxDistance) = DistanceRange;
            input_array[0] = (currentMinDistance - MinDistance.Item1) * 1.0 / (MinDistance.Item2 - MinDistance.Item1);
            input_array[1] = (currentMaxDistance - MaxDistance.Item1) * 1.0 / (MaxDistance.Item2 - MaxDistance.Item1);
            // 当前位置
            input_array[2] = (CurrentDistance - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            // 伤害槽
            input_array[3] = SelfChar.GetDefeatMarkCollection().GetTotalCount() / NeedDefeatMarkCount;
            input_array[4] = EnemyChar.GetDefeatMarkCollection().GetTotalCount() / NeedDefeatMarkCount;
            // 脚力
            input_array[5] = SelfChar.GetMobilityValue() / 1000;
            input_array[6] = EnemyChar.GetMobilityValue() / 1000;
            // 身法条
            input_array[7] = (SelfChar.GetSkillMobility() * 1000.0 / GlobalConfig.Instance.AgileSkillMobility) / 1000;
            input_array[8] = (EnemyChar.GetSkillMobility() * 1000.0 / GlobalConfig.Instance.AgileSkillMobility) / 1000;
            // 能不能普通攻击
            input_array[9] = CanNormalAttack.Item1 ? 1 : 0;
            input_array[10] = CanNormalAttack.Item2 ? 1 : 0;
            // 攻击范围
            input_array[11] = (SelfChar.GetAttackRange().Outer - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            input_array[12] = (SelfChar.GetAttackRange().Inner - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            input_array[13] = (EnemyChar.GetAttackRange().Outer - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            input_array[14] = (EnemyChar.GetAttackRange().Inner - currentMinDistance) * 1.0 / (currentMaxDistance - currentMinDistance);
            // 状态
            input_array[15] = ((int)(SelfChar.StateMachine.GetCurrentState().StateType) + 2.0) / 18.0;
            input_array[16] = ((int)(EnemyChar.StateMachine.GetCurrentState().StateType) + 2.0) / 18.0;

            Volume input = new(input_array.Length, 1, 1)
            {
                w = input_array
            };

            return input;
        }
    }


    internal class GameEnvironment
    {
        private static readonly Logger logger = LogManager.GetLogger("GameEnvironment");

        public GameEnvironment()
        {

        }

        /// <summary>
        /// 重新玩游戏
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public void Reset()
        {
            DataContext context = DomainManager.Combat.Context;
            var combat = DomainManager.Combat;

            var consummateLevel = 2;

            var list = DomainManager.Character.GmCmd_GetAllCharacterName().FindAll(x => DomainManager.Character.GetElement_Objects(x.CharId).GetConsummateLevel() == consummateLevel);

            var a = list.GetRandom(context.Random);

            var b = list.GetRandom(context.Random);

            AdaptableLog.Info("开始模拟NPC对战: " + a.Name + "(" + a.CharId + ")" + " VS " + b.Name + "(" + b.CharId + ")");

            if (combat.IsInCombat())
            {
                combat.EndNpcCombat(context);
            }
            // 治疗+解毒
            var aChar = DomainManager.Character.GetElement_Objects(a.CharId);
            var poisoned = aChar.GetPoisoned();
            poisoned.Initialize();
            aChar.SetPoisoned(ref poisoned, context);
            var injuries = aChar.GetInjuries();
            injuries.Initialize();
            aChar.SetInjuries(injuries, context);

            var bChar = DomainManager.Character.GetElement_Objects(b.CharId);
            poisoned = bChar.GetPoisoned();
            poisoned.Initialize();
            bChar.SetPoisoned(ref poisoned, context);
            injuries = bChar.GetInjuries();
            injuries.Initialize();
            bChar.SetInjuries(injuries, context);

            // 开始战斗
            combat.StartNpcCombat(context, new int[] { a.CharId, -1, -1, -1 }, new int[] { b.CharId, -1, -1, -1 });
            combat.SetTimeScale(context, 100f);
            combat.StartCombat(context);
            combat.SetTimeScale(context, 100f);
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="actionIndex"></param>
        public void Step(int actionIndex)
        { 
            
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <returns></returns>
        public GameState Render()
        {
            return GetCurrentState();
        }

        /// <summary>
        /// 结算奖励
        /// </summary>
        /// <param name="combat"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public double SettleReward()
        {
            DataContext context = DomainManager.Combat.Context;
            var combat = DomainManager.Combat;

            return 0;
        }

        /// <summary>
        /// 关闭环境
        /// </summary>
        public void Close(DataContext context)
        {
            if (DomainManager.Combat.IsInCombat())
            {
                DomainManager.Combat.EndNpcCombat(context);
            }
        }

        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private GameState GetCurrentState()
        {
            var combat = DomainManager.Combat;
            GameState state = new()
            {
                IsDone = !combat.IsInCombat(),
                DistanceRange = combat.GetDistanceRange(),
                CurrentDistance = combat.GetCurrentDistance(),
                NeedDefeatMarkCount = (int)GameData.Domains.Combat.SharedConstValue.NeedDefeatMarkCount[(int)combat.GetCombatType()],
                CanNormalAttack = new(combat.CanNormalAttack(true), combat.CanNormalAttack(false)),
                SelfChar = combat.GetCombatCharacter(true, false),
                EnemyChar = combat.GetCombatCharacter(false, false)
            };

            return state;
        }
    }
}
