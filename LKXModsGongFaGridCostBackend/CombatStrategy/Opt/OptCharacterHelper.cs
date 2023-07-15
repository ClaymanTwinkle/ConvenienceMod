using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.CombatStrategy.Utils;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Utilities;
using NLog;

namespace ConvenienceBackend.CombatStrategy.Opt
{
    internal class OptCharacterHelper
    {
        private static Logger _logger = LogManager.GetLogger("战斗策略");

        /// <summary>
        /// 施展技能
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="context"></param>
        /// <param name="selfChar"></param>
        /// <param name="skillId"></param>
        public static bool CastCombatSkill(CombatDomain instance, DataContext context, CombatCharacter selfChar, short skillId)
        {
            var skillItem = Config.CombatSkill.Instance[skillId];
            if (skillItem == null) return false;

            if (skillItem.EquipType == CombatSkillEquipType.Neigong)
            {
                // 没有装备内功
                if (!selfChar.GetEquippedCombatSkills().Contains(skillId)) return false;

                // 施展内功，单独处理
                // 目前内功只用在了处理施展中的功法上
                var preparingSkillId = selfChar.GetPreparingSkillId();
                if (preparingSkillId < 0)
                {
                    return false;
                }
                var effectDataList = DomainManager.SpecialEffect.GetAllCostNeiliEffectData(selfChar.GetId(), preparingSkillId);
                if (effectDataList.Count != 0)
                {
                    var hasCostNeiliEffect = false;
                    effectDataList.ForEach(x => {
                        if (x.EffectId == skillItem.DirectEffectID || x.EffectId == skillItem.ReverseEffectID)
                        {
                            DomainManager.SpecialEffect.CostNeiliEffect(context, selfChar.GetId(), preparingSkillId, (short)x.EffectId);
                            hasCostNeiliEffect = true;
                        }
                    });
                    return hasCostNeiliEffect;
                }

                return false;
            }

            CombatSkillData skillData = SkillUtils.GetCombatSkillData(instance, selfChar.GetId(), skillId);

            // 无装备该功法
            if (skillData == null) return false;

            // 功法目前不可使用
            if (!skillData.GetCanUse()) return false;

            // 同一个身法没必要重复施展
            if (selfChar.GetAffectingMoveSkillId() == skillId) return false;
            // 已有护体，没必要重复施展护体，也不能施展身法
            if (selfChar.GetAffectingDefendSkillId() >= 0 && (skillItem.EquipType == CombatSkillEquipType.Agile || skillItem.EquipType == CombatSkillEquipType.Defense)) return false;
            // 已有准备施展的技能，跳过
            if (selfChar.NeedUseSkillId >= 0) return false;
            // 正在施展的技能不重复施展了
            if (selfChar.GetPreparingSkillId() == skillId) return false;

            var charStateType = selfChar.StateMachine.GetCurrentState().StateType;
            // 正在准备技能，忽略
            if (charStateType == CombatCharacterStateType.PrepareSkill && !selfChar.CanCastDuringPrepareSkills.Contains(skillId)) return false;
            // 正在放技能，忽略
            if (charStateType == CombatCharacterStateType.CastSkill) return false;

            _logger.Info("准备施展 " + skillItem.Name);

            // 需要施展准备的功法
            instance.StartPrepareSkill(context, skillId, true);

            return true;
        }
    }
}
