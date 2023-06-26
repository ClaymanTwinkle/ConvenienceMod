using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CombatStrategy;
using FrameWork;
using HarmonyLib;
using UnityEngine.U2D;
using UnityEngine;
using System.Windows;
using DG.Tweening;
using FrameWork.ModSystem;
using static UnityEngine.UI.Scrollbar;
using UnityEngine.Events;
using Assets.Scripts.Game.Model;
using GameData.GameDataBridge;
using GameData.Domains.CombatSkill;
using GameData.Serializer;
using FrameWork.Tools.EnhancedRichText;
using Config;
using TMPro;
using ConvenienceFrontend.ManualArchive;
using ConvenienceFrontend.ModifyCombatSkill.Data;

namespace ConvenienceFrontend.ModifyCombatSkill
{
    internal class UI_FusionCombatSkill : UIBase
    {
        private static UIElement element;

        private GameObject _focus;
        private CScrollRect _scroll;

        private PopupWindow popupWindow;

        private short _mainCombatSkillId = -1;
        private short _subCombatSkillId = -1;

        private CombatSkillDisplayData _mainCombatSkillDisplayData = null;
        private CombatSkillDisplayData _subCombatSkillDisplayData = null;

        private RectTransform _mainCombatSkillView = null;
        private RectTransform _subCombatSkillView = null;

        private int _listenerId = -1;

        private CombatSkillItemWrapper _newCombatSkillItem = null;
        private CombatSkillItem _mainCombatSkillItem = null;
        private CombatSkillItem _subCombatSkillItem = null;

        private Dictionary<string, bool> _changeDic = new Dictionary<string, bool>();

        public static UIElement ShowUI(short mainCombatSkillId, short subCombatSkillId)
        {
            var element = GetUI();

            ArgumentBox argumentBox = EasyPool.Get<ArgumentBox>();
            argumentBox.Set("MainCombatSkillId", mainCombatSkillId);
            argumentBox.Set("SubCombatSkillId", subCombatSkillId);
            element.SetOnInitArgs(argumentBox);
            UIManager.Instance.ShowUI(element);
            return element;
        }

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
                GameObject gameObject = UIUtils.CreateMainUI("UI_CombatStrategySetting", "融合功法");
                var uiComponent = gameObject.AddComponent<UI_FusionCombatSkill>();
                uiComponent.UiType = UILayer.LayerVeryTop; // 3;
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
            // this._focus.AddComponent<CButton>().ClearAndAddListener(new Action(this.StopHotKeySetting));
            popupWindow = obj.GetComponentInChildren<PopupWindow>();
            popupWindow.ConfirmButton.gameObject.SetActive(true);
            popupWindow.ConfirmButton.onClick.RemoveAllListeners();
            popupWindow.ConfirmButton.interactable = false;
            popupWindow.ConfirmButton.onClick.AddListener(delegate () {
                // 确认
                CombatSkillExtraUtils.AddExtraCombatSkill(_newCombatSkillItem.Item);
                QuickHide();
            });
            popupWindow.CloseButton.gameObject.SetActive(false);
            popupWindow.CancelButton.gameObject.SetActive(true);
            popupWindow.CancelButton.onClick.RemoveAllListeners();
            popupWindow.CancelButton.onClick.AddListener(delegate ()
            {
                QuickHide();
            });
            popupWindow.TitleLabel.transform.parent.position = new Vector3(popupWindow.TitleLabel.transform.parent.position.x, popupWindow.TitleLabel.transform.parent.position.y + 50, popupWindow.TitleLabel.transform.parent.position.z);
            GameObject gameObject = GameObjectCreationUtils.InstantiateUIElement(popupWindow.transform, "VerticalScrollView");
            gameObject.SetActive(true);
            RectTransform component = popupWindow.transform.Find("ElementsRoot/").gameObject.GetComponent<RectTransform>();
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(component.rect.size.x, component.rect.size.y - 120);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 60, gameObject.transform.position.z);
            this._scroll = gameObject.GetComponent<CScrollRect>();
            // this._scroll.OnScrollEvent += OnScrollEvent;
            GameObject gameObject2 = this._scroll.Content.gameObject;
            RectTransform content = this._scroll.Content;
            Extentions.SetWidth(content, component.rect.size.x * 0.96f);
            UIUtils.CreateVerticalAutoSizeLayoutGroup(gameObject2).spacing = 15f;

