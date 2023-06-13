using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CombatStrategy;
using ConvenienceFrontend.TaiwuBuildingManager;
using FrameWork.ModSystem;
using FrameWork;
using HarmonyLib;
using UnityEngine.U2D;
using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;

namespace ConvenienceFrontend.QuicklyCreateCharacter
{
    internal class UI_RollFilter : UIBase
    {
        private static UIElement element;

        private CScrollRect _scroll;

        private Dictionary<string, Component> _componentDic = new Dictionary<string, Component>();
        public static UIElement GetUI()
        {
            UIElement result;
            if (UI_RollFilter.element != null && UI_RollFilter.element.UiBase != null)
            {
                result = UI_RollFilter.element;
            }
            else
            {
                UI_RollFilter.element = new UIElement
                {
                    Id = -1
                };
                Traverse.Create(UI_RollFilter.element).Field("_path").SetValue("UI_RollFilter");

                GameObject gameObject = GameObjectCreationUtils.CreatePopupWindow(null, "筛选条件", null, null, true, 0, 0, 1300, 1000);
                UI_RollFilter uiBase = gameObject.AddComponent<UI_RollFilter>();
                uiBase.UiType = UILayer.LayerPopUp; //3;
                uiBase.Element = UI_RollFilter.element;
                uiBase.RelativeAtlases = new SpriteAtlas[0];
                uiBase.Init(gameObject);
                UI_RollFilter.element.UiBase = uiBase;
                UI_RollFilter.element.UiBase.name = UI_RollFilter.element.Name;
                UIManager.Instance.PlaceUI(UI_RollFilter.element.UiBase);
                result = UI_RollFilter.element;
            }
            return result;
        }

        private void Init(GameObject obj)
        {
            obj.name = "popUpWindowBase";
            PopupWindow popupWindow = this.gameObject.GetComponent<PopupWindow>();
            popupWindow.ConfirmButton.gameObject.SetActive(false);
            popupWindow.CancelButton.gameObject.SetActive(false);
            popupWindow.CloseButton.gameObject.SetActive(true);
            popupWindow.CloseButton.onClick.RemoveAllListeners();
            popupWindow.CloseButton.onClick.AddListener(delegate ()
            {
                QuickHide();
            });
            RectTransform component = obj.GetComponent<RectTransform>();

            popupWindow.TitleLabel.transform.parent.position = new Vector3(popupWindow.TitleLabel.transform.parent.position.x, popupWindow.TitleLabel.transform.parent.position.y + 50, popupWindow.TitleLabel.transform.parent.position.z);
            GameObject gameObject = GameObjectCreationUtils.InstantiateUIElement(popupWindow.transform, "VerticalScrollView");
            gameObject.SetActive(true);
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(component.rect.size.x, component.rect.size.y - 220);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 60, gameObject.transform.position.z);
            this._scroll = gameObject.GetComponent<CScrollRect>();
            GameObject gameObject2 = this._scroll.Content.gameObject;
            RectTransform content = this._scroll.Content;
            Extentions.SetWidth(content, component.rect.size.x * 0.9f);
            UIUtils.CreateVerticalAutoSizeLayoutGroup(gameObject2).spacing = 15f;

            BuildCharacterCharacteristics(content);
            BuildQualifications(content);
            BuildMainAttribute(content);
            AncientTombInscription(content);
        }

        public override void OnInit(ArgumentBox argsBox)
        {
        }

        public override void QuickHide()
        {
            ConvenienceFrontend.SaveConfig();

            base.QuickHide();
        }

        private void OnEnable()
        {
            foreach (var keyValuePair in _componentDic)
            {
                if (keyValuePair.Value is CToggle)
                {
                    RefreshComponent((CToggle)keyValuePair.Value, keyValuePair.Key);
                }
                else if (keyValuePair.Value is CToggleGroup)
                {
                    RefreshComponent((CToggleGroup)keyValuePair.Value, keyValuePair.Key);
                }
                else if (keyValuePair.Value is CDropdown)
                {
                    RefreshComponent((CDropdown)keyValuePair.Value, keyValuePair.Key);
                }
                else if (keyValuePair.Value is TSlider)
                {
                    RefreshComponent((TSlider)keyValuePair.Value, keyValuePair.Key);
                }
                else if (keyValuePair.Value is TMP_InputField)
                {
                    RefreshComponent((TMP_InputField)keyValuePair.Value, keyValuePair.Key);
                }
            }
        }

