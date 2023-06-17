using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config;
using ConvenienceFrontend.CombatStrategy.config;
using ConvenienceFrontend.CombatStrategy.config.data;
using ConvenienceFrontend.CombatStrategy.ui;
using DG.Tweening;
using FrameWork;
using FrameWork.ModSystem;
using GameData.Domains.CombatSkill;
using GameData.GameDataBridge;
using HarmonyLib;
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
            bool flag = UI_CombatStrategySetting.element != null && UI_CombatStrategySetting.element.UiBase != null;
            UIElement result;
            if (flag)
            {
                result = UI_CombatStrategySetting.element;
            }
            else
            {
                UI_CombatStrategySetting.element = new UIElement
                {
                    Id = -1
                };
                Traverse.Create(UI_CombatStrategySetting.element).Field("_path").SetValue("UI_CombatStrategySetting");
                GameObject gameObject = UIUtils.CreateMainUI("UI_CombatStrategySetting");
                UI_CombatStrategySetting ui_CombatStrategySetting = gameObject.AddComponent<UI_CombatStrategySetting>();
                ui_CombatStrategySetting.UiType = UILayer.LayerPopUp; //3;
                ui_CombatStrategySetting.Element = UI_CombatStrategySetting.element;
                ui_CombatStrategySetting.RelativeAtlases = new SpriteAtlas[0];
                ui_CombatStrategySetting.Init(gameObject);
                UI_CombatStrategySetting.element.UiBase = ui_CombatStrategySetting;
                UI_CombatStrategySetting.element.UiBase.name = UI_CombatStrategySetting.element.Name;
                UIManager.Instance.PlaceUI(UI_CombatStrategySetting.element.UiBase);
                result = UI_CombatStrategySetting.element;
            }
            return result;
        }

        // Token: 0x06000056 RID: 86 RVA: 0x00005D5C File Offset: 0x00003F5C
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
                this.OnClick(popupWindow.CloseButton);
            });
            popupWindow.TitleLabel.transform.parent.position = new Vector3(popupWindow.TitleLabel.transform.parent.position.x, popupWindow.TitleLabel.transform.parent.position.y + 50, popupWindow.TitleLabel.transform.parent.position.z);
            GameObject gameObject = GameObjectCreationUtils.InstantiateUIElement(popupWindow.transform, "VerticalScrollView");
            gameObject.SetActive(true);
            RectTransform component = popupWindow.transform.Find("ElementsRoot/").gameObject.GetComponent<RectTransform>();
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(component.rect.size.x, component.rect.size.y - 120);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 60, gameObject.transform.position.z);
            this._scroll = gameObject.GetComponent<CScrollRect>();
            GameObject gameObject2 = this._scroll.Content.gameObject;
            RectTransform content = this._scroll.Content;
            Extentions.SetWidth(content, component.rect.size.x * 0.96f);
            UIUtils.CreateVerticalAutoSizeLayoutGroup(gameObject2).spacing = 15f;
            this.BuildMoveSettings(content);
            this.BuildAttackSettings(content);
            this.BuildTeammateCommandSettings(content);
            this.BuildHotKeySettings(content);
            this.BuildStrategyProgramme(content);
            this.BuildSkillStrategy(content);
        }

        // Token: 0x06000057 RID: 87 RVA: 0x00005F78 File Offset: 0x00004178
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
            this._otherSettings = gameobject.GetComponent<RectTransform>();
            Transform transform = gameobject.transform;
            string[] array = new string[Config.TrickType.Instance.Count];
            for (int i = 0; i < Config.TrickType.Instance.Count; i++)
            {
                array[i] = TrickType.Instance[i].Name;
            }
            this._needRemoveTrickTogGroup = UIUtils.CreateToggleGroup(UIUtils.CreateRow(transform), "RemoveTrick", "空A招式", array, array.Length, true, true);
        }

        // Token: 0x0600005A RID: 90 RVA: 0x00006460 File Offset: 0x00004660
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

        private void BuildStrategyProgramme(Transform parent)
        {
            var gameobject = UIUtils.CreateSettingPanel(parent, "StrategyProgramme", "策略方案");
            this._strategyProgramme = gameobject.GetComponent<RectTransform>();

            Transform transform = gameobject.transform;
            Transform parent2 = UIUtils.CreateRow(transform);

            GameObject dropdownGameObject = GameObjectCreationUtils.InstantiateUIElement(parent2, "CommonDropdown");
            Extentions.SetWidth(dropdownGameObject.GetComponent<RectTransform>(), 400f);
            CDropdown dropdown = dropdownGameObject.GetComponent<CDropdown>();
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();
            dropdown.AddOptions(ConfigManager.Programmes.ConvertAll(x => x.name));
            dropdown.value = ConfigManager.Settings.SelectStrategyIndex;
            dropdown.onValueChanged.AddListener(delegate (int val)
            {
                ConfigManager.Settings.SelectStrategyIndex = val;

                var anchoredPosition = this._scroll.Content.anchoredPosition;
                ClearAllStrategy();
                InitStrategy();

                this._scroll.ScrollTo(anchoredPosition);
                
                Invoke("RefreshUI", 0.2f);
                // LayoutRebuilder.MarkLayoutForRebuild(this._scroll.Content);
            });
            base.AddMono(dropdown, "StrategyProgrammeOptions");

            CButton addStrategyProgrammeButton = GameObjectCreationUtils.UGUICreateCButton(parent2, 10f, "新建方案");
            Extentions.SetHeight(addStrategyProgrammeButton.gameObject.GetComponent<RectTransform>(), 40f);
            addStrategyProgrammeButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16f;
            addStrategyProgrammeButton.ClearAndAddListener(delegate () {

                ShowInputTextPanel(parent2, "输入方案名称", "", delegate(string val) {
                    if (val != null && val != string.Empty)
                    {
                        var programme = new StrategyProgramme
                        {
                            name = val
                        };
                        ConfigManager.Programmes.Add(programme);

                        dropdown.ClearOptions();
                        dropdown.AddOptions(ConfigManager.Programmes.ConvertAll(x => x.name));
                        dropdown.value = dropdown.options.Count - 1;
                        dropdown.onValueChanged.Invoke(dropdown.value);
                    }
                });
            });

            CButton renameStrategyProgrammeButton = GameObjectCreationUtils.UGUICreateCButton(parent2, 10f, "重命名");
            Extentions.SetHeight(renameStrategyProgrammeButton.gameObject.GetComponent<RectTransform>(), 40f);
            renameStrategyProgrammeButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16f;
            renameStrategyProgrammeButton.ClearAndAddListener(delegate () {
                ShowInputTextPanel(parent2, "重新输入方案名称", dropdown.options[dropdown.value].text, delegate (string val) {
                    if (val != null && val != string.Empty)
                    {
                        ConfigManager.Programmes[ConfigManager.Settings.SelectStrategyIndex].name = val;
                        dropdown.options[dropdown.value].text = val;
                        dropdown.RefreshShownValue();
                    }
                });
            });

            CButton deleteStrategyProgrammeButton = GameObjectCreationUtils.UGUICreateCButton(parent2, 10f, "删除方案");
            Extentions.SetHeight(deleteStrategyProgrammeButton.gameObject.GetComponent<RectTransform>(), 40f);
            deleteStrategyProgrammeButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16f;
            deleteStrategyProgrammeButton.ClearAndAddListener(delegate () {
                if (dropdown.options.Count > 1)
                {
                    Action action = delegate ()
                    {
                        ConfigManager.Programmes.RemoveAt(dropdown.value);

                        dropdown.ClearOptions();
                        dropdown.AddOptions(ConfigManager.Programmes.ConvertAll(x => x.name));
                        dropdown.value = 0;
                        dropdown.onValueChanged.Invoke(dropdown.value);
                    };

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
                    UIUtils.showTips("警告", "请至少保留一个方案!");
                }
            });

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
                Transform transform = UIUtils.CreateStrategyPanel(this._strategySettings);
                this.RenderStrategy(transform.transform, strategy);
                LayoutRebuilder.MarkLayoutForRebuild(this._strategySettings.parent.GetComponent<RectTransform>());
            });
            this._conditionSetter = ConditionSetterPanel.Create(this._focus.transform);
            this._changeTacticsPanel = UIUtils.CreateChangeTactics(this._focus.transform).GetComponent<RectTransform>();
            this._switchWeaponPanel = UIUtils.CreateOneValueOptionsPanel(this._focus.transform).GetComponent<RectTransform>();
            this._teammateCommandPanel = UIUtils.CreateOneValueOptionsPanel(this._focus.transform).GetComponent<RectTransform>();
            this._moveActionSelectPanel = MoveActionSelectPanel.Create(this._focus.transform);
        }

        private void RefreshUI()
        {
            // this._scroll.ScrollTo(this._otherSettings);
            LayoutRebuilder.MarkLayoutForRebuild(this._scroll.Content);
            LayoutRebuilder.MarkLayoutForRebuild(this._strategySettings.parent.GetComponent<RectTransform>());
        }

        private void ShowSkillSelectUI(short selectedSkillId, List<short> skillIdList, Action<sbyte, short> onSelectedSkill)
        {
            if (Game.Instance.GetCurrentGameStateName() == EGameState.InGame)
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
                UIUtils.showTips("警告", "载入存档后才能进行选择");
            }
        }

        // Token: 0x0600005C RID: 92 RVA: 0x000066AC File Offset: 0x000048AC
        private void Awake()
        {
            Debug.Log("UI_CombatStrategySetting::Awake");
            this._distanceChangeSpeedTogGroup.InitPreOnToggle();
            this._distanceChangeSpeedTogGroup.OnActiveToggleChange = delegate (CToggle togNew, CToggle _)
            {
                CombatStrategyMod.Settings.DistanceChangeSpeed = togNew.Key;
            };
            this._needRemoveTrickTogGroup.InitPreOnToggle();
            this._needRemoveTrickTogGroup.OnActiveToggleChange = delegate (CToggle togNew, CToggle togOld)
            {
                if (togNew == null)
                {
                    CombatStrategyMod.Settings.RemoveTrick[togOld.Key] = togOld.isOn;
                }
                else
                {
                    CombatStrategyMod.Settings.RemoveTrick[togNew.Key] = togNew.isOn;
                }
            };

            this._selectSkillArgBox = EasyPool.Get<ArgumentBox>();
            _allActiveSkillItemList.Clear();
            foreach (CombatSkillItem combatSkillItem in CombatSkill.Instance)
            {
                if (combatSkillItem.EquipType != CombatSkillEquipType.Neigong && combatSkillItem.EquipType != CombatSkillEquipType.Assist)
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
        }

        // Token: 0x0600005F RID: 95 RVA: 0x000068A8 File Offset: 0x00004AA8
        private void OnGUI()
        {
            Debug.Log("UI_CombatStrategySetting::OnGUI");

            if (this._handlingKey != null)
            {
                this.ListenHotKey();
            }
        }

        // Token: 0x06000060 RID: 96 RVA: 0x000068CC File Offset: 0x00004ACC
        protected override void OnClick(CButton btn)
        {
            bool flag = btn.name == "Close";
            if (flag)
            {
                base.StartCoroutine(this.CoroutineConfirmChanges());
                this.QuickHide();
            }
        }

        // Token: 0x06000061 RID: 97 RVA: 0x00006904 File Offset: 0x00004B04
        private IEnumerator CoroutineConfirmChanges()
        {
            ArgumentBox box = EasyPool.Get<ArgumentBox>();
            box.Set("ShowBlackMask", true);
            box.Set("ShowWaitAnimation", true);
            UIElement.FullScreenMask.SetOnInitArgs(box);
            UIElement.FullScreenMask.Show();
            ValueTuple<bool, bool> valueTuple = ConfigManager.SaveJsons();
            bool settingsChanged = valueTuple.Item1;
            bool strategiesChanged = valueTuple.Item2;
            bool inCombat = this._inCombat;
            if (inCombat)
            {
                if (settingsChanged)
                {
                    GameDataBridge.AddMethodCall<ushort, string>(-1, 8, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_UpdateSettingsJson, ConfigManager.GetBackendSettingsJson());
                }
                if (strategiesChanged)
                {
                    GameDataBridge.AddMethodCall<ushort, string>(-1, 8, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_UpdateStrategiesJson, ConfigManager.GetStrategiesJson());
                }
            }
            yield return new WaitForEndOfFrame();
            UIElement.FullScreenMask.Hide(false);
            yield break;
        }

        // Token: 0x06000062 RID: 98 RVA: 0x00006914 File Offset: 0x00004B14
        private void InitAllSettings()
        {
            this._distanceChangeSpeedTogGroup.SetWithoutNotify(CombatStrategyMod.Settings.DistanceChangeSpeed, true);
            for (int i = 0; i < CombatStrategyMod.Settings.RemoveTrick.Length; i++)
            {
                this._needRemoveTrickTogGroup.SetWithoutNotify(i, CombatStrategyMod.Settings.RemoveTrick[i]);
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
            CToggle ctoggle = base.CGet<CToggle>(name);
            ctoggle.onValueChanged.RemoveAllListeners();
            ctoggle.isOn = CombatStrategyMod.Settings.GetBool(name);
            ctoggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                CombatStrategyMod.Settings.SetValue(name, isOn);
            });
        }

        // Token: 0x06000064 RID: 100 RVA: 0x00006A7C File Offset: 0x00004C7C
        private void InitSliderSetting(string name, int multiplyer = 1)
        {
            TSlider tslider = base.CGet<TSlider>(name);
            tslider.onValueChanged.RemoveAllListeners();
            tslider.value = (float)(CombatStrategyMod.Settings.GetInt(name) / multiplyer);
            tslider.onValueChanged.AddListener(delegate (float val)
            {
                CombatStrategyMod.Settings.SetValue(name, (int)val * multiplyer);
            });
        }

        // Token: 0x06000065 RID: 101 RVA: 0x00006AF0 File Offset: 0x00004CF0
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

        // Token: 0x06000066 RID: 102 RVA: 0x00006B48 File Offset: 0x00004D48
        private void InitStrategy()
        {
            int num = -1;
            for (int i = 0; i < CombatStrategyMod.Strategies.Count; i++)
            {
                Transform transform = (++num < this._strategySettings.childCount) ? this._strategySettings.GetChild(num) : UIUtils.CreateStrategyPanel(this._strategySettings);
                transform.gameObject.SetActive(true);
                this.RenderStrategy(transform.transform, CombatStrategyMod.Strategies[i]);
            }
        }

        private void ClearAllStrategy()
        {
            for (var i = 0; i < this._strategySettings.childCount; i++)
            {
                this._strategySettings.GetChild(i).gameObject.SetActive(false);
            }
        }

        // Token: 0x06000067 RID: 103 RVA: 0x00006BB8 File Offset: 0x00004DB8
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
            var btnList = new List<UI_PopupMenu.BtnData>();
            if (Game.Instance.GetCurrentGameStateName() != EGameState.InGame)
            {
                btnList.Add(new UI_PopupMenu.BtnData("选择功法", false, null, null, null, "载入存档后才能进行选择", -1, null, null, null));
            }
            else
            {
                btnList.Add(new UI_PopupMenu.BtnData("选择功法", true, new Action(() => {
                    var _onSelected = new Action<sbyte, short>((sbyte type, short skillId) => {
                        if (type == 1)
                        {
                            Debug.Log("选中功法" + skillId);
                            strategy.SetAction(skillId);
                            this.RenderStrategySkillText(strategy, skillRefers);
                        }
                        else
                        {
                            // cancel
                        }
                    });
                    ShowSkillSelectUI(strategy.skillId, _allActiveSkillItemList.ConvertAll(x => x.TemplateId), _onSelected);
                }), null, null, "自动施展功法", -1, null, null, null));
            }
            btnList.Add(new UI_PopupMenu.BtnData("变招", true, new Action(()=> {
                this.ShowChangeTacticsPanel(skillRefers, strategy);
            }), tipContent: "自动变招"));
            btnList.Add(new UI_PopupMenu.BtnData("切换武器", true, new Action(() => {
                this.ShowSwitchWeaponPanel(skillRefers, strategy);
            }), tipContent: "自动切换武器"));
            btnList.Add(new UI_PopupMenu.BtnData("队友协助", true, new Action(() => {
                this.ShowTeammateCommandPanel(skillRefers, strategy);
            }), tipContent: "可自动执行队友指令"));
            btnList.Add(new UI_PopupMenu.BtnData("自动移动", true, new Action(() => {
                this._moveActionSelectPanel.Show(skillRefers, strategy, new Action(() => {
                    this.RenderStrategySkillText(strategy, skillRefers);
                }));
            }), tipContent: "移动方式改为由策略触发，让移动变得更灵活；注意：该策略若执行了，默认的自动移动将不会执行"));
            btnList.Add(new UI_PopupMenu.BtnData("普通攻击", true, new Action(() => {
                strategy.type = (short)StrategyConst.StrategyType.NormalAttack;
                strategy.SetAction(new NormalAttackAction());
                this.RenderStrategySkillText(strategy, skillRefers);
            }), tipContent: "触发普通攻击"));
            btnList.Add(new UI_PopupMenu.BtnData("添加条件", true, delegate ()
            {
                Condition condition = new Condition();
                strategy.conditions.Add(condition);
                Transform transform3 = UIUtils.CreateDropDown(content).transform;
                castSkill.SetAsLastSibling();
                this.RenderCondition(transform3, strategy, condition);
                content.GetComponent<GridLayoutGroup>().CalculateLayoutInputVertical();
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                LayoutRebuilder.MarkLayoutForRebuild(transform.GetComponent<RectTransform>());
            }, null, null, "策略触发的前提条件，可添加多个条件", -1, null, null, null));
            btnList.Add(new UI_PopupMenu.BtnData("删除策略", true, delegate ()
            {
                CombatStrategyMod.Strategies.Remove(strategy);
                for (int k = transform.GetSiblingIndex() + 1; k < _strategySettings.childCount; k++)
                {
                    _strategySettings.GetChild(k).gameObject.GetComponent<Refers>().CGet<TextMeshProUGUI>("Priority").text = k.ToString();
                }
                Object.Destroy(transform.gameObject);
                LayoutRebuilder.MarkLayoutForRebuild(_strategySettings);
            }, null, null, null, -1, null, null, null));
            skillRefers.CGet<CButton>("Button").ClearAndAddListener(delegate ()
            {
                this.ShowMenu(btnList, castSkill.position);
            });
            for (int j = content.childCount - 1; j > num; j--)
            {
                UnityEngine.Object.Destroy(content.GetChild(j).gameObject);
            }
        }

        // Token: 0x06000068 RID: 104 RVA: 0x00006EBC File Offset: 0x000050BC
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
                }, null, null, null, -1, null, null, null),
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
                }, null, null, null, -1, null, null, null)
            };
            component.CGet<CButton>("Button").ClearAndAddListener(delegate ()
            {
                this.ShowMenu(btnList, transform.position);
            });
        }

        // Token: 0x06000069 RID: 105 RVA: 0x00006FA4 File Offset: 0x000051A4
        private void ShowMenu(List<UI_PopupMenu.BtnData> btnList, Vector3 position)
        {
            ArgumentBox argumentBox = EasyPool.Get<ArgumentBox>();
            argumentBox.SetObject("BtnInfo", btnList);
            argumentBox.SetObject("ScreenPos", UIManager.Instance.UiCamera.WorldToScreenPoint(position));
            argumentBox.SetObject("ItemSize", new Vector2(60f, 20f));
            argumentBox.SetObject("OnCancel", new Action(delegate ()
            {
            }));
            UIElement popupMenu = UIElement.PopupMenu;
            popupMenu.OnShowed = (Action)Delegate.Combine(popupMenu.OnShowed, new Action(delegate ()
            {
                this._scroll.SetScrollEnable(false);
            }));
            UIElement popupMenu2 = UIElement.PopupMenu;
            popupMenu2.OnHide = (Action)Delegate.Combine(popupMenu2.OnHide, new Action(delegate ()
            {
                this._scroll.SetScrollEnable(true);
            }));
            UIElement.PopupMenu.SetOnInitArgs(argumentBox);
            UIManager.Instance.ShowUI(UIElement.PopupMenu);
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

        // Token: 0x0600006B RID: 107 RVA: 0x00007110 File Offset: 0x00005310
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
                CombatStrategyMod.Settings.SetKey(this._handlingKey, keyCode);
                this.StopHotKeySetting();
            }
        }

        // Token: 0x06000071 RID: 113 RVA: 0x000077A4 File Offset: 0x000059A4
        private void RenderHotKeyPrefab(string name, bool isInit = false)
        {
            GameObject gameObject = base.CGet<GameObject>(name);
            Refers refer = gameObject.GetComponent<Refers>();
            KeyCode key = CombatStrategyMod.Settings.GetKey(name);
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
                        bool flag = this._handlingKey == name;
                        if (flag)
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

        private RectTransform _otherSettings;

        private RectTransform _strategyProgramme;

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
