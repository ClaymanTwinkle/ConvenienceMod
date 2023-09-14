using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
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
                StrategyConst.Item item = StrategyConst.ItemOptions[val];

                // 比较
                if (item.judgementIndex != -1)
                {
                    judgementOption.SetActive(true);
                    judgementDropDown.ClearOptions();
                    judgementDropDown.AddOptions(StrategyConst.JudgementList[item.judgementIndex].ToList());
                    judgementDropDown.value = 0;
                }
                else
                {
                    judgementOption.SetActive(false);
                }

                // 数值输入框
                if (item.ShowNumSetter)
                {
                    inputField.SetActive(true);
                    TextMeshProUGUI placeholder = (TextMeshProUGUI)input.placeholder;
                    placeholder.text = item.Multiplyer == 1f ? "0" : "0.0";
                    confirm.interactable = float.TryParse(input.text, out float num);
                    input.text = "";
                }
                else
                {
                    inputField.SetActive(false);
                }

                // 子选项
                if (item.OptionIndex >= 0)
                {
                    valueDropDown.ClearOptions();
                    valueDropDown.AddOptions(StrategyConst.OptionsList[item.OptionIndex].ToList<string>());
                    valueOption.SetActive(true);
                    confirm.interactable = true;
                    judgementDropDown.value = 0;
                }
                else
                {
                    valueOption.SetActive(false);
                }

                // 选择技能按钮
                selectButtonGameObject.SetActive(item.ShowSelectSkillBtn);
                selectButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16f;
                selectButton.GetComponentInChildren<TextMeshProUGUI>().text = "选择技能";
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
                playerOptions.value = (condition.isAlly ? 0 : 1);
                itemOptions.value = (int)condition.item;
                judgementOptions.value = (int)condition.judge;
                StrategyConst.Item item = StrategyConst.ItemOptions[(int)condition.item];
                bool showNumSetter = item.ShowNumSetter;
                if (showNumSetter)
                {
                    string format = (condition.item == JudgeItem.Distance) ? "f1" : "f0";
                    inputField.text = ((float)condition.value / item.Multiplyer).ToString(format);
                }
                if (item.OptionIndex >= 0)
                {
                    if (condition.item == JudgeItem.HasTrick)
                    {
                        valueOptions.value = condition.subType + 1;
                    }
                    else if (condition.item == JudgeItem.DefeatMarkCount)
                    {
                        valueOptions.value = condition.subType;
                    }
                    else if (condition.item == JudgeItem.Buff || condition.item == JudgeItem.Debuff)
                    {
                        var index = StrategyConst.GetSpecialEffectNameList().IndexOf(StrategyConst.GetSpecialEffectNameById(condition.subType));
                        valueOptions.value = Math.Max(index, 0);
                    }
                    else 
                    {
                        if (!showNumSetter)
                        {
                            valueOptions.value = condition.value;
                        }
                        else
                        {
                            valueOptions.value = condition.subType;
                        }
                    }
                }
                if (item.ShowSelectSkillBtn)
                {
                    CombatSkillItem combatSkillItem = CombatSkill.Instance[condition.subType];
                    selectButton.GetComponentInChildren<TextMeshProUGUI>().text = combatSkillItem != null ? combatSkillItem.Name : "选择技能";
                }
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
                condition.isAlly = (playerOptions.value == 0);
                int value = itemOptions.value;
                condition.item = (JudgeItem)value;
                condition.judge = (Judgement)judgementOptions.value;
                StrategyConst.Item uiItem = StrategyConst.ItemOptions[value];

                if (uiItem.ShowNumSetter)
                {
                    float.TryParse(inputField.text, out float floatValue);
                    condition.value = ((int)(floatValue * StrategyConst.ItemOptions[value].Multiplyer));
                }
                else
                {
                    condition.valueStr = valueOptions.options[valueOptions.value].text;
                    condition.value = valueOptions.value;
                }

                if (uiItem.ShowSelectSkillBtn)
                {
                    try {
                        CombatSkillItem combatSkillItem = CombatSkill.Instance[selectButton.GetComponentInChildren<TextMeshProUGUI>().text];
                        if (combatSkillItem != null)
                        {
                            condition.subType = combatSkillItem.TemplateId;
                        }
                        else
                        {
                            condition.subType = -1;
                        }
                    } catch {
                    }
                }
                else if (condition.item == JudgeItem.HasTrick)
                {
                    condition.subType = valueOptions.value - 1;
                }
                else if (condition.item == JudgeItem.Buff || condition.item == JudgeItem.Debuff)
                {
                    condition.subType = StrategyConst.GetSpecialEffectIdByName(valueOptions.options[valueOptions.value].text);
                }
                else
                {
                    condition.subType = valueOptions.value;
                }
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
}
