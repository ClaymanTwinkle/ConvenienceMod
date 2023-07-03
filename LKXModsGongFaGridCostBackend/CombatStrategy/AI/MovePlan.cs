using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvenienceBackend.CombatStrategy.Opt;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Utilities;

namespace ConvenienceBackend.CombatStrategy.AI
{
    /// <summary>
    /// AI移动策略
    /// </summary>
    internal class MovePlan : AIPlan
    {
        public bool HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            // 寻找敌人死角攻击
            var findAttackDeadAngle = true;
            // 躲避技能
            var tryEvasionSkills = true;

            // 我方攻击距离
            var selfAttackRange = selfChar.GetAttackRange();
            short moveOuter = selfAttackRange.Outer;
            short moveInner = selfAttackRange.Inner;

            // 敌人
            var enemyChar = instance.GetCombatCharacter(false, false);
            var enemyAttackRange = enemyChar.GetAttackRange();

            var enemyPreparingSkillId = enemyChar.GetPreparingSkillId();
            var SkillItem = Config.CombatSkill.Instance[enemyPreparingSkillId];
            if (SkillItem != null && SkillItem.EquipType == CombatSkillEquipType.Attack)
            {
                // 看看能不能躲避技能
                if (tryEvasionSkills)
                {

                }
            }
            else
            {
                // 寻找敌人死角攻击
                if (findAttackDeadAngle)
                {
                    if (moveInner < enemyAttackRange.Outer || moveOuter > enemyAttackRange.Inner)
                    {
                        // 无交集，直接移动即可，躲避技能
                    }
                    else if (moveInner <= enemyAttackRange.Inner && moveOuter >= enemyAttackRange.Outer)
                    {
                        // 有交集，但是自己攻击范围都在敌人的攻击范围内，没法逃脱，直接移动即可
                    }
                    else
                    {
                        // 有交集，可以躲避
                        if (moveInner > enemyAttackRange.Inner)
                        {
                            moveOuter = enemyAttackRange.Inner;
                        }
                        else
                        {
                            moveInner = enemyAttackRange.Outer;
                        }
                    }
                }
            }

            // 开始移动
            short currentDistance = instance.GetCurrentDistance();
            short targetDistance = (short)((moveInner - moveOuter) / 2 + moveOuter);
            if (currentDistance == targetDistance)
            {
                instance.SetMoveState(0, true);

                return false;
            }
            else
            {
                // 使用身法
                OptCharacterHelper.CastCombatSkill(instance, context, selfChar, FindMoveSkillId(instance, context, selfChar));

                // 需要移动
                var moveState = currentDistance > targetDistance ? (byte)1 : (byte)2;
                instance.SetMoveState(moveState, true);

                return (selfChar.GetAffectingMoveSkillId() >= 0) ? (selfChar.GetSkillMobility() > 0) : selfChar.GetMobilityLevel() > 0 && selfChar.GetMobilityValue() >= (short)CombatDomain.MoveCostMobility[(int)selfChar.GetMobilityLevel()];
            }
        }

        private static short FindMoveSkillId(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            var equippedAgileCombatSkillList = selfChar.GetEquippedCombatSkills().FindAll(x => Utils.SkillUtils.IsAgile(x)).ConvertAll(x => DomainManager.CombatSkill.GetElement_CombatSkills(new CombatSkillKey(selfChar.GetId(), x)));

            if (equippedAgileCombatSkillList.Count == 0) return -1;

            equippedAgileCombatSkillList.Sort((a, b) => {
                if (a.GetId().SkillTemplateId == 725 || b.GetId().SkillTemplateId == 725)
                {
                    // 小纵跃
                    return a.GetId().SkillTemplateId == 725 ? -1 : 1;
                }

                var skillItem1 = Config.CombatSkill.Instance[a.GetId().SkillTemplateId];
                var skillItem2 = Config.CombatSkill.Instance[b.GetId().SkillTemplateId];

                if ((a.GetActivationState() & b.GetActivationState()) == 0 && (a.GetActivationState() | b.GetActivationState()) != 0)
                { 
                    // 是否突破
                    return b.GetActivationState() == 0 ? -1 : 1;
                }

                if (skillItem1.Grade != skillItem2.Grade)
                {
                    // 品级
                    return skillItem1.Grade < skillItem2.Grade ? -1 : 1;
                }

                if (a.GetPower() != b.GetPower())
                {
                    // 威力
                    return a.GetPower() > b.GetPower() ? -1 : 1;
                }

                return -1;
            });

            return equippedAgileCombatSkillList.First().GetId().SkillTemplateId;
        }
    }
}
