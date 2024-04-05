using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains.Global;
using GameData.Domains.Taiwu;
using GameData.GameDataBridge;
using GameData.Utilities;
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
