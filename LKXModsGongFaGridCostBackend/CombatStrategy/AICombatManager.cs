﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvenienceBackend.CombatStrategy.AI;
using ConvenienceBackend.CombatStrategy.Opt;
using ConvenienceBackend.CombatStrategy.Utils;
using GameData.Common;
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

        /// <summary>
        /// 开始战斗，准备分析自身功法
        /// </summary>
        public static void StartCombat()
        { 
            ResetCombat();

        }

        /// <summary>
        /// 重置自身功法信息
        /// </summary>
        public static void ResetCombat()
        { 
            
        }

        /// <summary>
        /// 处理战斗
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="context"></param>
        /// <param name="selfChar"></param>
        public static void HandleCombatUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            foreach (var a in _aIPlans)
            {
                if (a.HandleUpdate(instance, context, selfChar))
                {
                    break;
                }
            }
        }
    }
}
