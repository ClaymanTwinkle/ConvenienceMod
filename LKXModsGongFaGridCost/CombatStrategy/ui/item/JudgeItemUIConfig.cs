using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.RectTransform;

namespace ConvenienceFrontend.CombatStrategy.ui.item
{
    public class JudgeItemUIConfig
    {
        public string Name;

        public bool ShowNumSetter;

        public float Multiplyer;

        public int OptionIndex;

        public bool ShowSelectSkillBtn;

        public int judgementIndex;

        public JudgeItemUIConfig(string name, bool showNumSetter, float multiplyer, int optionIndex, bool showSelectSkillBtn = false, int judgementIndex = -1)
        {
            this.Name = name;
            this.ShowNumSetter = showNumSetter;
            this.Multiplyer = multiplyer;
            this.OptionIndex = optionIndex;
            this.ShowSelectSkillBtn = showSelectSkillBtn;
            this.judgementIndex = judgementIndex;
        }

        public void OnSelect(ConditionUIHolder uiHolder)
        {
            // 比较
            if (judgementIndex != -1)
            {
                uiHolder.judgementOptions.gameObject.SetActive(true);
                uiHolder.judgementOptions.ClearOptions();
                uiHolder.judgementOptions.AddOptions(StrategyConst.JudgementList[judgementIndex].ToList());
                uiHolder.judgementOptions.value = 0;
            }
            else
            {
                uiHolder.judgementOptions.gameObject.SetActive(false);
            }

            // 数值输入框
            if (ShowNumSetter)
            {
                uiHolder.inputField.gameObject.SetActive(true);
                TextMeshProUGUI placeholder = (TextMeshProUGUI)uiHolder.inputField.placeholder;
                placeholder.text = Multiplyer == 1f ? "0" : "0.0";
                uiHolder.confirmButton.interactable = float.TryParse(uiHolder.inputField.text, out float num);
                uiHolder.inputField.text = "";
            }
            else
            {
                uiHolder.inputField.gameObject.SetActive(false);
            }

            // 子选项
            if (OptionIndex >= 0)
            {
                uiHolder.valueOptions.ClearOptions();
                uiHolder.valueOptions.AddOptions(StrategyConst.OptionsList[OptionIndex].ToList<string>());
                uiHolder.valueOptions.gameObject.SetActive(true);
                uiHolder.confirmButton.interactable = true;
                uiHolder.judgementOptions.value = 0;
            }
            else
            {
                uiHolder.valueOptions.gameObject.SetActive(false);
            }

            // 选择技能按钮
            uiHolder.selectButton.gameObject.SetActive(ShowSelectSkillBtn);
            uiHolder.selectButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16f;
            uiHolder.selectButton.GetComponentInChildren<TextMeshProUGUI>().text = "选择技能";
        }

        public void OnShow(Condition condition, ConditionUIHolder uiHolder)
        {
            uiHolder.playerOptions.value = (condition.isAlly ? 0 : 1);
            bool manualSendCallback = uiHolder.itemOptions.value == (int)condition.item;
            uiHolder.itemOptions.value = (int)condition.item;
            if (manualSendCallback)
            {
                uiHolder.itemOptions.onValueChanged.Invoke(uiHolder.itemOptions.value);
            }
            uiHolder.judgementOptions.value = (int)condition.judge;
            if (ShowNumSetter)
            {
                string format = (condition.item == JudgeItem.Distance) ? "f1" : "f0";
                uiHolder.inputField.text = ((float)condition.value / Multiplyer).ToString(format);
            }
            if (OptionIndex >= 0)
            {
                if (condition.item == JudgeItem.HasTrick)
                {
                    uiHolder.valueOptions.value = condition.subType + 1;
                }
                else if (condition.item == JudgeItem.DefeatMarkCount)
                {
                    uiHolder.valueOptions.value = condition.subType;
                }
                else if (condition.item == JudgeItem.Buff || condition.item == JudgeItem.Debuff)
                {
                    var index = StrategyConst.GetSpecialEffectNameList().IndexOf(StrategyConst.GetSpecialEffectNameById(condition.subType));
                    uiHolder.valueOptions.value = Math.Max(index, 0);
                }
                else
                {
                    if (!ShowNumSetter)
                    {
                        uiHolder.valueOptions.value = condition.value;
                    }
                    else
                    {
                        uiHolder.valueOptions.value = condition.subType;
                    }
                }
            }
            if (ShowSelectSkillBtn)
            {
                CombatSkillItem combatSkillItem = CombatSkill.Instance[condition.subType];
                uiHolder.selectButton.GetComponentInChildren<TextMeshProUGUI>().text = combatSkillItem != null ? combatSkillItem.Name : "选择技能";
            }
        }

