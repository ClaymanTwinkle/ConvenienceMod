using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.CombatStrategy.AI;
using ConvenienceBackend.CombatStrategy.Opt;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Utilities;

namespace ConvenienceBackend.CombatStrategy
{
    internal class AIPracticeManager
    {
        /// <summary>
        /// 处理战斗
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="context"></param>
        /// <param name="selfChar"></param>
        public static bool HandleCombatUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            // 轻灵功法向后跑
            var agileSkillId = FindMoveSkillId(instance, context, selfChar);
            if (agileSkillId >= 0)
            {
                if (selfChar.GetSkillMobility() == 0) 
                {
                    // 施展轻灵功法
                    OptCharacterHelper.CastCombatSkill(instance, context, selfChar, agileSkillId);
                }
            }

            // 护体功法
            var defenseSkillId = FindDefenseSkillId(instance, context, selfChar);
            if (defenseSkillId >= 0)
            {
                // 施展护体功法
                OptCharacterHelper.CastCombatSkill(instance, context, selfChar, defenseSkillId);
            }

            // 催迫功法
            var attackSkillId = FindAttackSkillId(instance, context, selfChar);
            if (attackSkillId >= 0)
            {
                // 施展催迫功法
                OptCharacterHelper.CastCombatSkill(instance, context, selfChar, attackSkillId);
            }

            // 自动移动
            // 如果有轻灵功法就往后走
            if (agileSkillId >= 0 && selfChar.GetSkillMobility() > 0)
            {
                instance.SetMoveState(MoveState.Backward, true, true);
            }
            else if (defenseSkillId >= 0 || attackSkillId >= 0)
            {
                // 移动到攻击距离内
                var selfAttackRange = selfChar.GetAttackRange();
                short moveOuter = selfAttackRange.Outer;
                short moveInner = selfAttackRange.Inner;
                short currentDistance = instance.GetCurrentDistance();
                short targetDistance = (short)((moveInner - moveOuter) / 2 + moveOuter);
                if (currentDistance == targetDistance)
                {
                    instance.SetMoveState(0, true);

                    return false;
                }
                else
                {
                    // 需要移动
                    var moveState = currentDistance > targetDistance ? (byte)MoveState.Forward : (byte)MoveState.Backward;
                    instance.SetMoveState(moveState, true, true);
                }
            }

            // 自动攻击
            if (defenseSkillId >= 0 || attackSkillId >= 0)
            {
                if (instance.CanNormalAttack(true))
                {
                    if (instance.InAttackRange(selfChar))
                    {
                        instance.NormalAttack(context, true);
                    }
                }
            }

            return agileSkillId >= 0 || defenseSkillId >= 0 || attackSkillId >= 0;
        }

        private static short FindMoveSkillId(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            var combatSkill = selfChar.GetEquippedCombatSkills().
                FindAll(x => Utils.SkillUtils.IsAgile(x)).ConvertAll(x => DomainManager.CombatSkill.GetElement_CombatSkills(new CombatSkillKey(selfChar.GetId(), x))).FirstOrDefault( x => x.GetPracticeLevel() < 100);

            if (combatSkill == null) return -1;

            return combatSkill.GetId().SkillTemplateId;
        }

        private static short FindDefenseSkillId(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            var combatSkill = selfChar.GetEquippedCombatSkills().
                FindAll(x => Utils.SkillUtils.IsDefense(x)).ConvertAll(x => DomainManager.CombatSkill.GetElement_CombatSkills(new CombatSkillKey(selfChar.GetId(), x))).FirstOrDefault(x => x.GetPracticeLevel() < 100);

            if (combatSkill == null) return -1;

            return combatSkill.GetId().SkillTemplateId;
        }

        private static short FindAttackSkillId(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            var combatSkill = selfChar.GetEquippedCombatSkills().
                FindAll(x => Utils.SkillUtils.IsAttack(x)).ConvertAll(x => DomainManager.CombatSkill.GetElement_CombatSkills(new CombatSkillKey(selfChar.GetId(), x))).FirstOrDefault(x => x.GetPracticeLevel() < 100);

            if (combatSkill == null) return -1;

            return combatSkill.GetId().SkillTemplateId;
        }
    }
}
