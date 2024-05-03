using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ConvenienceFrontend.FastTaiwu
{
    internal class FastTaiwuFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
            // UI_CombatResult
        }

        /// <summary>
        /// 奇遇加速
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="____curCarrierTravelTimeReduction"></param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_AdventureInfo), "SetCarrierAnimation")]
        public static void UI_AdventureInfo_SetCarrierAnimation_Prefix(UI_AdventureInfo __instance, ref sbyte ____curCarrierTravelTimeReduction)
        {
            ____curCarrierTravelTimeReduction = SByte.MaxValue;
        }
    }
}
