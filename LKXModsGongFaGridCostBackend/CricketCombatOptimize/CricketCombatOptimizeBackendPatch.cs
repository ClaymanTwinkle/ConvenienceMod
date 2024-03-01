using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains.Taiwu;
using GameData.Domains.TaiwuEvent.EventOption;
using HarmonyLib;

namespace ConvenienceBackend.CricketCombatOptimize
{
    internal class CricketCombatOptimizeBackendPatch : BaseBackendPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionConditionMatcher), "MonthCooldownCount")]
        public static bool OptionConditionMatcher_MonthCooldownCount_PrePatch(string arg0, int arg1, sbyte arg2, ref bool __result)
        {
            if ("CriketCombatInteract".Equals(arg0))
            {
                // 无限制斗促织
                __result = true;
                return false;
            }

            return true;
        }
    }
}