            BuildCombatSkillDisplayerView(content);
            BuildSelectFusionItem(content);
        }

        public override void OnInit(ArgumentBox argsBox)
        {
            _mainCombatSkillId = -1;
            _subCombatSkillId = -1;

            _mainCombatSkillDisplayData = null;
            _subCombatSkillDisplayData = null;
            _changeDic.Clear();

            argsBox.Get("MainCombatSkillId", out _mainCombatSkillId);
            argsBox.Get("SubCombatSkillId", out _subCombatSkillId);

            _mainCombatSkillItem = Config.CombatSkill.Instance[_mainCombatSkillId];
            _subCombatSkillItem = Config.CombatSkill.Instance[_subCombatSkillId];
            _newCombatSkillItem = CombatSkillExtraUtils.CreateNewCombatSkillItem(_mainCombatSkillItem);

            _listenerId = GameDataBridge.RegisterListener(OnNotifyGameData);
            UpdateCombatSkillDisplayData();
            
        }

        public override void QuickHide()
        {
            base.QuickHide();
            _listenerId = -1;
        }

        public void UpdateCombatSkillDisplayData()
        {
            CombatSkillModel.GetCombatSkillDisplayData(_listenerId, SingletonObject.getInstance<BasicGameData>().TaiwuCharId, new List<short> { _mainCombatSkillId, _subCombatSkillId });
        }

        public override void OnNotifyGameData(List<NotificationWrapper> notifications)
        {
            foreach (NotificationWrapper notification2 in notifications)
            {
                Notification notification = notification2.Notification;
                if (notification.Type == 1)
                {
                    if (notification.DomainId == 7 && notification.MethodId == 0)
                    {
                        List<CombatSkillDisplayData> itemList = null;
                        Serializer.Deserialize(notification2.DataPool, notification.ValueOffset, ref itemList);
                        if (itemList != null)
                        {
                            itemList.ForEach(x =>
                            {
                                if (x.TemplateId == _mainCombatSkillId)
                                {
                                    _mainCombatSkillDisplayData = x;
                                    Debug.Log("_mainCombatSkillDisplayData-" + _mainCombatSkillDisplayData.ReadingState);
                                }
                                else if (x.TemplateId == _subCombatSkillId)
                                {
                                    _subCombatSkillDisplayData = x;
                                    Debug.Log("_subCombatSkillDisplayData");
                                }
                            });
                        }

                        if (_mainCombatSkillDisplayData != null && _subCombatSkillDisplayData != null)
                        {
                            RefreshUI();
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            CGet<CToggleGroup>("ToggleGroup_CastAnimation").InitPreOnToggle();
            CGet<CToggleGroup>("ToggleGroup_CastAnimation").OnActiveToggleChange = delegate (CToggle togNew, CToggle _)
            {
                _newCombatSkillItem.SetCastAnimation(togNew.Key == 0 ? _mainCombatSkillItem.CastAnimation : _subCombatSkillItem.CastAnimation);
                _newCombatSkillItem.SetCastParticle(togNew.Key == 0 ? _mainCombatSkillItem.CastParticle : _subCombatSkillItem.CastParticle);
                _newCombatSkillItem.SetCastSoundEffect(togNew.Key == 0 ? _mainCombatSkillItem.CastSoundEffect : _subCombatSkillItem.CastSoundEffect);
                _newCombatSkillItem.SetPrepareAnimation(togNew.Key == 0 ? _mainCombatSkillItem.PrepareAnimation : _subCombatSkillItem.PrepareAnimation);

                _changeDic["ToggleGroup_CastAnimation"] = togNew.Key != 0;
                CheckConfirmButton();

            };

            CGet<CToggleGroup>("ToggleGroup_SkillEffectID").InitPreOnToggle();
            CGet<CToggleGroup>("ToggleGroup_SkillEffectID").OnActiveToggleChange = delegate (CToggle togNew, CToggle _)
            {
                _newCombatSkillItem.SetDirectEffectID(togNew.Key == 0 ? _mainCombatSkillItem.DirectEffectID : _subCombatSkillItem.DirectEffectID);
                _newCombatSkillItem.SetReverseEffectID(togNew.Key == 0 ? _mainCombatSkillItem.ReverseEffectID : _subCombatSkillItem.ReverseEffectID);

                _changeDic["ToggleGroup_SkillEffectID"] = togNew.Key != 0;
                CheckConfirmButton();
            };
        }

        private void CheckConfirmButton()
        {
            popupWindow.ConfirmButton.interactable = _changeDic.Count > 0 && _changeDic.Any(x=>x.Value);
        }

        /// <summary>
        /// 技能图标
        /// </summary>
        /// <param name="parent"></param>
        private void BuildCombatSkillDisplayerView(Transform parent)
        {
            var parents = UIUtils.CreateRow(parent);

            var mainCombatSkillGameObject = new GameObject("mainCombatSkillGameObject");
            mainCombatSkillGameObject.transform.parent = parents;
            _mainCombatSkillView = mainCombatSkillGameObject.AddComponent<RectTransform>();
            Extentions.SetWidth(_mainCombatSkillView, 500);
            BuildCombatSkillInfo(_mainCombatSkillView, "main");

            var subCombatSkillGameObject = new GameObject("subCombatSkillGameObject");
            subCombatSkillGameObject.transform.parent = parents;
            _subCombatSkillView = subCombatSkillGameObject.AddComponent<RectTransform>();
            Extentions.SetWidth(_subCombatSkillView, 500);
            BuildCombatSkillInfo(_subCombatSkillView, "sub");

            UIUtils.CreateRow(parent);
            UIUtils.CreateRow(parent);
        }

        private void BuildSelectFusionItem(Transform parent)
        {
            var ToggleGroup_CastAnimation = UIUtils.CreateToggleGroup(UIUtils.CreateRow(parent), "ToggleGroup_CastAnimation", "施展动画", new string[] { "使用左边", "使用右边" }, 1, false, false);
            for (var i = 0; i < ToggleGroup_CastAnimation.Count(); i++)
            {
                ToggleGroup_CastAnimation.GetAll()[i].transform.position += new Vector3(60 + i*420, 0, 0);
            }
            AddMono(ToggleGroup_CastAnimation, "ToggleGroup_CastAnimation");

            var ToggleGroup_SkillEffectID = UIUtils.CreateToggleGroup(UIUtils.CreateRow(parent), "ToggleGroup_SkillEffectID", "正逆练特效", new string[] { "使用左边", "使用右边" }, 1, false, false);
            for (var i = 0; i < ToggleGroup_SkillEffectID.Count(); i++)
            {
                ToggleGroup_SkillEffectID.GetAll()[i].transform.position += new Vector3(38 + i * 420, 0, 0);
            }
            AddMono(ToggleGroup_SkillEffectID);
            AddMono(ToggleGroup_SkillEffectID, "ToggleGroup_SkillEffectID");
        }

        private void BuildCombatSkillInfo(Transform parent, string tag)
        {
            AddMono(GameObjectCreationUtils.UGUICreateCImage(parent, new Vector2(0, -20), new Vector2(100, 100), null), tag + "SkillType");
            AddMono(GameObjectCreationUtils.UGUICreateCImage(parent, new Vector2(0, -30), new Vector2(100, 100), null), tag + "FiveElementsType");
            AddMono(GameObjectCreationUtils.UGUICreateCImage(parent, new Vector2(-40, -60), new Vector2(40, 40), null), tag + "GradeBack");
            AddMono(GameObjectCreationUtils.UGUICreateTMPText(parent, new Vector2(-45, -60), new Vector2(40, 40), 23f, ""), tag + "Grade");
            AddMono(GameObjectCreationUtils.UGUICreateTMPText(parent, new Vector2(0, -100), new Vector2(200, 50), 32f, ""), tag + "Name");
        }

        private void RefreshCombatSkillInfo()
        {
            RefreshCombatSkillInfo("main");
            RefreshCombatSkillInfo("sub");
        }

        private void RefreshCombatSkillInfo(string tag)
        {
            var combatSkillDisplayData = tag.Equals("main") ? _mainCombatSkillDisplayData : _subCombatSkillDisplayData;
            var combatSkillItem = Config.CombatSkill.Instance[combatSkillDisplayData.TemplateId];

            CGet<TextMeshProUGUI>(tag + "Name").text = combatSkillItem.Name.SetColor(Colors.Instance.GradeColors[combatSkillItem.Grade]);
            CGet<CImage>(tag + "FiveElementsType").enabled = false;
            CGet<CImage>(tag + "GradeBack").SetSprite(ItemView.GetGradeIcon(combatSkillItem.Grade));
            CGet<TextMeshProUGUI>(tag + "Grade").text = LocalStringManager.Get($"LK_ShortGrade_{combatSkillItem.Grade}");
            var cImageSkillType = CGet<CImage>(tag + "SkillType");
            cImageSkillType.SetSprite(combatSkillItem.Icon, autoNativeSize: true);
            cImageSkillType.SetColor(Colors.Instance.FiveElementsColors[combatSkillItem.FiveElements]);

        }

        private void RefreshUI()
        {
            RefreshCombatSkillInfo();
        }
    }
}
