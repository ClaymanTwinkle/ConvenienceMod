using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvenienceBackend.CombatStrategy.Data;
using GameData.Domains;
using GameData.Domains.Character.Display;
using GameData.Domains.CombatSkill;
using GameData.Utilities;
using Newtonsoft.Json.Linq;

namespace ConvenienceBackend.CombatStrategy
{
    internal class AutoHelper
    {
        public static List<Strategy> AutoGenerateStrategies()
        {
            List<Strategy> strategies = new List<Strategy>();

            var equippedCombatSkills = DomainManager.Taiwu.GetTaiwu().GetEquippedCombatSkills().ToList().FindAll(x=>x>-1).ConvertAll(x=> Config.CombatSkill.Instance[x]);
            equippedCombatSkills.Sort((CombatSkillItem a, CombatSkillItem b) => {
                if (a.EquipType != b.EquipType)
                {
                    // 优先催破
                    return a.EquipType - b.EquipType;
                }

                if (a.Grade != b.Grade)
                {
                    if (a.EquipType == CombatSkillEquipType.Agile)
                    {
                        // 身法优先品级低的
                        return a.Grade - b.Grade;
                    }
                    return b.Grade - a.Grade;
                }

                if (a.EquipType == CombatSkillEquipType.Agile)
                { 
                    // 身法优先无门无派
                    return a.SectId - b.SectId;
                }

                return 0; 
            });
            foreach (var combatSkillItem in equippedCombatSkills)
            {
                var skillId = combatSkillItem.TemplateId;
                Strategy strategy = null;
                switch (combatSkillItem.EquipType)
                {
                    case CombatSkillEquipType.Attack: // 催破
                        strategy = CreateStrategy(skillId);
                        break;
                    case CombatSkillEquipType.Agile: // 身法
                        strategy = CreateStrategy(skillId);
                        strategy.conditions.Add(new Data.Condition
                        {
                            isAlly = true,
                            item = JudgeItem.SkillMobility,
                            judge = Judgement.Equals,
                            value = 0
                        });
                        break;
                    case CombatSkillEquipType.Defense: // 护体
                        strategy = CreateStrategy(skillId);
                        if (combatSkillItem.FightBackDamage == 0)
                        {
                            strategy.conditions.Add(new Data.Condition
                            {
                                isAlly = false,
                                item = JudgeItem.PreparingAction,
                                judge = Judgement.Equals,
                                value = 0
                            });
                        }
                        break;
                }

                if (strategy != null)
                {
                    strategies.Add(strategy);
                }
            }
            return strategies;
        }

        private static Strategy CreateStrategy(short skillId)
        {
            Strategy strategy = new Strategy
            {
                type = (short)StrategyType.ReleaseSkill,
                skillId = skillId,
                conditions = new List<Data.Condition>()
            };

            return strategy;
        }
    }
}
