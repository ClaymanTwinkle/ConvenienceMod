using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var equippedCombatSkills = DomainManager.Taiwu.GetTaiwu().GetEquippedCombatSkills();
            foreach (var skillId in equippedCombatSkills)
            {
                var combatSkillItem = Config.CombatSkill.Instance[skillId];
                if (combatSkillItem != null)
                {
                    Strategy strategy = null;
                    switch (combatSkillItem.EquipType)
                    {
                        case CombatSkillEquipType.Attack: // 催破
                            strategy = CreateStrategy(skillId);
                            break;
                        case CombatSkillEquipType.Agile: // 身法
                            strategy = CreateStrategy(skillId);
                            strategy.conditions.Add(new Data.Condition { 
                                isAlly= true,
                                item = JudgeItem.SkillMobility,
                                judge = Judgement.Equals,
                                value = 0
                            });
                            break;
                        case CombatSkillEquipType.Defense: // 护体
                            strategy = CreateStrategy(skillId);
                            break;
                    }

                    if (strategy != null)
                    {
                        strategies.Add(strategy);
                    }
                }
            }
            return strategies;
        }

        private static Strategy CreateStrategy(short skillId)
        {
            Strategy strategy = new Strategy();
            strategy.type = (short)StrategyType.ReleaseSkill;
            strategy.skillId = skillId;
            strategy.conditions = new List<Data.Condition>();

            return strategy;
        }
    }
}
