using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CombatStrategy;
using FrameWork.ModSystem;
using GameData.Domains.Item;
using HarmonyLib;
using UnityEngine;

namespace ConvenienceFrontend.ShopShortcut
{
    internal class ShopShortcutFrontPatch : BaseFrontPatch
    {
        private static CButton _selectAllMaterialItems = null;

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_Shop), "OnInit")]
        public static void UI_Shop_OnInit_Pro(UI_Shop __instance, ItemScrollView ____shopItemScroll)
        {
            if (_selectAllMaterialItems != null) return;

            var confirmButton = __instance.CGet<CButton>("Confirm");

            _selectAllMaterialItems = GameObjectCreationUtils.UGUICreateCButton(confirmButton.transform.parent, new Vector2(-300, 0), new Vector2(120, 50), 16, "材料清仓");

            _selectAllMaterialItems.ClearAndAddListener(delegate {
                var itemList = ____shopItemScroll.SortAndFilter.OutputItemList;
                for (var i = itemList.Count - 1; i >= 0; i--)
                {
                    var itemDisplayData = itemList[i];
                    if (itemDisplayData.Key.ItemType != ItemType.Material) continue;
                    ItemView itemView = ____shopItemScroll.FindActiveItem(itemDisplayData.Key);
                    var dragDropGroup = __instance.GetComponent<DragDropGroup>();
                    if (dragDropGroup.DraggingIdentify == null)
                    {
                        Traverse.Create(__instance).Method("PutShopItemToTrade", itemView, itemDisplayData.Amount).GetValue();
                    }
                }
            });
        }
    }
}
