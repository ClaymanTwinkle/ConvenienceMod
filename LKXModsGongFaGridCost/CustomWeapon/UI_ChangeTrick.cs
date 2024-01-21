using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using FrameWork.ModSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Object = UnityEngine.Object;
using ConvenienceFrontend.CombatStrategy;
using HarmonyLib;
using UnityEngine.U2D;
using GameData.Domains.Item;
using static TMPro.TMP_Dropdown;
using GameData.Utilities;

namespace ConvenienceFrontend.CustomWeapon
{
    internal class UI_ChangeTrick : UIBase
    {
        private static UIElement element;

        public static UIElement GetUI()
        {
            UIElement result;
            if (UI_ChangeTrick.element != null && UI_ChangeTrick.element.UiBase != null)
            {
                result = UI_ChangeTrick.element;
            }
            else
            {
                UI_ChangeTrick.element = new UIElement
                {
                    Id = -1
                };
                Traverse.Create(UI_ChangeTrick.element).Field("_path").SetValue("UI_Change_Trick");

                GameObject gameObject = GameObjectCreationUtils.CreatePopupWindow(null, "", null, null, true, 0, 0, 800, 200);
                UI_ChangeTrick ui_ChangeTrick = gameObject.AddComponent<UI_ChangeTrick>();
                ui_ChangeTrick.UiType = UILayer.LayerPopUp; //3;
                ui_ChangeTrick.Element = UI_ChangeTrick.element;
                ui_ChangeTrick.RelativeAtlases = new SpriteAtlas[0];
                ui_ChangeTrick.Init(gameObject);
                UI_ChangeTrick.element.UiBase = ui_ChangeTrick;
                UI_ChangeTrick.element.UiBase.name = UI_ChangeTrick.element.Name;
                UIManager.Instance.PlaceUI(UI_ChangeTrick.element.UiBase);
                result = UI_ChangeTrick.element;
            }
            return result;
        }

        private void Init(GameObject obj)
        {
            obj.name = "popUpWindowBase";
            PopupWindow popupWindow = this.gameObject.GetComponent<PopupWindow>();
            popupWindow.TitleLabel.gameObject.SetActive(false);
            popupWindow.ConfirmButton.gameObject.SetActive(true);
            popupWindow.CancelButton.gameObject.SetActive(true);
            popupWindow.CancelButton.onClick.RemoveAllListeners();
            popupWindow.CancelButton.onClick.AddListener(delegate ()
            {
                QuickHide();
            });
            RectTransform component = obj.GetComponent<RectTransform>();
            var gameObject2 = UIUtils.CreateRow(component.transform).gameObject;
            gameObject2.transform.position = new Vector3(gameObject2.transform.position.x - 120, gameObject2.transform.position.y, gameObject2.transform.position.z);

            for (var i = 0; i < 6; i++)
            {
                GameObject gameObject3 = GameObjectCreationUtils.InstantiateUIElement(gameObject2.transform, "CommonDropdown");
                Extentions.SetWidth(gameObject3.GetComponent<RectTransform>(), 80f);
                CDropdown dropdown = gameObject3.GetComponent<CDropdown>();
                var optionsList = new List<String>();
                var value = 0;
                for (var j = 0; j < Config.TrickType.Instance.Count; j++)
                {
                    var option = Config.TrickType.Instance[j].Name;
                    optionsList.Add(option);
                }
                dropdown.AddOptions(optionsList);
                dropdown.SetValueWithoutNotify(value);
                base.AddMono(dropdown, "Options" + i);
            }
        }

        public override void OnInit(ArgumentBox argsBox)
        {
            argsBox.Get("Tricks", out List<String> tricks);
            argsBox.Get("ItemKey", out ItemKey itemKey);

            var allowUseTrickSet = new HashSet<string>();
            var allowUseTrickIds = Config.Weapon.Instance[itemKey.TemplateId].Tricks;
            foreach (var trickId in allowUseTrickIds)
            {
                allowUseTrickSet.Add(Config.TrickType.Instance[trickId].Name);
            }

            var allowUseTrickList = new List<String>();
            foreach (var trick in allowUseTrickSet)
            {
                allowUseTrickList.Add(trick);
            }

            PopupWindow popupWindow = this.gameObject.GetComponent<PopupWindow>();
            for (var i = 0; i < tricks.Count; i++)
            {
                CDropdown dropdown = base.CGet<CDropdown>("Options" + i);
                if (dropdown != null) 
                {
                    dropdown.ClearOptions();
                    dropdown.AddOptions(allowUseTrickList);

                    var value = dropdown.options.FindIndex((OptionData data) => data.text == tricks[i]);
                    if (value > -1) 
                    {
                        dropdown.SetValueWithoutNotify(value);
                    }
                }
            }

            popupWindow.OnConfirmClick = delegate ()
            {
                Debug.Log("提交！！！");
                var newTricks = new List<String>();
                for (var i = 0; i < tricks.Count; i++)
                {
                    CDropdown dropdown = base.CGet<CDropdown>("Options" + i);
                    if (dropdown != null)
                    {
                        newTricks.Add(dropdown.options[dropdown.value].text);
                    }
                }

                AsyncMethodCall<ItemKey, List<String>>(6, 160, itemKey, newTricks, delegate (int offset, RawDataPool dataPool)
                {
                    this.ShowDialog("修改结果", "修改成功", null);
                });
                QuickHide();
            };
        }

        private void ShowDialog(string title, string message, Action onYes)
        {
            DialogCmd dialogCmd = new DialogCmd();
            dialogCmd.Type = 1;
            dialogCmd.Title = title;
            dialogCmd.Content = message;
            if (onYes != null)
            {
                dialogCmd.Yes = onYes;
            }
            if (onYes != null)
            {
                dialogCmd.No = onYes;
            }
            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", dialogCmd));
            UIManager.Instance.ShowUI(UIElement.Dialog);
        }
    }
}
