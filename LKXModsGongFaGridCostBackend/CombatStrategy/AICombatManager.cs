using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvenienceBackend.CombatSimulator;
using ConvenienceBackend.CombatStrategy.AI;
using ConvenienceBackend.CombatStrategy.Opt;
using ConvenienceBackend.CombatStrategy.Utils;
using DeepQLearning.DRLAgent;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Utilities;

namespace ConvenienceBackend.CombatStrategy
{
    internal class AICombatManager
    {
        private static List<AIPlan> _aIPlans = new() {
            // 猴戏
            new MonkeyPlayPlan(),
            // 移动
            new MovePlan(),
            // 普攻
            new NormalAttackPlan(),
            // 套路
            new CombatRoutinePlan()
        };

        private static GameEnvironment _environment = new();
        private static DeepQLearn _brain;

        /// <summary>
        /// 开始战斗，准备分析自身功法
        /// </summary>
        public static void StartCombat()
        { 
            ResetCombat();
            _brain = DeepQLearnManager.LoadOrCreateDeepQLearn();
            _brain.learning = false;
        }

        /// <summary>
        /// 重置自身功法信息
        /// </summary>
        public static void ResetCombat()
        {
            _brain = null;
        }

        /// <summary>
        /// 处理战斗
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="context"></param>
        /// <param name="selfChar"></param>
        public static void HandleCombatUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            if (_brain == null) return;

            var state = _environment.Render();
            // get action from brain
            var actionIndex = _brain.forward(state.ToNetInput());
            _environment.Step(actionIndex);
        }
    }
}
