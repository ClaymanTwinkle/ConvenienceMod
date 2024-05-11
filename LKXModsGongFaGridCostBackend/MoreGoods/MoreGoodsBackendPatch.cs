using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using Config.ConfigCells;
using GameData.Common;
using GameData.Domains.Item;
using GameData.Domains.Merchant;
using HarmonyLib;
using HarmonyLib.Tools;
using NLog;

namespace ConvenienceBackend.MoreGoods
{
    internal class MoreGoodsBackendPatch : BaseBackendPatch
    {
        private static NLog.Logger _logger = LogManager.GetLogger("更多工具出售");

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MerchantData), "GetGoodsPreset")]
        public static bool MerchantData_GetGoodsPreset_Prefix(MerchantData __instance, MerchantItem template, int index, ref IList<PresetItemTemplateIdGroup> __result)
        {
            IList<PresetItemTemplateIdGroup> result;
            switch (index)
            {
                case 0:
                    result = template.Goods0;
                    break;
                case 1:
                    result = template.Goods1;
                    break;
                case 2:
                    result = template.Goods2;
                    break;
                case 3:
                    result = template.Goods3;
                    break;
                case 4:
                    result = template.Goods4;
                    break;
                case 5:
                    result = template.Goods5;
                    break;
                case 6:
                    result = template.Goods6;
                    break;
                case 7:
                    result = template.Goods7;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            __result = new List<PresetItemTemplateIdGroup>();

            foreach (var group in result)
            {
                if (group.ItemType == ItemType.CraftTool)
                {
                    __result.Add(new PresetItemTemplateIdGroup(GameData.Domains.Item.ItemType.TypeId2TypeName[group.ItemType], group.StartId, Math.Min((sbyte)(group.GroupLength + 15), sbyte.MaxValue)));
                }
                else
                {
                    __result.Add(group);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MerchantData), "CalculateCharacterBehaviourDiscount")]
        public static bool MerchantData_CalculateCharacterBehaviourDiscount_Prefix(MerchantData __instance, DataContext ctx, ItemKey itemKey, GameData.Domains.Character.Character character)
        {
            foreach (var key in __instance.PriceChangeData.Keys)
            {
                if (key.TemplateEquals(itemKey)) 
                {
                    __instance.PriceChangeData[itemKey] = __instance.PriceChangeData[key];
                    return false;
                }
            }
            return true;
        }
    }
}
