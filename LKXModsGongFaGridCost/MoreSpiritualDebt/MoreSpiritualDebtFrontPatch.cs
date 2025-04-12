using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ConvenienceFrontend.MoreSpiritualDebt
{
    internal class MoreSpiritualDebtFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GlobalConfig), "Init")]
        public static void GlobalConfig_Init_PostPatch(GlobalConfig __instance)
        {
            //__instance.SpiritualDebtLimit[1] = 100000;
        }
    }
}