        private void BuildCharacterCharacteristics(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "CharacterCharacteristics", "人物特性").transform;

            AddComponent(UIUtils.CreateToggle(UIUtils.CreateRow(transform), "Toggle_FilterEnableAllBlueFeature", "人物特性只要全蓝"), "Toggle_FilterEnableAllBlueFeature");
            AddComponent(UIUtils.CreateInputField(UIUtils.CreateRow(transform), new Vector2(0, 0), new Vector2(800, 40), "InputField_FilterAllFeatures", "", "不世奇才 一把小刀 手长脚长", "包含特性"), "InputField_FilterAllFeatures");
        }

        private void BuildQualifications(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "CharacterQualifications", "人物资质").transform;

            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "ToggleGroup_FilterLifeSkillQualificationGrowthType", "技艺成长", CharacterDataTool.QualificationGrowthTypeNameArray, CharacterDataTool.QualificationGrowthTypeNameArray.Length, true, true), "ToggleGroup_FilterLifeSkillQualificationGrowthType");
            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "ToggleGroup_FilterLifeSkillQualificationsTypes", "指定技艺类型", CharacterDataTool.LifeSkillNameArray, CharacterDataTool.LifeSkillNameArray.Length, true, true), "ToggleGroup_FilterLifeSkillQualificationsTypes");
            AddComponent(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "SliderBar_LifeSkillQualificationsValue", 0, 100, 1, "f0", "期望指定技艺类型资质超过"), "SliderBar_LifeSkillQualificationsValue");

            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "ToggleGroup_FilterCombatSkillQualificationGrowthType", "功法成长", CharacterDataTool.QualificationGrowthTypeNameArray, CharacterDataTool.QualificationGrowthTypeNameArray.Length, true, true), "ToggleGroup_FilterCombatSkillQualificationGrowthType");
            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "ToggleGroup_FilterCombatSkillQualificationsTypes", "指定功法类型", CharacterDataTool.CombatSkillNameArray, CharacterDataTool.CombatSkillNameArray.Length, true, true), "ToggleGroup_FilterCombatSkillQualificationsTypes");
            AddComponent(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "SliderBar_FilterCombatSkillQualificationsValue", 0, 100, 1, "f0", "期望指定功法类型资质超过"), "SliderBar_FilterCombatSkillQualificationsValue");
        }

        private void BuildMainAttribute(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "CharacterMainAttribute", "人物主要属性").transform;

            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "ToggleGroup_FilterMainAttributeTypes", "指定属性类型", CharacterDataTool.MainAttributeNameArray, CharacterDataTool.MainAttributeNameArray.Length, true, true), "ToggleGroup_FilterMainAttributeTypes");
            AddComponent(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "SliderBar_FilterMainAttributeValue", 0, 100, 1, "f0", "期望指定主要属性类型超过"), "SliderBar_FilterMainAttributeValue");
        }

        /// <summary>
        /// 古冢遗刻
        /// </summary>
        /// <param name="parent"></param>
        private void AncientTombInscription(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "CharacterAncientTombInscription", "古冢遗刻").transform;

            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "ToggleGroup_FilterLifeSkillBookName", "技艺书类别", CharacterDataTool.LifeSkillNameArray, CharacterDataTool.LifeSkillNameArray.Length, true, true), "ToggleGroup_FilterLifeSkillBookName");
            AddComponent(UIUtils.CreateInputField(UIUtils.CreateRow(transform), new Vector2(0, 0), new Vector2(300, 40), "InputField_FilterCombatSkillBookName", "", "云龙九现腿", "功法书"), "InputField_FilterCombatSkillBookName");
            AddComponent(UIUtils.CreateInputField(UIUtils.CreateRow(transform), new Vector2(0, 0), new Vector2(300, 40), "InputField_FilterDirectAndReverse", "正正正正正", "正正正正正", "功法书正逆练"), "InputField_FilterDirectAndReverse");
            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "ToggleGroup_FilterGeneralPrinciples", "功法总纲", CharacterDataTool.GeneralPrinciplesNameArray, CharacterDataTool.GeneralPrinciplesNameArray.Length, true, true), "ToggleGroup_FilterGeneralPrinciples");
        }

        private void AddComponent(TSlider component, string name)
        {
            base.AddMono(component, name);
            _componentDic[name] = component;

            RefreshComponent(component, name);
        }

        private void RefreshComponent(TSlider component, string name)
        {
            component.onValueChanged.RemoveAllListeners();
            component.value = (float)ConvenienceFrontend.Config.GetTypedValue<Double>(name);
            component.onValueChanged.AddListener(delegate (float val) {
                ConvenienceFrontend.Config[name] = (Double)val;
            });
        }

        private void AddComponent(TMP_InputField component, string name)
        {
            base.AddMono(component, name);
            _componentDic[name] = component;

            RefreshComponent(component, name);
        }

        private void RefreshComponent(TMP_InputField component, string name)
        {
            component.onValueChanged.RemoveAllListeners();
            component.text = ConvenienceFrontend.Config.GetTypedValue<string>(name);
            component.onValueChanged.AddListener(delegate (string val) {
                ConvenienceFrontend.Config[name] = val;
            });
        }

        private void AddComponent(CDropdown component, string name)
        {
            base.AddMono(component, name);
            _componentDic[name] = component;

            RefreshComponent(component, name);
        }

        private void RefreshComponent(CDropdown component, string name)
        {
            component.onValueChanged.RemoveAllListeners();
            component.value = (int)ConvenienceFrontend.Config.GetTypedValue<Int64>(name);
            component.onValueChanged.AddListener(delegate (int val) {
                ConvenienceFrontend.Config[name] = (Int64)val;
            });
        }

        private void AddComponent(CToggle component, string name)
        {
            base.AddMono(component, name);
            _componentDic[name] = component;

            RefreshComponent(component, name);
        }

        private void RefreshComponent(CToggle component, string name)
        {
            component.onValueChanged.RemoveAllListeners();
            component.isOn = ConvenienceFrontend.Config.GetTypedValue<bool>(name);
            component.onValueChanged.AddListener(delegate (bool val) {
                ConvenienceFrontend.Config[name] = val;
            });
        }

        private void AddComponent(CToggleGroup component, string name)
        {
            base.AddMono(component, name);
            _componentDic[name] = component;

            RefreshComponent(component, name);
        }

        private void RefreshComponent(CToggleGroup component, string name)
        {
            component.InitPreOnToggle();
            component.OnActiveToggleChange = delegate (CToggle togNew, CToggle togOld)
            {
                JArray bools = ConvenienceFrontend.Config.GetTypedValue<JArray>(name) ?? new JArray();
                if (bools.Count == 0)
                {
                    for (var i = 0; i < component.Count(); i++)
                    {
                        bools.Add(false);
                    }
                }
                if (togOld != null)
                {
                    bools[togOld.Key] = togOld.isOn;
                }

                if (togNew != null)
                {
                    bools[togNew.Key] = togNew.isOn;
                }
                ConvenienceFrontend.Config[name] = bools;
            };
            JArray boolArray = ConvenienceFrontend.Config.GetTypedValue<JArray>(name) ?? new JArray();
            for (var i = 0; i < boolArray.Count; i++)
            {
                if (i < boolArray.Count)
                {
                    component.SetWithoutNotify(i, (bool)boolArray[i]);
                }
                else
                {
                    component.SetWithoutNotify(i, false);
                }
            }
        }
    }
}
