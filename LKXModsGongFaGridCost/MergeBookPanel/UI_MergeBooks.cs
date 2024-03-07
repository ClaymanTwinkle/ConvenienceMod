using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using FrameWork;
using GameData.Domains.Item.Display;
using GameData.Domains.Item;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using GameData.Domains.Character;
using GameData.Domains.Taiwu;
using static GEvent;

namespace ConvenienceFrontend.MergeBookPanel
{
    public class UI_MergeBooks : UIBase
    {
        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000036 RID: 54 RVA: 0x00004E3F File Offset: 0x0000303F
        // (set) Token: 0x06000037 RID: 55 RVA: 0x00004E47 File Offset: 0x00003047
        public bool Visible { get; private set; }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000038 RID: 56 RVA: 0x00004E50 File Offset: 0x00003050
        private bool TypeIsCombatSkill
        {
            get
            {
                return this.skillTypeTogGroup.GetActive().Key == 0;
            }
        }

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000039 RID: 57 RVA: 0x00004E65 File Offset: 0x00003065
        private int SubTypeIndex
        {
            get
            {
                return this.skillSubTypeTogGroup.GetActive().Key;
            }
        }

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x0600003A RID: 58 RVA: 0x00004E77 File Offset: 0x00003077
        private bool OperationIsMerge
        {
            get
            {
                return this.operationTypeTogGroup.GetActive().Key == 0;
            }
        }

        // Token: 0x0600003B RID: 59 RVA: 0x00004E8C File Offset: 0x0000308C
        public override void OnInit(ArgumentBox argsBox)
        {
        }

        // Token: 0x0600003C RID: 60 RVA: 0x00004E90 File Offset: 0x00003090
        private void Awake()
        {
            GameLog.LogMessage("UI_MergeBooks OnAwake");
            base.AddMono(this.itemHolder, "ItemHolder");
            base.AddMono(this.scrollView, "ScrollView");
            base.AddMono(this.buttonMerge, "MergeButton");
            base.AddMono(this.toggleSaveUnSelectBook, "SaveUnSelectBook");
            this.itemHolder.transform.parent.GetChild(1).gameObject.GetComponent<ItemView>().Names[12] = ItemViewKeyUsingBg;
            GameLog.LogMessage("OnItemRender");
            this.infinityScroll.OnItemRender = new Action<int, Refers>(this.OnRenderItem);
            GameLog.LogMessage("OnItemRender done");
            this.InitToggleGroups();
        }

