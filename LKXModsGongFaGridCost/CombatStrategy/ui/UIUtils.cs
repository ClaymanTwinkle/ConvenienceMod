using System;
using System.Collections.Generic;
using System.Linq;
using FrameWork.ModSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Object = UnityEngine.Object;
using static UnityEngine.UI.GridLayoutGroup;
using DG.Tweening;
using static DG.Tweening.DOTweenAnimation;
using FrameWork;

namespace ConvenienceFrontend.CombatStrategy
{
    // Token: 0x0200000D RID: 13
    internal class UIUtils
    {
        // Token: 0x06000022 RID: 34 RVA: 0x000032E0 File Offset: 0x000014E0
        public static void PrepareMaterial()
        {
            ResLoader.Load<GameObject>("RemakeResources/Prefab/Views/UI_SystemSetting", new Action<GameObject>(PrefabLoaded), null);
            ResLoader.Load<GameObject>("RemakeResources/Prefab/Views/UI_ModPanel", new Action<GameObject>(PrefabLoaded), null);
            UIUtils.BuildToggle();
            UIUtils.BuildSliderBar();
            UIUtils.BuildDropDown();
        }

        internal static void PrefabLoaded(GameObject obj)
		{
			bool flag = obj.name == "UI_SystemSetting";
			if (flag)
			{
				UIUtils._ui_SystemSetting = obj;
			}
			else
			{
				bool flag2 = obj.name == "UI_ModPanel";
				if (flag2)
				{
					UIUtils._ui_ModPanel = obj;
				}
			}
		}

        // Token: 0x06000023 RID: 35 RVA: 0x00003334 File Offset: 0x00001534
        private static void BuildToggle()
        {
            UIUtils.toggle = Object.Instantiate<GameObject>(UIUtils._ui_ModPanel.transform.GetChild(5).GetChild(2).gameObject);
            UIUtils.toggle.SetActive(false);
            RectTransform component = UIUtils.toggle.GetComponent<RectTransform>();
            Extentions.SetPivot(component, UIUtils.LeftCenter);
            component.anchoredPosition = new Vector2(0f, -30f);
            component.sizeDelta = new Vector2(400f, 60f);
            RectTransform component2 = UIUtils.toggle.transform.GetChild(0).GetComponent<RectTransform>();
            Extentions.SetPivot(component2, UIUtils.LeftCenter);
            component2.anchoredPosition = Vector2.zero;
        }

