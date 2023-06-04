﻿using System;
using System.Collections.Generic;
using FrameWork.ModSystem;
using HarmonyLib;
using UnityEngine.U2D;
using UnityEngine;
using FrameWork;
using GameData.Domains.Mod;
using ConvenienceFrontend.CombatStrategy;
using MoonSharp.Interpreter;
using ConvenienceFrontend.CombatStrategy.config;
using GameData.GameDataBridge;
using Newtonsoft.Json;
using UnityEngine.UI;
using GameData.Utilities;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace ConvenienceFrontend.TaiwuBuildingManager
{
    internal class UI_TaiwuBuildingManager : UIBase
    {
        private static UIElement element;
        
        private CScrollRect _scroll;

        private Dictionary<string, Component> _componentDic = new Dictionary<string, Component>();

        public static UIElement GetUI()
        {
            UIElement result;
            if (UI_TaiwuBuildingManager.element != null && UI_TaiwuBuildingManager.element.UiBase != null)
            {
                result = UI_TaiwuBuildingManager.element;
            }
            else
            {
                UI_TaiwuBuildingManager.element = new UIElement
                {
                    Id = -1
                };
                Traverse.Create(UI_TaiwuBuildingManager.element).Field("_path").SetValue("UI_TaiwuBuildingManager");

                GameObject gameObject = GameObjectCreationUtils.CreatePopupWindow(null, "种田管家", null, null, true, 0, 0, 1300, 1000);
                UI_TaiwuBuildingManager uiBase = gameObject.AddComponent<UI_TaiwuBuildingManager>();
                uiBase.UiType = UILayer.LayerPopUp; //3;
                uiBase.Element = UI_TaiwuBuildingManager.element;
                uiBase.RelativeAtlases = new SpriteAtlas[0];
                uiBase.Init(gameObject);
                UI_TaiwuBuildingManager.element.UiBase = uiBase;
                UI_TaiwuBuildingManager.element.UiBase.name = UI_TaiwuBuildingManager.element.Name;
                UIManager.Instance.PlaceUI(UI_TaiwuBuildingManager.element.UiBase);
                result = UI_TaiwuBuildingManager.element;
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
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(component.rect.size.x, component.rect.size.y - 180);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 120, gameObject.transform.position.z);
            this._scroll = gameObject.GetComponent<CScrollRect>();
            GameObject gameObject2 = this._scroll.Content.gameObject;
            RectTransform content = this._scroll.Content;
            Extentions.SetWidth(content, component.rect.size.x * 0.9f);
            UIUtils.CreateVerticalAutoSizeLayoutGroup(gameObject2).spacing = 15f;

            this.BuildBuildingSettings(content);
            this.BuildCollectResourceSettings(content);
        }

        public override void OnInit(ArgumentBox argsBox)
        {
        }

        private void Awake()
        {
            Debug.Log("UI_TaiwuBuildingManager::Awake");
        }

        private void OnEnable()
        {
            Debug.Log("UI_TaiwuBuildingManager::OnEnable");
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
            }
        }

        public override void QuickHide()
        {
            ConvenienceFrontend.SaveConfig();
            GameDataBridge.AddMethodCall<ushort, string>(-1, 5, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_Load_Settings, JsonConvert.SerializeObject(ConvenienceFrontend.Config));
            base.QuickHide();
        }

        private void BuildBuildingSettings(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "BuildingSettings", "建筑设置").transform;
            UIUtils.CreateSubTitle(transform, "拆除无用资源");
            
            AddComponent(UIUtils.CreateToggle(UIUtils.CreateRow(transform), "Toggle_EnableRemoveUselessResource", "过月时，自动拆除无用资源"), "Toggle_EnableRemoveUselessResource");

            string[] options = new string[]
            {
                "杂草",
                "乱石",
                "废墟"
            };
            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "Toggle_IntUselessResourceType", "要拆除的无用资源类型", options, 3, true, true), "Toggle_IntUselessResourceType");

            UIUtils.CreateSubTitle(transform, "人员分配");
            AddComponent(UIUtils.CreateToggle(UIUtils.CreateRow(transform), "Toggle_EnableBuildingAutoWork", "建筑默认勾选自动工作"), "Toggle_EnableBuildingAutoWork");
        }

        private void BuildCollectResourceSettings(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "CollectResourceSettings", "采集设置").transform;
            AddComponent(UIUtils.CreateToggle(UIUtils.CreateRow(transform), "Toggle_EnableCollectResource", "过月时，自动采资源"), "Toggle_EnableCollectResource");

            string[] options = new string[]
            {
                "食材",
                "木材",
                "金铁",
                "玉石",
                "织物",
                "药材"
            };
            AddComponent(UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "Toggle_IntCollectResourceType", "要采集的资源类型(不选则默认采最少资源)", options, 1, true, true), "Toggle_IntCollectResourceType");
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
