using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using FrameWork.ModSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace ConvenienceFrontend.ExchangeBook
{
    public class UIBuilder
    {
        public static void PrepareMaterial()
        {
            string assetPath = "RemakeResources/Prefab/Views/UI_ExchangeBook";
            Action<GameObject> onLoad;
            if ((onLoad = UIBuilder.OnPrefabLoaded) == null)
            {
                onLoad = (UIBuilder.onPrefabLoaded = new Action<GameObject>(UIBuilder.OnPrefabLoaded));
            }
            ResLoader.Load<GameObject>(assetPath, onLoad, null);
            string assetPath2 = "RemakeResources/Prefab/Views/Reading/UI_Reading";
            Action<GameObject> onLoad2;
            if ((onLoad2 = UIBuilder.OnPrefabLoaded) == null)
            {
                onLoad2 = (UIBuilder.onPrefabLoaded = new Action<GameObject>(UIBuilder.OnPrefabLoaded));
            }
            ResLoader.Load<GameObject>(assetPath2, onLoad2, null);
        }

        // Token: 0x0600000A RID: 10 RVA: 0x0000243C File Offset: 0x0000063C
        public static GameObject BuildMainUI(string name)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.SetActive(false);
            gameObject.layer = 5;
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.SetAnchor(Vector2.zero, Vector2.one);
            rectTransform.sizeDelta = Vector2.zero;
            gameObject.transform.localScale = Vector3.one;
            Canvas canvas = gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<ConchShipGraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = "UI";
            GameObject gameObject2 = GameObjectCreationUtils.InstantiateUIElement(gameObject.transform, "UIMask");
            gameObject2.GetComponent<RectTransform>().SetAnchor(Vector2.zero, Vector2.one);
            GameObject gameObject3 = new GameObject("MoveIn");
            GameObject gameObject4 = new GameObject("MoveOut");
            gameObject3.transform.SetParent(gameObject.transform);
            gameObject4.transform.SetParent(gameObject.transform);
            GameObject gameObject5 = UnityEngine.Object.Instantiate<GameObject>(UIBuilder.UI_ExchangeBook.transform.Find("MainWindow").gameObject, gameObject.transform);
            gameObject5.name = "MainWindow";
            DOTweenAnimation dotweenAnimation = gameObject3.AddComponent<DOTweenAnimation>();
            dotweenAnimation.name = "MoveIn";
            DOTweenAnimation dotweenAnimation2 = gameObject4.AddComponent<DOTweenAnimation>();
            dotweenAnimation2.name = "MoveOut";
            dotweenAnimation.animationType = DOTweenAnimation.AnimationType.Move;
            dotweenAnimation.endValueV3 = new Vector3(0f, 1500f, 0f);
            dotweenAnimation.duration = 0.2f;
            dotweenAnimation.easeType = Ease.Linear;
            dotweenAnimation2.animationType = DOTweenAnimation.AnimationType.Move;
            dotweenAnimation2.endValueV3 = new Vector3(0f, 1500f, 0f);
            dotweenAnimation2.duration = 0.2f;
            dotweenAnimation2.easeType = Ease.Linear;
            dotweenAnimation.targetIsSelf = (dotweenAnimation2.targetIsSelf = false);
            dotweenAnimation.isRelative = (dotweenAnimation2.isRelative = true);
            dotweenAnimation.isFrom = true;
            dotweenAnimation2.isFrom = false;
            dotweenAnimation.isValid = (dotweenAnimation2.isValid = true);
            dotweenAnimation.target = (dotweenAnimation2.target = gameObject5.GetComponent<RectTransform>());
            dotweenAnimation.targetGO = (dotweenAnimation2.targetGO = gameObject5);
            DOTweenAnimation.TargetType targetType = DOTweenAnimation.TargetType.RectTransform;
            dotweenAnimation2.targetType = DOTweenAnimation.TargetType.RectTransform;
            dotweenAnimation.targetType = targetType;
            dotweenAnimation.autoKill = (dotweenAnimation2.autoKill = false);
            dotweenAnimation.autoPlay = (dotweenAnimation2.autoPlay = false);
            UnityEngine.Object.Destroy(gameObject5.transform.Find("TaiwuBooks/").gameObject);
            UnityEngine.Object.Destroy(gameObject5.transform.Find("ExchangeArea/Self/").gameObject);
            UnityEngine.Object.Destroy(gameObject5.transform.Find("ExchangeArea/Splitter/").gameObject);
            UnityEngine.Object.Destroy(gameObject5.transform.Find("ExchangeArea/NpcPrestige/").gameObject);
            gameObject5.transform.Find("ExchangeArea/Npc/").gameObject.GetComponent<RectTransform>().SetWidth(2160f);
            GameObject gameObject6 = gameObject5.transform.Find("NpcBooks/").gameObject;
            UnityEngine.Object.Destroy(gameObject6.transform.Find("Load/").gameObject);
            RectTransform component = gameObject6.GetComponent<RectTransform>();
            component.SetWidth(component.rect.width * 2f);
            component.anchoredPosition = new Vector2(1100f, 670f);
            gameObject6.transform.Find("NpcItemScroll/").gameObject.GetComponent<RectTransform>().SetWidth(1696f);
            GameObject gameObject7 = UnityEngine.Object.Instantiate<Transform>(UIBuilder.UI_Reading.transform.Find("MainWindow/BookBg/LifeSkillTypeTogGroup/"), gameObject6.transform).gameObject;
            gameObject7.name = "LifeSkillTypeTogGroup";
            RectTransform component2 = gameObject7.GetComponent<RectTransform>();
            component2.SetAnchor(Vector2.up, Vector2.up);
            component2.anchoredPosition = new Vector2(0f, -100f);
            CToggleGroup component3 = gameObject7.GetComponent<CToggleGroup>();
            gameObject7.transform.GetChild(0).gameObject.SetActive(false);
            for (int i = 1; i < gameObject7.transform.childCount; i++)
            {
                gameObject7.transform.GetChild(i).gameObject.SetActive(i <= 14);
            }
            for (int j = 0; j < 14; j++)
            {
                Refers component4 = component3.Get(j).GetComponent<Refers>();
                component4.CGet<TextMeshProUGUI>("Label").text = Config.CombatSkillType.Instance[j].Name;
                string filterCombatSkillTypeIcon = CommonUtils.GetFilterCombatSkillTypeIcon(j);
                component4.CGet<CImage>("Icon").SetSprite(filterCombatSkillTypeIcon, false, null);
            }
            component3.SetAllowOnNum(1);
            component3.AllowSwitchOff = false;
            component3.Set(0, true, false);
            GameObject gameObject8 = UnityEngine.Object.Instantiate<GameObject>(gameObject7, gameObject6.transform).gameObject;
            gameObject8.name = "GradeToggleGroup";
            RectTransform component5 = gameObject8.GetComponent<RectTransform>();
            component5.anchoredPosition = new Vector2(0f, -300f);
            CToggleGroup component6 = gameObject8.GetComponent<CToggleGroup>();
            for (int k = 10; k < 15; k++)
            {
                component6.transform.GetChild(k).gameObject.SetActive(false);
            }
            for (int l = 0; l < 9; l++)
            {
                Refers component7 = component6.Get(l).GetComponent<Refers>();
                component7.CGet<TextMeshProUGUI>("Label").text = (LocalStringManager.Get(string.Format("LK_Num_{0}", 9 - l)) + LocalStringManager.Get("LK_Item_Grade")).SetColor(Colors.Instance.GradeColors[l]);
                component7.CGet<CImage>("Icon").gameObject.SetActive(false);
            }
            gameObject5.transform.Find("NpcBooks/NpcItemScroll/ItemSortAndFilter/Back/").gameObject.GetComponent<RectTransform>().SetWidth(1755f);
            gameObject5.transform.Find("NpcBooks/NpcItemScroll/Viewport/").gameObject.GetComponent<RectTransform>().SetWidth(1696f);
            gameObject5.transform.Find("NpcBooks/NpcItemScroll/Viewport/Content").gameObject.GetComponent<RectTransform>().SetWidth(1678f);
            GameObject gameObject9 = gameObject5.transform.Find("NpcBooks/NpcItemScroll/ItemSortAndFilter/Filter/").gameObject;
            for (int m = 0; m < gameObject9.transform.childCount; m++)
            {
                gameObject9.transform.GetChild(m).gameObject.SetActive(m < 6);
                gameObject9.transform.GetChild(m).gameObject.name = m.ToString();
            }
            GameObject gameObject10 = UnityEngine.Object.Instantiate<GameObject>(gameObject9, gameObject9.transform.parent);
            gameObject10.name = "FirstPageFilter";
            for (int n = 1; n < 6; n++)
            {
                GameObject gameObject11 = gameObject10.transform.GetChild(n).GetChild(1).gameObject;
                UnityEngine.Object.DestroyImmediate(gameObject11.GetComponent<TextLanguage>());
                gameObject11.GetComponent<TextMeshProUGUI>().text = LocalStringManager.Get(string.Format("LK_CombatSkill_First_Page_Type_{0}", n - 1));
            }
            CToggleGroup component8 = gameObject10.GetComponent<CToggleGroup>();
            component8.SetAllowOnNum(1);
            component8.AllowSwitchOff = false;
            GameObject gameObject12 = UnityEngine.Object.Instantiate<GameObject>(gameObject10, gameObject9.transform.parent);
            gameObject12.name = "DirectPageFilter";
            gameObject12.GetComponent<RectTransform>().anchoredPosition = new Vector2(400f, 0f);
            gameObject12.transform.GetChild(0).gameObject.SetActive(false);
            for (int num = 0; num < 5; num++)
            {
                GameObject gameObject13 = gameObject12.transform.GetChild(num + 1).GetChild(1).gameObject;
                gameObject13.GetComponent<TextMeshProUGUI>().text = LocalStringManager.Get(string.Format("LK_CombatSkill_Direct_Page_{0}", num));
            }
            CToggleGroup component9 = gameObject12.GetComponent<CToggleGroup>();
            component9.SetAllowOnNum(5);
            component9.AllowSwitchOff = true;
            component9.AllowUncheck = true;
            GameObject gameObject14 = UnityEngine.Object.Instantiate<GameObject>(gameObject12, gameObject9.transform.parent);
            gameObject14.name = "ReversePageFilter";
            gameObject14.GetComponent<RectTransform>().anchoredPosition = new Vector2(720f, 0f);
            for (int num2 = 0; num2 < 5; num2++)
            {
                GameObject gameObject15 = gameObject14.transform.GetChild(num2 + 1).GetChild(1).gameObject;
                gameObject15.GetComponent<TextMeshProUGUI>().text = LocalStringManager.Get(string.Format("LK_CombatSkill_Reverse_Page_{0}", num2));
            }
            GameObject gameObject16 = GameObjectCreationUtils.InstantiateUIElement(gameObject9.transform.parent, "AutoBackLabel");
            RectTransform component10 = gameObject16.GetComponent<RectTransform>();
            component10.SetAnchor(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            component10.anchoredPosition = new Vector2(1050f, 0f);
            Transform child = component10.GetChild(1);
            child.GetComponent<LayoutElement>().minWidth = 50f;
            child.GetComponent<TextMeshProUGUI>().text = "完整页";
            GameObject gameObject17 = UnityEngine.Object.Instantiate<GameObject>(gameObject12, gameObject9.transform.parent);
            gameObject17.name = "CompletePageFilter";
            gameObject17.GetComponent<RectTransform>().anchoredPosition = new Vector2(1100f, 0f);
            for (int num3 = 1; num3 < 6; num3++)
            {
                GameObject gameObject18 = gameObject17.transform.GetChild(num3).GetChild(1).gameObject;
                gameObject18.GetComponent<TextMeshProUGUI>().text = LocalStringManager.Get(string.Format("LK_Num_{0}", num3));
            }
            ItemScrollView component11 = gameObject5.transform.Find("NpcBooks/NpcItemScroll/").gameObject.GetComponent<ItemScrollView>();
            component11.DetailViewLineCount = 6;
            component11.SimpleViewLineCount = 10;
            InfinityScroll componentInChildren = component11.GetComponentInChildren<InfinityScroll>();
            componentInChildren.LineCount = 6;
            componentInChildren.InitPageCount();
            gameObject9.SetActive(false);
            return gameObject;
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00002EAC File Offset: 0x000010AC
        [CompilerGenerated]
        internal static void OnPrefabLoaded(GameObject obj)
		{
			bool flag = obj.name == "UI_ExchangeBook";
			if (flag)
			{
				UIBuilder.UI_ExchangeBook = obj;
			}
			else
			{
				bool flag2 = obj.name == "UI_Reading";
				if (flag2)
				{
					UIBuilder.UI_Reading = obj;
				}
			}
		}

		// Token: 0x04000006 RID: 6
		private static GameObject UI_ExchangeBook;

        // Token: 0x04000007 RID: 7
        public static GameObject UI_Reading;

        public static Action<GameObject> onPrefabLoaded;
    }
}
