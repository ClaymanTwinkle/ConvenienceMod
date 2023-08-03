using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Config;
using ConvenienceFrontend.CombatStrategy.config;
using ConvenienceFrontend.CombatStrategy.config.data;
using ConvenienceFrontend.CombatStrategy.ui;
using ConvenienceFrontend.Utils;
using DG.Tweening;
using FrameWork;
using FrameWork.ModSystem;
using GameData.Domains.CombatSkill;
using GameData.GameDataBridge;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaiwuModdingLib.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ConvenienceFrontend.CombatStrategy
{
    // Token: 0x02000010 RID: 16
    internal class UI_CombatStrategySetting : UIBase
    {
        // Token: 0x06000055 RID: 85 RVA: 0x00005C78 File Offset: 0x00003E78
        public static UIElement GetUI()
        {
            UIElement result;
            if (element != null && element.UiBase != null)
            {
                result = element;
            }
            else
            {
                element = new UIElement
                {
                    Id = -1
                };
                Traverse.Create(element).Field("_path").SetValue("UI_CombatStrategySetting");
                GameObject gameObject = UIUtils.CreateMainUI("UI_CombatStrategySetting", "自动战斗设置");
                var uiComponent = gameObject.AddComponent<UI_CombatStrategySetting>();
                uiComponent.UiType = UILayer.LayerPopUp; //3;
                uiComponent.Element = element;
                uiComponent.RelativeAtlases = new SpriteAtlas[0];
                uiComponent.Init(gameObject);
                element.UiBase = uiComponent;
                element.UiBase.name = element.Name;
                UIManager.Instance.PlaceUI(element.UiBase);
                result = element;
            }
            return result;
        }

        private void Init(GameObject obj)
        {
            this.AnimIn = obj.transform.Find("FadeIn").GetComponent<DOTweenAnimation>();
            this.AnimOut = obj.transform.Find("FadeOut").GetComponent<DOTweenAnimation>();
            this.AnimIn.hasOnPlay = true;
            this.AnimIn.onPlay = new UnityEvent();
            this.AnimOut.hasOnPlay = true;
            this.AnimOut.onPlay = new UnityEvent();
            this._focus = GameObjectCreationUtils.InstantiateUIElement(obj.transform, "UIMask");
            this._focus.name = "Focus";
            this._focus.SetActive(false);
            Extentions.SetAnchor(this._focus.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            this._focus.AddComponent<CButton>().ClearAndAddListener(new Action(this.StopHotKeySetting));
            PopupWindow popupWindow = obj.GetComponentInChildren<PopupWindow>();
            popupWindow.CloseButton.onClick.RemoveAllListeners();
            popupWindow.CloseButton.onClick.AddListener(delegate ()
            {
                SaveConfigAndSend();
                this.QuickHide();
            });
            popupWindow.TitleLabel.transform.parent.position = new Vector3(popupWindow.TitleLabel.transform.parent.position.x, popupWindow.TitleLabel.transform.parent.position.y + 50, popupWindow.TitleLabel.transform.parent.position.z);
            GameObject gameObject = GameObjectCreationUtils.InstantiateUIElement(popupWindow.transform, "VerticalScrollView");
            gameObject.SetActive(true);
            RectTransform component = popupWindow.transform.Find("ElementsRoot/").gameObject.GetComponent<RectTransform>();
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(component.rect.size.x, component.rect.size.y - 120);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 60, gameObject.transform.position.z);
            this._scroll = gameObject.GetComponent<CScrollRect>();
            this._scroll.OnScrollEvent += OnScrollEvent;
            GameObject gameObject2 = this._scroll.Content.gameObject;
            RectTransform content = this._scroll.Content;
            Extentions.SetWidth(content, component.rect.size.x * 0.96f);
            UIUtils.CreateVerticalAutoSizeLayoutGroup(gameObject2).spacing = 15f;

            this.BuildAICombat(content);
            this.BuildStrategyProgramme(content);
            this.BuildSkillStrategy(content);
            this.BuildMoveSettings(content);
            this.BuildAttackSettings(content);
            this.BuildTeammateCommandSettings(content);
            this.BuildHotKeySettings(content);
        }

        private void BuildAICombat(Transform parent)
        {
            if (!ConvenienceFrontend.IsLocalTest()) return;
            Transform transform = UIUtils.CreateSettingPanel(parent, "AICombat", "AI设置").transform;
            base.AddMono(UIUtils.CreateToggle(transform, "UseAICombat", "AI代打", "注意：优先会使用AI代打，战斗策略将不会生效"), "UseAICombat");
        }

        private void BuildMoveSettings(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "MoveSettings", "移动设置").transform;
            UIUtils.CreateSubTitle(transform, "一般设置");
            base.AddMono(UIUtils.CreateToggle(UIUtils.CreateRow(transform), "AutoMove", "自动移动"), "AutoMove");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "TargetDistance", 20, 120, 10, "f1", "目标距离", null), "TargetDistance");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "TargetDistance2", 20, 120, 10, "f1", "备用目标距离", null), "TargetDistance2");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "MobilityAllowForward", 50, 100, 100, "p0", "当脚力值大于", "时，允许前进"), "MobilityAllowForward");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "MobilityAllowBackward", 50, 100, 100, "p0", "当脚力值大于", "时，允许后退"), "MobilityAllowBackward");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "MobilityRecoverCap", 50, 100, 100, "p0", "脚力值恢复到", "后，重新开始移动"), "MobilityRecoverCap");
            string[] options = new string[]
            {
                "快",
                "中",
                "慢"
            };
            this._distanceChangeSpeedTogGroup = UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "DistanceChangeSpeed", "距离调整速度", options, 1, false, false);
            UIUtils.CreateSubTitle(transform, "跳跃功法设置");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "DistanceAllowJumpForward", 0, 101, 10, "f1", "与目标距离相距超过", "时，向前跳跃"), "DistanceAllowJumpForward");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "DistanceAllowJumpBackward", 0, 101, 10, "f1", "与目标距离相距超过", "时，向后跳跃"), "DistanceAllowJumpBackward");
            Transform parent2 = UIUtils.CreateRow(transform);
            base.AddMono(UIUtils.CreateToggle(parent2, "JumpPassTargetDistance", "允许", "禁止", null, "跳过目标距离"), "JumpPassTargetDistance");
            base.AddMono(UIUtils.CreateToggle(parent2, "JumpOutOfAttackRange", "允许", "禁止", null, "跳出攻击范围"), "JumpOutOfAttackRange");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "MinJumpPosition", 20, 120, 10, "f1", "如果向前跳跃落点会越过", "距离，则停止跳跃蓄力"), "MinJumpPosition");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "MaxJumpPosition", 20, 120, 10, "f1", "如果向后跳跃落点会越过", "距离，则停止跳跃蓄力"), "MaxJumpPosition");
            CToggle ctoggle = UIUtils.CreateToggle(UIUtils.CreateRow(transform), "AllowOppositeMoveInJumpingSkill", "允许", "禁止", null, "向跳跃方向的相反方向移动");
            base.AddMono(ctoggle, "AllowOppositeMoveInJumpingSkill");
        }

        // Token: 0x06000058 RID: 88 RVA: 0x00006248 File Offset: 0x00004448
        private void BuildAttackSettings(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "AttackSettings", "攻击设置").transform;
            Transform parent2 = UIUtils.CreateRow(transform);
            base.AddMono(UIUtils.CreateToggle(parent2, "AutoAttack", "自动攻击"), "AutoAttack");
            base.AddMono(UIUtils.CreateToggle(parent2, "IgnoreRange", "无视", "只限", null, "攻击范围"), "IgnoreRange");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "AttackBufferMin", 0, 30, 10, "f1", "在攻击范围下限的", "距离内，不自动攻击"), "AttackBufferMin");
            base.AddMono(UIUtils.CreateSliderBar(UIUtils.CreateRow(transform), "AttackBufferMax", 0, 30, 10, "f1", "在攻击范围上限的", "距离内，不自动攻击"), "AttackBufferMax");
            base.AddMono(UIUtils.CreateToggle(UIUtils.CreateRow(transform), "ShowAutoAttackTips", "自动攻击提示"), "ShowAutoAttackTips");
        }

        // Token: 0x06000059 RID: 89 RVA: 0x00006338 File Offset: 0x00004538
        private void BuildTeammateCommandSettings(Transform parent)
        {
            var gameobject = UIUtils.CreateSettingPanel(parent, "TeammateCommandSettings", "其他设置");
            Transform transform = gameobject.transform;
            string[] array = new string[Config.TrickType.Instance.Count];
            for (int i = 0; i < Config.TrickType.Instance.Count; i++)
            {
                array[i] = TrickType.Instance[i].Name;
            }
            this._needRemoveTrickTogGroup = UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "RemoveTrick", "空挥招式", array, array.Length, true, true, "空挥武器，不会产生式，不会打到敌人");

        }

        /// <summary>
        /// 添加快捷键
        /// </summary>
        /// <param name="parent"></param>
        private void BuildHotKeySettings(Transform parent)
        {
            Transform transform = UIUtils.CreateSettingPanel(parent, "HotKeySettings", "热键设置").transform;
            base.AddMono(UIUtils.CreateHotKey(transform, "SwitchAutoMoveKey", "自动移动"), "SwitchAutoMoveKey");
            base.AddMono(UIUtils.CreateHotKey(transform, "SwitchTargetDistanceKey", "切换备用目标距离"), "SwitchTargetDistanceKey");
            base.AddMono(UIUtils.CreateHotKey(transform, "IncreaseDistanceKey", "减少目标距离"), "IncreaseDistanceKey");
            base.AddMono(UIUtils.CreateHotKey(transform, "DecreaseDistanceKey", "增加目标距离"), "DecreaseDistanceKey");
            base.AddMono(UIUtils.CreateHotKey(transform, "SwitchAutoAttackKey", "自动攻击"), "SwitchAutoAttackKey");
            base.AddMono(UIUtils.CreateHotKey(transform, "SwitchAutoCastSkillKey", "执行策略"), "SwitchAutoCastSkillKey");
        }

        /// <summary>
        /// 构建策略方案
        /// </summary>
        /// <param name="parent"></param>
        private void BuildStrategyProgramme(Transform parent)
        {
            var gameobject = UIUtils.CreateSettingPanel(parent, "StrategyProgramme", "策略方案");

            Transform transform = gameobject.transform;
            Transform parent2 = UIUtils.CreateRow(transform);

            GameObject dropdownGameObject = GameObjectCreationUtils.InstantiateUIElement(parent2, "CommonDropdown");
            Extentions.SetWidth(dropdownGameObject.GetComponent<RectTransform>(), 400f);
            CDropdown dropdown = dropdownGameObject.GetComponent<CDropdown>();
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();
            dropdown.AddOptions(ConfigManager.Programmes.ConvertAll(x => x.name));
            dropdown.value = ConfigManager.GlobalSettings.SelectStrategyIndex;
            dropdown.onValueChanged.AddListener(delegate (int val)
            {
                ConfigManager.ChangeStrategyProgramme(val);
                RefreshCurrentStrategyUI();
            });
            base.AddMono(dropdown, "StrategyProgrammeOptions");

            CButton manageStrategyProgrammeButton = GameObjectCreationUtils.UGUICreateCButton(parent2, 10f, "管理方案", width: 150, height: 40);
            manageStrategyProgrammeButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 18f;
            manageStrategyProgrammeButton.ClearAndAddListener(delegate ()
            {
                var btnList = new List<UI_PopupMenu.BtnData>
                {
                    new UI_PopupMenu.BtnData("新建方案", true, new Action(() =>
                    {
                        ShowInputTextPanel(parent2, "输入方案名称", "", delegate (string val)
                        {
                            if (val != null && val != string.Empty)
                            {
                                var programme = ConfigManager.CreateNewStrategyProgramme(val);

                                RefreshStrategyProgrammeOptions();
                            }
                        });
                    })),
                    new UI_PopupMenu.BtnData("方案改名", true, new Action(() =>
                    {
                        ShowInputTextPanel(parent2, "输入方案名称", dropdown.options[dropdown.value].text, delegate (string val)
                        {
                            if (val != null && val != string.Empty)
                            {
                                ConfigManager.CurrentStrategyProgramme.name = val;
                                dropdown.options[dropdown.value].text = val;
                                dropdown.RefreshShownValue();
                            }
                        });
                    })),
                    new UI_PopupMenu.BtnData("复制方案", true, new Action(() =>
                    {
                        var copyStrategy = ConfigManager.CopyStrategyProgramme();
                        copyStrategy.name += "（复制版）" + ConfigManager.Programmes.Count;
                        // 刷新UI
                        RefreshStrategyProgrammeOptions();
                    })),
                    new UI_PopupMenu.BtnData("导出方案", true, new Action(() =>
                    {
                        UIUtils.ShowTips("提示", "已将方案导出到剪切板，可以粘贴给其他人使用。");
                        GUIUtility.systemCopyBuffer = ConfigManager.GetCurrentStrategyProgrammeJson();
                    })),
                    new UI_PopupMenu.BtnData("导入方案", true, new Action(() =>
                    {
                        var Programme = ConfigManager.CreateNewStrategyProgrammeFromClipboard();
                        if (Programme != null)
                        {
                            RefreshStrategyProgrammeOptions();
                            UIUtils.ShowTips("提示", "已从剪切板导入方案【"+ Programme.name +"】");
                        }
                        else
                        {
                            UIUtils.ShowTips("提示", "剪切板无方案内容，请复制方案内容后再点击导入！");
                        }
                    })),

                    new UI_PopupMenu.BtnData("<color=yellow>自动生成</color>", true, new Action(() =>
                    {
                        if (!ConvenienceFrontend.IsInGame())
                        {
                            UIUtils.ShowTips("提示", "请进入游戏中使用");
                            return;
                        }
                        var programme = ConfigManager.CreateNewStrategyProgramme("自动生成策略" + ConfigManager.Programmes.Count);

                        RefreshStrategyProgrammeOptions();

                        GameDataBridgeUtils.SendData(8, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_AutoGenerateStrategy, ConfigManager.GetEnableStrategiesJson(), new Action<string>(json=>{
                            if (json != null)
                            {
                                programme.strategies = JsonConvert.DeserializeObject<List<Strategy>>(json);
                                programme.strategies.ForEach(x => x.enabled = true);
                                RefreshCurrentStrategyUI();
                            }
                        }));
                    })),
                    new UI_PopupMenu.BtnData("<color=red>删除方案</color>", true, new Action(() =>
                    {
                        if (dropdown.options.Count > 1)
                        {
                            void action ()
                            {
                                ConfigManager.Programmes.RemoveAt(dropdown.value);

                                dropdown.ClearOptions();
                                dropdown.AddOptions(ConfigManager.Programmes.ConvertAll(x => x.name));
                                dropdown.value = 0;
                                dropdown.onValueChanged.Invoke(dropdown.value);
                            }

                            DialogCmd dialogCmd = new DialogCmd
                            {
                                Title = "删除方案",
                                Content = "是否删除方案【" + dropdown.options[dropdown.value].text + "】？",
                                Type = 1,
                                Yes = action
                            };
                            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", dialogCmd));
                            UIManager.Instance.ShowUI(UIElement.Dialog);
                        }
                        else
                        {
                            UIUtils.ShowTips("警告", "请至少保留一个方案!");
                        }
                    }))
                };

                ShowMenu(btnList, manageStrategyProgrammeButton.transform.position);
            });

            CButton autoFillStrategyButton = GameObjectCreationUtils.UGUICreateCButton(parent2, 10f, "<color=yellow>快速填充功法</color>", width: 150, height: 40);
            autoFillStrategyButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 18f;
            autoFillStrategyButton.ClearAndAddListener(delegate () {
                if (!ConvenienceFrontend.IsInGame())
                {
                    UIUtils.ShowTips("提示", "请进入游戏中使用");
                    return;
                }
                GameDataBridgeUtils.SendData(8, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_AutoGenerateStrategy, ConfigManager.GetAllStrategiesJson(), new Action<string>(json => {
                    if (json != null)
                    {
                        var programme = ConfigManager.CurrentStrategyProgramme;
                        programme.strategies = JsonConvert.DeserializeObject<List<Strategy>>(json);
                        programme.strategies.ForEach(x => x.enabled = true);
                        RefreshCurrentStrategyUI();
                    }
                }));
            });
            UIUtils.ShowMouseTipDisplayer(autoFillStrategyButton.gameObject, "根据当前玩家的功法Build，快速填充功法到当前策略里");
            autoFillStrategyButton.gameObject.SetActive(false);
            base.AddMono(autoFillStrategyButton, "AutoFillStrategyButton");

            this._inputTextPanel = UIUtils.CreateInputTextPanel(this._focus.transform).GetComponent<RectTransform>();
        }

        // Token: 0x0600005B RID: 91 RVA: 0x00006510 File Offset: 0x00004710
        private void BuildSkillStrategy(Transform parent)
        {
            this._strategySettings = UIUtils.CreateSettingPanel(parent, "StrategySettings", "执行策略").GetComponent<RectTransform>();
            GameObject gameObject = UIUtils.CreateSingleToggle(this._strategySettings.parent, "AutoCastSkill", "开", "关", Vector2.zero);
            RectTransform component = gameObject.GetComponent<RectTransform>();
            Extentions.SetAnchor(component, Vector2.up, Vector2.up);
            component.anchoredPosition = new Vector2(170f, -21f);
            base.AddMono(gameObject.GetComponent<CToggle>(), "AutoCastSkill");

            GameObject buttonMoreGameObject = GameObjectCreationUtils.InstantiateUIElement(this._strategySettings.parent, "ButtonMore");
            RectTransform component2 = buttonMoreGameObject.GetComponent<RectTransform>();
            Extentions.SetAnchor(component2, Vector2.up, Vector2.up);
            Extentions.SetPivot(component2, Vector2.up);
            component2.anchoredPosition = new Vector2(220f, 0f);
            component2.sizeDelta = new Vector2(42f, 42f);
            component2.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(42f, 42f);
            component2.GetChild(1).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(42f, 42f);
            component2.GetChild(2).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(24f, 24f);
            buttonMoreGameObject.GetComponent<CButton>().ClearAndAddListener(delegate ()
            {
                Strategy strategy = new Strategy();
                CombatStrategyMod.Strategies.Add(strategy);
                Transform transform = CreateOrTakeStrategyPanel(CombatStrategyMod.Strategies.Count-1);
                this.RenderStrategy(transform.transform, strategy);
                LayoutRebuilder.MarkLayoutForRebuild(this._strategySettings.parent.GetComponent<RectTransform>());
            });
            UIUtils.ShowMouseTipDisplayer(buttonMoreGameObject.gameObject, "添加策略项");
            this._conditionSetter = ConditionSetterPanel.Create(this._focus.transform);
            this._changeTacticsPanel = UIUtils.CreateChangeTactics(this._focus.transform).GetComponent<RectTransform>();
            this._switchWeaponPanel = UIUtils.CreateOneValueOptionsPanel(this._focus.transform).GetComponent<RectTransform>();
            this._teammateCommandPanel = UIUtils.CreateOneValueOptionsPanel(this._focus.transform).GetComponent<RectTransform>();
            this._moveActionSelectPanel = MoveActionSelectPanel.Create(this._focus.transform);
        }

        private void Awake()
        {
            Debug.Log("UI_CombatStrategySetting::Awake");
            this._distanceChangeSpeedTogGroup.InitPreOnToggle();
            this._distanceChangeSpeedTogGroup.OnActiveToggleChange = delegate (CToggle togNew, CToggle _)
            {
                CombatStrategyMod.ProgrammeSettingsSettings.DistanceChangeSpeed = togNew.Key;
            };
            this._needRemoveTrickTogGroup.InitPreOnToggle();
            this._needRemoveTrickTogGroup.OnActiveToggleChange = delegate (CToggle togNew, CToggle togOld)
            {
                if (togNew == null)
                {
                    CombatStrategyMod.ProgrammeSettingsSettings.RemoveTrick[togOld.Key] = togOld.isOn;
                }
                else
                {
                    CombatStrategyMod.ProgrammeSettingsSettings.RemoveTrick[togNew.Key] = togNew.isOn;
                }
            };

            this._selectSkillArgBox = EasyPool.Get<ArgumentBox>();
            _allActiveSkillItemList.Clear();
            foreach (CombatSkillItem combatSkillItem in CombatSkill.Instance)
            {
                if (combatSkillItem.EquipType != CombatSkillEquipType.Assist)
                {
                    _allActiveSkillItemList.Add(combatSkillItem);
                }
            }
            this._selectSkillArgBox.Set("ShowCombatSkill", true);
            this._selectSkillArgBox.Set("ShowLifeSkill", false);
            this._selectSkillArgBox.Set("CheckEquipRequirePracticeLevel", false);
            this._selectSkillArgBox.SetObject("UnselectableCombatSkillList", new List<short>());
            this._selectSkillArgBox.Set("ShowNone", false);
        }

        // Token: 0x0600005D RID: 93 RVA: 0x00006874 File Offset: 0x00004A74
        public override void OnInit(ArgumentBox argsBox)
        {
            Debug.Log("UI_CombatStrategySetting::OnInit");
            if (argsBox != null)
            {
                argsBox.Get("InCombat", out this._inCombat);
            }
        }

        // Token: 0x0600005E RID: 94 RVA: 0x0000688E File Offset: 0x00004A8E
        private void OnEnable()
        {
            Debug.Log("UI_CombatStrategySetting::OnEnable");
            this.InitAllSettings();
            this.InitHotKeySettings();
            this.InitStrategy();
            this.RefreshAutoFillStrategyButton();
            Invoke("ScrollToTop", 0.2f);
        }

        // Token: 0x0600005F RID: 95 RVA: 0x000068A8 File Offset: 0x00004AA8
        private void OnGUI()
        {
            if (this._handlingKey != null)
            {
                this.ListenHotKey();
            }
        }

        /// <summary>
        /// 保存并发送
        /// </summary>
        private void SaveConfigAndSend()
        {
            Debug.Log("SaveConfigAndSend");

            ValueTuple<bool, bool> valueTuple = ConfigManager.SaveJsons();
            bool settingsChanged = valueTuple.Item1;
            bool strategiesChanged = valueTuple.Item2;
            if (this._inCombat)
            {
                if (settingsChanged)
                {
                    CombatStrategyMod.SendSettings();
                }
                if (strategiesChanged)
                {
                    GameDataBridge.AddMethodCall<ushort, string>(-1, 8, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_UpdateStrategiesJson, ConfigManager.GetEnableStrategiesJson());
                }
            }
        }

        /// <summary>
        /// 初始化所有Settings
        /// </summary>
        private void InitAllSettings()
        {
            this._distanceChangeSpeedTogGroup.SetWithoutNotify(CombatStrategyMod.ProgrammeSettingsSettings.DistanceChangeSpeed, true);
            for (int i = 0; i < CombatStrategyMod.ProgrammeSettingsSettings.RemoveTrick.Length; i++)
            {
                this._needRemoveTrickTogGroup.SetWithoutNotify(i, CombatStrategyMod.ProgrammeSettingsSettings.RemoveTrick[i]);
            }
            foreach (string name in SettingsConst.ToggleParams)
            {
                this.InitToggleSetting(name);
            }
            foreach (string name2 in SettingsConst.DistanceParams)
            {
                this.InitSliderSetting(name2, 1);
            }
            foreach (string name3 in SettingsConst.MobilityParams)
            {
                this.InitSliderSetting(name3, 10);
            }
        }

        // Token: 0x06000063 RID: 99 RVA: 0x00006A18 File Offset: 0x00004C18
        private void InitToggleSetting(string name)
        {
            if (!base.Names.Contains(name)) return;
            CToggle ctoggle = base.CGet<CToggle>(name);
            if (ctoggle == null) return;
            ctoggle.onValueChanged.RemoveAllListeners();
            ctoggle.isOn = CombatStrategyMod.ProgrammeSettingsSettings.GetBool(name);
            ctoggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                CombatStrategyMod.ProgrammeSettingsSettings.SetValue(name, isOn);
            });
        }

        // Token: 0x06000064 RID: 100 RVA: 0x00006A7C File Offset: 0x00004C7C
        private void InitSliderSetting(string name, int multiplyer = 1)
        {
            TSlider tslider = base.CGet<TSlider>(name);
            tslider.onValueChanged.RemoveAllListeners();
            tslider.value = (float)(CombatStrategyMod.ProgrammeSettingsSettings.GetInt(name) / multiplyer);
            tslider.onValueChanged.AddListener(delegate (float val)
            {
                CombatStrategyMod.ProgrammeSettingsSettings.SetValue(name, (int)val * multiplyer);
            });
        }

        /// <summary>
        /// 初始化所有快捷键
        /// </summary>
        private void InitHotKeySettings()
        {
            this._handlingKey = null;
            this.RenderHotKeyPrefab("SwitchAutoMoveKey", true);
            this.RenderHotKeyPrefab("SwitchTargetDistanceKey", true);
            this.RenderHotKeyPrefab("IncreaseDistanceKey", true);
            this.RenderHotKeyPrefab("DecreaseDistanceKey", true);
            this.RenderHotKeyPrefab("SwitchAutoAttackKey", true);
            this.RenderHotKeyPrefab("SwitchAutoCastSkillKey", true);
        }

        /// <summary>
        /// 初始化所有策略
        /// </summary>
        private void InitStrategy()
        {
            for (int i = 0; i < CombatStrategyMod.Strategies.Count; i++)
            {
                Transform transform = CreateOrTakeStrategyPanel(i);
                this.RenderStrategy(transform.transform, CombatStrategyMod.Strategies[i]);
            }
        }

        private Transform CreateOrTakeStrategyPanel(int num)
        {
            var transform = num < this._strategySettings.childCount ? this._strategySettings.GetChild(num) : UIUtils.CreateStrategyPanel(this._strategySettings);
            transform.gameObject.SetActive(true);

            return transform;
        }

        /// <summary>
        /// 清理所有策略
        /// </summary>
        private void ClearAllStrategy()
        {
            for (var i = this._strategySettings.childCount - 1; i >= 0; i--)
            {
                var child = this._strategySettings.GetChild(i);
                child.SetParent(null);

                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 渲染策略
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="strategy"></param>
        private void RenderStrategy(Transform transform, Strategy strategy)
        {
            var oRefers = transform.GetComponent<Refers>();
            oRefers.CGet<TextMeshProUGUI>("Priority").text = (transform.GetSiblingIndex() + 1).ToString();
            oRefers.CGet<CButton>("PriorityBtn").ClearAndAddListener(delegate ()
            {
                Action<int> action = (delegate (int val)
                {
                    CombatStrategyMod.Strategies.Remove(strategy);
                    CombatStrategyMod.Strategies.Insert(val - 1, strategy);
                    transform.SetSiblingIndex(val - 1);
                    for (int k = 0; k < CombatStrategyMod.Strategies.Count; k++)
                    {
                        _strategySettings.GetChild(k).GetComponent<Refers>().CGet<TextMeshProUGUI>("Priority").text = (k + 1).ToString();
                    }
                });
                ArgumentBox argumentBox = EasyPool.Get<ArgumentBox>();
                argumentBox.Set("MinCount", 1).Set("MaxCount", CombatStrategyMod.Strategies.Count).Set("InitCount", transform.GetSiblingIndex() + 1);
                argumentBox.SetObject("FollowTarget", oRefers.CGet<CButton>("PriorityBtn").transform);
                argumentBox.SetObject("ItemTransform", oRefers.CGet<CButton>("PriorityBtn").transform);
                argumentBox.SetObject("OnConfirmSetCount", action);
                argumentBox.SetObject("ScreenPos", UIManager.Instance.UiCamera.WorldToScreenPoint(oRefers.CGet<CButton>("PriorityBtn").transform.position));
                argumentBox.SetObject("ItemSize", new Vector2(60f, 30f));
                UIElement.SetSelectCount.SetOnInitArgs(argumentBox);
                UIManager.Instance.ShowUI(UIElement.SetSelectCount);
                Refers refers = UIElement.SetSelectCount.UiBaseAs<UI_SetSelectCount>().CGet<Refers>("SliceDownSheet");
                GameObject confirm = refers.CGet<GameObject>("Conflict_Confirm");
                refers.CGet<RectTransform>("CancelBtnPos").anchoredPosition = new Vector2(33f, 40f);
                GameObject cancel = refers.CGet<PositionFollower>("Conflict_Cancel").gameObject;
                GameObject bigCancel = refers.transform.GetChild(11).gameObject;
                confirm.SetActive(true);
                cancel.SetActive(true);
                bigCancel.SetActive(false);
                UIElement setSelectCount = UIElement.SetSelectCount;
                setSelectCount.OnHide = (Action)Delegate.Combine(setSelectCount.OnHide, new Action(delegate ()
                {
                    oRefers.CGet<CButton>("PriorityBtn").transform.SetParent(transform);
                    confirm.SetActive(false);
                    cancel.SetActive(false);
                    bigCancel.SetActive(true);
                }));
            });
            CToggle ctoggle = oRefers.CGet<CToggle>("Toggle");
            ctoggle.onValueChanged.RemoveAllListeners();
            ctoggle.isOn = strategy.enabled;
            ctoggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                strategy.enabled = isOn;
            });
            var content = oRefers.CGet<RectTransform>("Content");
            int num = -1;
            for (int i = 0; i < strategy.conditions.Count; i++)
            {
                Transform transform2 = (++num < content.childCount) ? content.GetChild(num) : UIUtils.CreateDropDown(content).transform;

                this.RenderCondition(transform2, strategy, strategy.conditions[i]);
            }
            var castSkill = ((++num < content.childCount) ? content.GetChild(num) : UIUtils.CreateDropDown(content).transform);
            var skillRefers = castSkill.GetComponent<Refers>();
            skillRefers.CGet<TextMeshProUGUI>("Label").text = "执行";

            this.RenderStrategySkillText(strategy, skillRefers);
            var btnList = new List<UI_PopupMenu.BtnData>
            {
                new UI_PopupMenu.BtnData("选择功法", true, new Action(() =>
                {
                    if (!ConvenienceFrontend.IsInGame())
                    {
                        UIUtils.ShowTips("提示", "请进入游戏中使用");
                        return;
                    }

                    var _onSelected = new Action<sbyte, short>((sbyte type, short skillId) =>
                    {
                        if (type == 1)
                        {
                            Debug.Log("选中功法" + skillId);
                            strategy.type = (short)StrategyConst.StrategyType.ReleaseSkill;
                            strategy.SetAction(skillId);
                            this.RenderStrategySkillText(strategy, skillRefers);
                        }
                        else
                        {
                            // cancel
                        }
                    });
                    ShowSkillSelectUI(strategy.skillId, _allActiveSkillItemList.ConvertAll(x => x.TemplateId), _onSelected);
                })),
                new UI_PopupMenu.BtnData("变招", true, new Action(() =>
                {
                    this.ShowChangeTacticsPanel(skillRefers, strategy);
                })),
                new UI_PopupMenu.BtnData("切换武器", true, new Action(() =>
                {
                    this.ShowSwitchWeaponPanel(skillRefers, strategy);
                })),
                new UI_PopupMenu.BtnData("队友协助", true, new Action(() =>
                {
                    this.ShowTeammateCommandPanel(skillRefers, strategy);
                })),
                new UI_PopupMenu.BtnData("自动移动", true, new Action(() =>
                {
                    this._moveActionSelectPanel.Show(skillRefers, strategy, new Action(() =>
                    {
                        this.RenderStrategySkillText(strategy, skillRefers);
                    }));
                })),
                new UI_PopupMenu.BtnData("普通攻击", true, new Action(() =>
                {
                    strategy.type = (short)StrategyConst.StrategyType.NormalAttack;
                    strategy.SetAction(new NormalAttackAction());
                    this.RenderStrategySkillText(strategy, skillRefers);
                })),
                new UI_PopupMenu.BtnData("<color=yellow>添加条件</color>", true, delegate ()
                {
                    Condition condition = new Condition();
                    strategy.conditions.Add(condition);
                    Transform transform3 = UIUtils.CreateDropDown(content).transform;
                    castSkill.SetAsLastSibling();
                    this.RenderCondition(transform3, strategy, condition);
                    content.GetComponent<GridLayoutGroup>().CalculateLayoutInputVertical();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                    LayoutRebuilder.MarkLayoutForRebuild(transform.GetComponent<RectTransform>());
                }),
                new UI_PopupMenu.BtnData("<color=red>删除策略</color>", true, delegate ()
                {
                    CombatStrategyMod.Strategies.Remove(strategy);
                    for (int k = transform.GetSiblingIndex() + 1; k < _strategySettings.childCount; k++)
                    {
                        _strategySettings.GetChild(k).gameObject.GetComponent<Refers>().CGet<TextMeshProUGUI>("Priority").text = k.ToString();
                    }
                    Object.Destroy(transform.gameObject);
                    LayoutRebuilder.MarkLayoutForRebuild(_strategySettings);
                })
            };
            skillRefers.CGet<CButton>("Button").ClearAndAddListener(delegate ()
            {
                this.ShowMenu(btnList, castSkill.position);
            });
            for (int j = content.childCount - 1; j > num; j--)
            {
                UnityEngine.Object.Destroy(content.GetChild(j).gameObject);
            }
        }

        /// <summary>
        /// 渲染条件UI
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="strategy"></param>
        /// <param name="condition"></param>
        private void RenderCondition(Transform transform, Strategy strategy, Condition condition)
        {
            Refers component = transform.GetComponent<Refers>();
            component.CGet<TextMeshProUGUI>("Label").text = ((transform.GetSiblingIndex() == 0) ? "当" : "并且");
            this.RenderConditionText(component, condition);
            List<UI_PopupMenu.BtnData> btnList = new List<UI_PopupMenu.BtnData>
            {
                new UI_PopupMenu.BtnData("设置条件", true, delegate()
                {
                    this.ShowConditionSetter(transform, condition);
                }),
                new UI_PopupMenu.BtnData("删除条件", true, delegate()
                {
                    strategy.conditions.Remove(condition);
                    transform.SetAsLastSibling();
                    bool flag = strategy.conditions.Count > 0;
                    if (flag)
                    {
                        transform.parent.GetChild(0).GetComponent<Refers>().CGet<TextMeshProUGUI>("Label").text = "当";
                    }
                    Object.Destroy(transform.gameObject);
                    LayoutRebuilder.MarkLayoutForRebuild(this._strategySettings);
                })
            };
            component.CGet<CButton>("Button").ClearAndAddListener(delegate ()
            {
                this.ShowMenu(btnList, transform.position);
            });
        }

        /// <summary>
        /// 显示菜单
        /// </summary>
        /// <param name="btnList"></param>
        /// <param name="position"></param>
        private void ShowMenu(List<UI_PopupMenu.BtnData> btnList, Vector3 position)
        {
            UIUtils.ShowMenu(btnList, position, delegate() {
                this._scroll.SetScrollEnable(false);
            }, delegate() {
                this._scroll.SetScrollEnable(true);
            });
        }

        // Token: 0x0600006A RID: 106 RVA: 0x0000709C File Offset: 0x0000529C
        private void RenderStrategySkillText(Strategy strategy, Refers refers)
        {
            var label = refers.CGet<TextMeshProUGUI>("DropDownLabel");
            switch (strategy.type)
            {
                case (short)StrategyConst.StrategyType.ReleaseSkill:
                    if (strategy.skillId < 0)
                    {
                        label.text = Extentions.SetColor("未选择策略..", Color.gray);
                    }
                    else
                    {
                        CombatSkillItem combatSkillItem = CombatSkill.Instance[strategy.skillId];
                        label.text = Extentions.SetGradeColor(combatSkillItem.Name, (int)combatSkillItem.Grade);
                    }
                    break;
                case (short)StrategyConst.StrategyType.ChangeTrick:
                    label.text = "变招["+strategy.changeTrickAction.trick+"] 击打["+ strategy.changeTrickAction.body +"]";
                    break;
                case (short)StrategyConst.StrategyType.SwitchWeapons:
                    label.text = "切换武器["+ StrategyConst.WeaponIndexOptions[strategy.switchWeaponAction.weaponIndex] +"]";
                    break;
                case (short)StrategyConst.StrategyType.ExecTeammateCommand:
                    label.text = "队友指令[" + StrategyConst.GetTeammateCommandList()[strategy.teammateCommandAction.id] + "]";
                    break;
                case (short)StrategyConst.StrategyType.AutoMove:
                    label.text = StrategyConst.MoveActionOptions[strategy.autoMoveAction.type];
                    break;
                case (short)StrategyConst.StrategyType.NormalAttack:
                    label.text = "普通攻击";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 渲染条件UI
        /// </summary>
        /// <param name="refers"></param>
        /// <param name="condition"></param>
        private void RenderConditionText(Refers refers, Condition condition)
        {
            if (condition.IsComplete())
            {
                refers.CGet<TextMeshProUGUI>("DropDownLabel").text = Extentions.SetColor(condition.GetShowDesc(), new Color(0.9725f, 0.902f, 0.7569f));
            }
            else
            {
                refers.CGet<TextMeshProUGUI>("DropDownLabel").text = Extentions.SetColor("未设置条件..", Color.grey);
            }
        }

        /// <summary>
        /// 显示条件设置面板
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="condition"></param>
        private void ShowConditionSetter(Transform parent, Condition condition)
        {
            Action renderConditionText = delegate() {
                this.RenderConditionText(parent.GetComponentInParent<Refers>(), condition);
            };
            Action<int, Action<sbyte, short>> showSkillSelectUI = delegate (int value, Action< sbyte, short>  onSelect) {
                ShowSkillSelectUI(
                (short)condition.value,
                        ((JudgeItem)value == JudgeItem.AffectingSkill ? _allActiveSkillItemList.FindAll(x => x.EquipType != CombatSkillEquipType.Attack) : _allActiveSkillItemList).ConvertAll(x => x.TemplateId),
                        onSelect
                    );
            };

            _conditionSetter.ShowConditionSetter(parent, condition, renderConditionText, showSkillSelectUI);
        }

        /// <summary>
        /// 展示修改变招面板
        /// </summary>
        private void ShowChangeTacticsPanel(Refers parentRefers, Strategy strategy)
        {
            var parent = parentRefers.transform;
            Vector3 vector = UIManager.Instance.UiCamera.WorldToScreenPoint(parent.position);
            this._changeTacticsPanel.position = UIManager.Instance.UiCamera.ScreenToWorldPoint(vector);
            this._changeTacticsPanel.anchoredPosition += new Vector2(40f, -50f);
            this._changeTacticsPanel.gameObject.SetActive(true);
            this._focus.SetActive(true);
            Refers refers = this._changeTacticsPanel.gameObject.GetComponent<Refers>();
            var trickOptions = refers.CGet<CDropdown>("TrickOptions");
            var bodyOptions = refers.CGet<CDropdown>("BodyOptions");

            trickOptions.value = Math.Max(trickOptions.options.FindIndex(x => x.text.Equals(strategy.changeTrickAction?.trick)), 0);
            bodyOptions.value = Math.Max(bodyOptions.options.FindIndex(x => x.text.Equals(strategy.changeTrickAction?.body)), 0);

            refers.CGet<CButton>("Confirm").ClearAndAddListener(delegate ()
            {
                var trick = trickOptions.options[trickOptions.value].text;
                var body = bodyOptions.options[bodyOptions.value].text;

                strategy.type = (short)StrategyConst.StrategyType.ChangeTrick;
                strategy.SetAction(new ChangeTrickAction(trick, body));

                this.RenderStrategySkillText(strategy, parentRefers);

                this._changeTacticsPanel.gameObject.SetActive(false);
                this._focus.SetActive(false);
            });
            this._changeTacticsPanel.GetComponent<Refers>().CGet<CButton>("Cancel").ClearAndAddListener(delegate ()
            {
                this._changeTacticsPanel.gameObject.SetActive(false);
                this._focus.SetActive(false);
            });
        }

        /// <summary>
        /// 显示切换武器面板
        /// </summary>
        /// <param name="parentRefers"></param>
        /// <param name="strategy"></param>
        private void ShowSwitchWeaponPanel(Refers parentRefers, Strategy strategy)
        {
            var parent = parentRefers.transform;
            Vector3 vector = UIManager.Instance.UiCamera.WorldToScreenPoint(parent.position);
            this._switchWeaponPanel.position = UIManager.Instance.UiCamera.ScreenToWorldPoint(vector);
            this._switchWeaponPanel.anchoredPosition += new Vector2(40f, -50f);
            this._switchWeaponPanel.gameObject.SetActive(true);
            this._focus.SetActive(true);
            Refers refers = this._switchWeaponPanel.gameObject.GetComponent<Refers>();
            var valueOptions = refers.CGet<CDropdown>("ValueOptions");
            valueOptions.ClearOptions();
            valueOptions.AddOptions(StrategyConst.WeaponIndexOptions.ToList());

            valueOptions.value = strategy.switchWeaponAction?.weaponIndex ?? valueOptions.value;

            refers.CGet<CButton>("Confirm").ClearAndAddListener(delegate ()
            {
                var weaponIndex = valueOptions.value;

                strategy.type = (short)StrategyConst.StrategyType.SwitchWeapons;
                strategy.SetAction(new SwitchWeaponAction((sbyte)weaponIndex));

                this.RenderStrategySkillText(strategy, parentRefers);

                this._switchWeaponPanel.gameObject.SetActive(false);
                this._focus.SetActive(false);
            });
            this._switchWeaponPanel.GetComponent<Refers>().CGet<CButton>("Cancel").ClearAndAddListener(delegate ()
            {
                this._switchWeaponPanel.gameObject.SetActive(false);
                this._focus.SetActive(false);
            });
        }

        /// <summary>
        /// 显示队友指令面板
        /// </summary>
        /// <param name="parentRefers"></param>
        /// <param name="strategy"></param>
        private void ShowTeammateCommandPanel(Refers parentRefers, Strategy strategy)
        {
            var parent = parentRefers.transform;
            Vector3 vector = UIManager.Instance.UiCamera.WorldToScreenPoint(parent.position);
            this._teammateCommandPanel.position = UIManager.Instance.UiCamera.ScreenToWorldPoint(vector);
            this._teammateCommandPanel.anchoredPosition += new Vector2(40f, -50f);
            this._teammateCommandPanel.gameObject.SetActive(true);
            this._focus.SetActive(true);
            Refers refers = this._teammateCommandPanel.gameObject.GetComponent<Refers>();

            var valueOptions = refers.CGet<CDropdown>("ValueOptions");
            valueOptions.ClearOptions();
            valueOptions.AddOptions(StrategyConst.GetTeammateCommandList());

            valueOptions.value = strategy.teammateCommandAction?.id ?? valueOptions.value;

            refers.CGet<CButton>("Confirm").ClearAndAddListener(delegate ()
            {
                var value = valueOptions.value;

                strategy.type = (short)StrategyConst.StrategyType.ExecTeammateCommand;
                strategy.SetAction(new TeammateCommandAction((sbyte)value));

                this.RenderStrategySkillText(strategy, parentRefers);

                this._teammateCommandPanel.gameObject.SetActive(false);
                this._focus.SetActive(false);
            });
            this._teammateCommandPanel.GetComponent<Refers>().CGet<CButton>("Cancel").ClearAndAddListener(delegate ()
            {
                this._teammateCommandPanel.gameObject.SetActive(false);
                this._focus.SetActive(false);
            });
        }

        /// <summary>
        /// 显示输入面板
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tips"></param>
        /// <param name="input"></param>
        /// <param name="inputAction"></param>
        private void ShowInputTextPanel(Transform parent, string tips, string input, Action<String> inputAction)
        {
            Vector3 vector = UIManager.Instance.UiCamera.WorldToScreenPoint(parent.position);
            this._inputTextPanel.position = UIManager.Instance.UiCamera.ScreenToWorldPoint(vector);
            this._inputTextPanel.anchoredPosition += new Vector2(40f, -50f);
            this._inputTextPanel.gameObject.SetActive(true);
            this._focus.SetActive(true);
            Refers refers = this._inputTextPanel.gameObject.GetComponent<Refers>();
            var confirm = refers.CGet<CButton>("Confirm");
            refers.CGet<TextMeshProUGUI>("Tips").text = tips;
            var inputField = refers.CGet<TMP_InputField>("InputField");
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(delegate (string val) {
                confirm.enabled = val.Trim() != string.Empty;
            });
            inputField.text = input;

            refers.CGet<CButton>("Confirm").ClearAndAddListener(delegate ()
            {
                this._inputTextPanel.gameObject.SetActive(false);
                this._focus.SetActive(false);
                inputAction.Invoke(inputField.text);
            });

            refers.CGet<CButton>("Cancel").ClearAndAddListener(delegate ()
            {
                this._inputTextPanel.gameObject.SetActive(false);
                this._focus.SetActive(false);
            });
        }

        // Token: 0x0600006D RID: 109 RVA: 0x00007558 File Offset: 0x00005758
        private void FocusTarget(Transform target)
        {
            bool flag = null == target;
            if (!flag)
            {
                this._scroll.SetScrollEnable(false);
                Transform parent = target.parent;
                int siblingIndex = target.GetSiblingIndex();
                this._focusItemParent = new ValueTuple<Transform, int>(parent, siblingIndex);
                this._focus.SetActive(true);
                target.SetParent(this._focus.transform, true);
                target.SetAsLastSibling();
            }
        }

        // Token: 0x0600006E RID: 110 RVA: 0x000075C4 File Offset: 0x000057C4
        private void TargetLostFocus(Transform target)
        {
            bool flag = target == null;
            if (!flag)
            {
                target.SetParent(this._focusItemParent.Item1, true);
                target.SetSiblingIndex(this._focusItemParent.Item2);
                this._focus.SetActive(false);
                this._scroll.SetScrollEnable(true);
            }
        }

        // Token: 0x0600006F RID: 111 RVA: 0x00007620 File Offset: 0x00005820
        private void StopHotKeySetting()
        {
            if (this._handlingKey != null)
            {
                this.RenderHotKeyPrefab(this._handlingKey, false);
            }
        }

        // Token: 0x06000070 RID: 112 RVA: 0x0000764C File Offset: 0x0000584C
        private void ListenHotKey()
        {
            Debug.Log("UI_CombatStrategySetting::ListenHotKey");
            KeyCode keyCode = 0;
            if (Input.anyKey)
            {
                if (Input.GetMouseButton(3))
                {
                    keyCode = KeyCode.Mouse3; //326;
                }
                else
                {
                    if (Input.GetMouseButton(4))
                    {
                        keyCode = KeyCode.Mouse4; //327;
                    }
                    else
                    {
                        if (Input.GetMouseButton(5))
                        {
                            keyCode = KeyCode.Mouse5; // 328;
                        }
                        else
                        {
                            if (Input.GetMouseButton(6))
                            {
                                keyCode = KeyCode.Mouse6; //329;
                            }
                        }
                    }
                }
            }
            if (Input.anyKeyDown && Event.current != null && (Event.current.isKey || Event.current.isMouse))
            {
                if (Event.current.isKey)
                {
                    keyCode = Event.current.keyCode;
                }
                else
                {
                    if (Event.current.isMouse)
                    {
                        if (Event.current.button == 0)
                        {
                            keyCode = KeyCode.Mouse0; //323;
                        }
                        else
                        {
                            if (Event.current.button == 1)
                            {
                                keyCode = KeyCode.Mouse1; //324;
                            }
                            else
                            {
                                if (Event.current.button == 2)
                                {
                                    keyCode = KeyCode.Mouse2; //325;
                                }
                            }
                        }
                    }
                }
            }
            if (keyCode != KeyCode.Mouse0 && keyCode != 0)
            {
                if (keyCode == KeyCode.Escape) // 27
                {
                    keyCode = 0;
                }
                CombatStrategyMod.GlobalSettings.SetKey(this._handlingKey, keyCode);
                this.StopHotKeySetting();
            }
        }

        // Token: 0x06000071 RID: 113 RVA: 0x000077A4 File Offset: 0x000059A4
        private void RenderHotKeyPrefab(string name, bool isInit = false)
        {
            GameObject gameObject = base.CGet<GameObject>(name);
            Refers refer = gameObject.GetComponent<Refers>();
            KeyCode key = CombatStrategyMod.GlobalSettings.GetKey(name);
            refer.CGet<TextMeshProUGUI>("Key").gameObject.SetActive(key > 0);
            refer.CGet<TextMeshProUGUI>("Key").text = key.ToString();
            CToggle ctoggle = refer.CGet<CToggle>("Toggle");
            ctoggle.isOn = false;
            if (isInit)
            {
                refer.CGet<TextMeshProUGUI>("FunctionKey").gameObject.SetActive(false);
                refer.CGet<TextMeshProUGUI>("FunctionKey").gameObject.GetComponent<UIRectSizeController>().ControlList[0].Target.gameObject.SetActive(false);
                refer.CGet<TextMeshProUGUI>("Key").gameObject.GetComponent<UIRectSizeController>().ControlList[0].Target.gameObject.SetActive(key > 0);
                refer.CGet<TextMeshProUGUI>("AddMark").gameObject.SetActive(false);
                ctoggle.onValueChanged.RemoveAllListeners();
                ctoggle.onValueChanged.AddListener(delegate (bool on)
                {
                    if (on)
                    {
                        this._handlingKey = name;
                        this.FocusTarget(refer.CGet<RectTransform>("FocusRoot"));
                    }
                    else
                    {
                        if (this._handlingKey == name)
                        {
                            this._handlingKey = null;
                            this.TargetLostFocus(refer.CGet<RectTransform>("FocusRoot"));
                        }
                    }
                });
            }
            refer.CGet<RectTransform>("FocusRoot").GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
            LayoutRebuilder.MarkLayoutForRebuild(refer.CGet<RectTransform>("FocusRoot"));
        }

        private void OnScrollEvent()
        {
            // Debug.Log("OnScrollEvent " + this._scroll.Content.sizeDelta);
        }

        private void RefreshAutoFillStrategyButton()
        {
            var autoFillStrategyButton = CGet<CButton>("AutoFillStrategyButton");
            autoFillStrategyButton.gameObject.SetActive(ConfigManager.CurrentStrategyProgramme.strategies.Count == 0);
        }

        /// <summary>
        /// 刷新方案选项UI
        /// </summary>
        private void RefreshStrategyProgrammeOptions()
        {
            var dropdown = CGet<CDropdown>("StrategyProgrammeOptions");

            dropdown.ClearOptions();
            dropdown.AddOptions(ConfigManager.Programmes.ConvertAll(x => x.name));
            dropdown.value = dropdown.options.Count - 1;
            dropdown.onValueChanged.Invoke(dropdown.value);
        }


        /// <summary>
        /// 刷新当前策略UI
        /// </summary>
        private void RefreshCurrentStrategyUI()
        {
            InitAllSettings();
            ClearAllStrategy();
            InitStrategy();
            RefreshAutoFillStrategyButton();
            Invoke("RefreshStrategyUI", 0.2f);
        }

        /// <summary>
        /// 延迟刷新策略UI，并滚动到最上
        /// </summary>
        private void RefreshStrategyUI()
        {
            ScrollToTop();
            LayoutRebuilder.MarkLayoutForRebuild(this._scroll.Content);
            LayoutRebuilder.MarkLayoutForRebuild(this._strategySettings.parent.GetComponent<RectTransform>());
        }

        /// <summary>
        /// 滚动到顶部
        /// </summary>
        private void ScrollToTop()
        {
            this._scroll.Content.anchoredPosition = new Vector2(0, -this._scroll.Content.sizeDelta.y / 2);
            this._scroll.ScrollTo(this._scroll.Content.anchoredPosition);
        }

        /// <summary>
        /// 显示技能选择面板
        /// </summary>
        /// <param name="selectedSkillId"></param>
        /// <param name="skillIdList"></param>
        /// <param name="onSelectedSkill"></param>
        private void ShowSkillSelectUI(short selectedSkillId, List<short> skillIdList, Action<sbyte, short> onSelectedSkill)
        {
            if (ConvenienceFrontend.IsInGame())
            {
                var _onSelected = new Action<sbyte, short>((sbyte type, short skillId) => {
                    onSelectedSkill.Invoke(type, skillId);
                });

                _selectSkillArgBox.Set("CharId", SingletonObject.getInstance<BasicGameData>().TaiwuCharId);
                _selectSkillArgBox.SetObject("Callback", _onSelected);
                _selectSkillArgBox.Set("PrevCombatSkillId", selectedSkillId);
                _selectSkillArgBox.SetObject("CombatSkillIdList", skillIdList);
                UIElement.SelectSkill.SetOnInitArgs(_selectSkillArgBox);
                UIManager.Instance.ShowUI(UIElement.SelectSkill);
            }
            else
            {
                UIUtils.ShowTips("警告", "载入存档后才能进行选择");
            }
        }

        // Token: 0x04000070 RID: 112
        private CScrollRect _scroll;

        // Token: 0x04000071 RID: 113
        private CToggleGroup _distanceChangeSpeedTogGroup;

        // Token: 0x04000072 RID: 114
        private CToggleGroup _needRemoveTrickTogGroup;

        // Token: 0x04000074 RID: 116
        private static UIElement element;

        // Token: 0x04000075 RID: 117
        private ValueTuple<Transform, int> _focusItemParent;

        // Token: 0x04000076 RID: 118
        private string _handlingKey;

        // Token: 0x04000077 RID: 119
        private bool _inCombat;

        // Token: 0x04000078 RID: 120
        private RectTransform _strategySettings;

        /// <summary>
        /// 条件设置面板
        /// </summary>
        private ConditionSetterPanel _conditionSetter;

        /// <summary>
        /// 变招设置面板
        /// </summary>
        private RectTransform _changeTacticsPanel;

        /// <summary>
        /// 切换武器面板
        /// </summary>
        private RectTransform _switchWeaponPanel;

        /// <summary>
        /// 队友指令面板
        /// </summary>
        private RectTransform _teammateCommandPanel;

        /// <summary>
        /// 移动动作选择面板
        /// </summary>
        private MoveActionSelectPanel _moveActionSelectPanel;

        private RectTransform _inputTextPanel;

        // Token: 0x0400007A RID: 122
        private GameObject _focus;

        // Token: 0x0400007B RID: 123
        private ArgumentBox _selectSkillArgBox;

        /// <summary>
        /// 所有主动技能id（身法、护体、催迫，包括boss）
        /// </summary>
        private List<CombatSkillItem> _allActiveSkillItemList = new List<CombatSkillItem>();
    }
}
