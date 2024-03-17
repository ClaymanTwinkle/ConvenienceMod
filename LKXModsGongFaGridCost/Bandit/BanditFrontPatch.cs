using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using GameData.Domains.Item.Display;
using GameData.Domains.Item;
using GameData.Domains.TaiwuEvent.DisplayEvent;
using GameData.Utilities;
using HarmonyLib;
using TMPro;
using UnityEngine;
using GameData.Domains.Character.Display;
using GameData.Domains.Character;
using System.Reflection;
using Config;

namespace ConvenienceFrontend.Bandit
{
    internal class BanditFrontPatch : BaseFrontPatch
    {
        private static CButton _kidnapButton = null;

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_EventWindow), "UpdateOptionScroll")]
        public static void UpdateOptionScroll_Postfix(UI_EventWindow __instance)
        {
            var displayingEventData = SingletonObject.getInstance<EventModel>().DisplayingEventData;
            if (displayingEventData == null) return;
            var eventGuid = displayingEventData.EventGuid;

            switch (eventGuid)
            {
                case "aee604fc-c0b8-468e-bf51-8665e2844c00":
                case "ad16ffb4-63fa-4282-b67c-c784beecdf0c":
                case "ed52b469-6369-40be-8090-6f18d4cc431c":
                    if (_kidnapButton != null && _kidnapButton.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(_kidnapButton.gameObject);
                        _kidnapButton = null;
                    }
                    CButton cButton = __instance.CGet<CButton>("Confirm");
                    TextMeshProUGUI contentTxt = __instance.CGet<TextMeshProUGUI>("EventContent");
                    _kidnapButton = UnityEngine.Object.Instantiate<GameObject>(cButton.gameObject).GetComponent<CButton>();
                    _kidnapButton.transform.SetParent(contentTxt.transform.parent, false);
                    _kidnapButton.transform.localPosition += new Vector3(150f, 50f, 0f);
                    _kidnapButton.gameObject.SetActive(true);
                    _kidnapButton.interactable = true;
                    GameObject go = UnityEngine.Object.Instantiate<GameObject>(contentTxt.gameObject);
                    TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
                    text.font = contentTxt.font;
                    text.fontSize = contentTxt.fontSize;
                    text.color = contentTxt.color;
                    text.text = "关押对手";
                    text.raycastTarget = false;
                    go.transform.SetParent(_kidnapButton.transform, false);
                    go.transform.localPosition = new Vector3(0f, 35f, 0f);
                    _kidnapButton.ClearAndAddListener(delegate
                    {
                        // ItemDisplayData ropeData = this._kidnapRopes[index];
                        CharacterDisplayData charData = displayingEventData.TargetCharacter;
                        var taiwuCharId = SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
                        ArgumentBox argBox = EasyPool.Get<ArgumentBox>();
                        // argBox.SetObject("initItemKey", ropeData.Key);
                        argBox.Set("filterType", ItemSortAndFilter.ItemFilterType.Other);
                        argBox.Set("itemSubType", ItemSubType.Rope);
                        argBox.SetObject("callback", new Action<ItemKey>(delegate (ItemKey ropeItemKey)
                        {
                            CharacterDomainHelper.MethodCall.AddKidnappedCharacter(taiwuCharId, charData.CharacterId, ropeItemKey);

                            var info = displayingEventData.EventOptionInfos.Find((EventOptionInfo x) => {
                                return x.OptionContent.Contains("任其离开");
                            });
                            Traverse.Create(__instance).Method("SelectOption", new object[]
                            {
                            info
                            }).GetValue();
                        }));
                        UIElement.SelectItem.SetOnInitArgs(argBox);
                        UIManager.Instance.ShowUI(UIElement.SelectItem);
                    });
                    break;
                default:
                    if (_kidnapButton != null && _kidnapButton.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(_kidnapButton.gameObject);
                        _kidnapButton = null;
                    }
                    break;
                
            }
        }
    }
}
