using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Character;
using GameData.Domains.Item;
using System.Windows;
using HarmonyLib;
using TMPro;

namespace ConvenienceFrontend.CricketCombatOptimize
{
    /// <summary>
    /// 蛐蛐战斗优化
    /// </summary>
    internal class CricketCombatOptimizeFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        /// <summary>
        /// 对方的蛐蛐都可见
        /// </summary>
        /// <param name="visible"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CricketCombat), "SetEnemyCricketsVisible")]
        public static void UI_CricketCombat_SetEnemyCricketsVisible_Prefix(ref bool visible)
        {
            visible = true;
        }

        private static bool randomFirstMove = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CricketCombat), "OnListenerIdReady")]
        public static void UI_CricketCombat_OnListenerIdReady_Prefix(UI_CricketCombat __instance)
        {
            randomFirstMove = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Utils_Random), "RandomCheck", new Type[] { typeof(int), typeof(int) })]
        public static bool Utils_Random_RandomCheck_Prefix(UI_CricketCombat __instance, int rate, int totalRate, ref bool __result)
        {
            if (rate == 50 && randomFirstMove) 
            {
                randomFirstMove = false;

                __result = true;
                return false;
            }
            randomFirstMove = false;
            return true;
        }
    }
}
