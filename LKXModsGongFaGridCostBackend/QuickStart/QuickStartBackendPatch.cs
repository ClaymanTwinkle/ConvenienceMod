using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Global;
using GameData.Domains.Item;
using GameData.Domains.Item.Display;
using GameData.Domains.Merchant;
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MerchantDomain), "ExchangeBook")]
        public static void MerchantDomain_ExchangeBook_Postfix(MerchantDomain __instance, DataContext context, int npcId, List<ItemDisplayData> boughtItems, List<ItemDisplayData> soldItems, int selfAuthority, int npcAuthority)
        {
            foreach (ItemDisplayData itemData in boughtItems)
            {
                ItemKey itemKey = itemData.Key;
                ItemBase item = DomainManager.Item.GetBaseItem(itemKey);
                
                AdaptableLog.Info("MerchantDomain_ExchangeBook_Prefix : " + item.GetName() + ", ownerId=" + item.Owner.OwnerId + ", ItemSourceType=" + itemData.ItemSourceType);
            }
        }
    }
}
