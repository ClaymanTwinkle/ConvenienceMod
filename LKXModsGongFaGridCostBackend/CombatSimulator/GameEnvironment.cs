using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvnetSharp;
using DeepQLearning.DRLAgent;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Utilities;
using HarmonyLib;
using NLog;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceBackend.CombatSimulator
{
    internal class GameState 
    {
        public const int MAX_STATE_COUNT = 17;

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
            double[] input_array = new double[MAX_STATE_COUNT];

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

    internal class GameReward
    {
        /// <summary>
        /// 普通攻击击中收益
        /// </summary>
        public double normalAttackReward;

        /// <summary>
        /// 躲避收益，躲避普攻
        /// </summary>
        public double avoidNormalAttackReward;

        /// <summary>
        /// 伤害收益
        /// </summary>
        public double damageReward;

        public double CalcTotal()
        { 
            return normalAttackReward + avoidNormalAttackReward + damageReward;
        }

        public void Clear()
        { 
            normalAttackReward = 0;
            avoidNormalAttackReward = 0;
            damageReward = 0;
        }
    }

    internal enum ActionType
    { 
        /// <summary>
        /// 呆立不动
        /// </summary>
        Idle,
        /// <summary>
        /// 向前移动
        /// </summary>
        MoveForward,
        /// <summary>
        /// 向后移动
        /// </summary>
        MoveBackward,
        /// <summary>
        /// 普通攻击
        /// </summary>
        NormalAttack,

    }

    internal class GameEnvironment
    {
        private static readonly Logger logger = LogManager.GetLogger("模拟训练");

        /// <summary>
        /// 最大的动作个数
        /// </summary>
        public const int MAX_ACTION_COUNT = 4; 

        private readonly GameReward Reward = new();

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
            Reward.Clear();

            DataContext context = DataContextManager.GetCurrentThreadDataContext();
            var combat = DomainManager.Combat;

            var consummateLevel = 0;

            var list = DomainManager.Character.GmCmd_GetAllCharacterName().FindAll(x => DomainManager.Character.GetElement_Objects(x.CharId).GetConsummateLevel() == consummateLevel);

            var a = list.GetRandom(context.Random);

            var b = list.GetRandom(context.Random);
            while (b.CharId == a.CharId)
            { 
                b = list.GetRandom(context.Random);
            }

            logger.Info("开始模拟NPC对战: " + a.Name + "(" + a.CharId + ")" + " VS " + b.Name + "(" + b.CharId + ")");

            if (combat.IsInCombat())
            {
                logger.Info("当前在战斗中，先结束战斗");
                combat.EndCombat(combat.GetCombatCharacter(true, true), context, false, false);
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
            RegisterCombatEvents();

            combat.SetWaitingDelaySettlement(false, context);
            combat.CallMethod("PrepareCombat", BindingFlags.Instance | BindingFlags.NonPublic, context, (short)2, new int[] { a.CharId, -1, -1, -1 }, new int[] { b.CharId, -1, -1, -1 });
            // combat.StartNpcCombat(context, new int[] { a.CharId, -1, -1, -1 }, new int[] { b.CharId, -1, -1, -1 });
            // combat.PrepareCombat(context, 2, new int[] { a.CharId, -1, -1, -1 }, new int[] { b.CharId, -1, -1, -1 });
            // this.SetSkillPower(skillPower, context);
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
            DataContext context = DataContextManager.GetCurrentThreadDataContext();
            var combat = DomainManager.Combat;

            switch ((ActionType)actionIndex)
            {
                case ActionType.Idle:
                case ActionType.MoveForward:
                case ActionType.MoveBackward:
                    combat.SetMoveState((byte)actionIndex, true, true);
                    break;

                case ActionType.NormalAttack:
                    combat.NormalAttack(context, true);
                    break;
            }
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
            DataContext context = DataContextManager.GetCurrentThreadDataContext();
            var combat = DomainManager.Combat;
            var reward = Reward.CalcTotal();
            Reward.Clear();
            return reward;
        }

        /// <summary>
        /// 关闭环境
        /// </summary>
        public void Close()
        {
            DataContext context = DataContextManager.GetCurrentThreadDataContext();
            var combat = DomainManager.Combat;

            if (combat.IsInCombat())
            {
                combat.EndCombat(combat.GetCombatCharacter(true, true), context, false, false);
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

        private void RegisterCombatEvents()
        {
            Events.RegisterHandler_NormalAttackCalcHitEnd(OnNormalAttackCalcHitEnd);
            Events.RegisterHandler_DistanceChanged(OnDistanceChanged);

            Events.RegisterHandler_CombatSettlement(OnCombatSettlement);
            Events.RegisterHandler_CombatEnd(OnCombatEnd);
        }

        private void UnRegisterCombatEvents()
        {
            Events.UnRegisterHandler_NormalAttackCalcHitEnd(OnNormalAttackCalcHitEnd);
            Events.UnRegisterHandler_DistanceChanged(OnDistanceChanged);

            Events.UnRegisterHandler_CombatSettlement(OnCombatSettlement);
            Events.UnRegisterHandler_CombatEnd(OnCombatEnd);
        }


        /// <summary>
        /// 战斗结算
        /// </summary>
        /// <param name="context"></param>
        /// <param name="combatStatus"></param>
        private void OnCombatSettlement(DataContext context, sbyte combatStatus)
        {
            if (combatStatus == CombatStatusType.SelfFail)
            {
                logger.Info("战斗结束，我方战败");
            }
            else if (combatStatus == CombatStatusType.EnemyFail)
            {
                logger.Info("战斗结束，敌方战败");
            }
            else if (combatStatus == CombatStatusType.SelfFlee)
            {
                logger.Info("战斗结束，我方逃跑");
            }
            else if (combatStatus == CombatStatusType.EnemyFlee)
            {
                logger.Info("战斗结束，敌方逃跑");
            }
        }

        /// <summary>
        /// 战斗结束
        /// </summary>
        /// <param name="context"></param>
        private void OnCombatEnd(DataContext context)
        {
            UnRegisterCombatEvents();
        }

        /// <summary>
        /// 普攻判定
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="trickType"></param>
        /// <param name="pursueIndex"></param>
        /// <param name="hit"></param>
        /// <param name="isFightBack"></param>
        private void OnNormalAttackCalcHitEnd(DataContext context, CombatCharacter attacker, CombatCharacter defender, sbyte trickType, int pursueIndex, bool hit, bool isFightBack)
        {
            //logger.Debug(
            //    string.Concat(new string[] {
            //            attacker.IsAlly ? "【我方】" : "【敌方】",
            //            GetCharName(attacker.GetId()),
            //            "进行",
            //            isFightBack ? "反击": (pursueIndex == 0 ? "普攻" : "第"+ pursueIndex +"次追击"),
            //            hit ? "成功命中" : "被对方躲开了"
            //    })
            //);
            if (attacker.IsAlly)
            {
                Reward.normalAttackReward += 1;
            }
            else
            {
            }
        }

        private void OnDistanceChanged(DataContext context, CombatCharacter mover, short distance, bool isMove, bool isForced)
        {
            if (!isMove) return;
            //logger.Debug(
            //    string.Concat(new string[] {
            //            mover.IsAlly ? "【我方】" : "【敌方】",
            //            GetCharName(mover.GetId()),
            //            "向",
            //            distance < 0 ? "前" : "后",
            //            "移动",
            //            Math.Abs(distance) + "距离",
            //            "到" + DomainManager.Combat.GetCurrentDistance()
            //    })
            //);
        }

        private static string GetCharName(int charId)
        {
            var a = DomainManager.Character.GetRealName(charId);
            return a.surname + a.givenName;
        }
    }
}
