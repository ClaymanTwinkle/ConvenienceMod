using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using Config.Common;
using ConvenienceBackend.CombatStrategy.Opt;
using ConvenienceBackend.CombatStrategy.Utils;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Utilities;

namespace ConvenienceBackend.CombatStrategy.AI
{
    internal class MonkeyPlayPlan : AIPlan
    {
        private readonly List<AIPlan> _playList = new() { 
            new QingshenshuPlay(),
        };

        public bool HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            var enemyChar = instance.GetCombatCharacter(false, false);
            // 根据精纯境界，决定是否开局猴戏
            if (enemyChar.GetCharacter().GetConsummateLevel() < selfChar.GetCharacter().GetConsummateLevel())
            {
                AdaptableLog.Info("对手太弱，不用猴戏");
                return false;
            }

            foreach (var item in _playList)
            {
                if (item.HandleUpdate(instance, context, selfChar))
                {
                    return true;
                }
            }

            return false;
        }

    }

    /// <summary>
    /// 轻身术猴戏
    /// </summary>
    internal class QingshenshuPlay : AIPlan
    {
        bool AIPlan.HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            var equippedCombatSkillIds = selfChar.GetEquippedCombatSkills();

            short qingshenshuId = 108;
            if (equippedCombatSkillIds.Contains(qingshenshuId) && SkillUtils.IsSkillBrokenOut(selfChar.GetId(), qingshenshuId))
            {
                var combatStateCollection = selfChar.GetBuffCombatStateCollection();

                // 判断是否是buff叠满
                foreach (KeyValuePair<short, ValueTuple<short, bool, int>> buff in combatStateCollection.StateDict)
                {
                    var stateId = buff.Key;

                    if (stateId != CombatState.DefKey.QingShenShu) continue;

                    CombatStateItem configData = CombatState.Instance[stateId];
                    if (configData == null) continue;

                    short power = buff.Value.Item1;

                    short maxPower = selfChar.GetCombatStatePowerLimit(1);

                    if (power >= maxPower) return false;
                }

                if (selfChar.GetAffectingMoveSkillId() == qingshenshuId)
                {
                    // 轻身术已经在施展了
                    var combatSkill = SkillUtils.GetCombatSkill(selfChar.GetId(), qingshenshuId);
                    var isDirect = combatSkill.GetDirection() == 0; // 是否是正练
                    
                    instance.SetMoveState(isDirect ? (byte)1 : (byte)2, true);
                }
                else
                {
                    instance.SetMoveState(0, true);
                    OptCharacterHelper.CastCombatSkill(instance, context, selfChar, qingshenshuId);
                }

                return true;
            }

            return false;
        }
    }
}
