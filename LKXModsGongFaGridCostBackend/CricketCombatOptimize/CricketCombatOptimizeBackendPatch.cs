using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Taiwu;
using GameData.Domains.TaiwuEvent;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Domains.TaiwuEvent.EventOption;
using HarmonyLib;
using NLog;

namespace ConvenienceBackend.CricketCombatOptimize
{
    internal class CricketCombatOptimizeBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("蛐蛐优化");

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionConditionMatcher), "InteractionOffCooldown")]
        public static bool OptionConditionMatcher_InteractionOffCooldown_PrePatch(int arg0, short arg1, ref bool __result)
        {
            if (7 == arg1)
            {
                // 无限制斗促织
                __result = true;
                return false;
            }

            return true;
        }
    }
}
