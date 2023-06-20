using System;
using FrameWork;
using HarmonyLib;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceFrontend.CustomSteal
{
    internal class CustomStealFrontPatch : BaseFrontPatch
    {
        private static bool _ignoreFindTreasureAnim = true;

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "Toggle_IgnoreFindTreasureAnim", ref _ignoreFindTreasureAnim);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_FindTreasure), "OnInit")]
        public static void UI_FindTreasure_OnInitPostfixPatch(UI_FindTreasure __instance, ArgumentBox argsBox)
        {
            if (!_ignoreFindTreasureAnim) return;
            __instance.ModifyField("FindAnimTimeBase", 0.3F, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            __instance.ModifyField("FadeTime", 0.03F, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_CollectResource), "OnInit")]
        public static void UI_CollectResource_OnInitPostfixPatch(UI_CollectResource __instance)
        {
            if (!_ignoreFindTreasureAnim) return;
            __instance.ModifyField("CollectAniTimeBase", 0.02F, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            __instance.ModifyField("FadeTime", 0.003F, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            __instance.ModifyField("TextChangeDelayTime", 0.005F, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
    }
}