        // Token: 0x0600003D RID: 61 RVA: 0x00004F3C File Offset: 0x0000313C
        private void OnEnable()
        {
            GameLog.LogMessage("UI_MergeBooks Enable");
            this.skillTypeTogGroup.Set(0, true, true);
            this.operationTypeTogGroup.Set(1, true, true);
            this.operationTypeTogGroup.Set(0, true, true);
            this.ClearAndSetBooks();
            this.buttonMerge.ClearAndAddListener(delegate
            {
                this.OnClickMerge();
            });
            this.toggleSaveUnSelectBook.onValueChanged.RemoveAllListeners();
            this.toggleSaveUnSelectBook.onValueChanged.AddListener(delegate (bool val) {
                MergeBookPanelFrontPatch.EnableGenerateTwoBooks = val;
            });
            this.toggleSaveUnSelectBook.transform.parent.gameObject.SetActive(false);
            this.buttonMerge.gameObject.SetActive(false);
            this.buttonTransform.ClearAndAddListener(delegate
            {
                this.OnClickTransform();
            });
            this.buttonTransform.gameObject.SetActive(false);
            this.SetBookInfoActive(false);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x00004FDC File Offset: 0x000031DC
        private void InitToggleGroups()
        {
            this.skillSubTypeTogGroup.AllowSwitchOff = false;
            this.skillSubTypeTogGroup.AllowUncheck = false;
            this.skillSubTypeTogGroup.AddAllChildToggles();
            CToggle[] componentsInTopChildren = this.skillSubTypeTogGroup.transform.GetComponentsInTopChildren<CToggle>(false);
            for (int i = 0; i < componentsInTopChildren.Length; i++)
            {
                componentsInTopChildren[i].Key--;
            }
            this.skillSubTypeTogGroup.InitPreOnToggle();
            this.skillSubTypeTogGroup.OnActiveToggleChange = delegate (CToggle p0, CToggle p1)
            {
                GameLog.LogMessage(string.Format("skillSubTypeTogGroup switch to {0}", this.skillSubTypeTogGroup.GetActive().Key));
                this.UpdateBookList();
            };

            this.skillTypeTogGroup.AllowSwitchOff = false;
            this.skillTypeTogGroup.AllowUncheck = false;
            this.skillTypeTogGroup.AddAllChildToggles();
            this.skillTypeTogGroup.InitPreOnToggle();
            this.skillTypeTogGroup.OnActiveToggleChange = delegate (CToggle p0, CToggle p1)
            {
                UI_MergeBooks.RefreshSubTypeTog(this.skillSubTypeTogGroup, this.TypeIsCombatSkill);
                this.skillSubTypeTogGroup.Set(-1, true, true);
                GameLog.LogMessage(string.Format("skillTypeTogGroup switch to {0}", this.skillTypeTogGroup.GetActive().Key));
                GameLog.LogMessage(string.Format("skillSubTypeTogGroup switch to {0}", this.skillSubTypeTogGroup.GetActive().Key));
            };
            this.skillTypeTogGroup.Set(0, true, true);

            this.operationTypeTogGroup.AllowSwitchOff = false;
            this.operationTypeTogGroup.AllowUncheck = false;
            this.operationTypeTogGroup.AddAllChildToggles();
            this.operationTypeTogGroup.InitPreOnToggle();
            this.operationTypeTogGroup.OnActiveToggleChange = delegate (CToggle togNew, CToggle togOld)
            {
                this.toggleSaveUnSelectBook.SetIsOnWithoutNotify(MergeBookPanelFrontPatch.EnableGenerateTwoBooks);
                this.toggleSaveUnSelectBook.transform.parent.gameObject.SetActive(togNew.Key == 0);
                this.buttonMerge.gameObject.SetActive(togNew.Key == 0);
                this.buttonTransform.gameObject.SetActive(togNew.Key == 1);
                this.SetPageList();
            };
            this.operationTypeTogGroup.Set(0, true, true);
            this.operationTypeTogGroup.gameObject.SetActive(false);
        }

        // Token: 0x0600003F RID: 63 RVA: 0x00005118 File Offset: 0x00003318
        private static void RefreshSubTypeTog(CToggleGroup toggleGroup, bool isCombatSkill)
        {
            int count = isCombatSkill ? 14 : 16;
            for (int i = 0; i < toggleGroup.transform.childCount; i++)
            {
                toggleGroup.transform.GetChild(i).gameObject.SetActive(i <= count);
            }
            for (int j = 0; j < count; j++)
            {
                toggleGroup.Get(j).GetComponent<Refers>().CGet<TextMeshProUGUI>("Label").text = (isCombatSkill ? CombatSkillType.Instance[j].Name : Config.LifeSkillType.Instance[j].Name);
            }
        }

        // Token: 0x06000040 RID: 64 RVA: 0x000051AE File Offset: 0x000033AE
        private void SetBookInfoActive(bool active)
        {
            this.bookName.gameObject.SetActive(active);
            this.bookIcon.gameObject.SetActive(active);
            this.bookDesc.gameObject.SetActive(active);
        }

        // Token: 0x06000041 RID: 65 RVA: 0x000051E4 File Offset: 0x000033E4
        private void ClearAndSetBooks()
        {
            UI_CharacterMenuItems itemMenu = UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>();
            this._inventoryItems = itemMenu.GetFieldValue<List<ItemDisplayData>>("_inventoryItems");
            this._lifeSkillBooks.Clear();
            this._combatSkillBooks.Clear();
            int itemCount = (this._inventoryItems != null) ? this._inventoryItems.Count : 0;
            for (int i = 0; i < itemCount; i++)
            {
                ItemDisplayData item = this._inventoryItems[i];
                if (item.Key.ItemType == 10)
                {
                    ((SkillBook.Instance[item.Key.TemplateId].ItemSubType == 1001) ? this._combatSkillBooks : this._lifeSkillBooks).Add(item);
                    this._inventoryBooks.Add(item);
                }
            }
            this.UpdateBookList();
        }

        // Token: 0x06000042 RID: 66 RVA: 0x000052AC File Offset: 0x000034AC
        private void UpdateBookList()
        {
            this._inventoryBooks.Clear();
            this._selectedItems.Clear();
            this._selectedBookPageInfo.Clear();
            this._currMergedBook = ItemKey.Invalid;
            List<ItemDisplayData> list = this.TypeIsCombatSkill ? this._combatSkillBooks : this._lifeSkillBooks;
            IEnumerable<ItemDisplayData> range = from d in list.Where(delegate (ItemDisplayData d)
            {
                SkillBookItem skillBookItem = SkillBook.Instance[d.Key.TemplateId];
                if (!this.TypeIsCombatSkill)
                {
                    return (int)skillBookItem.LifeSkillType == this.SubTypeIndex || this.SubTypeIndex == -1;
                }
                return (int)skillBookItem.CombatSkillType == this.SubTypeIndex || this.SubTypeIndex == -1;
            })
                                                 select d;
            if (list.Count > 0)
            {
                this._inventoryBooks.AddRange(range);
            }
            this._inventoryBooks.Sort(delegate (ItemDisplayData a, ItemDisplayData b)
            {
                SkillBookItem configA = SkillBook.Instance[a.Key.TemplateId];
                int ret = SkillBook.Instance[b.Key.TemplateId].Grade.CompareTo(configA.Grade);
                if (ret == 0)
                {
                    return b.Key.TemplateId.CompareTo(a.Key.TemplateId);
                }
                return ret;
            });
            this.infinityScroll.SetDataCount(this._inventoryBooks.Count);
            this.InitAllPageList();
            this.SetBookInfoActive(false);
        }

        // Token: 0x06000043 RID: 67 RVA: 0x00005394 File Offset: 0x00003594
        private void OnRenderItem(int index, Refers itemRefers)
        {
            ItemDisplayData itemData = this._inventoryBooks[index];
            ItemView itemView = itemRefers as ItemView;
            itemView.SetData(itemData, false, -1, false, true, null, false);
            itemView.SetClickEvent(delegate
            {
                this.OnClickBook(itemView, itemData);
                this.infinityScroll.ReRender();
            });
            itemView.SetInteractable(true);
            itemView.SetHighLight(false);
            itemView.CGet<GameObject>(ItemViewKeyUsingBg).SetActive(this._currMergedBook.IsValid() && !itemData.Key.TemplateEquals(this._currMergedBook));
            itemView.CGet<GameObject>(ItemViewKeySelectStatus).SetActive(this._selectedItems.Contains(itemData));
        }

        // Token: 0x06000044 RID: 68 RVA: 0x0000547C File Offset: 0x0000367C
        private void InitItemScrollContent()
        {
            Transform transformItem = this.itemHolder.transform;
            GameObject itemPrefab = transformItem.GetChild(0).gameObject;
            int itemCount = (this._inventoryBooks != null) ? this._inventoryBooks.Count : 0;
            for (int i = 0; i < itemCount; i++)
            {
                if (i >= transformItem.childCount)
                {
                    UnityEngine.Object.Instantiate<GameObject>(itemPrefab, transformItem);
                }
                ItemDisplayData itemData = this._inventoryBooks[i];
                ItemView itemView = transformItem.GetChild(i).GetComponent<ItemView>();
                itemView.SetData(itemData, false, -1, false, true, null, false);
                itemView.SetClickEvent(delegate
                {
                    this.OnClickBook(itemView, itemData);
                });
                itemView.SetInteractable(true);
                itemView.SetHighLight(false);
                itemView.CGet<GameObject>(ItemViewKeyUsingBg).SetActive(false);
                itemView.CGet<GameObject>(ItemViewKeySelectStatus).SetActive(false);
                itemView.gameObject.SetActive(true);
            }
        }

        // Token: 0x06000045 RID: 69 RVA: 0x000055A0 File Offset: 0x000037A0
        private void InitPageList(GameObject bookPage)
        {
            bookPage.SetActive(false);
            Transform pageHolder = bookPage.transform.GetChild(0);
            for (int i = 0; i < pageHolder.childCount; i++)
            {
                Transform page = pageHolder.GetChild(i);
                PageView pageView = page.GetComponent<PageView>();
                int pageIdx = i;
                if (bookPage.name == "BookPageReverse")
                {
                    pageIdx += 10;
                }
                else if (bookPage.name == "BookPageDirect")
                {
                    pageIdx += 5;
                }
                pageView.SetClickEvent(delegate
                {
                    this.OnClickPage(pageView, pageIdx);
                });
            }
            this.ClearAndHidePageList(bookPage);
        }

        // Token: 0x06000046 RID: 70 RVA: 0x00005664 File Offset: 0x00003864
        private void ClearAndHidePageList(GameObject bookPage)
        {
            bookPage.SetActive(false);
            Transform pageHolder = bookPage.transform.GetChild(0);
            for (int i = 0; i < pageHolder.childCount; i++)
            {
                Transform page = pageHolder.GetChild(i);
                for (int j = 0; j < page.childCount; j++)
                {
                    page.GetChild(j).gameObject.SetActive(false);
                    page.GetChild(j).gameObject.SetActive(false);
                }
                page.Find("PageIndex").gameObject.SetActive(true);
                page.Find("PageName").gameObject.SetActive(true);
            }
        }

        // Token: 0x06000047 RID: 71 RVA: 0x00005700 File Offset: 0x00003900
        private void InitAllPageList()
        {
            this.pageObjectList[0] = this.bookPageOutline;
            this.pageObjectList[1] = this.bookPageDirect;
            this.pageObjectList[2] = this.bookPageReverse;
            foreach (GameObject page in this.pageObjectList)
            {
                this.InitPageList(page);
            }
            this.bookPageLifeSkill.SetActive(false);
            this.operationTypeTogGroup.gameObject.SetActive(false);
            this.toggleSaveUnSelectBook.transform.parent.gameObject.SetActive(false);
            this.buttonMerge.gameObject.SetActive(false);
            this.buttonTransform.gameObject.SetActive(false);
        }

        // Token: 0x06000048 RID: 72 RVA: 0x00005798 File Offset: 0x00003998
        private void ClearAllPageList()
        {
            foreach (GameObject page in this.pageObjectList)
            {
                this.ClearAndHidePageList(page);
            }
            this.bookPageLifeSkill.SetActive(false);
            this.operationTypeTogGroup.gameObject.SetActive(false);
            this.toggleSaveUnSelectBook.transform.parent.gameObject.SetActive(false);
            this.buttonMerge.gameObject.SetActive(false);
            this.buttonTransform.gameObject.SetActive(false);
        }

        // Token: 0x06000049 RID: 73 RVA: 0x00005804 File Offset: 0x00003A04
        private void SetPageList()
        {
            this.SetPageSelected();
            int pageNumber = this._isCombatBook ? UI_MergeBooks._stateList.Length : 5;
            if (this.OperationIsMerge)
            {
                for (int i = 0; i < pageNumber; i++)
                {
                    bool readed = UI_MergeBooks._readingProgressList[i] == 100;
                    PageView pageView = this.GetPageObject(i);
                    pageView.CGet<GameObject>("CompleteTips").SetActive(UI_MergeBooks._stateList[i] == 0);
                    pageView.CGet<GameObject>("IncompleteTips").SetActive(UI_MergeBooks._stateList[i] == 1);
                    pageView.CGet<GameObject>("LostTips").SetActive(UI_MergeBooks._stateList[i] == 2);
                    bool pageExist = UI_MergeBooks._stateList[i] != 3;
                    pageView.CGet<GameObject>("Lock").SetActive(!pageExist);
                    pageView.CGet<GameObject>("ReadedTips").SetActive(readed && pageExist);
                    pageView.CGet<GameObject>("NotReadTips").SetActive(!readed && pageExist);
                    TextMeshProUGUI progressComplete = pageView.CGet<TextMeshProUGUI>("ProgressComplete");
                    TextMeshProUGUI progressIncomplete = pageView.CGet<TextMeshProUGUI>("ProgressIncomplete");
                    (readed ? progressComplete : progressIncomplete).text = string.Format("{0}%", UI_MergeBooks._readingProgressList[i]);
                    progressComplete.gameObject.SetActive(readed && pageExist);
                    progressIncomplete.gameObject.SetActive(!readed && pageExist);
                    if (this._isCombatBook)
                    {
                        pageView.SetInteractable(pageExist);
                        pageView.CGet<GameObject>(PageViewKeySelectMark).SetActive(false);
                    }
                }
                return;
            }
            for (int j = 0; j < pageNumber; j++)
            {
                bool readed2 = UI_MergeBooks._readingProgressList[j] == 100;
                PageView pageView2 = this.GetPageObject(j);
                bool pageExist2 = UI_MergeBooks._stateList[j] != 3;
                pageView2.CGet<GameObject>("CompleteTips").SetActive(readed2 || UI_MergeBooks._stateList[j] == 0);
                pageView2.CGet<GameObject>("IncompleteTips").SetActive(!readed2 && UI_MergeBooks._stateList[j] == 1);
                pageView2.CGet<GameObject>("LostTips").SetActive(!readed2 && UI_MergeBooks._stateList[j] == 2);
                pageView2.CGet<GameObject>("Lock").SetActive(!readed2 && !pageExist2);
                pageView2.CGet<GameObject>("ReadedTips").SetActive(readed2);
                pageView2.CGet<GameObject>("NotReadTips").SetActive(!readed2 && pageExist2);
                TextMeshProUGUI progressComplete2 = pageView2.CGet<TextMeshProUGUI>("ProgressComplete");
                TextMeshProUGUI progressIncomplete2 = pageView2.CGet<TextMeshProUGUI>("ProgressIncomplete");
                (readed2 ? progressComplete2 : progressIncomplete2).text = string.Format("{0}%", UI_MergeBooks._readingProgressList[j]);
                progressComplete2.gameObject.SetActive(readed2);
                progressIncomplete2.gameObject.SetActive(!readed2 && pageExist2);
                if (this._isCombatBook)
                {
                    pageView2.SetInteractable(readed2 || pageExist2);
                    pageView2.CGet<GameObject>(PageViewKeySelectMark).SetActive(false);
                }
            }
        }

        // Token: 0x0600004A RID: 74 RVA: 0x00005AF8 File Offset: 0x00003CF8
        private PageView GetPageObject(int idx)
        {
            if (!this._isCombatBook)
            {
                return this.bookPageLifeSkill.transform.GetChild(0).GetChild(idx).GetComponent<PageView>();
            }
            return this.pageObjectList[idx / 5].transform.GetChild(0).GetChild(idx % 5).GetComponent<PageView>();
        }

        // Token: 0x0600004B RID: 75 RVA: 0x00005B4C File Offset: 0x00003D4C
        private void OnDisable()
        {
            GameLog.LogMessage("UI_MergeBooks Disable");
            this._inventoryBooks.Clear();
            this._selectedItems.Clear();
            this._selectedBookPageInfo.Clear();
        }

        // Token: 0x0600004C RID: 76 RVA: 0x00005B79 File Offset: 0x00003D79
        protected override void OnClick(CButton btn)
        {
            if (btn.name == "Close")
            {
                this.QuickHide();
            }
        }

        // Token: 0x0600004D RID: 77 RVA: 0x00005B94 File Offset: 0x00003D94
        private void OnClickMerge()
        {
            if (this._selectedItems.Count < 2)
            {
                this.ShowDialog("提示", "请先选择多本合并书籍", null);
                return;
            }
            if (!this.ValidatePages())
            {
                this.ShowDialog("提示", "请先选择页（总纲页 x 1，普通页 x 5）", null);
                return;
            }
            GameLog.LogMessage("Start Merge. Remove books");
            UI_CharacterMenuItems itemMenu = UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>();
            this.RemoveMergedBooks();
            GameLog.LogMessage("Gen New Book");
            ushort pageState = 0;
            ushort remainPageState = 0;
            byte pageType = 0;
            short durability = (short)((int)this.CalDurabilities()).Clamp(1, 999);
            int pageNumber = this._isCombatBook ? UI_MergeBooks._selectedPages.Length : 5;
            for (int i = 0; i < pageNumber; i++)
            {
                int pageIdx = UI_MergeBooks._selectedPages[i];
                pageState |= (ushort)(UI_MergeBooks._stateList[pageIdx] << i * 2);
                if (this._isCombatBook)
                {
                    int remainPageIdx = (i == 0) ? UI_MergeBooks._selectedPages[i] : this.ReverseIndex(UI_MergeBooks._selectedPages[i]);
                    sbyte tmpState = (sbyte)((UI_MergeBooks._stateList[remainPageIdx] == 3) ? 2 : UI_MergeBooks._stateList[remainPageIdx]);
                    remainPageState |= (ushort)(tmpState << i * 2);
                    if (i == 0)
                    {
                        pageType |= (byte)UI_MergeBooks._selectedPages[i];
                    }
                    else
                    {
                        pageType |= (byte)(UI_MergeBooks._selectedPages[i] / 5 - 1 << i + 2);
                    }
                }
            }
            GameLog.LogMessage(string.Format("Gen book result PageState: {0}", pageState));
            short remainDura = (short)(this._selectedItems.Sum((ItemDisplayData book) => (int)book.Durability) - (int)durability).Clamp(1, 999);
            if (MergeBookPanelFrontPatch.EnableGenerateTwoBooks)
            {
                GameDataBridge.AddMethodCall<ItemKey, short, ushort, byte, ushort, short>(itemMenu.Element.GameDataListenerId, 4, ModMono.MethodIDCreateBook, this._currMergedBook, durability, pageState, pageType, remainPageState, remainDura);
            }
            else
            {
                GameDataBridge.AddMethodCall<ItemKey, short, ushort, byte>(itemMenu.Element.GameDataListenerId, 4, ModMono.MethodIDCreateBook, this._currMergedBook, durability, pageState, pageType);
            }
            UI_CharacterMenu characterMenu = UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>();
            this.ShowDialog("合并结果", "合并成功", null);
            // itemMenu.SetPrivateField("_needRefresh", true);
            // GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 28, characterMenu.CurCharacterId);
            CharacterDomainHelper.MethodCall.GetAllInventoryItems(itemMenu.Element.GameDataListenerId, characterMenu.CurCharacterId);
            // base.AsynchMethodCall<int>(4, 28, characterMenu.CurCharacterId, delegate (int p0, RawDataPool p1)
            CharacterDomainHelper.AsyncMethodCall.GetAllInventoryItems(null, characterMenu.CurCharacterId, delegate (int p0, RawDataPool p1)
            {
                this.ClearAndSetBooks();
            });
        }

        // Token: 0x0600004E RID: 78 RVA: 0x00005DDD File Offset: 0x00003FDD
        private int ReverseIndex(int x)
        {
            return (x + x / 5 * 5) % 15;
        }

        // Token: 0x0600004F RID: 79 RVA: 0x00005DEC File Offset: 0x00003FEC
        private void OnClickTransform()
        {
            if (this._selectedItems.Count != 1)
            {
                this.ShowDialog("提示", "请先选择编撰的书籍，一本！", null);
                return;
            }
            if (!this.ValidatePages())
            {
                this.ShowDialog("提示", "请先选择页（总纲页 x 1，普通页 x 5）", null);
                return;
            }
            GameLog.LogMessage("Gen New Book");
            byte pageType = 0;
            if (this._isCombatBook)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (i == 0)
                    {
                        pageType |= (byte)UI_MergeBooks._selectedPages[i];
                    }
                    else
                    {
                        pageType |= (byte)(UI_MergeBooks._selectedPages[i] / 5 - 1 << i + 2);
                    }
                }
            }
            GameDataBridge.AddMethodCall<ItemKey, short, short, byte, bool>(UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>().Element.GameDataListenerId, 4, ModMono.MethodIDTransformBook, this._currMergedBook, this._selectedItems[0].Durability, this._selectedItems[0].MaxDurability, pageType, MergeBookPanelFrontPatch.EnableAllPagesTranform);
            UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>();
            this.ShowDialog("编撰结果", "编撰成功", null);
            this.ClearAndSetBooks();
        }