        // Token: 0x06000024 RID: 36 RVA: 0x000033E4 File Offset: 0x000015E4
        private static void BuildSliderBar()
        {
            UIUtils.sliderBar = Object.Instantiate<GameObject>(UIUtils._ui_ModPanel.transform.GetChild(5).GetChild(3).gameObject);
            UIUtils.sliderBar.SetActive(false);
            UIUtils.sliderBar.GetComponent<RectTransform>().sizeDelta = new Vector2(380f, 60f);
            Object.DestroyImmediate(UIUtils.sliderBar.transform.GetChild(0).gameObject);
            RectTransform component = UIUtils.sliderBar.transform.GetChild(0).GetComponent<RectTransform>();
            Extentions.SetAnchor(component, UIUtils.LeftCenter, UIUtils.LeftCenter);
            component.anchoredPosition = Vector2.zero;
            UIUtils.sliderBar.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(10f, 0f);
            UIUtils.sliderBar.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(8f, 0f);
            RectTransform component2 = UIUtils.sliderBar.transform.GetChild(3).GetComponent<RectTransform>();
            component2.offsetMin = new Vector2(112f, -5f);
            component2.offsetMax = new Vector2(-13f, 5f);
            CSlider component3 = component2.GetComponent<CSlider>();
            RectTransform fillRect = component3.fillRect;
            RectTransform handleRect = component3.handleRect;
            string clickAudioKey = component3.ClickAudioKey;
            Object.DestroyImmediate(component3);
            TSlider tslider = component2.gameObject.AddComponent<TSlider>();
            tslider.fillRect = fillRect;
            tslider.handleRect = handleRect;
            tslider.ClickAudioKey = clickAudioKey;
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00003574 File Offset: 0x00001774
        private static void BuildDropDown()
        {
            UIUtils.dropDown = Object.Instantiate<GameObject>(UIUtils._ui_ModPanel.transform.GetChild(5).GetChild(1).gameObject);
            UIUtils.dropDown.SetActive(false);
            UIUtils.dropDown.GetComponent<RectTransform>().sizeDelta = new Vector2(380f, 60f);
            RectTransform component = UIUtils.dropDown.transform.GetChild(0).GetComponent<RectTransform>();
            Extentions.SetAnchor(component, UIUtils.LeftCenter, UIUtils.LeftCenter);
            Extentions.SetPivot(component, UIUtils.LeftCenter);
            component.anchoredPosition = Vector2.zero;
            component.sizeDelta = new Vector2(80f, 35f);
            GameObject gameObject = component.GetChild(1).gameObject;
            Object.Destroy(gameObject.GetComponent<LayoutElement>());
            Object.Destroy(gameObject.GetComponent<UIRectSizeController>());
            Object.Destroy(gameObject.GetComponent<ContentSizeFitter>());
            Object.Destroy(gameObject.GetComponent<MouseTipDisplayer>());
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(60f, 25f);
            GameObject gameObject2 = UIUtils.dropDown.transform.GetChild(1).gameObject;
            RectTransform component2 = UIUtils.dropDown.transform.GetChild(1).GetComponent<RectTransform>();
            component2.anchoredPosition = new Vector2(80f, 0f);
            component2.sizeDelta = new Vector2(-100f, -12f);
            Object.DestroyImmediate(gameObject2.GetComponent<CDropdown>());
            Object.DestroyImmediate(component2.GetChild(4).gameObject);
            Object.DestroyImmediate(component2.GetChild(3).gameObject);
            Refers component3 = UIUtils.dropDown.GetComponent<Refers>();
            CButton cbutton = gameObject2.AddComponent<CButton>();
            component3.AddMono(cbutton, "Button");
            TextMeshProUGUI componentInChildren = gameObject2.GetComponentInChildren<TextMeshProUGUI>();
            componentInChildren.fontSizeMax = 24f;
            componentInChildren.enableAutoSizing = true;
            component3.AddMono(componentInChildren, "DropDownLabel");
        }

        // Token: 0x06000026 RID: 38 RVA: 0x00003758 File Offset: 0x00001958
        private static GameObject GetFreeLabel(Transform parent, string label)
        {
            bool flag = UIUtils.freeLabel == null;
            if (flag)
            {
                UIUtils.freeLabel = Object.Instantiate<GameObject>(UIUtils._ui_ModPanel.transform.GetChild(5).GetChild(3).GetChild(0).gameObject);
                Extentions.SetPivot(UIUtils.freeLabel.GetComponent<RectTransform>(), UIUtils.LeftCenter);
                RectTransform component = UIUtils.freeLabel.transform.GetChild(0).GetComponent<RectTransform>();
                Extentions.SetPivot(component, UIUtils.LeftCenter);
                component.anchoredPosition = Vector2.zero;
                UIUtils.freeLabel.GetComponentInChildren<MouseTipDisplayer>().enabled = false;
            }
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.freeLabel, parent);
            gameObject.SetActive(true);
            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = label;
            return gameObject;
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00003820 File Offset: 0x00001A20
        private static GameObject GetSingleToggle()
        {
            bool flag = UIUtils.singleToggle == null;
            if (flag)
            {
                UIUtils.singleToggle = GameObjectCreationUtils.InstantiateUIElement(null, "CommonToggle1_Switch");
                RectTransform component = UIUtils.singleToggle.GetComponent<RectTransform>();
                Extentions.SetAnchor(component, UIUtils.LeftCenter, UIUtils.LeftCenter);
                Extentions.SetPivot(component, UIUtils.LeftCenter);
                foreach (RectTransform rectTransform in UIUtils.singleToggle.GetComponentsInChildren<RectTransform>(true))
                {
                    Extentions.SetHeight(rectTransform, 42f);
                }
            }
            return UIUtils.singleToggle;
        }

        // Token: 0x06000028 RID: 40 RVA: 0x000038B4 File Offset: 0x00001AB4
        public static GameObject CreateMainUI(string name)
        {
            GameObject gameObject = GameObjectCreationUtils.CreatePopupWindow(null, Vector2.zero, new Vector2(1300f, 1000f), "自动战斗设置", null, null, true);
            gameObject.name = "popUpWindowBase";
            PopupWindow component = gameObject.GetComponent<PopupWindow>();
            component.ConfirmButton.gameObject.SetActive(false);
            component.CloseButton.gameObject.SetActive(true);
            GameObject gameObject2 = gameObject.transform.parent.gameObject;
            gameObject2.name = name;
            gameObject2.SetActive(false);
            gameObject2.layer = 5;
            Extentions.SetAnchor(gameObject2.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            gameObject2.AddComponent<ConchShipGraphicRaycaster>();
            GameObject gameObject3 = new GameObject("FadeIn");
            GameObject gameObject4 = new GameObject("FadeOut");
            gameObject3.transform.SetParent(gameObject2.transform);
            gameObject4.transform.SetParent(gameObject2.transform);
            DOTweenAnimation dotweenAnimation = gameObject3.AddComponent<DOTweenAnimation>();
            DOTweenAnimation dotweenAnimation2 = gameObject4.AddComponent<DOTweenAnimation>();
            dotweenAnimation.animationType = (dotweenAnimation2.animationType = AnimationType.Fade); // 7
            dotweenAnimation.endValueFloat = (dotweenAnimation2.endValueFloat = 0f);
            dotweenAnimation.duration = (dotweenAnimation2.duration = 0.2f);
            dotweenAnimation.easeType = (dotweenAnimation2.easeType = Ease.Linear); // 1
            dotweenAnimation.targetIsSelf = (dotweenAnimation2.targetIsSelf = false);
            dotweenAnimation.isRelative = (dotweenAnimation2.isRelative = true);
            dotweenAnimation.isFrom = true;
            dotweenAnimation2.isFrom = false;
            dotweenAnimation.isValid = (dotweenAnimation2.isValid = true);
            dotweenAnimation.target = (dotweenAnimation2.target = gameObject.AddComponent<CanvasGroup>());
            dotweenAnimation.targetGO = (dotweenAnimation2.targetGO = gameObject);
            dotweenAnimation.targetType = (dotweenAnimation2.targetType = TargetType.CanvasGroup); // 2
            dotweenAnimation.autoKill = (dotweenAnimation2.autoKill = false);
            dotweenAnimation.autoPlay = (dotweenAnimation2.autoPlay = false);
            return gameObject2;
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00003AC4 File Offset: 0x00001CC4
        public static VerticalLayoutGroup CreateVerticalAutoSizeLayoutGroup(GameObject obj)
        {
            VerticalLayoutGroup verticalLayoutGroup = obj.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childForceExpandHeight = true;
            verticalLayoutGroup.childForceExpandWidth = false;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childAlignment = 0;
            ContentSizeFitter contentSizeFitter = obj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; // 2;
            contentSizeFitter.horizontalFit = 0;
            return verticalLayoutGroup;
        }

        // Token: 0x0600002A RID: 42 RVA: 0x00003B20 File Offset: 0x00001D20
        public static GameObject CreateSettingPanel(Transform parent, string name, string title)
        {
            GameObject gameObject = new GameObject(name);
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            Extentions.SetAnchor(rectTransform, Vector2.up, Vector2.up);
            rectTransform.sizeDelta = parent.GetComponent<RectTransform>().rect.size;
            rectTransform.SetParent(parent, false);
            UIUtils.CreateTitle(rectTransform, title);
            UIUtils.DrawBorder(rectTransform);
            GameObject gameObject2 = new GameObject("Settings");
            RectTransform rectTransform2 = gameObject2.AddComponent<RectTransform>();
            Extentions.SetAnchor(rectTransform2, Vector2.up, Vector2.up);
            rectTransform2.offsetMax = new Vector2(rectTransform.rect.width - 40f, -55f);
            rectTransform2.offsetMin = new Vector2(40f, -55f);
            Extentions.SetWidth(rectTransform2, rectTransform.rect.width - 80f);
            Extentions.SetPivot(rectTransform2, Vector2.up);
            rectTransform2.SetParent(rectTransform, false);
            UIUtils.CreateVerticalAutoSizeLayoutGroup(gameObject2).padding = new RectOffset(40, 0, 0, 0);
            UIRectSizeController uirectSizeController = gameObject2.AddComponent<UIRectSizeController>();
            List<UIRectSizeController.FollowerConfig> list = new List<UIRectSizeController.FollowerConfig>();
            UIRectSizeController.FollowerConfig item = default(UIRectSizeController.FollowerConfig);
            item.Target = rectTransform;
            item.SizeOffset = new Vector2(95f, 80f);
            list.Add(item);
            uirectSizeController.ControlList = list;
            return gameObject2;
        }

        // Token: 0x0600002B RID: 43 RVA: 0x00003C6C File Offset: 0x00001E6C
        public static GameObject CreateSubTitle(Transform parent, string label)
        {
            bool flag = UIUtils.subTitle == null;
            if (flag)
            {
                UIUtils.subTitle = UIUtils._ui_SystemSetting.transform.Find("MainWindow/VerticalScrollView/Viewport/Content/HotKeySettings/HeadLine/").gameObject;
            }
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.subTitle, parent);
            gameObject.GetComponentInChildren<TextMeshProUGUI>().SetText(label, true);
            return gameObject;
        }

        // Token: 0x0600002C RID: 44 RVA: 0x00003CC8 File Offset: 0x00001EC8
        public static GameObject CreateTitle(Transform parent, string label)
        {
            bool flag = UIUtils.title == null;
            if (flag)
            {
                UIUtils.title = Object.Instantiate<GameObject>(UIUtils._ui_ModPanel.transform.GetChild(4).GetChild(4).GetChild(2).GetChild(2).gameObject);
                Object.DestroyImmediate(UIUtils.title.GetComponentInChildren<TextLanguage>());
                RectTransform component = UIUtils.title.GetComponent<RectTransform>();
                Extentions.SetAnchor(component, Vector2.up, Vector2.up);
                component.anchoredPosition = new Vector2(60f, -20f);
            }
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.title, parent);
            gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = label;
            return gameObject;
        }

        // Token: 0x0600002D RID: 45 RVA: 0x00003D84 File Offset: 0x00001F84
        public static GameObject DrawBorder(Transform parent)
        {
            bool flag = UIUtils.borderBox == null;
            if (flag)
            {
                UIUtils.borderBox = UIUtils._ui_SystemSetting.transform.GetChild(3).GetChild(5).GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject;
            }
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.borderBox, parent).gameObject;
            Extentions.SetAnchor(gameObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            return gameObject;
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00003E08 File Offset: 0x00002008
        public static Transform CreateRow(Transform parent)
        {
            bool flag = UIUtils.rowContainer == null;
            if (flag)
            {
                UIUtils.rowContainer = UIUtils._ui_ModPanel.transform.GetChild(5).GetChild(0).gameObject;
            }
            return Object.Instantiate<GameObject>(UIUtils.rowContainer, parent).transform;
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003E5C File Offset: 0x0000205C
        public static TSlider CreateSliderBar(Transform parent, string name, int minVal, int maxVal, int divider, string format, string preLabel, string endLabel = null)
        {
            UIUtils.GetFreeLabel(parent, preLabel);
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.sliderBar, parent);
            bool flag = endLabel != null;
            if (flag)
            {
                UIUtils.GetFreeLabel(parent, endLabel);
            }
            gameObject.SetActive(true);
            gameObject.name = name;
            TSlider componentInChildren = gameObject.GetComponentInChildren<TSlider>();
            componentInChildren.minValue = (float)minVal;
            componentInChildren.maxValue = (float)maxVal;
            componentInChildren.wholeNumbers = true;
            TextMeshProUGUI label = gameObject.GetComponent<Refers>().CGet<TextMeshProUGUI>("CurValue");
            componentInChildren.label = label;
            componentInChildren.Divider = divider;
            componentInChildren.format = format;
            return componentInChildren;
        }

        // Token: 0x06000030 RID: 48 RVA: 0x00003EF4 File Offset: 0x000020F4
        public static CToggle CreateToggle(Transform parent, string name, string label)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.toggle, parent);
            gameObject.SetActive(true);
            gameObject.name = name;
            gameObject.GetComponent<Refers>().CGet<TextMeshProUGUI>("Label").text = label;
            return gameObject.transform.GetChild(1).GetComponent<CToggle>();
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00003F4C File Offset: 0x0000214C
        public static CToggle CreateToggle(Transform parent, string name, string labelOn, string labelOff, string preLabel = null, string endLabel = null)
        {
            GameObject gameObject = new GameObject(name);
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(200f, -30f);
            rectTransform.sizeDelta = new Vector2(400f, 60f);
            rectTransform.SetParent(parent);
            Vector2 vector = Vector2.zero;
            bool flag = preLabel != null;
            if (flag)
            {
                GameObject gameObject2 = UIUtils.GetFreeLabel(rectTransform, preLabel);
                vector += new Vector2(gameObject2.GetComponent<RectTransform>().rect.width + 5f, 0f);
            }
            GameObject gameObject3 = UIUtils.CreateSingleToggle(rectTransform, name, labelOn, labelOff, vector);
            vector += new Vector2(75f, 0f);
            bool flag2 = endLabel != null;
            if (flag2)
            {
                GameObject gameObject4 = UIUtils.GetFreeLabel(rectTransform, endLabel);
                gameObject4.GetComponent<RectTransform>().anchoredPosition = vector;
            }
            return gameObject3.GetComponent<CToggle>();
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00004038 File Offset: 0x00002238
        private static CToggle CreateSingleToggle(Transform parent, int key, string label, Vector2 position)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.GetSingleToggle(), parent);
            gameObject.SetActive(true);
            gameObject.name = key.ToString();
            gameObject.GetComponent<RectTransform>().anchoredPosition = position;
            Refers component = gameObject.GetComponent<Refers>();
            component.CGet<TextMeshProUGUI>("LabelOff").text = label;
            component.CGet<TextMeshProUGUI>("LabelOn").text = label;
            component.CGet<CToggle>("Toggle").Key = key;
            return component.CGet<CToggle>("Toggle");
        }

