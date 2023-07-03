using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains.Combat;

namespace ConvenienceBackend.CombatStrategy.AI
{
    /// <summary>
    /// 普攻策略    
    /// </summary>
    internal class NormalAttackPlan : AIPlan
    {
        public bool HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar)
        {
            if (instance.CanNormalAttack(true))
            {
                if (instance.InAttackRange(selfChar))
                {
                    instance.NormalAttack(context, true);
                    return true;
                }
            }
            return false;
        }
    }
}