        // Token: 0x06000050 RID: 80 RVA: 0x00005EEC File Offset: 0x000040EC
        private void OnClickSplit()
        {
            if (this._selectedItems.Count != 1 || !this._isCombatBook)
            {
                this.ShowDialog("提示", "请先选择一本拆分的书籍", null);
                return;
            }
            this.RemoveMergedBooks();
            GameLog.LogMessage("Gen New Book");
            byte pageType = 0;
            for (int i = 0; i < UI_MergeBooks._selectedPages.Length; i++)
            {
                if (i == 0)
                {
                    pageType |= (byte)UI_MergeBooks._selectedPages[i];
                }
                else
                {
                    pageType |= (byte)(UI_MergeBooks._selectedPages[i] / 5 - 1 << i + 2);
                }
            }
            UI_CharacterMenuItems ui_CharacterMenuItems = UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>();
            GameDataBridge.AddMethodCall<ItemKey, short, short, byte>(ui_CharacterMenuItems.Element.GameDataListenerId, 4, ModMono.MethodIDTransformBook, this._currMergedBook, this._selectedItems[0].Durability, this._selectedItems[0].MaxDurability, pageType);
            UI_CharacterMenu characterMenu = UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>();
            this.ShowDialog("编撰结果", "编撰成功", null);
            // ui_CharacterMenuItems.SetPrivateField("_needRefresh", true);
            // GameDataBridge.AddMethodCall<int>(ui_CharacterMenuItems.Element.GameDataListenerId, 4, 28, characterMenu.CurCharacterId);
            CharacterDomainHelper.MethodCall.GetAllInventoryItems(ui_CharacterMenuItems.Element.GameDataListenerId, characterMenu.CurCharacterId);
            CharacterDomainHelper.AsyncMethodCall.GetAllInventoryItems(null, characterMenu.CurCharacterId, delegate (int p0, RawDataPool p1) {
                this.ClearAndSetBooks();
            });
            //base.AsynchMethodCall<int>(4, 28, characterMenu.CurCharacterId, delegate (int p0, RawDataPool p1)
            //{
            //    this.ClearAndSetBooks();
            //});
        }

