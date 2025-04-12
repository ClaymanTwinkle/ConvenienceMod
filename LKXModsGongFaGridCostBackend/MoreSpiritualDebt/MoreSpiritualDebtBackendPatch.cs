using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains.Extra;
using GameData.Domains.Map;
using GameData.Utilities;
using HarmonyLib;

namespace ConvenienceBackend.MoreSpiritualDebt
{
    internal class MoreSpiritualDebtBackendPatch : BaseBackendPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GlobalConfig), "Init")]
        public static void GlobalConfig_Init_PostPatch(GlobalConfig __instance)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtraDomain), "SetAreaSpiritualDebt")]
        public static void MapDomain_SetAreaSpiritualDebt_PrePatch(ExtraDomain __instance, DataContext context, short areaId, short value)
        {
            GlobalConfig.Instance.SpiritualDebtLimit[1] = 100000;
            AdaptableLog.Info("MapDomain:SetAreaSpiritualDebt " + value);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtraDomain), "SetAreaSpiritualDebt")]
        public static void MapDomain_SetAreaSpiritualDebt_PostPatch(ExtraDomain __instance, DataContext context, short areaId, short value)
        {
            AdaptableLog.Info("MapDomain:SetAreaSpiritualDebt " + value);
            GlobalConfig.Instance.SpiritualDebtLimit[1] = 1000;
        }
    }
}
