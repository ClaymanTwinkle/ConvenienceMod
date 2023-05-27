using System;
using System.Collections.Generic;
using System.Linq;
using FrameWork;
using FrameWork.ModSystem;
using GameData.Domains.Item;
using GameData.Domains.Item.Display;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ConvenienceFrontend.CustomWeapon
{
    internal class CustomWeaponFrontPatch : BaseFrontPatch
    {
        private static bool _enableCustomWeapon = false;

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "Toggle_EnableCustomWeapon", ref _enableCustomWeapon);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemScrollView), "SetItemToPopupMenuMode")]
        public static void SetItemToPopupMenuMode(ItemScrollView __instance, InfinityScroll ____itemScroll, ItemKey key,ref List<UI_PopupMenu.BtnData> btnList, Action onCancel)
        {
            if (!_enableCustomWeapon) return;

            if (key.ItemType != ItemType.Weapon) return;
            var itemData = __instance.SortAndFilter.OutputItemList.Find((ItemDisplayData data) => data.Key.Equals(key));
            if (itemData == null) return;

            int index = __instance.SortAndFilter.OutputItemList.FindIndex((ItemDisplayData data) => data.Key.Equals(key));
            ItemView itemView = ____itemScroll.GetActiveCell(index).GetComponent<ItemView>();
            var parentTransform = itemView.transform.parent;
            CScrollRect scrollRect = ____itemScroll.GetComponent<CScrollRect>();

            btnList.Add(new UI_PopupMenu.BtnData("调整招式", true, delegate
            {
                int _listenerId = -1;
                void OnNotifyGameData(List<NotificationWrapper> notifications)
                {
                    GameDataBridge.UnregisterListener(_listenerId);
                    _listenerId = -1;
                    foreach (NotificationWrapper notification in notifications)
                    {
                        if (notification.Notification.DomainId == 6 && notification.Notification.MethodId == 10)
                        {
                            int offset = notification.Notification.ValueOffset;
                            RawDataPool dataPool = notification.DataPool;

                            List<sbyte> item = null;
                            Serializer.Deserialize(dataPool, offset, ref item);
                            string[] tricks = new string[item.Count];
                            for (int i = 0; i < item.Count; i++)
                            {
                                var text = Config.TrickType.Instance[item[i]].Name;
                                tricks[i] = text;
                            }

                            var element = UI_ChangeTrick.GetUI();
                            ArgumentBox box = EasyPool.Get<ArgumentBox>();
                            box.SetObject("Tricks", tricks.ToList<String>());
                            box.SetObject("ItemKey", key);
                            element.SetOnInitArgs(box);
                            element.OnShowed = (Action)delegate
                            {
                                // itemView.SetHighLight(show: true);
                                scrollRect.SetScrollEnable(canScroll: false);
                            };
                            element.OnHide = delegate
                            {
                                Debug.Log("隐藏弹窗");
                                itemView.transform.SetParent(parentTransform);
                                // itemView.SetHighLight(show: false);
                                scrollRect.SetScrollEnable(canScroll: true);
                                onCancel.Invoke();
                            };
                            UIManager.Instance.ShowUI(element);
                        }
                    }

                }
                _listenerId = GameDataBridge.RegisterListener(OnNotifyGameData);
                GameDataBridge.AddMethodCall<ItemKey>(_listenerId, 6, 10, key);
            }));
        }
    }
}
