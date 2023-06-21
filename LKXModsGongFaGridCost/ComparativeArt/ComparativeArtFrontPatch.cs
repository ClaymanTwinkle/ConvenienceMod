using HarmonyLib;
using TaiwuModdingLib.Core.Utils;
using UnityEngine;

namespace ConvenienceFrontend
{
    internal class ComparativeArtFrontPatch : BaseFrontPatch
    {
        private static bool _artAlwaysWin = false;

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "Toggle_ArtAlwaysWin", ref _artAlwaysWin);
        }


        [HarmonyPostfix, HarmonyPatch(typeof(UI_LifeSkillCombat), "UpdateButtonDisplay")]
        public static void UI_LifeSkillCombat_UpdateButtonDisplay_PostfixPatch(UI_LifeSkillCombat __instance, bool isTaiwuRound)
        {
            if (!_artAlwaysWin) return;

            __instance.CGet<CButton>("BtnForceGiveUp").onClick.Invoke();
        }
    }
}
