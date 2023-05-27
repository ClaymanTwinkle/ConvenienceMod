using HarmonyLib;
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


        [HarmonyPrefix, HarmonyPatch(typeof(LifeSkillCombatModel), "TaiwuTryForceWin")]
        public static bool TaiwuTryForceWin_PrefixPatch(ref bool __result)
        {
            Debug.Log("TaiwuTryForceWin");
            if (!_artAlwaysWin) return true;
            __result = true;
            return false;
        }


    }
}
