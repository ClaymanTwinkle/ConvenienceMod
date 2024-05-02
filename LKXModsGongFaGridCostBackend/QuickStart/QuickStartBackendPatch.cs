using System;
using GameData.Domains.Global;
using GameData.Domains.Taiwu;
using GameData.Domains.TaiwuEvent;
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuEvent), "ToDisplayData")]
        public static void TaiwuEvent_ToDisplayData_Prefix(TaiwuEvent __instance)
        {
            if ("a9d0bcd8-e378-4ee9-96a6-1e5b9db17371".Equals(__instance.EventGuid))
            {
                var eventOptions = __instance.EventConfig.EventOptions;
                if (eventOptions != null)
                {
                    for (int i = 0; i < eventOptions.Length; i++)
                    {
                        if (eventOptions[i].OptionContent.Contains("交换藏书"))
                        {
                            eventOptions[i].OnOptionVisibleCheck = new Func<bool>(delegate () { return true; });
                            return;
                        }
                    }
                }
            }
        }
    }
}
