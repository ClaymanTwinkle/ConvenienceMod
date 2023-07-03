using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.CombatStrategy.Opt;
using ConvenienceBackend.CombatStrategy.Utils;
using GameData.Common;
using GameData.Domains.Combat;
using GameData.Utilities;

namespace ConvenienceBackend.CombatStrategy.AI
{
    /// <summary>
    /// 战斗套路
    /// </summary>
    internal class CombatRoutinePlan : AIPlan
    {
        private List<AIPlan> _routineList = new List<AIPlan>() { 
            new 拔刀斩(),
            new 龙爪手(),
            new 大拙手(),
            new 时停剑(),
            new 套娃剑(),
            new CommonRoutine()
        };

        public bool HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            foreach (var routine in _routineList)
            {
                if (routine.HandleUpdate(instance, context, selfChar))
                { 
                    return true;
                }
            }

            return false;
        }
    }

    internal class CommonRoutine : AIPlan
    {
        bool AIPlan.HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            var enemyChar = instance.GetCombatCharacter(false, false);
            if (SkillUtils.IsAttack(enemyChar.GetPreparingSkillId()))
            {
                // 施展防御
                var allDefenseSkillList = selfChar.GetEquippedCombatSkills().FindAll(x => SkillUtils.IsDefense(x)).FindAll(x => Config.CombatSkill.Instance[x].FightBackDamage == 0).FindAll(x => SkillUtils.GetCombatSkillData(instance, selfChar.GetId(), x).GetCanUse());
                if (allDefenseSkillList.Count > 0)
                {
                    allDefenseSkillList.Sort((a, b) => {
                        var aIsSkillBrokenOut = SkillUtils.IsSkillBrokenOut(selfChar.GetId(), a);
                        var bIsSkillBrokenOut = SkillUtils.IsSkillBrokenOut(selfChar.GetId(), b);
                        if (aIsSkillBrokenOut != bIsSkillBrokenOut)
                        {
                            return bIsSkillBrokenOut ? 1 : -1;
                        }

                        var aGrade = Config.CombatSkill.Instance[a].Grade;
                        var bGrade = Config.CombatSkill.Instance[b].Grade;

                        if (aGrade != bGrade)
                        {
                            return aGrade < bGrade ? 1 : -1;
                        }

                        var aPower = SkillUtils.GetCombatSkill(selfChar.GetId(), a).GetPower();
                        var bPower = SkillUtils.GetCombatSkill(selfChar.GetId(), b).GetPower();

                        if (aPower != bPower)
                        {
                            return aPower < bPower ? 1 : -1;
                        }

                        return 0;
                    });

                    if (OptCharacterHelper.CastCombatSkill(instance, context, selfChar, allDefenseSkillList.First()))
                    {
                        return true;
                    }
                }
            }

            // 施展功法
            var allAttackSkillList = selfChar.GetEquippedCombatSkills().FindAll(x => SkillUtils.IsAttack(x)).FindAll(x => SkillUtils.GetCombatSkillData(instance, selfChar.GetId(), x).GetCanUse());
            
            allAttackSkillList.Sort((a, b) => {
                var aIsSkillBrokenOut = SkillUtils.IsSkillBrokenOut(selfChar.GetId(), a);
                var bIsSkillBrokenOut = SkillUtils.IsSkillBrokenOut(selfChar.GetId(), b);
                if (aIsSkillBrokenOut != bIsSkillBrokenOut)
                {
                    return bIsSkillBrokenOut ? 1 : -1;
                }

                var aGrade = Config.CombatSkill.Instance[a].Grade;
                var bGrade = Config.CombatSkill.Instance[b].Grade;

                if (aGrade != bGrade)
                {
                    return aGrade < bGrade ? 1 : -1;
                }

                var aPower = SkillUtils.GetCombatSkill(selfChar.GetId(), a).GetPower();
                var bPower = SkillUtils.GetCombatSkill(selfChar.GetId(), b).GetPower();

                if (aPower != bPower)
                {
                    return aPower < bPower ? 1 : -1;
                }

                return 0;
            });

            if (allAttackSkillList.Count > 0)
            {
                if (OptCharacterHelper.CastCombatSkill(instance, context, selfChar, allAttackSkillList.First()))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal class 拔刀斩 : AIPlan
    {
        bool AIPlan.HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {

            return false;
        }
    }

    internal class 龙爪手 : AIPlan
    {
        public bool HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            return false;
        }
    }

    internal class 大拙手 : AIPlan
    {
        public bool HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            return false;
        }
    }

    internal class 时停剑 : AIPlan
    {
        bool AIPlan.HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            return false;
        }
    }

    internal class 套娃剑 : AIPlan
    {
        bool AIPlan.HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            return false;
        }
    }
}
