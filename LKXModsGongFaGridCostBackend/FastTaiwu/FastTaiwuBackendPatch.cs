using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Item;
using GameData.Domains.Merchant;
using GameData.Domains.TaiwuEvent;
using HarmonyLib;
using NLog;

namespace ConvenienceBackend.FastTaiwu
{
    internal class FastTaiwuBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("吾太急");

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MerchantDomain), "DeleteCaravanItem")]
        public static bool MerchantDomain_DeleteCaravanItem_Prefix(MerchantDomain __instance, ItemKey itemKey)
        {
            if (itemKey.Id == 0)
            {
                _logger.Error("拿到一个ItemKey.id=0的，" + itemKey.ItemType);
                return false;
            }

            return true;
        }
    }
}
