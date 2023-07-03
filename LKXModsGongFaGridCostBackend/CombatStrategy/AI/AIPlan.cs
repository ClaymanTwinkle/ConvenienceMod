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
    /// AI计划
    /// </summary>
    public interface AIPlan
    {
        bool HandleUpdate(CombatDomain instance, DataContext context, CombatCharacter selfChar);
    }
}