        public void OnConfirm(Condition condition, ConditionUIHolder uiHolder)
        {
            int value = uiHolder.itemOptions.value;

            condition.isAlly = (uiHolder.playerOptions.value == 0);
            condition.item = (JudgeItem)value;
            condition.judge = (Judgement)uiHolder.judgementOptions.value;

            if (ShowNumSetter)
            {
                float.TryParse(uiHolder.inputField.text, out float floatValue);
                condition.value = ((int)(floatValue * StrategyConst.ItemOptions[value].Multiplyer));
            }
            else
            {
                condition.valueStr = uiHolder.valueOptions.options[uiHolder.valueOptions.value].text;
                condition.value = uiHolder.valueOptions.value;
            }

            if (ShowSelectSkillBtn)
            {
                try
                {
                    CombatSkillItem combatSkillItem = CombatSkill.Instance[uiHolder.selectButton.GetComponentInChildren<TextMeshProUGUI>().text];
                    if (combatSkillItem != null)
                    {
                        condition.subType = combatSkillItem.TemplateId;
                    }
                    else
                    {
                        condition.subType = -1;
                    }
                }
                catch
                {
                }
            }
            else if (condition.item == JudgeItem.HasTrick)
            {
                condition.subType = uiHolder.valueOptions.value - 1;
            }
            else if (condition.item == JudgeItem.Buff || condition.item == JudgeItem.Debuff)
            {
                condition.subType = StrategyConst.GetSpecialEffectIdByName(uiHolder.valueOptions.options[uiHolder.valueOptions.value].text);
            }
            else
            {
                condition.subType = uiHolder.valueOptions.value;
            }
        }

        public string GetShowDesc(Condition condition)
        {
            JudgeItem judgeItem = condition.item;

            StringBuilder stringBuilder = new StringBuilder();
            if (judgeItem > JudgeItem.Distance)
            {
                stringBuilder.Append(StrategyConst.PlayerOptions[condition.isAlly ? 0 : 1]).Append(' ');
            }
            if (ShowSelectSkillBtn)
            {
                CombatSkillItem combatSkillItem = CombatSkill.Instance[condition.subType];
                if (combatSkillItem != null)
                {
                    stringBuilder.Append(combatSkillItem.Name).Append(' ');
                }
                else
                {
                    stringBuilder.Append("未选择技能").Append(' ');
                }
            }
            else if (judgeItem == JudgeItem.HasTrick && condition.subType >= 0)
            {
                stringBuilder.Append(StrategyConst.TrickTypeOptions[condition.subType + 1]).Append(' ');
            }
            else if (judgeItem == JudgeItem.DefeatMarkCount)
            {
                stringBuilder.Append(StrategyConst.DefeatMarkOptions[condition.subType]).Append(' ');
            }
            else if (judgeItem == JudgeItem.Buff || judgeItem == JudgeItem.Debuff)
            {
                stringBuilder.Append(StrategyConst.GetSpecialEffectNameById(condition.subType)).Append(' ');
            }
            else if (judgeItem == JudgeItem.CharacterAttribute)
            {
                if (OptionIndex > -1)
                {
                    stringBuilder.Append(StrategyConst.OptionsList[OptionIndex][condition.subType]);
                }
            }
            stringBuilder.Append(Name).Append(' ');

            // value
            if (judgeItem == JudgeItem.WeaponType)
            {
                stringBuilder.Append(StrategyConst.WeaponTypeOptions[condition.value]).Append(' ');
            }
            else if (judgeItem == JudgeItem.PreparingAction)
            {
                stringBuilder.Append(StrategyConst.SkillTypeOptions[condition.value]).Append(' ');
            }
            else if (judgeItem == JudgeItem.CanUseSkill)
            {
                stringBuilder.Append(StrategyConst.SatisfiedorDissatisfied[condition.value]).Append(' ');
            }
            else if (judgeItem == JudgeItem.AffectingSkill || judgeItem == JudgeItem.EquippingSkill)
            {
                stringBuilder.Append(StrategyConst.YesOrNo[condition.value]).Append(' ');
            }
            else if (judgeItem == JudgeItem.DirectionSkill)
            {
                stringBuilder.Append(StrategyConst.SkillDirection[condition.value]).Append(' ');
            }
            else if (judgeItem == JudgeItem.CurrentTrick)
            {
                stringBuilder.Append(TrickType.Instance[condition.value].Name).Append(' ');
            }

            if (judgementIndex != -1)
            {
                stringBuilder.Append(StrategyConst.JudgementList[judgementIndex][(int)condition.judge]).Append(' ');
            }

            if (ShowNumSetter)
            {
                string format = (judgeItem == JudgeItem.Distance) ? "f1" : "f0";
                float multiplyer = Multiplyer;
                stringBuilder.Append(((float)condition.value / multiplyer).ToString(format));
            }

            return stringBuilder.ToString();
        }
    }
}
