using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;

namespace ConvenienceBackend.CombatStrategy.Utils
{
    internal class SkillUtils
    {
        /// <summary>
        /// 判断是否是内功
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public static bool IsNeigong(short skillId)
        {
            var skillItem = Config.CombatSkill.Instance[skillId];

            return skillItem != null && skillItem.EquipType == CombatSkillEquipType.Neigong;
        }

        /// <summary>
        /// 判断是否是护体
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public static bool IsDefense(short skillId)
        {
            var skillItem = Config.CombatSkill.Instance[skillId];

            return skillItem != null && skillItem.EquipType == CombatSkillEquipType.Defense;
        }

        /// <summary>
        /// 判断是否是催破
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public static bool IsAttack(short skillId)
        {
            var skillItem = Config.CombatSkill.Instance[skillId];

            return skillItem != null && skillItem.EquipType == CombatSkillEquipType.Attack;
        }

        /// <summary>
        /// 判断是否是轻功
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public static bool IsAgile(short skillId)
        {
            var skillItem = Config.CombatSkill.Instance[skillId];

            return skillItem != null && skillItem.EquipType == CombatSkillEquipType.Agile;
        }

        /// <summary>
        /// 获取战斗技能
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="charId"></param>
        /// <param name="skillTemplateId"></param>
        /// <returns></returns>
        public static CombatSkill GetCombatSkill(int charId, short skillTemplateId)
        {
            CombatSkillKey combatSkillKey = new(charId, skillTemplateId);
            return DomainManager.CombatSkill.GetElement_CombatSkills(combatSkillKey);
        }

        /// <summary>
        /// 获取战斗技能
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="charId"></param>
        /// <param name="skillTemplateId"></param>
        /// <returns></returns>
        public static CombatSkillData GetCombatSkillData(CombatDomain instance, int charId, short skillTemplateId)
        {
            CombatSkillKey combatSkillKey = new(charId, skillTemplateId);
            instance.TryGetElement_SelfSkillDataDict(combatSkillKey, out CombatSkillData skillData);

            return skillData;
        }

        /// <summary>
        /// 判断技能是否突破
        /// </summary>
        /// <param name="charId"></param>
        /// <param name="skillTemplateId"></param>
        /// <returns></returns>
        public static bool IsSkillBrokenOut(int charId, short skillTemplateId)
        {
            var combatSkill = GetCombatSkill(charId, skillTemplateId);

            return combatSkill != null && combatSkill.GetActivationState() != 0;
        }
    }
}
