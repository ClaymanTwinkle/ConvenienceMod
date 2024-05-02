using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using ConvenienceFrontend.CombatStrategy.ui.item;
using FrameWork.ModSystem;
using TMPro;
using UnityEngine;

namespace ConvenienceFrontend.CombatStrategy.ui
{
    public class ConditionSetterPanel
    {
        private RectTransform _conditionSetter;

        public static ConditionSetterPanel Create(Transform parent) { return new ConditionSetterPanel(parent); }
        
        private ConditionSetterPanel(Transform parent) {
            this._conditionSetter = CreateConditionSetter(parent).GetComponent<RectTransform>();
        }

        /// <summary>
        /// 条件选择面板
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static GameObject CreateConditionSetter(Transform parent)
        {
            GameObject sliceDownSheet = UIUtils.CreateSliceDownSheetPanel(parent);
            GameObject panelGameObject = sliceDownSheet.transform.Find("Panel").gameObject;
            Refers refers = sliceDownSheet.GetComponent<Refers>();
            CButton confirm = sliceDownSheet.transform.Find("Confirm").GetComponent<CButton>();

            // 选择我方/敌方
            GameObject playerOptionsGameObject = GameObjectCreationUtils.InstantiateUIElement(panelGameObject.transform, "CommonDropdown");
            Extentions.SetWidth(playerOptionsGameObject.GetComponent<RectTransform>(), 180f);
            CDropdown playerOptions = playerOptionsGameObject.GetComponent<CDropdown>();
            playerOptions.AddOptions(StrategyConst.PlayerOptions.ToList<string>());
            refers.AddMono(playerOptions, "PlayerOptions");

            // 按钮
            CButton selectButton = GameObjectCreationUtils.UGUICreateCButton(panelGameObject.transform, 36f, "未选择");
            var selectButtonGameObject = selectButton.gameObject;
            Extentions.SetWidth(selectButtonGameObject.GetComponent<RectTransform>(), 180f);
            Extentions.SetHeight(selectButtonGameObject.GetComponent<RectTransform>(), 40f);
            refers.AddMono(selectButton, "SelectButton");
            selectButtonGameObject.SetActive(false);

            // 条件选项
            GameObject itemOptionsGameObject = GameObjectCreationUtils.InstantiateUIElement(panelGameObject.transform, "CommonDropdown");
            Extentions.SetWidth(itemOptionsGameObject.GetComponent<RectTransform>(), 180f);
            CDropdown itemOptions = itemOptionsGameObject.GetComponent<CDropdown>();
            itemOptions.AddOptions(StrategyConst.ItemOptions.ToList().ConvertAll<String>(x => x.Name));
            refers.AddMono(itemOptions, "ItemOptions");

            // 二次条件选项
            GameObject valueOption = GameObjectCreationUtils.InstantiateUIElement(panelGameObject.transform, "CommonDropdown");
            Extentions.SetWidth(valueOption.GetComponent<RectTransform>(), 180f);
            CDropdown valueDropDown = valueOption.GetComponent<CDropdown>();
            refers.AddMono(valueDropDown, "ValueOptions");
            valueOption.SetActive(false);

            // 比较大小
            GameObject judgementOption = GameObjectCreationUtils.InstantiateUIElement(panelGameObject.transform, "CommonDropdown");
            Extentions.SetWidth(judgementOption.GetComponent<RectTransform>(), 180f);
            CDropdown judgementDropDown = judgementOption.GetComponent<CDropdown>();
            // judgementDropDown.AddOptions(StrategyConst.JudgementOptions.ToList<string>());
            refers.AddMono(judgementDropDown, "JudgementOptions");
            judgementOption.SetActive(false);

            // 输入框
            GameObject inputField = GameObjectCreationUtils.InstantiateUIElement(panelGameObject.transform, "CommonInputField");
            Extentions.SetWidth(inputField.GetComponent<RectTransform>(), 180f);
            TMP_InputField input = inputField.GetComponent<TMP_InputField>();
            refers.AddMono(input, "InputField");
            inputField.SetActive(false);

            itemOptions.onValueChanged.RemoveAllListeners();
            itemOptions.onValueChanged.AddListener(delegate (int val)
            {
                JudgeItemUIConfig item = StrategyConst.ItemOptions[val];
                item.OnSelect
                (
                    new ConditionUIHolder(playerOptions, itemOptions, judgementDropDown, input, valueDropDown, selectButton, confirm)
                );
            });
            sliceDownSheet.SetActive(false);
            return sliceDownSheet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="condition"></param>
        public void ShowConditionSetter(Transform parent, Condition condition, Action renderConditionText, Action<int, Action<sbyte, short>> showSkillSelectUI)
        {
            Vector3 vector = UIManager.Instance.UiCamera.WorldToScreenPoint(parent.position);
            this._conditionSetter.position = UIManager.Instance.UiCamera.ScreenToWorldPoint(vector);
            this._conditionSetter.anchoredPosition += new Vector2(40f, -50f);
            this._conditionSetter.gameObject.SetActive(true);
            this._conditionSetter.parent.gameObject.SetActive(true);  // _focus
            Refers refers = this._conditionSetter.gameObject.GetComponent<Refers>();
            var confirmButton = refers.CGet<CButton>("Confirm");
            var cancelButton = refers.CGet<CButton>("Cancel");
            var playerOptions = refers.CGet<CDropdown>("PlayerOptions");
            var itemOptions = refers.CGet<CDropdown>("ItemOptions");
            var judgementOptions = refers.CGet<CDropdown>("JudgementOptions");
            var inputField = refers.CGet<TMP_InputField>("InputField");
            var valueOptions = refers.CGet<CDropdown>("ValueOptions");
            var selectButton = refers.CGet<CButton>("SelectButton");

            if (condition.IsComplete())
            {
                JudgeItemUIConfig item = StrategyConst.ItemOptions[(int)condition.item];
                item.OnShow(condition, new ConditionUIHolder(playerOptions, itemOptions, judgementOptions, inputField, valueOptions, selectButton, confirmButton));
            }
            else
            {
                playerOptions.value = 0;
                itemOptions.value = 0;
                itemOptions.onValueChanged.Invoke(0);
                judgementOptions.value = 0;
                valueOptions.value = 0;
                inputField.text = "0";
                selectButton.GetComponentInChildren<TextMeshProUGUI>().text = "未选择";
            }
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(delegate (string val)
            {
                int value = itemOptions.value;

                confirmButton.interactable = float.TryParse(val, out var num);
            });

            selectButton.ClearAndAddListener(delegate ()
            {
                int value = itemOptions.value;

                var _onSelected = new Action<sbyte, short>((sbyte type, short skillId) =>
                {
                    if (type == 1)
                    {
                        Debug.Log("选中功法" + skillId);
                        CombatSkillItem selectSkillItem = CombatSkill.Instance[skillId];
                        if (selectSkillItem != null) selectButton.GetComponentInChildren<TextMeshProUGUI>().text = selectSkillItem.Name;
                    }
                    else
                    {
                        // cancel
                    }
                });
                showSkillSelectUI(value, _onSelected);
            });

            confirmButton.ClearAndAddListener(delegate ()
            {
                int value = itemOptions.value;
                JudgeItemUIConfig uiItem = StrategyConst.ItemOptions[value];
                uiItem.OnConfirm(condition, new ConditionUIHolder(playerOptions, itemOptions, judgementOptions, inputField, valueOptions, selectButton, confirmButton));
                renderConditionText();
                this._conditionSetter.gameObject.SetActive(false);
                this._conditionSetter.parent.gameObject.SetActive(false); // _focus
            });
            cancelButton.ClearAndAddListener(delegate ()
            {
                this._conditionSetter.gameObject.SetActive(false);
                this._conditionSetter.parent.gameObject.SetActive(false); // _focus
            });
        }
    }

    public struct ConditionUIHolder
    {
        /// <summary>
        /// 条件对象
        /// </summary>
        public CDropdown playerOptions;

        /// <summary>
        /// 条件类型
        /// </summary>
        public CDropdown itemOptions;
        /// <summary>
        /// 比较
        /// </summary>
        public CDropdown judgementOptions;
        /// <summary>
        /// 输入框
        /// </summary>
        public TMP_InputField inputField;

        /// <summary>
        /// 二次条件选择
        /// </summary>
        public CDropdown valueOptions;

        /// <summary>
        /// 选择按钮
        /// </summary>
        public CButton selectButton;

        /// <summary>
        /// 确认按钮
        /// </summary>
        public CButton confirmButton;

        public ConditionUIHolder(CDropdown playerOptions, CDropdown itemOptions, CDropdown judgementOptions, TMP_InputField inputField, CDropdown valueOption, CButton selectButton, CButton confirmButton)
        {
            this.playerOptions = playerOptions;
            this.itemOptions = itemOptions;
            this.judgementOptions = judgementOptions;
            this.inputField = inputField;
            this.valueOptions = valueOption;
            this.selectButton = selectButton;
            this.confirmButton = confirmButton;
        }
    }
}
