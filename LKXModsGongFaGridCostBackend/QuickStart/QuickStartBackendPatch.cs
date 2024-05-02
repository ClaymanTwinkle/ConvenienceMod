using System;
using GameData.Domains.Global;
using GameData.Domains.Taiwu;
using HarmonyLib;

namespace ConvenienceBackend.QuickStart
{
    internal class QuickStartBackendPatch : BaseBackendPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GlobalDomain), "GetGlobalFlag")]
        public static bool GlobalDomain_GetGlobalFlag_Prefix(TaiwuDomain __instance, sbyte flagType, ref bool __result)
        {
            if (flagType == GlobalFlagType.PastPerformArea)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