        // Token: 0x06000051 RID: 81 RVA: 0x00006018 File Offset: 0x00004218
        private void RemoveMergedBooks()
        {
            UI_CharacterMenu characterMenu = UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>();
            UI_CharacterMenuItems itemMenu = (UI_CharacterMenuItems)characterMenu.AllSubPages[3];
            ItemScrollView itemScroll = itemMenu.GetFieldValue<ItemScrollView>("_itemScroll");
            List<ItemDisplayData> inventoryItems = itemMenu.GetFieldValue<List<ItemDisplayData>>("_inventoryItems");
            for (int i = 0; i < this._selectedItems.Count; i++)
            {
                ItemDisplayData book = this._selectedItems[i];
                inventoryItems.Remove(book);
                itemMenu.CallPrivateMethod("ClearItemUsingState", new object[]
                {
                    book
                });
                GameDataBridge.AddMethodCall<int, ItemKey, int>(itemMenu.Element.GameDataListenerId, 6, ItemDomainHelper.MethodIds.DiscardItem, characterMenu.CurCharacterId, book.Key, 1);
            }
            itemScroll.SetItemList(ref this._inventoryItems, false, null, false, null);
            this.ClearAllPageList();
        }

        // Token: 0x06000052 RID: 82 RVA: 0x000060D8 File Offset: 0x000042D8
        private bool ValidatePages()
        {
            if (this._isCombatBook)
            {
                int[] selectedPages = UI_MergeBooks._selectedPages;
                for (int i = 0; i < selectedPages.Length; i++)
                {
                    if (selectedPages[i] == -1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Token: 0x06000053 RID: 83 RVA: 0x0000610C File Offset: 0x0000430C
        private void OnClickBook(ItemView itemView, ItemDisplayData itemData)
        {
            ItemKey itemKey = itemData.Key;
            if (this._selectedItems.Count > 0 && itemKey.TemplateId != this._currMergedBook.TemplateId)
            {
                return;
            }
            bool selected = this._selectedItems.Contains(itemData);
            itemView.SetHighLight(!selected);
            itemView.CGet<GameObject>(ItemViewKeySelectStatus).SetActive(!selected);
            if (selected)
            {
                GameLog.LogMessage("uncheck");
                this._selectedItems.Remove(itemData);
                this._selectedBookPageInfo.RemoveAll((SkillBookPageDisplayData elem) => elem.ItemKey.Equals(itemKey));
                this.RecalMergePages();
                if (this._selectedItems.Count == 0)
                {
                    this._currMergedBook = ItemKey.Invalid;
                    this.ClearAllPageList();
                    this.SetBookInfoActive(false);
                    return;
                }
            }
            else
            {
                GameLog.LogMessage("check");
                this._selectedItems.Add(itemData);
                if (this._selectedItems.Count == 1)
                {
                    this._currMergedBook = itemKey;
                    SkillBookItem configData = SkillBook.Instance[itemData.Key.TemplateId];
                    this._isCombatBook = (configData.ItemSubType == 1001);
                    this.bookName.text = configData.Name;
                    this.bookIcon.SetSprite(configData.Icon, false, null);
                    MouseTip_Util.SetMultiLineAutoHeightText(this.bookDesc, configData.Desc);
                    this.SetBookInfoActive(true);
                    if (this._isCombatBook)
                    {
                        GameObject[] array = this.pageObjectList;
                        for (int i = 0; i < array.Length; i++)
                        {
                            array[i].SetActive(true);
                        }
                    }
                    else
                    {
                        this.bookPageLifeSkill.SetActive(true);
                    }
                    this.toggleSaveUnSelectBook.SetIsOnWithoutNotify(MergeBookPanelFrontPatch.EnableGenerateTwoBooks);
                    this.toggleSaveUnSelectBook.transform.parent.gameObject.SetActive(true);
                    this.buttonMerge.gameObject.SetActive(true);
                    this.operationTypeTogGroup.gameObject.SetActive(true);
                    this.operationTypeTogGroup.Set(0, true, true);
                    
                    SingletonObject.getInstance<AsyncMethodDispatcher>().AsyncMethodCall(6, ModMono.MethodIDGetBookReadingProgress, itemKey, delegate (int offset, RawDataPool dataPool) {
                        OnGetReadingProgress(offset, dataPool);
                    });
                    // base.AsynchMethodCall<ItemKey>(6, 157, itemKey, new Action<int, RawDataPool>(this.OnGetReadingProgress));
                    this.SetPageSelected();
                }
                ItemDomainHelper.AsyncMethodCall.GetSkillBookPagesInfo(null, itemKey, delegate (int offset, RawDataPool dataPool) {
                    this.OnGetPageInfo(offset, dataPool);
                });
            }
        }

        // Token: 0x06000054 RID: 84 RVA: 0x0000632C File Offset: 0x0000452C
        private void OnClickPage(PageView pageView, int pageIdx)
        {
            if (pageIdx < 5)
            {
                if (UI_MergeBooks._selectedPages[0] != -1)
                {
                    pageView.transform.parent.GetChild(UI_MergeBooks._selectedPages[0]).GetComponent<PageView>().CGet<GameObject>(PageViewKeySelectMark).SetActive(false);
                }
                UI_MergeBooks._selectedPages[0] = pageIdx;
                pageView.CGet<GameObject>(PageViewKeySelectMark).SetActive(true);
                return;
            }
            int subpageIdx = pageIdx % 5 + 1;
            if (UI_MergeBooks._selectedPages[subpageIdx] != -1)
            {
                int prevObectjIdx = UI_MergeBooks._selectedPages[subpageIdx] / 5;
                this.pageObjectList[prevObectjIdx].transform.GetChild(0).GetChild(pageIdx % 5).GetComponent<PageView>().CGet<GameObject>(PageViewKeySelectMark).SetActive(false);
            }
            UI_MergeBooks._selectedPages[subpageIdx] = pageIdx;
            pageView.CGet<GameObject>(PageViewKeySelectMark).SetActive(true);
        }

        // Token: 0x06000055 RID: 85 RVA: 0x000063F0 File Offset: 0x000045F0
        private void SetPageSelected()
        {
            if (this._isCombatBook)
            {
                for (int i = 0; i < UI_MergeBooks._selectedPages.Length; i++)
                {
                    UI_MergeBooks._selectedPages[i] = -1;
                }
                return;
            }
            for (int j = 0; j < 5; j++)
            {
                UI_MergeBooks._selectedPages[j] = j;
            }
        }

        // Token: 0x06000056 RID: 86 RVA: 0x00006434 File Offset: 0x00004634
        private void MaskOtherBooks(ItemKey itemKey)
        {
            Transform transformItem = this.itemHolder.transform;
            int itemCount = (this._inventoryBooks != null) ? this._inventoryBooks.Count : 0;
            for (int i = 0; i < itemCount; i++)
            {
                ItemDisplayData itemData = this._inventoryBooks[i];
                if (itemKey.TemplateId != itemData.Key.TemplateId)
                {
                    transformItem.GetChild(i).GetComponent<ItemView>().CGet<GameObject>(ItemViewKeyUsingBg).SetActive(true);
                }
            }
        }

        // Token: 0x06000057 RID: 87 RVA: 0x000064AC File Offset: 0x000046AC
        private void RemoveMask()
        {
            int itemCount = (this._inventoryBooks != null) ? this._inventoryBooks.Count : 0;
            for (int i = 0; i < itemCount; i++)
            {
                this.itemHolder.transform.GetChild(i).GetComponent<ItemView>().CGet<GameObject>(ItemViewKeyUsingBg).SetActive(false);
            }
        }

        // Token: 0x06000058 RID: 88 RVA: 0x00006504 File Offset: 0x00004704
        private void OnGetPageInfo(int offset, RawDataPool dataPool)
        {
            SkillBookPageDisplayData displayData = null;
            Serializer.Deserialize(dataPool, offset, ref displayData);
            this._selectedBookPageInfo.Add(displayData);
            this.RecalMergePages();
        }

        // Token: 0x06000059 RID: 89 RVA: 0x00006530 File Offset: 0x00004730
        private void OnGetReadingProgress(int offset, RawDataPool dataPool)
        {
            sbyte[] readingProgress = new sbyte[15];
            Serializer.Deserialize(dataPool, offset, ref readingProgress);
            GameLog.LogMessage<sbyte>(readingProgress);
            readingProgress.CopyTo(UI_MergeBooks._readingProgressList, 0);
            if (this._isCombatBook && MergeBookPanelFrontPatch.EnableOutlineTranform)
            {
                for (int i = 0; i < 5; i++)
                {
                    UI_MergeBooks._readingProgressList[i] = 100;
                }
            }
            if (MergeBookPanelFrontPatch.EnableAllPagesTranform)
            {
                for (int j = 0; j < 15; j++)
                {
                    UI_MergeBooks._readingProgressList[j] = 100;
                }
            }
        }

        // Token: 0x0600005A RID: 90 RVA: 0x000065A4 File Offset: 0x000047A4
        private void RecalMergePages()
        {
            for (int i = 0; i < UI_MergeBooks._stateList.Length; i++)
            {
                UI_MergeBooks._stateList[i] = 3;
            }
            foreach (SkillBookPageDisplayData p in this._selectedBookPageInfo)
            {
                for (int j = 0; j < p.ReadingProgress.Length; j++)
                {
                    int idx = j;
                    if (this._isCombatBook)
                    {
                        idx = ((j == 0) ? ((int)p.Type[0]) : ((int)(5 + p.Type[j] * 5) + j - 1));
                    }
                    UI_MergeBooks._stateList[idx] = ((UI_MergeBooks._stateList[idx] > p.State[j]) ? p.State[j] : UI_MergeBooks._stateList[idx]);
                }
            }
            this.SetPageList();
        }

        // Token: 0x0600005B RID: 91 RVA: 0x0000667C File Offset: 0x0000487C
        private short CalDurabilities()
        {
            float totalDura = 0f;
            foreach (ItemDisplayData book in this._selectedItems)
            {
                totalDura += (float)book.Durability;
            }
            if (!this._isCombatBook)
            {
                for (int i = 0; i < this._selectedBookPageInfo.Count; i++)
                {
                    sbyte[] mergedBookPageState = this._selectedBookPageInfo[i].State;
                    short mergedBookDura = this._selectedItems[i].Durability;
                    for (int j = 0; j < 5; j++)
                    {
                        if (mergedBookPageState[j] > UI_MergeBooks._stateList[j])
                        {
                            if (UI_MergeBooks._stateList[j] == 0 && mergedBookPageState[j] == 1)
                            {
                                totalDura -= (float)mergedBookDura * 0.1f;
                            }
                            else if (UI_MergeBooks._stateList[j] == 0 && mergedBookPageState[j] == 2)
                            {
                                totalDura -= (float)mergedBookDura * 0.18f;
                            }
                            else
                            {
                                totalDura -= (float)mergedBookDura * 0.08f;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int k = 0; k < this._selectedBookPageInfo.Count; k++)
                {
                    sbyte[] mergedBookPageState2 = this._selectedBookPageInfo[k].State;
                    sbyte[] mergedBookPageType = this._selectedBookPageInfo[k].Type;
                    short mergedBookDura2 = this._selectedItems[k].Durability;
                    for (int l = 0; l < 6; l++)
                    {
                        int pageIdx = UI_MergeBooks._selectedPages[l];
                        int genPagetype = (l == 0) ? pageIdx : (pageIdx / 5 - 1);
                        if (mergedBookPageState2[l] > UI_MergeBooks._stateList[pageIdx])
                        {
                            if (UI_MergeBooks._stateList[pageIdx] == 0 && (mergedBookPageState2[l] == 2 || (int)mergedBookPageType[l] != genPagetype))
                            {
                                totalDura -= (float)mergedBookDura2 * 0.15f;
                            }
                            else if (UI_MergeBooks._stateList[pageIdx] == 0 && mergedBookPageState2[l] == 1)
                            {
                                totalDura -= (float)mergedBookDura2 * 0.083f;
                            }
                            else
                            {
                                totalDura -= (float)mergedBookDura2 * 0.067f;
                            }
                        }
                    }
                }
            }
            return (short)totalDura;
        }

        // Token: 0x0600005C RID: 92 RVA: 0x00006888 File Offset: 0x00004A88
        private void ResizeScroll()
        {
            if (this.scrollView != null)
            {
                RectTransform viewport = this.scrollView.Viewport;
                this._cellSize = this.itemHolder.GetComponent<GridLayoutGroup>().cellSize;
                this._spacing = this.itemHolder.GetComponent<GridLayoutGroup>().spacing;
                float hight = (float)((this._inventoryBooks.Count + this._lineCount - 1) / this._lineCount) * (this._cellSize.y + this._spacing.y);
                RectTransform component = this.itemHolder.GetComponent<RectTransform>();
                component.sizeDelta = component.sizeDelta.SetY(hight);
                this.scrollView.ScrollBar.value = 0f;
                this.scrollView.CallPrivateMethod("SetToPositionByValue", new object[]
                {
                    0f
                });
            }
        }

        // Token: 0x0600005D RID: 93 RVA: 0x00006968 File Offset: 0x00004B68
        private void ShowDialog(string title, string message, Action onYes)
        {
            DialogCmd dialogCmd = new DialogCmd();
            dialogCmd.Type = 1;
            dialogCmd.Title = title;
            dialogCmd.Content = message;
            if (onYes != null)
            {
                dialogCmd.Yes = onYes;
            }
            if (onYes != null)
            {
                dialogCmd.No = onYes;
            }
            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", dialogCmd));
            UIManager.Instance.ShowUI(UIElement.Dialog);
        }

        // Token: 0x0400002E RID: 46
        public const sbyte Invalid = 3;

        // Token: 0x0400002F RID: 47
        public const sbyte Complete = 0;

        // Token: 0x04000030 RID: 48
        public const sbyte Incomplete = 1;

        // Token: 0x04000031 RID: 49
        public const sbyte Lost = 2;

        // Token: 0x04000032 RID: 50
        public GameObject itemHolder;

        // Token: 0x04000033 RID: 51
        public CScrollRect scrollView;

        // Token: 0x04000034 RID: 52
        public InfinityScroll infinityScroll;

        // Token: 0x04000035 RID: 53
        public TextMeshProUGUI bookName;

        // Token: 0x04000036 RID: 54
        public CImage bookIcon;

        // Token: 0x04000037 RID: 55
        public TextMeshProUGUI bookDesc;

        // Token: 0x04000038 RID: 56
        public GameObject bookPageOutline;

        // Token: 0x04000039 RID: 57
        public GameObject bookPageDirect;

        // Token: 0x0400003A RID: 58
        public GameObject bookPageReverse;

        // Token: 0x0400003B RID: 59
        public GameObject bookPageLifeSkill;

        public CToggle toggleSaveUnSelectBook;

        // Token: 0x0400003C RID: 60
        public CButton buttonMerge;

        // Token: 0x0400003D RID: 61
        public CButton buttonTransform;

        // Token: 0x0400003E RID: 62
        public CButton buttonSplit;

        // Token: 0x0400003F RID: 63
        public CToggleGroup skillTypeTogGroup;

        // Token: 0x04000040 RID: 64
        public CToggleGroup skillSubTypeTogGroup;

        // Token: 0x04000041 RID: 65
        public CToggleGroup operationTypeTogGroup;

        // Token: 0x04000042 RID: 66
        private readonly int _lineCount = 9;

        // Token: 0x04000043 RID: 67
        private Vector2 _cellSize;

        // Token: 0x04000044 RID: 68
        private Vector2 _spacing;

        // Token: 0x04000045 RID: 69
        private ItemKey _currMergedBook;

        // Token: 0x04000046 RID: 70
        private bool _isCombatBook;

        // Token: 0x04000048 RID: 72
        private List<ItemDisplayData> _inventoryItems;

        // Token: 0x04000049 RID: 73
        private List<ItemDisplayData> _inventoryBooks = new List<ItemDisplayData>();

        // Token: 0x0400004A RID: 74
        private List<ItemDisplayData> _combatSkillBooks = new List<ItemDisplayData>();

        // Token: 0x0400004B RID: 75
        private List<ItemDisplayData> _lifeSkillBooks = new List<ItemDisplayData>();

        // Token: 0x0400004C RID: 76
        private readonly List<ItemDisplayData> _selectedItems = new List<ItemDisplayData>();

        // Token: 0x0400004D RID: 77
        private readonly List<SkillBookPageDisplayData> _selectedBookPageInfo = new List<SkillBookPageDisplayData>();

        // Token: 0x0400004E RID: 78
        public GameObject[] pageObjectList = new GameObject[3];

        // Token: 0x0400004F RID: 79
        private static readonly sbyte[] _stateList = new sbyte[15];

        // Token: 0x04000050 RID: 80
        private static readonly sbyte[] _readingProgressList = new sbyte[15];

        // Token: 0x04000051 RID: 81
        private static readonly int[] _selectedPages = new int[6];

        private static readonly string ItemViewKeyUsingBg = "UsingBgx";
        private static readonly string ItemViewKeySelectStatus = "SelectStatus";
        private static readonly string PageViewKeySelectMark = "SelectMark";

    }
}
