using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ConvenienceFrontend.SettingsOpt
{
    internal class SettingsOptFrontPatch : BaseFrontPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_SystemSetting), "InitBackupSettings")]
        public static void UI_SystemSetting_InitBackupSettings_Prefix(UI_SystemSetting __instance)
        {
            CSlider backupCountSlider = __instance.CGet<CSlider>("RecordBackupCountSlider");
            backupCountSlider.maxValue = 127;
        }

    }
}