        // Token: 0x06000033 RID: 51 RVA: 0x000040C0 File Offset: 0x000022C0
        public static GameObject CreateSingleToggle(Transform parent, string name, string labelOn, string labelOff, Vector2 position)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.GetSingleToggle(), parent);
            gameObject.SetActive(true);
            gameObject.name = name;
            gameObject.GetComponent<RectTransform>().anchoredPosition = position;
            UIRectSizeController.FollowerConfig value = gameObject.GetComponentInChildren<UIRectSizeController>().ControlList[0];
            value.SizeOffset = new Vector2(22f, 0f);
            gameObject.GetComponentInChildren<UIRectSizeController>().ControlList[0] = value;
            Refers component = gameObject.GetComponent<Refers>();
            component.CGet<TextMeshProUGUI>("LabelOff").text = labelOff;
            component.CGet<TextMeshProUGUI>("LabelOn").text = labelOn;
            return gameObject;
        }

        // Token: 0x06000034 RID: 52 RVA: 0x00004164 File Offset: 0x00002364
        public static CToggleGroup CreateToggleGroup(Transform parent, string name, string label, string[] options, int allowOnNum = 1, bool allowUncheck = false, bool allowSwitchOff = false)
        {
            UIUtils.GetFreeLabel(parent, label);
            bool flag = UIUtils.emptyContainer == null;
            if (flag)
            {
                UIUtils.emptyContainer = new GameObject("emptyContainer");
                RectTransform rectTransform = UIUtils.emptyContainer.AddComponent<RectTransform>();
                Extentions.SetAnchor(rectTransform, Vector2.up, Vector2.up);
                rectTransform.anchoredPosition = new Vector2(280f, -30f);
                rectTransform.sizeDelta = new Vector2(560f, 60f);
            }
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.emptyContainer, parent);
            gameObject.SetActive(true);
            gameObject.name = name;
            CToggleGroup ctoggleGroup = gameObject.AddComponent<CToggleGroup>();
            Vector2 vector = new Vector2(-100f, 0f);
            int num = 0;
            Transform transform = gameObject.transform;
            for (int i = 0; i < options.Length; i++)
            {
                if (num == 7)
                {
                    transform = Object.Instantiate<GameObject>(UIUtils.emptyContainer, UIUtils.CreateRow(parent.parent).transform).transform;
                    vector = new Vector2(25f, 0f);
                    num = 0;
                }
                vector += new Vector2(100f, 0f);
                ctoggleGroup.Add(UIUtils.CreateSingleToggle(transform, i, options[i], vector));
                num++;
            }
            ctoggleGroup.SetAllowOnNum(allowOnNum);
            ctoggleGroup.AllowUncheck = allowUncheck;
            ctoggleGroup.AllowSwitchOff = allowSwitchOff;
            return ctoggleGroup;
        }

        // Token: 0x06000035 RID: 53 RVA: 0x000042D4 File Offset: 0x000024D4
        public static GameObject CreateHotKey(Transform parent, string name, string label)
        {
            bool flag = UIUtils.hotkey == null;
            if (flag)
            {
                UIUtils.hotkey = UIUtils._ui_SystemSetting.transform.Find("MainWindow/VerticalScrollView/Viewport/Content/HotKeySettings/ContentLine/").gameObject;
            }
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.hotkey, parent);
            gameObject.SetActive(true);
            gameObject.name = name;
            Refers component = gameObject.GetComponent<Refers>();
            TextMeshProUGUI textMeshProUGUI = component.CGet<TextMeshProUGUI>("Label");
            textMeshProUGUI.text = label;
            textMeshProUGUI.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            return gameObject;
        }

        // Token: 0x06000036 RID: 54 RVA: 0x0000435C File Offset: 0x0000255C
        public static GameObject CreateDropDown(Transform parent)
        {
            GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.dropDown, parent);
            gameObject.SetActive(true);
            return gameObject;
        }

        // Token: 0x06000037 RID: 55 RVA: 0x00004384 File Offset: 0x00002584
        public static Transform CreateStrategyPanel(Transform parent)
        {
            bool flag = UIUtils.strategyObj == null;
            if (flag)
            {
                UIUtils.strategyObj = new GameObject("Strategy");
                RectTransform rectTransform = UIUtils.strategyObj.AddComponent<RectTransform>();
                Extentions.SetAnchor(rectTransform, Vector2.up, Vector2.up);
                Extentions.SetWidth(rectTransform, 1000f);
                Extentions.SetPivot(rectTransform, Vector2.up);
                Refers refers = UIUtils.strategyObj.AddComponent<Refers>();
                GameObject gameObject = Object.Instantiate<GameObject>(UIUtils.freeLabel, rectTransform);
                RectTransform component = gameObject.GetComponent<RectTransform>();
                Extentions.SetAnchor(component, Vector2.up, Vector2.up);
                Extentions.SetPivot(component, Vector2.up);
                component.anchoredPosition = new Vector2(0f, -15f);
                gameObject.GetComponentInChildren<LayoutElement>().minWidth = 40f;
                MouseTipDisplayer componentInChildren = gameObject.GetComponentInChildren<MouseTipDisplayer>();
                componentInChildren.enabled = true;
                componentInChildren.PresetParam = new string[]
                {
                    "自动释放功法的优先度，点击以进行调整"
                };
                CButton cbutton = gameObject.AddComponent<CButton>();
                refers.AddMono(cbutton, "PriorityBtn");
                refers.AddMono(gameObject.GetComponentInChildren<TextMeshProUGUI>(), "Priority");
                GameObject gameObject2 = UIUtils.CreateSingleToggle(rectTransform, "Toggle", "启用", "禁用", Vector2.zero);
                RectTransform component2 = gameObject2.GetComponent<RectTransform>();
                Extentions.SetAnchor(component2, Vector2.up, Vector2.up);
                Extentions.SetPivot(component2, Vector2.up);
                component2.anchoredPosition = new Vector2(75f, -12f);
                refers.AddMono(gameObject2.GetComponent<CToggle>(), "Toggle");
                GameObject gameObject3 = new GameObject("Content");
                RectTransform rectTransform2 = gameObject3.AddComponent<RectTransform>();
                Extentions.SetAnchor(rectTransform2, Vector2.up, Vector2.up);
                Extentions.SetWidth(rectTransform2, 760f);
                Extentions.SetPivot(rectTransform2, Vector2.up);
                rectTransform2.SetParent(rectTransform, false);
                rectTransform2.anchoredPosition = new Vector2(180f, 0f);
                refers.AddMono(rectTransform2, "Content");
                GridLayoutGroup gridLayoutGroup = gameObject3.AddComponent<GridLayoutGroup>();
                gridLayoutGroup.spacing = new Vector2(10f, -10f);
                gridLayoutGroup.startCorner = 0;
                gridLayoutGroup.startAxis = 0;
                gridLayoutGroup.childAlignment = TextAnchor.UpperLeft; //0;
                gridLayoutGroup.constraint = Constraint.FixedColumnCount; //1;
                gridLayoutGroup.constraintCount = 2;
                gridLayoutGroup.cellSize = new Vector2(380f, 60f);
                ContentSizeFitter contentSizeFitter = gameObject3.gameObject.AddComponent<ContentSizeFitter>();
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; //2;
                contentSizeFitter.horizontalFit = 0;
                UIRectSizeController uirectSizeController = gameObject3.AddComponent<UIRectSizeController>();
                List<UIRectSizeController.FollowerConfig> list = new List<UIRectSizeController.FollowerConfig>();
                UIRectSizeController.FollowerConfig item = default(UIRectSizeController.FollowerConfig);
                item.Target = rectTransform;
                item.SizeOffset = new Vector2(200f, 10f);
                list.Add(item);
                uirectSizeController.ControlList = list;
            }
            return Object.Instantiate<GameObject>(UIUtils.strategyObj, parent).transform;
        }

        public static GameObject CreateSliceDownSheetPanel(Transform parent)
        {
            GameObject sliceDownSheet = GameObjectCreationUtils.InstantiateUIElement(parent, "SliceDownSheet");
            GameObject gameObject2 = sliceDownSheet.transform.Find("AutoWidthLablePreset/Label/").gameObject;
            gameObject2.name = "Panel";
            gameObject2.transform.SetParent(sliceDownSheet.transform);
            Object.DestroyImmediate(gameObject2.GetComponent<TextStyle>());
            Object.DestroyImmediate(gameObject2.GetComponent<TextMeshProUGUI>());
            Object.DestroyImmediate(gameObject2.GetComponent<LayoutElement>());
            HorizontalLayoutGroup horizontalLayoutGroup = gameObject2.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childControlHeight = false;
            horizontalLayoutGroup.childControlWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = false;
            horizontalLayoutGroup.childForceExpandWidth = true;
            UIRectSizeController component = gameObject2.GetComponent<UIRectSizeController>();
            component.ControlList.Clear();
            List<UIRectSizeController.FollowerConfig> controlList = component.ControlList;
            UIRectSizeController.FollowerConfig item = default(UIRectSizeController.FollowerConfig);
            item.SizeOffset = new Vector2(60f, 20f);
            item.Target = sliceDownSheet.GetComponent<RectTransform>();
            controlList.Add(item);
            sliceDownSheet.transform.Find("AutoWidthLablePreset").gameObject.SetActive(false);
            Refers component2 = sliceDownSheet.GetComponent<Refers>();
            component2.Names.Clear();
            component2.Objects.Clear();
            CButton confirm = sliceDownSheet.transform.Find("Confirm").GetComponent<CButton>();
            component2.AddMono(confirm, "Confirm");
            CButton component3 = sliceDownSheet.transform.Find("Cancel").GetComponent<CButton>();
            component2.AddMono(component3, "Cancel");

            return sliceDownSheet;
        }

        /// <summary>
        /// 创建变招选择面板
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject CreateChangeTactics(Transform parent)
        {
            GameObject sliceDownSheet = CreateSliceDownSheetPanel(parent);
            GameObject gameObject2 = sliceDownSheet.transform.Find("Panel").gameObject;

            Refers refers = sliceDownSheet.GetComponent<Refers>();
            CButton confirm = sliceDownSheet.transform.Find("Confirm").GetComponent<CButton>();
            CButton component3 = sliceDownSheet.transform.Find("Cancel").GetComponent<CButton>();

            // 招式选择
            GameObject trickOptionsGameObject = GameObjectCreationUtils.InstantiateUIElement(gameObject2.transform, "CommonDropdown");
            Extentions.SetWidth(trickOptionsGameObject.GetComponent<RectTransform>(), 180f);
            CDropdown trickOptions = trickOptionsGameObject.GetComponent<CDropdown>();
            trickOptions.AddOptions(StrategyConst.GetTrickNameList());
            refers.AddMono(trickOptions, "TrickOptions");

            // 击打部位
            GameObject bodyOptionsGameObject = GameObjectCreationUtils.InstantiateUIElement(gameObject2.transform, "CommonDropdown");
            Extentions.SetWidth(bodyOptionsGameObject.GetComponent<RectTransform>(), 180f);
            CDropdown bodyOptions = bodyOptionsGameObject.GetComponent<CDropdown>();
            bodyOptions.AddOptions(StrategyConst.GetBodyPartList());
            refers.AddMono(bodyOptions, "BodyOptions");

            sliceDownSheet.SetActive(false);
            return sliceDownSheet;
        }

        public static GameObject CreateOneValueOptionsPanel(Transform parent)
        {
            GameObject sliceDownSheet = CreateSliceDownSheetPanel(parent);
            GameObject gameObject2 = sliceDownSheet.transform.Find("Panel").gameObject;

            Refers refers = sliceDownSheet.GetComponent<Refers>();
            CButton confirm = sliceDownSheet.transform.Find("Confirm").GetComponent<CButton>();
            CButton cancel = sliceDownSheet.transform.Find("Cancel").GetComponent<CButton>();

            // 招式选择
            GameObject trickOptionsGameObject = GameObjectCreationUtils.InstantiateUIElement(gameObject2.transform, "CommonDropdown");
            Extentions.SetWidth(trickOptionsGameObject.GetComponent<RectTransform>(), 180f);
            CDropdown valueOptions = trickOptionsGameObject.GetComponent<CDropdown>();
            refers.AddMono(valueOptions, "ValueOptions");

            sliceDownSheet.SetActive(false);
            return sliceDownSheet;
        }

        public static GameObject CreateInputTextPanel(Transform parent)
        {
            GameObject sliceDownSheet = CreateSliceDownSheetPanel(parent);
            GameObject gameObject2 = sliceDownSheet.transform.Find("Panel").gameObject;

            Refers refers = sliceDownSheet.GetComponent<Refers>();
            CButton confirm = sliceDownSheet.transform.Find("Confirm").GetComponent<CButton>();
            CButton cancel = sliceDownSheet.transform.Find("Cancel").GetComponent<CButton>();

            var tips = GameObjectCreationUtils.UGUICreateTMPText(gameObject2.transform, new Vector2(0, 0), new Vector2(150, 60), 20f, "");
            refers.AddMono(tips, "Tips");

            var inputField = GameObjectCreationUtils.UGUICreateTMPInputField(gameObject2.transform, new Vector2(0, 0), new Vector2(250, 60), 20f, "InputField", "");
            TextMeshProUGUI placeholder = (TextMeshProUGUI)inputField.placeholder;
            placeholder.text = "";
            refers.AddMono(inputField, "InputField");

            sliceDownSheet.SetActive(false);
            return sliceDownSheet;
        }

        public static void showTips(string title, string content)
        {
            DialogCmd dialogCmd = new DialogCmd
            {
                Title = title,
                Content = content,
                Type = 0
            };
            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", dialogCmd));
            UIManager.Instance.ShowUI(UIElement.Dialog);
        }

        // Token: 0x0400004B RID: 75
        private static readonly Vector2 LeftCenter = new Vector2(0f, 0.5f);

        // Token: 0x0400004C RID: 76
        private static GameObject _ui_ModPanel;

        // Token: 0x0400004D RID: 77
        private static GameObject _ui_SystemSetting;

        // Token: 0x0400004E RID: 78
        private static GameObject sliderBar;

        // Token: 0x0400004F RID: 79
        private static GameObject dropDown;

        // Token: 0x04000050 RID: 80
        private static GameObject toggle;

        // Token: 0x04000051 RID: 81
        private static GameObject singleToggle;

        // Token: 0x04000052 RID: 82
        private static GameObject hotkey;

        // Token: 0x04000053 RID: 83
        private static GameObject emptyContainer;

        // Token: 0x04000054 RID: 84
        private static GameObject rowContainer;

        // Token: 0x04000055 RID: 85
        private static GameObject title;

        // Token: 0x04000056 RID: 86
        private static GameObject subTitle;

        // Token: 0x04000057 RID: 87
        private static GameObject borderBox;

        // Token: 0x04000058 RID: 88
        private static GameObject freeLabel;

        // Token: 0x04000059 RID: 89
        private static GameObject strategyObj;
    }
}
