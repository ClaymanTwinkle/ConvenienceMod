using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

namespace ConvenienceFrontend.MergeBookPanel
{
    public class ModMono : MonoBehaviour
    {
        // Token: 0x0600000F RID: 15 RVA: 0x00002448 File Offset: 0x00000648
        private void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
            this.PrepareRes();
            GEvent.Add(UiEvents.OnUIElementShow, new GEvent.Callback(this.OnCharacterMenuShow));
            GEvent.Add(UiEvents.OnUIElementHide, new GEvent.Callback(this.OnCharacterMenuHide));
            GameLog.LogMessage("MergeBookPanel");
        }

        // Token: 0x06000010 RID: 16 RVA: 0x0000249E File Offset: 0x0000069E
        private void OnDestroy()
        {
            GEvent.Remove(UiEvents.OnUIElementShow, new GEvent.Callback(this.OnCharacterMenuShow));
            GEvent.Remove(UiEvents.OnUIElementHide, new GEvent.Callback(this.OnCharacterMenuHide));
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000024D0 File Offset: 0x000006D0
        private void Update()
        {
            if (ModMono.MergeBooks == null && this.PrepareDone())
            {
                ModMono.MergeBooks = UnityEngine.Object.Instantiate<GameObject>(ModMono.pagePrefab, base.transform, false).AddComponent<UI_MergeBooks>();
                ModMono.MergeBooks.name = "MergePage";
                this.CreateMergeBookUI();
            }
            if (!ModMono.mergePageMounted)
            {
                GameObject itemSubpage = GameObject.Find("LayerPopUp/UI_CharacterMenuItems/ElementsRoot");
                if (itemSubpage)
                {
                    ModMono.MergeBooks.transform.SetParent(itemSubpage.transform);
                    ModMono.MergeBooks.transform.localPosition = new Vector3(-16f, 890f, 0f);
                    ModMono.MergeBooks.transform.localScale = Vector3.one;
                    ModMono.mergePageMounted = true;
                }
            }
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00002590 File Offset: 0x00000790
        private bool PrepareDone()
        {
            return ModMono.itemHolderPrefab != null && ModMono.pagePrefab != null && ModMono.scrollBarPrefab != null && ModMono.listBackPrefab != null && ModMono.bookPagePrefab != null && ModMono.buttonPrefab != null && ModMono.bookNamePrefab != null && ModMono.bookIconPrefab != null && ModMono.bookDescPrefab != null && ModMono.skillTypeTogGroupPrefab != null && ModMono.skillSubTypeTogPrefab != null && ModMono.lifeSkillDetailPrefab != null;
        }

        // Token: 0x06000013 RID: 19 RVA: 0x00002640 File Offset: 0x00000840
        private void PrepareRes()
        {
            ResLoader.Load<GameObject>(Path.Combine(ModMono.rootPrefabPath, "UI_CombatResult"), delegate (GameObject obj)
            {
                GameLog.LogMessage("ResLoader.Load UI_CombatResult");

                if (obj != null)
                {
                    UIBase ui = obj.GetComponent<UIBase>();
                    if (ui != null)
                    {
                        ModMono.itemHolderPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<RectTransform>("ItemHolder").gameObject, base.transform, false);
                        ModMono.itemHolderPrefab.name = "ItemHolderPrefab";
                    }
                }
            }, null);
            ResLoader.Load<GameObject>(Path.Combine(ModMono.rootPrefabPath, "CharacterMenu/UI_CharacterMenuLifeSkill"), delegate (GameObject obj)
            {
                GameLog.LogMessage("ResLoader.Load CharacterMenu/UI_CharacterMenuLifeSkill");

                if (obj != null)
                {
                    UIBase ui = obj.GetComponent<UIBase>();
                    if (ui != null)
                    {
                        ModMono.lifeSkillDetailPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<Refers>("Detail").gameObject, base.transform, false);
                        ModMono.lifeSkillDetailPrefab.name = "LifeSkillDetailPrefab";
                        RectTransform skillHolder = ModMono.lifeSkillDetailPrefab.GetComponent<Refers>().CGet<RectTransform>("SkillHolder");
                        for (int i = 0; i < skillHolder.childCount; i++)
                        {
                            Refers component = skillHolder.GetChild(i).GetComponent<Refers>();
                            UnityEngine.Object.Destroy(component.CGet<TextMeshProUGUI>("AddAttainment").gameObject);
                            component.CGet<TextMeshProUGUI>("Name").transform.localPosition = new Vector3(90f, 0f, 0f);
                            component.CGet<TextMeshProUGUI>("Name").fontSize = 23f;
                            RectTransform pageHolder = component.CGet<RectTransform>("PageHolder");
                            for (int j = 0; j < pageHolder.childCount; j++)
                            {
                                Refers pageRefers = pageHolder.GetChild(j).GetComponent<Refers>();
                                pageRefers.GetComponent<RectTransform>().sizeDelta = new Vector2(105f, 82f);
                                UnityEngine.Object.Destroy(pageRefers.CGet<GameObject>("ReadProgress").gameObject);
                                UnityEngine.Object.Destroy(pageRefers.CGet<GameObject>("NoUnlock").gameObject);
                                Transform transform = pageRefers.CGet<CanvasGroup>("UnlockInfo").transform;
                                transform.parent.localPosition = new Vector2(130f, 0f);
                                UnityEngine.Object.Destroy(transform.GetChild(1).gameObject);
                                UnityEngine.Object.Destroy(transform.GetChild(2).gameObject);
                                UnityEngine.Object.Destroy(transform.GetChild(3).gameObject);
                                pageRefers.CGet<GameObject>("NotRead").GetComponent<RectTransform>().sizeDelta = new Vector2(105f, 82f);
                                if (ModMono.lockPrefab == null)
                                {
                                    ModMono.lockPrefab = UnityEngine.Object.Instantiate<GameObject>(pageRefers.CGet<GameObject>("NotRead").transform.GetChild(0).gameObject, base.transform, false);
                                }
                            }
                        }
                    }
                }
            }, null);
            ResLoader.Load<GameObject>(Path.Combine(ModMono.rootPrefabPath, "CharacterMenu/UI_CharacterMenuItems"), delegate (GameObject obj)
            {
                GameLog.LogMessage("ResLoader.Load CharacterMenu/UI_CharacterMenuItems");

                if (obj != null)
                {
                    UIBase ui = obj.GetComponent<UIBase>();
                    if (ui != null)
                    {
                        ModMono.pagePrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<Refers>("KidnapPage").gameObject, base.transform);
                        ModMono.pagePrefab.name = "PagePrefab";
                        UnityEngine.Object.Destroy(ModMono.pagePrefab.transform.Find("KidnapSlotScroll").gameObject);
                        UnityEngine.Object.Destroy(ModMono.pagePrefab.GetComponent<Refers>());
                        ModMono.scrollBarPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<Refers>("ItemPage").transform.Find("ItemScrollView/VerticalScrollbar").gameObject, base.transform);
                        ModMono.scrollBarPrefab.name = "ScrollBarPrefab";
                        ModMono.listBackPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<Refers>("ItemPage").transform.Find("ItemListBack").gameObject, base.transform);
                        ModMono.listBackPrefab.name = "BookListBack";
                        ModMono.multiOptButtonPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<Refers>("ItemPage").CGet<CButton>("BtnMultiplySelect").gameObject, base.transform);
                        UnityEngine.Object.Destroy(ModMono.multiOptButtonPrefab.transform.Find("Disabled").gameObject);
                        UnityEngine.Object.Destroy(ModMono.multiOptButtonPrefab.transform.Find("Normal/Text").GetComponent<TextLanguage>());
                    }
                }
            }, null);
            ResLoader.Load<GameObject>(Path.Combine(ModMono.rootPrefabPath, "MouseTip/UI_MouseTipBook"), delegate (GameObject obj)
            {
                GameLog.LogMessage("ResLoader.Load MouseTip/UI_MouseTipBook");

                if (obj != null)
                {
                    UIBase ui = obj.GetComponent<UIBase>();
                    if (ui != null)
                    {
                        GameLog.LogMessage("Instantiate LifeSkill");
                        ModMono.bookPagePrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<GameObject>("LifeSkill"), base.transform, false);
                        ModMono.bookPagePrefab.name = "BookPagePrefab";
                        GameObject pageName = ui.CGet<RectTransform>("CombatSkillPageHolder").Find("Page0/PageName").gameObject;
                        Transform pageHolder = ModMono.bookPagePrefab.transform.Find("LifeSkillPageHolder");
                        pageHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(355f, 155f);
                        for (int i = 0; i < pageHolder.childCount; i++)
                        {
                            GameLog.LogMessage("Instantiate pageName");
                            UnityEngine.Object.Instantiate<GameObject>(pageName, pageHolder.GetChild(i), false).name = "PageName";
                            GameLog.LogMessage("Instantiate SelectMark");
                            UnityEngine.Object.Instantiate<Transform>(ModMono.itemHolderPrefab.transform.GetChild(0).Find("SelectStatus"), pageHolder.GetChild(i), false).name = "SelectMark"; // Camera_UIRoot/Canvas/LayerPart/UI_CombatResult/AnimationRoot/MainWindow/ResultScroll/Viewport/Content/ItemHolder/ItemView/SelectStatus
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ModMono.lockPrefab, pageHolder.GetChild(i), false);
                            gameObject.name = "Lock";
                            gameObject.transform.localPosition = new Vector3(0f, -15f, 0f);
                            gameObject.GetComponent<CImage>().enabled = true;
                            gameObject.SetActive(false);
                            UnityEngine.Object.Destroy(pageHolder.GetChild(i).Find("PageIndex").GetComponent<TextLanguage>());
                            UnityEngine.Object.Destroy(pageHolder.GetChild(i).Find("IncompleteTips").GetComponent<TextLanguage>());
                            pageHolder.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(70f, 155f);
                            pageHolder.GetChild(i).Find("PageIndex").localPosition = new Vector3(-19f, 47f, 0f);
                            pageHolder.GetChild(i).Find("CompleteTips").localPosition = new Vector3(0f, 0f, 0f);
                            pageHolder.GetChild(i).Find("IncompleteTips").localPosition = new Vector3(0f, 0f, 0f);
                            pageHolder.GetChild(i).Find("LostTips").localPosition = new Vector3(0f, 0f, 0f);
                            pageHolder.GetChild(i).Find("ReadedTips").localPosition = new Vector3(0f, -30f, 0f);
                            pageHolder.GetChild(i).Find("NotReadTips").localPosition = new Vector3(0f, -30f, 0f);
                            pageHolder.GetChild(i).Find("ProgressComplete").localPosition = new Vector3(0f, -58f, 0f);
                            pageHolder.GetChild(i).Find("ProgressIncomplete").localPosition = new Vector3(0f, -58f, 0f);
                        }
                        TextMeshProUGUI[] componentsInChildren = ModMono.bookPagePrefab.GetComponentsInChildren<TextMeshProUGUI>();
                        for (int j = 0; j < componentsInChildren.Length; j++)
                        {
                            componentsInChildren[j].fontSize = 20f;
                        }
                        pageHolder.GetComponent<HorizontalLayoutGroup>().enabled = true;
                        GameLog.LogMessage("Instantiate Name");
                        ModMono.bookNamePrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<TextMeshProUGUI>("Name").gameObject, base.transform, false);
                        ModMono.bookNamePrefab.name = "BookNamePrefab";
                        GameLog.LogMessage("Instantiate ItemIcon");
                        ModMono.bookIconPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<CImage>("ItemIcon").gameObject, base.transform, false);
                        ModMono.bookIconPrefab.name = "BookIconPrefab";
                        GameLog.LogMessage("Instantiate Desc");
                        ModMono.bookDescPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<TextMeshProUGUI>("Desc").gameObject, base.transform, false);
                        ModMono.bookDescPrefab.name = "BookDescPrefab";
                        ModMono.bookDescPrefab.GetComponent<TextMeshProUGUI>().fontSize = 20f;
                    }
                }
            }, null);
            ResLoader.Load<GameObject>(Path.Combine(ModMono.rootPrefabPath, "CharacterMenu/UI_CharacterMenuInfo"), delegate (GameObject obj)
            {
                GameLog.LogMessage("ResLoader.Load CharacterMenu/UI_CharacterMenuInfo");

                if (obj != null)
                {
                    UIBase ui = obj.GetComponent<UIBase>();
                    if (ui != null)
                    {
                        ModMono.buttonPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<CButton>("TalkBtn").gameObject, base.transform, false);
                        ModMono.buttonPrefab.name = "ButtonPrefab";
                        UnityEngine.Object.Destroy(ModMono.buttonPrefab.transform.Find("Icon").gameObject);
                        UnityEngine.Object.Destroy(ModMono.buttonPrefab.transform.Find("LabelDisable").gameObject);
                        UnityEngine.Object.Destroy(ModMono.buttonPrefab.GetComponent<Refers>());
                        Transform transform = ModMono.buttonPrefab.transform.Find("Label");
                        UnityEngine.Object.Destroy(transform.GetComponent<TextLanguage>());
                        transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        transform.GetComponent<TextMeshProUGUI>().text = "合并";
                    }
                }
            }, null);
            ResLoader.Load<GameObject>(Path.Combine(ModMono.rootPrefabPath, "Reading/UI_Reading"), delegate (GameObject obj)
            {
                GameLog.LogMessage("ResLoader.Load Reading/UI_Reading");
                if (obj != null)
                {
                    UIBase ui = obj.GetComponent<UIBase>();
                    if (ui != null)
                    {
                        ModMono.skillTypeTogGroupPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<CToggleGroup>("SkillTypeTogGroup").gameObject, base.transform, false);
                        ModMono.skillTypeTogGroupPrefab.name = "SkillTypeTogGroupPrefab";
                        ModMono.skillSubTypeTogPrefab = UnityEngine.Object.Instantiate<GameObject>(ui.CGet<CToggleGroup>("SkillSubTypeTogGroup").GetAll()[0].gameObject, base.transform, false);
                        ModMono.skillSubTypeTogPrefab.name = "SkillSubTypeTogPrefab";
                        UnityEngine.Object.Destroy(ModMono.skillSubTypeTogPrefab.transform.Find("Icon").gameObject);
                        ModMono.skillSubTypeTogPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 40f);
                        UnityEngine.Object.Destroy(ModMono.skillTypeTogGroupPrefab.transform.GetChild(0).Find("Label").GetComponent<TextLanguage>());
                        UnityEngine.Object.Destroy(ModMono.skillTypeTogGroupPrefab.transform.GetChild(1).Find("Label").GetComponent<TextLanguage>());
                    }
                }
            }, null);
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00002714 File Offset: 0x00000914
        private void CreateMergeBookUI()
        {
            ModMono.MergeBooks.Element = new UIElement
            {
                Id = ModMono.elementID
            };
            ModMono.MergeBooks.Element.SetPrivateField("_path", "UI_MergeBooks");
            RectTransform listBack = UnityEngine.Object.Instantiate<GameObject>(ModMono.listBackPrefab, ModMono.MergeBooks.transform).GetComponent<RectTransform>();
            listBack.gameObject.name = "BookListBack";
            listBack.anchoredPosition = Vector2.zero;
            this.SetAnchorAndOffset(listBack, new Vector2(-760f, -830f), new Vector2(410f, 30f));
            GameObject scrollViewPort = new GameObject("ScrollViewPort");
            scrollViewPort.AddComponent<RectMask2D>();
            RectTransform tempTransform = scrollViewPort.GetComponent<RectTransform>();
            this.SetAnchorAndOffset(tempTransform, new Vector2(-750f, -800f), new Vector2(350f, 0f));
            tempTransform.SetParent(ModMono.MergeBooks.transform, false);
            ModMono.MergeBooks.itemHolder = UnityEngine.Object.Instantiate<GameObject>(ModMono.itemHolderPrefab, scrollViewPort.transform);
            ModMono.MergeBooks.itemHolder.name = "ItemHolder";
            ModMono.MergeBooks.itemHolder.transform.localPosition = Vector3.zero;
            UnityEngine.Object.Destroy(ModMono.MergeBooks.itemHolder.GetComponent<GridLayoutGroup>());
            tempTransform = ModMono.MergeBooks.itemHolder.GetComponent<RectTransform>();
            tempTransform.anchoredPosition = Vector2.zero;
            this.SetAnchorAndOffset(tempTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-560f, -800f), new Vector2(590f, 0f));
            RectTransform scrollBar = UnityEngine.Object.Instantiate<GameObject>(ModMono.scrollBarPrefab, ModMono.MergeBooks.transform).GetComponent<RectTransform>();
            scrollBar.gameObject.name = "ScrollBar";
            this.SetAnchorAndOffset(scrollBar, new Vector2(365f, -800f), new Vector2(373f, 0f));
            ModMono.MergeBooks.scrollView = ModMono.MergeBooks.gameObject.AddComponent<CScrollRect>();
            ModMono.MergeBooks.scrollView.Viewport = scrollViewPort.GetComponent<RectTransform>();
            ModMono.MergeBooks.scrollView.Content = ModMono.MergeBooks.itemHolder.GetComponent<RectTransform>();
            ModMono.MergeBooks.scrollView.ScrollBar = scrollBar.GetComponent<CScrollbar>();
            ModMono.MergeBooks.scrollView.Direction = CScrollRect.ScrollDirection.Vertical;
            ModMono.MergeBooks.scrollView.ScrollSpeed = 50f;
            ModMono.MergeBooks.scrollView.DampedCoefficient = 0.15f;
            ModMono.MergeBooks.scrollView.AdjustSpeed = 50f;
            ModMono.MergeBooks.scrollView.AutoBindTriggerElement = false;
            PointerTrigger component = ModMono.MergeBooks.gameObject.GetComponent<PointerTrigger>();
            component.EnterEvent = new UnityEvent();
            component.ExitEvent = new UnityEvent();
            Transform itemPrefab = ModMono.MergeBooks.itemHolder.transform.GetChild(0);
            itemPrefab.SetParent(scrollViewPort.transform, false);
            ModMono.MergeBooks.infinityScroll = ModMono.MergeBooks.gameObject.AddComponent<InfinityScroll>();
            ModMono.MergeBooks.infinityScroll.SrcPrefab = itemPrefab.GetComponent<ItemView>();
            ModMono.MergeBooks.infinityScroll.LineCount = 9;
            ModMono.MergeBooks.infinityScroll.Padding = new Vector2(40f, 10f);
            ModMono.MergeBooks.infinityScroll.Gap = new Vector2(4f, 4f);
            ModMono.MergeBooks.infinityScroll.InitPageCount();
            RectTransform pageBack = UnityEngine.Object.Instantiate<GameObject>(ModMono.listBackPrefab, ModMono.MergeBooks.transform).GetComponent<RectTransform>();
            pageBack.gameObject.name = "BookPageBack";
            pageBack.anchoredPosition = Vector2.zero;
            this.SetAnchorAndOffset(pageBack, new Vector2(400f, -830f), new Vector2(800f, 30f));
            ModMono.MergeBooks.bookName = UnityEngine.Object.Instantiate<GameObject>(ModMono.bookNamePrefab, ModMono.MergeBooks.transform, false).GetComponent<TextMeshProUGUI>();
            ModMono.MergeBooks.bookName.name = "BookName";
            ModMono.MergeBooks.bookName.transform.localPosition = new Vector3(650f, -20f, 0f);
            ModMono.MergeBooks.bookDesc = UnityEngine.Object.Instantiate<GameObject>(ModMono.bookDescPrefab, ModMono.MergeBooks.transform, false).GetComponent<TextMeshProUGUI>();
            ModMono.MergeBooks.bookDesc.name = "BookDesc";
            ModMono.MergeBooks.bookDesc.transform.localPosition = new Vector3(440f, -80f, 0f);
            ModMono.MergeBooks.bookDesc.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 100f);
            ModMono.MergeBooks.bookIcon = UnityEngine.Object.Instantiate<GameObject>(ModMono.bookIconPrefab, ModMono.MergeBooks.transform, false).GetComponent<CImage>();
            ModMono.MergeBooks.bookIcon.name = "BookIcon";
            ModMono.MergeBooks.bookIcon.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            ModMono.MergeBooks.bookIcon.transform.localPosition = new Vector3(480f, -35f, 0f);
            GameObject mergeResult = new GameObject("MergeResult");
            VerticalLayoutGroup verticalLayoutGroup = mergeResult.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.childForceExpandWidth = false;
            verticalLayoutGroup.spacing = 10f;
            tempTransform = mergeResult.GetComponent<RectTransform>();
            tempTransform.SetParent(ModMono.MergeBooks.transform, false);
            this.SetAnchorAndOffset(tempTransform, new Vector2(410f, -740f), new Vector2(790f, -260f));
            ModMono.MergeBooks.bookPageOutline = UnityEngine.Object.Instantiate<GameObject>(ModMono.bookPagePrefab, mergeResult.transform, false);
            ModMono.MergeBooks.bookPageOutline.name = "BookPageOutLine";
            tempTransform = ModMono.MergeBooks.bookPageOutline.GetComponent<RectTransform>();
            this.SetAnchorAndOffset(tempTransform, new Vector2(10f, -150f), new Vector2(370f, 0f));
            ModMono.MergeBooks.bookPageDirect = UnityEngine.Object.Instantiate<GameObject>(ModMono.MergeBooks.bookPageOutline, mergeResult.transform, false);
            ModMono.MergeBooks.bookPageDirect.name = "BookPageDirect";
            ModMono.MergeBooks.bookPageReverse = UnityEngine.Object.Instantiate<GameObject>(ModMono.MergeBooks.bookPageOutline, mergeResult.transform, false);
            ModMono.MergeBooks.bookPageReverse.name = "BookPageReverse";
            ModMono.MergeBooks.bookPageLifeSkill = UnityEngine.Object.Instantiate<GameObject>(ModMono.MergeBooks.bookPageOutline, mergeResult.transform, false);
            ModMono.MergeBooks.bookPageLifeSkill.name = "BookPageLifeSkill";
            Transform pageHolder = ModMono.MergeBooks.bookPageOutline.transform.GetChild(0);
            for (int i = 0; i < pageHolder.childCount; i++)
            {
                pageHolder.GetChild(i).Find("PageIndex").GetComponent<TextMeshProUGUI>().text = "总纲";
                pageHolder.GetChild(i).Find("PageName").GetComponent<TextMeshProUGUI>().text = this.OutlinePageName[i];
                pageHolder.GetChild(i).Find("IncompleteTips").GetComponent<TextMeshProUGUI>().text = "×残缺";
                pageHolder.GetChild(i).gameObject.AddComponent<CButton>();
                PageView pageView = pageHolder.GetChild(i).gameObject.AddComponent<PageView>();
                pageHolder.GetChild(i).gameObject.AddComponent<CEmptyGraphic>();
                Refers refers = pageHolder.GetChild(i).GetComponent<Refers>();
                GameObject selectMark = pageHolder.GetChild(i).Find("SelectMark").gameObject;
                pageView.Objects = refers.Objects;
                pageView.Names = refers.Names;
                pageView.AddMono(selectMark, "SelectMark");
                pageView.AddMono(pageHolder.GetChild(i).Find("Lock").gameObject, "Lock");
            }
            pageHolder = ModMono.MergeBooks.bookPageDirect.transform.GetChild(0);
            for (int j = 0; j < pageHolder.childCount; j++)
            {
                pageHolder.GetChild(j).Find("PageName").GetComponent<TextMeshProUGUI>().text = this.DirectPageName[j];
                pageHolder.GetChild(j).Find("PageName").GetComponent<TextMeshProUGUI>().color = new Color(0.6235f, 0.8784f, 0.8627f, 1f);
                pageHolder.GetChild(j).Find("IncompleteTips").GetComponent<TextMeshProUGUI>().text = "×残缺";
                pageHolder.GetChild(j).gameObject.AddComponent<CButton>();
                pageHolder.GetChild(j).gameObject.AddComponent<CEmptyGraphic>();
                PageView pageView2 = pageHolder.GetChild(j).gameObject.AddComponent<PageView>();
                Refers refers2 = pageHolder.GetChild(j).GetComponent<Refers>();
                GameObject selectMark2 = pageHolder.GetChild(j).Find("SelectMark").gameObject;
                pageView2.Objects = refers2.Objects;
                pageView2.Names = refers2.Names;
                pageView2.AddMono(selectMark2, "SelectMark");
                pageView2.AddMono(pageHolder.GetChild(j).Find("Lock").gameObject, "Lock");
            }
            pageHolder = ModMono.MergeBooks.bookPageReverse.transform.GetChild(0);
            for (int k = 0; k < pageHolder.childCount; k++)
            {
                pageHolder.GetChild(k).Find("PageName").GetComponent<TextMeshProUGUI>().text = this.ReversePageName[k];
                pageHolder.GetChild(k).Find("PageName").GetComponent<TextMeshProUGUI>().color = new Color(0.7569f, 0.4118f, 0.1529f, 1f);
                pageHolder.GetChild(k).Find("IncompleteTips").GetComponent<TextMeshProUGUI>().text = "×残缺";
                pageHolder.GetChild(k).gameObject.AddComponent<CButton>();
                pageHolder.GetChild(k).gameObject.AddComponent<CEmptyGraphic>();
                PageView pageView3 = pageHolder.GetChild(k).gameObject.AddComponent<PageView>();
                Refers refers3 = pageHolder.GetChild(k).GetComponent<Refers>();
                GameObject selectMark3 = pageHolder.GetChild(k).Find("SelectMark").gameObject;
                pageView3.Objects = refers3.Objects;
                pageView3.Names = refers3.Names;
                pageView3.AddMono(selectMark3, "SelectMark");
                pageView3.AddMono(pageHolder.GetChild(k).Find("Lock").gameObject, "Lock");
            }
            pageHolder = ModMono.MergeBooks.bookPageLifeSkill.transform.GetChild(0);
            for (int l = 0; l < pageHolder.childCount; l++)
            {
                UnityEngine.Object.Destroy(pageHolder.GetChild(l).Find("PageName").gameObject);
                UnityEngine.Object.Destroy(pageHolder.GetChild(l).Find("SelectMark").gameObject);
                pageHolder.GetChild(l).Find("IncompleteTips").GetComponent<TextMeshProUGUI>().text = "×残缺";
                PageView pageView4 = pageHolder.GetChild(l).gameObject.AddComponent<PageView>();
                Refers refers4 = pageHolder.GetChild(l).GetComponent<Refers>();
                pageView4.Objects = refers4.Objects;
                pageView4.Names = refers4.Names;
                pageView4.AddMono(pageHolder.GetChild(l).Find("Lock").gameObject, "Lock");
            }
            ModMono.MergeBooks.buttonMerge = UnityEngine.Object.Instantiate<GameObject>(ModMono.buttonPrefab, ModMono.MergeBooks.transform, false).GetComponent<CButton>();
            ModMono.MergeBooks.buttonMerge.name = "MergeButton";
            ModMono.MergeBooks.buttonMerge.transform.localPosition = new Vector3(712f, -770f, 0f);
            ModMono.MergeBooks.buttonMerge.GetComponent<RectTransform>().sizeDelta = new Vector2(110f, 46f);
            ModMono.MergeBooks.buttonTransform = UnityEngine.Object.Instantiate<GameObject>(ModMono.buttonPrefab, ModMono.MergeBooks.transform, false).GetComponent<CButton>();
            ModMono.MergeBooks.buttonTransform.name = "TransformButton";
            ModMono.MergeBooks.buttonTransform.transform.localPosition = new Vector3(712f, -770f, 0f);
            ModMono.MergeBooks.buttonTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(110f, 46f);
            ModMono.MergeBooks.buttonTransform.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = "编撰";
            ModMono.MergeBooks.buttonSplit = UnityEngine.Object.Instantiate<GameObject>(ModMono.buttonPrefab, ModMono.MergeBooks.transform, false).GetComponent<CButton>();
            ModMono.MergeBooks.buttonSplit.name = "SplitButton";
            ModMono.MergeBooks.buttonSplit.transform.localPosition = new Vector3(475f, -770f, 0f);
            ModMono.MergeBooks.buttonSplit.GetComponent<RectTransform>().sizeDelta = new Vector2(110f, 46f);
            ModMono.MergeBooks.buttonSplit.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = "拆分";
            ModMono.MergeBooks.buttonSplit.gameObject.SetActive(false);
            ModMono.MergeBooks.skillTypeTogGroup = UnityEngine.Object.Instantiate<GameObject>(ModMono.skillTypeTogGroupPrefab, ModMono.MergeBooks.transform, false).GetComponent<CToggleGroup>();
            ModMono.MergeBooks.skillTypeTogGroup.name = "SkillTypeTogGroup";
            ModMono.MergeBooks.skillTypeTogGroup.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 38f);
            ModMono.MergeBooks.skillTypeTogGroup.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 38f);
            ModMono.MergeBooks.skillTypeTogGroup.transform.localPosition = new Vector3(-620f, 115f, 0f);
            ModMono.MergeBooks.skillSubTypeTogGroup = new GameObject("SkillSubTypeTogGroup").AddComponent<CToggleGroup>();
            ModMono.MergeBooks.skillSubTypeTogGroup.name = "SkillSubTypeTogGroup";
            tempTransform = ModMono.MergeBooks.skillSubTypeTogGroup.gameObject.AddComponent<RectTransform>();
            tempTransform.SetParent(ModMono.MergeBooks.transform, false);
            tempTransform.pivot = Vector2.zero;
            tempTransform.sizeDelta = new Vector2(1360f, 40f);
            tempTransform.localPosition = new Vector3(-750f, 30f, 0f);
            HorizontalLayoutGroup horizontalLayoutGroup = ModMono.MergeBooks.skillSubTypeTogGroup.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childControlHeight = false;
            horizontalLayoutGroup.childControlWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = false;
            horizontalLayoutGroup.childForceExpandWidth = false;
            for (int m = 0; m < 17; m++)
            {
                UnityEngine.Object.Instantiate<GameObject>(ModMono.skillSubTypeTogPrefab, ModMono.MergeBooks.skillSubTypeTogGroup.transform, false).name = string.Format("SkillSubTypeTog{0}", m);
            }
            ModMono.MergeBooks.operationTypeTogGroup = UnityEngine.Object.Instantiate<GameObject>(ModMono.skillTypeTogGroupPrefab, ModMono.MergeBooks.transform, false).GetComponent<CToggleGroup>();
            ModMono.MergeBooks.operationTypeTogGroup.name = "OperationTypeTogGroup";
            Transform child = ModMono.MergeBooks.operationTypeTogGroup.transform.GetChild(0);
            Transform togTranform = ModMono.MergeBooks.operationTypeTogGroup.transform.GetChild(1);
            child.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 38f);
            child.Find("Label").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            child.Find("Label").GetComponent<TextMeshProUGUI>().text = "合并";
            UnityEngine.Object.Destroy(child.Find("Icon").gameObject);
            togTranform.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 38f);
            togTranform.Find("Label").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            togTranform.Find("Label").GetComponent<TextMeshProUGUI>().text = "编撰";
            UnityEngine.Object.Destroy(togTranform.Find("Icon").gameObject);
            ModMono.MergeBooks.operationTypeTogGroup.transform.localPosition = new Vector3(650f, -38f, 0f);
        }

        // Token: 0x06000015 RID: 21 RVA: 0x000037CD File Offset: 0x000019CD
        private void CreateLibraryUI()
        {
        }

        // Token: 0x06000016 RID: 22 RVA: 0x000037D0 File Offset: 0x000019D0
        private void CreateBottomButton()
        {
            UI_CharacterMenuItems characterMenuItem = UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>();
            ModMono.mergeToolsButton = UnityEngine.Object.Instantiate<GameObject>(ModMono.multiOptButtonPrefab, characterMenuItem.CGet<Refers>("ItemPage").transform.Find("BottomBar"), false).GetComponent<CButton>();
            ModMono.mergeToolsButton.name = "MergeToolsButton";
            ModMono.mergeToolsButton.transform.localPosition = new Vector3(-312f, 34f, 240f);
            ModMono.mergeToolsButton.transform.Find("Normal/Image").GetComponent<CImage>().SetSprite("sp_icon_jiyi_11", false, null);
            ModMono.mergeToolsButton.transform.Find("Normal/Text").GetComponent<TextMeshProUGUI>().text = "合并工具";
            ModMono.mergeToolsButton.interactable = true;
            ModMono.mergeToolsButton.ClearAndAddListener(delegate
            {
                GameLog.LogMessage("Merger All Tools");
                if (!MergeAll.isMerging)
                {
                    MergeAll.MergeAllTools();
                }
            });
            // 没有值
            ModMono.mergeLifeBooksButton = UnityEngine.Object.Instantiate<GameObject>(ModMono.multiOptButtonPrefab, characterMenuItem.CGet<Refers>("ItemPage").transform.Find("BottomBar"), false).GetComponent<CButton>();
            ModMono.mergeLifeBooksButton.name = "MergeLifeBooksButton";
            ModMono.mergeLifeBooksButton.transform.localPosition = new Vector3(-140f, 34f, 240f);
            ModMono.mergeLifeBooksButton.transform.Find("Normal/Image").GetComponent<CImage>().SetSprite("sp_icon_wuxue", false, null);
            ModMono.mergeLifeBooksButton.transform.Find("Normal/Text").GetComponent<TextMeshProUGUI>().text = "合并技艺";
            ModMono.mergeLifeBooksButton.interactable = true;
            ModMono.mergeLifeBooksButton.ClearAndAddListener(delegate
            {
                GameLog.LogMessage("Merger All Life Books");
                if (!MergeAll.isMerging)
                {
                    MergeAll.MergeAllLifeBooks();
                }
            });
            GameObject buttomButtonGroup = new GameObject("buttomButtonGroup");
            HorizontalLayoutGroup horizontalLayoutGroup = buttomButtonGroup.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childControlHeight = false;
            horizontalLayoutGroup.childControlWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = false;
            horizontalLayoutGroup.childForceExpandWidth = false;
            horizontalLayoutGroup.spacing = -22f;
            RectTransform component = buttomButtonGroup.GetComponent<RectTransform>();
            component.SetParent(characterMenuItem.CGet<Refers>("ItemPage").transform.Find("BottomBar"), false);
            component.localScale = Vector3.one;
            component.localPosition = new Vector3(-325f, 34f, 0f);
            component.sizeDelta = new Vector2(510f, 60f);
            Transform transform = characterMenuItem.CGet<Refers>("ItemPage").transform.Find("BottomBar/BtnMultiplySelect");
            transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            ModMono.mergeToolsButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            ModMono.mergeLifeBooksButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            transform.SetParent(buttomButtonGroup.transform);
            ModMono.mergeToolsButton.transform.SetParent(buttomButtonGroup.transform);
            ModMono.mergeLifeBooksButton.transform.SetParent(buttomButtonGroup.transform);
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00003AE8 File Offset: 0x00001CE8
        private void SetAnchorAndOffset(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00003B08 File Offset: 0x00001D08
        private void SetAnchorAndOffset(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.up;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        // Token: 0x06000019 RID: 25 RVA: 0x00003B30 File Offset: 0x00001D30
        private void OnCharacterMenuShow(ArgumentBox argbox)
        {
            UIElement uiElement;
            if (argbox.Get<UIElement>("Element", out uiElement) && uiElement.Name == "UI_CharacterMenu")
            {
                ModMono.mergePageMounted = false;
                ModMono.libraryPageMounted = false;
                if (ModMono.mergeToolsButton == null)
                {
                    this.CreateBottomButton();
                }
                bool isTaiwu = UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>().CurCharacterId == SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
                ModMono.mergeToolsButton.gameObject.SetActive(isTaiwu);
                ModMono.mergeLifeBooksButton.gameObject.SetActive(isTaiwu);
            }
        }

        // Token: 0x0600001A RID: 26 RVA: 0x00003BBC File Offset: 0x00001DBC
        private void OnCharacterMenuHide(ArgumentBox argbox)
        {
            UIElement uiElement;
            argbox.Get<UIElement>("Element", out uiElement);
            if (uiElement.Name == "UI_CharacterMenu")
            {
                
                if (UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>().CurTabIndex > 2)
                {
                    UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>().CurTabIndex = 0;
                }
                ModMono.MergeBooks.gameObject.SetActive(false);
                ModMono.MergeBooks.transform.SetParent(base.transform);

                if (UIElement.CharacterMenuLifeSkill.UiBaseAs<UI_CharacterMenuLifeSkill>().CurTabIndex > 2)
                {
                    UIElement.CharacterMenuLifeSkill.UiBaseAs<UI_CharacterMenuLifeSkill>().CurTabIndex = 0;
                }
            }
        }

        // Token: 0x04000007 RID: 7
        public static GameObject itemHolderPrefab;

        // Token: 0x04000008 RID: 8
        public static GameObject pagePrefab;

        // Token: 0x04000009 RID: 9
        public static GameObject scrollBarPrefab;

        // Token: 0x0400000A RID: 10
        public static GameObject listBackPrefab;

        // Token: 0x0400000B RID: 11
        public static GameObject bookPagePrefab;

        // Token: 0x0400000C RID: 12
        public static GameObject buttonPrefab;

        // Token: 0x0400000D RID: 13
        public static GameObject bookNamePrefab;

        // Token: 0x0400000E RID: 14
        public static GameObject bookIconPrefab;

        // Token: 0x0400000F RID: 15
        public static GameObject bookDescPrefab;

        // Token: 0x04000010 RID: 16
        public static GameObject skillTypeTogGroupPrefab;

        // Token: 0x04000011 RID: 17
        public static GameObject skillSubTypeTogPrefab;

        // Token: 0x04000012 RID: 18
        public static GameObject lockPrefab;

        // Token: 0x04000013 RID: 19
        public static GameObject multiOptButtonPrefab;

        // Token: 0x04000014 RID: 20
        public static GameObject lifeSkillDetailPrefab;

        // Token: 0x04000015 RID: 21
        public static UI_MergeBooks MergeBooks;

        // Token: 0x04000016 RID: 22
        public static UI_Library Library;

        // Token: 0x04000017 RID: 23
        public static CButton mergeToolsButton;

        // Token: 0x04000018 RID: 24
        public static CButton mergeLifeBooksButton;

        // Token: 0x04000019 RID: 25
        private static string rootPrefabPath = "RemakeResources/Prefab/Views/";

        // Token: 0x0400001A RID: 26
        private static bool mergePageMounted = false;

        // Token: 0x0400001B RID: 27
        private static bool libraryPageMounted = false;

        // Token: 0x0400001C RID: 28
        private static readonly int elementID = 1337;

        // Token: 0x0400001D RID: 29
        public readonly string[] OutlinePageName = new string[]
        {
            "承",
            "合",
            "解",
            "异",
            "独"
        };

        // Token: 0x0400001E RID: 30
        public readonly string[] DirectPageName = new string[]
        {
            "修",
            "思",
            "源",
            "参",
            "藏"
        };

        // Token: 0x0400001F RID: 31
        public readonly string[] ReversePageName = new string[]
        {
            "用",
            "奇",
            "巧",
            "化",
            "绝"
        };

        // Token: 0x04000020 RID: 32
        public const ushort MethodIDCreateBook = 151;

        // Token: 0x04000021 RID: 33
        public const ushort MethodIDTransformBook = 154;

        // Token: 0x04000022 RID: 34
        public const ushort MethodIDMergeAllTools = 155;

        // Token: 0x04000023 RID: 35
        public const ushort MethodIDMergeAllLifeBooks = 156;

        // Token: 0x04000024 RID: 36
        public const ushort MethodIDGetBookReadingProgress = 157;
    }
}
