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
                        case CombatSkillEquipType.Attack:
                            strategy = CreateStrategy(skillId);
                            break;
                        case CombatSkillEquipType.Defense:
                            strategy = CreateStrategy(skillId);
                            break;
                        case CombatSkillEquipType.Agile:
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
