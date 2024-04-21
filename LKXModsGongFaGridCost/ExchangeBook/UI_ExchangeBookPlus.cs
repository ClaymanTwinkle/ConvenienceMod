using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using DG.Tweening;
using FrameWork;
using GameData.Common;
using GameData.Domains.Character.AvatarSystem;
using GameData.Domains.Character.Display;
using GameData.Domains.Character;
using GameData.Domains.Item.Display;
using GameData.Domains.Item;
using GameData.Domains.TaiwuEvent.DisplayEvent;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using TMPro;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine;
using GameData.Domains.Merchant;
using GameData.Domains.Taiwu;
using GameData.Domains.CombatSkill;
using GameData.Domains.Map;
using GameData.Domains.Organization;
using GameData.Domains.Extra;
using GameData.Domains.Combat;
using GameData.Domains.Adventure;
using GameData.Domains.Building;
using GameData.Domains.LegendaryBook;
using GameData.Domains.TutorialChapter;
using GameData.Domains.TaiwuEvent;

namespace ConvenienceFrontend.ExchangeBook
{
    public class UI_ExchangeBookPlus : UIBase
    {
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x0600000D RID: 13 RVA: 0x00002EF4 File Offset: 0x000010F4
        private int Grade
        {
            get
            {
                return this._gradeTogGroup.GetActive().Key;
            }
        }

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x0600000E RID: 14 RVA: 0x00002F06 File Offset: 0x00001106
        private int SkillType
        {
            get
            {
                return this._skillTypeTogGroup.GetActive().Key;
            }
        }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x0600000F RID: 15 RVA: 0x00002F18 File Offset: 0x00001118
        private int FirstPageIdx
        {
            get
            {
                return this._firstPageTogGroup.GetActive().Key;
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002F2C File Offset: 0x0000112C
        private bool ShouldShowItem(ItemDisplayData item)
        {
            SkillBookItem skillBookItem = SkillBook.Instance[item.Key.TemplateId];
            bool isCombatSkill = this._isCombatSkill;
            bool result;
            if (isCombatSkill)
            {
                ValueTuple<sbyte[], sbyte[]> valueTuple = this._pagesDatas[item.Key.Id];
                sbyte[] pageInfo = valueTuple.Item1;
                sbyte[] pageState2 = valueTuple.Item2;
                bool flag = (int)skillBookItem.Grade != this.Grade || (int)skillBookItem.CombatSkillType != this.SkillType || (this.FirstPageIdx > 0 && this.FirstPageIdx - 1 != (int)pageInfo[0]);
                result = (!flag && (this._directPageTogGroup.GetAllActive().TrueForAll((CToggle toggle) => pageInfo[toggle.Key] == 0) && this._reversePageTogGroup.GetAllActive().TrueForAll((CToggle toggle) => pageInfo[toggle.Key] == 1)) && this._completePageTogGroup.GetAllActive().TrueForAll((CToggle toggle) => pageState2[toggle.Key] == 0));
            }
            else
            {
                bool flag2 = (int)skillBookItem.Grade != this.Grade || (int)skillBookItem.LifeSkillType != this.SkillType;
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    sbyte[] pageState = this._pagesDatas[item.Key.Id].Item2;
                    result = this._completePageTogGroup.GetAllActive().TrueForAll((CToggle toggle) => pageState[toggle.Key - 1] == 0);
                }
            }
            return result;
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000030BC File Offset: 0x000012BC
        private void UpdateCurrList()
        {
            this._currItems.Clear();
            foreach (KeyValuePair<int, List<ItemDisplayData>> keyValuePair in this._npcItems)
            {
                foreach (ItemDisplayData item in keyValuePair.Value)
                {
                    bool flag = this.ShouldShowItem(item);
                    if (flag)
                    {
                        this._currItems.Add(item);
                    }
                }
            }
            this.UpdateShopDisplay();
        }

        // Token: 0x06000012 RID: 18 RVA: 0x0000317C File Offset: 0x0000137C
        private void UpdateShopDisplay()
        {
            this._npcItemScroll.SetItemList(ref this._currItems, false, null, false, null);
            RectTransform content = this._npcItemScroll.GetComponent<CScrollRect>().Content;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, content.sizeDelta.y + 30f);
        }

        // Token: 0x06000013 RID: 19 RVA: 0x000031CC File Offset: 0x000013CC
        public static UIElement GetUI()
        {
            bool flag = UI_ExchangeBookPlus.element != null && UI_ExchangeBookPlus.element.UiBase != null;
            UIElement result;
            if (flag)
            {
                result = UI_ExchangeBookPlus.element;
            }
            else
            {
                UI_ExchangeBookPlus.element = new UIElement();
                Traverse.Create(UI_ExchangeBookPlus.element).Field("_path").SetValue("UI_ExchangeBookPlus");
                GameObject gameObject = UIBuilder.BuildMainUI("UI_ExchangeBookPlus");
                UI_ExchangeBookPlus ui_ExchangeBookPlus = gameObject.AddComponent<UI_ExchangeBookPlus>();
                ui_ExchangeBookPlus.UiType = UILayer.LayerPopUp;
                ui_ExchangeBookPlus.Element = UI_ExchangeBookPlus.element;
                ui_ExchangeBookPlus.RelativeAtlases = new SpriteAtlas[0];
                ui_ExchangeBookPlus.Init(gameObject);
                UI_ExchangeBookPlus.element.UiBase = ui_ExchangeBookPlus;
                UI_ExchangeBookPlus.element.UiBase.name = UI_ExchangeBookPlus.element.Name;
                UIManager.Instance.PlaceUI(UI_ExchangeBookPlus.element.UiBase);
                ui_ExchangeBookPlus.NeedDataListenerId = true;
                result = UI_ExchangeBookPlus.element;
            }
            return result;
        }

        // Token: 0x06000014 RID: 20 RVA: 0x000032B0 File Offset: 0x000014B0
        private void Init(GameObject obj)
        {
            this.AnimIn = obj.transform.Find("MoveIn").GetComponent<DOTweenAnimation>();
            this.AnimOut = obj.transform.Find("MoveOut").GetComponent<DOTweenAnimation>();
            this.AnimIn.hasOnPlay = true;
            this.AnimIn.onPlay = new UnityEvent();
            this.AnimOut.hasOnPlay = true;
            this.AnimOut.onPlay = new UnityEvent();
            this._exchangeNpcBase = obj.transform.Find("MainWindow/ExchangeArea/Npc/").gameObject.GetComponent<RectTransform>();
            this._confirm = obj.transform.Find("MainWindow/ExchangeArea/ConfirmFrame/Confirm").gameObject.GetComponent<CButton>();
            base.AddMono(obj.transform.Find("MainWindow/NpcBooks/ImgTitle36/Title").gameObject.GetComponent<TextMeshProUGUI>(), "NpcTitle");
            this._npcItemScroll = obj.transform.Find("MainWindow/NpcBooks/NpcItemScroll/").gameObject.GetComponent<ItemScrollView>();
            base.AddMono(obj.transform.Find("MainWindow/ExchangeArea/SelfPrestige/").gameObject.GetComponent<RectTransform>(), "SelfAuthority");
            base.AddMono(obj.transform.Find("MainWindow/ExchangeArea/Load/").gameObject.GetComponent<RectTransform>(), "Load");
            this._confirm.ClearAndAddListener(delegate
            {
                this.OnClick(this._confirm);
            });
            CButton close = obj.transform.Find("MainWindow/Close").GetComponent<CButton>();
            close.ClearAndAddListener(delegate
            {
                this.OnClick(close);
            });
            CButton component = obj.transform.Find("MainWindow/ExchangeArea/Reset").GetComponent<CButton>();
            close.ClearAndAddListener(delegate
            {
                this.OnClick(close);
            });
            this._skillTypeTogGroup = obj.transform.Find("MainWindow/NpcBooks/LifeSkillTypeTogGroup/").gameObject.GetComponent<CToggleGroup>();
            this._gradeTogGroup = obj.transform.Find("MainWindow/NpcBooks/GradeToggleGroup/").gameObject.GetComponent<CToggleGroup>();
            this._skillTypeTogGroup.InitPreOnToggle(-1);
            this._gradeTogGroup.InitPreOnToggle(-1);
            this._skillTypeTogGroup.OnActiveToggleChange = delegate (CToggle p0, CToggle p1)
            {
                this.UpdateCurrList();
            };
            this._gradeTogGroup.OnActiveToggleChange = delegate (CToggle p0, CToggle p1)
            {
                this.UpdateCurrList();
            };
            this._firstPageTogGroup = this._npcItemScroll.transform.Find("ItemSortAndFilter/FirstPageFilter/").gameObject.GetComponent<CToggleGroup>();
            this._directPageTogGroup = this._npcItemScroll.transform.Find("ItemSortAndFilter/DirectPageFilter/").gameObject.GetComponent<CToggleGroup>();
            this._reversePageTogGroup = this._npcItemScroll.transform.Find("ItemSortAndFilter/ReversePageFilter/").gameObject.GetComponent<CToggleGroup>();
            this._completePageTogGroup = this._npcItemScroll.transform.Find("ItemSortAndFilter/CompletePageFilter/").gameObject.GetComponent<CToggleGroup>();
            this._firstPageTogGroup.InitPreOnToggle(-1);
            this._firstPageTogGroup.OnActiveToggleChange = delegate (CToggle p0, CToggle p1)
            {
                this.UpdateCurrList();
            };
            this._directPageTogGroup.InitPreOnToggle(-1);
            this._reversePageTogGroup.InitPreOnToggle(-1);
            this._completePageTogGroup.InitPreOnToggle(-1);
            this._directPageTogGroup.OnActiveToggleChange = delegate (CToggle togNew, CToggle togOld)
            {
                bool flag = togNew != null;
                if (flag)
                {
                    this._reversePageTogGroup.SetWithoutNotify(togNew.Key, false);
                }
                this.UpdateCurrList();
            };
            this._reversePageTogGroup.OnActiveToggleChange = delegate (CToggle togNew, CToggle togOld)
            {
                bool flag = togNew != null;
                if (flag)
                {
                    this._directPageTogGroup.SetWithoutNotify(togNew.Key, false);
                }
                this.UpdateCurrList();
            };
            this._completePageTogGroup.OnActiveToggleChange = delegate (CToggle p0, CToggle p1)
            {
                this.UpdateCurrList();
            };
            obj.SetActive(false);
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00003638 File Offset: 0x00001838
        public override void OnInit(ArgumentBox argsBox)
        {
            this._npcDatas.Clear();
            this._taiwuId = SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
            argsBox.Get("OrganizationId", out this._organizationId);
            string str;
            argsBox.Get("OrganizationName", out str);
            argsBox.Get("IsCombatSkill", out this._isCombatSkill);
            base.CGet<TextMeshProUGUI>("NpcTitle").text = str + "的藏书";
            this._npcItems.Clear();
            this._pagesDatas.Clear();
            this._currItems.Clear();
            this._taiwuLoads = new ValueTuple<int, int>(0, 0);
            this._learnedBooks.Clear();
            this._authorities.Clear();
            this._exchangeNpcScroll = this._exchangeNpcBase.Find("TradeItemScroll").GetComponent<ItemScrollView>();
            this._exchangeNpcScroll.Init();
            this._exchangeNpcScroll.SetItemList(ref this._exchangeList, true, "exchange_book_npc1", this._exchangeNpcScroll.SortAndFilter.IsDetailView, new Action<ItemDisplayData, ItemView>(this.OnRenderTradeNpcItem));
            bool isCombatSkill = this._isCombatSkill;
            if (isCombatSkill)
            {
                for (int i = 1; i < this._skillTypeTogGroup.transform.childCount; i++)
                {
                    this._skillTypeTogGroup.transform.GetChild(i).gameObject.SetActive(i <= 14);
                }
                for (int j = 0; j < 14; j++)
                {
                    Refers component = this._skillTypeTogGroup.Get(j).GetComponent<Refers>();
                    component.CGet<TextMeshProUGUI>("Label").text = Config.CombatSkillType.Instance[j].Name;
                    string filterCombatSkillTypeIcon = CommonUtils.GetFilterCombatSkillTypeIcon(j);
                    component.CGet<CImage>("Icon").SetSprite(filterCombatSkillTypeIcon, false, null);
                    component.CGet<CImage>("Icon").SetSprite(filterCombatSkillTypeIcon, false, null);
                    component.CGet<GameObject>("BookCountBack").SetActive(false);
                }
                bool flag = this.SkillType > 14;
                if (flag)
                {
                    this._skillTypeTogGroup.Set(1, true, false);
                }
            }
            else
            {
                for (int k = 1; k < this._skillTypeTogGroup.transform.childCount; k++)
                {
                    this._skillTypeTogGroup.transform.GetChild(k).gameObject.SetActive(k <= 16);
                }
                for (int l = 0; l < 16; l++)
                {
                    Refers component2 = this._skillTypeTogGroup.Get(l).GetComponent<Refers>();
                    component2.CGet<TextMeshProUGUI>("Label").text = Config.LifeSkillType.Instance[l].Name;
                    string filterLifeSkillTypeIcon = CommonUtils.GetFilterLifeSkillTypeIcon(l);
                    component2.CGet<CImage>("Icon").SetSprite(filterLifeSkillTypeIcon, false, null);
                    component2.CGet<GameObject>("BookCountBack").SetActive(false);
                }
            }
            this._confirm.interactable = false;
            this._npcItemScroll.Init();
            this._npcItemScroll.SetItemList(ref this._currItems, true, "exchange_book_npc", this._npcItemScroll.SortAndFilter.IsDetailView, new Action<ItemDisplayData, ItemView>(this.OnRenderNpcItem));
            this._firstPageTogGroup.SetInteractable(this._isCombatSkill, null);
            this._directPageTogGroup.SetInteractable(this._isCombatSkill, null);
            this._reversePageTogGroup.SetInteractable(this._isCombatSkill, null);
            this.UpdateCurrList();
            ArgumentBox argumentBox = EasyPool.Get<ArgumentBox>();
            argumentBox.Set("ShowBlackMask", false);
            argumentBox.Set("ShowWaitAnimation", true);
            UIElement.FullScreenMask.SetOnInitArgs(argumentBox);
            UIElement.FullScreenMask.Show();
            UIElement uielement = this.Element;
            uielement.OnListenerIdReady = (Action)Delegate.Combine(uielement.OnListenerIdReady, new Action(this.OnListenerIdReady));
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00003A34 File Offset: 0x00001C34
        private void OnListenerIdReady()
        {
            OrganizationDomainHelper.AsyncMethodCall.GetSettlementMembers(null, (short)this._organizationId, delegate (int offset, RawDataPool dataPool)
            {
                List<CharacterDisplayData> list = null;
                Serializer.Deserialize(dataPool, offset, ref list);
                this._npcDatas.AddRange(list);
                bool flag = list.Count > 0;
                if (flag)
                {
                    CharacterDomainHelper.AsyncMethodCall.GetHighestGradeCombatSkillById(null, this._npcDatas[0].CharacterId, delegate (int offsetOrg, RawDataPool dataPoolOrg)
                    {
                        Serializer.Deserialize(dataPoolOrg, offsetOrg, ref this._approveHighestGrave);
                    });
                    base.StartCoroutine(this.CoroutineGetBook());
                }
                for (int i = 0; i < this._npcDatas.Count; i++)
                {
                    this.MonitorFields.Add(new UIBase.MonitorDataField(4, 0, (ulong)((long)this._npcDatas[i].CharacterId), new uint[]
                    {
                        34U
                    }));
                }
                this.Element.MonitorData();
            });
            this.MonitorFields.Add(new UIBase.MonitorDataField(4, 0, (ulong)((long)this._taiwuId), new uint[]
            {
                104U,
                105U,
                34U,
                60U
            }));
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00003A88 File Offset: 0x00001C88
        public override void OnNotifyGameData(List<NotificationWrapper> notifications)
        {
            foreach (NotificationWrapper notificationWrapper in notifications)
            {
                Notification notification = notificationWrapper.Notification;
                bool flag = notification.Type > 0;
                if (!flag)
                {
                    DataUid uid = notification.Uid;
                    bool flag2 = uid.DomainId == 4 && uid.DataId == 0 && (int)uid.SubId0 == this._taiwuId && (uid.SubId1 == 105U || uid.SubId1 == 104U);
                    if (flag2)
                    {
                        int num = 0;
                        Serializer.Deserialize(notificationWrapper.DataPool, notification.ValueOffset, ref num);
                        bool flag3 = uid.SubId1 == 105U;
                        if (flag3)
                        {
                            this._taiwuLoads.Item2 = num;
                        }
                        else
                        {
                            this._taiwuLoads.Item1 = num;
                        }
                    }
                    else
                    {
                        bool flag4 = uid.DomainId == 4 && uid.DataId == 0 && uid.SubId1 == 34U;
                        if (flag4)
                        {
                            ResourceInts resourceInts = default(ResourceInts);
                            Serializer.Deserialize(notificationWrapper.DataPool, notification.ValueOffset, ref resourceInts);
                            bool flag5 = this._authorities.ContainsKey((int)uid.SubId0);
                            if (flag5)
                            {
                                this._authorities[(int)uid.SubId0] = resourceInts.Get(7);
                            }
                            else
                            {
                                this._authorities.Add((int)uid.SubId0, resourceInts.Get(7));
                            }
                        }
                        else
                        {
                            bool flag6 = uid.DomainId == 4 && uid.DataId == 0 && (int)uid.SubId0 == this._taiwuId && uid.SubId1 == 60U;
                            if (flag6)
                            {
                                Serializer.Deserialize(notificationWrapper.DataPool, notification.ValueOffset, ref this._learnedBooks);
                                this._npcItemScroll.ReRender();
                            }
                        }
                    }
                }
            }
            this.UpdateAuthorityAndLoad();
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00003C90 File Offset: 0x00001E90
        private void UpdateAuthorityAndLoad()
        {
            this._totalAuthority = 0;
            int weight = 0;
            RectTransform rectTransform = base.CGet<RectTransform>("SelfAuthority");
            RectTransform rectTransform2 = base.CGet<RectTransform>("Load");
            this._exchangeList.ForEach(delegate (ItemDisplayData element)
            {
                weight += element.Weight;
                this._totalAuthority += element.Price;
            });
            float num = (float)(this._taiwuLoads.Item2 + weight) / 100f;
            string text = string.Format("{0:F1}", num);
            string text2 = (this._totalAuthority > 0) ? string.Format("-{0}", this._totalAuthority).SetColor("brightred") : "";
            rectTransform2.Find("CurValue").GetComponent<TextMeshProUGUI>().text = ((this._taiwuLoads.Item2 + weight > this._taiwuLoads.Item1) ? text.SetColor("brightred") : text);
            rectTransform2.Find("MaxValue").GetComponent<TextMeshProUGUI>().text = string.Format(" / {0:F1}", (float)((double)this._taiwuLoads.Item1 / 100.0));
            rectTransform.Find("Value").GetComponent<TextMeshProUGUI>().text = string.Format("{0}", this._authorities[this._taiwuId]);
            rectTransform.Find("Delta").GetComponent<TextMeshProUGUI>().text = text2;
        }

        // Token: 0x06000019 RID: 25 RVA: 0x00003E18 File Offset: 0x00002018
        private void OnRenderNpcItem(ItemDisplayData itemDisplayData, ItemView itemView)
        {
            MouseTipDisplayer mouseTipDisplayer = itemView.CGet<MouseTipDisplayer>("Tip");
            bool flag = false;
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            CharacterDisplayData characterDisplayData = this._npcDatas[itemDisplayData.SpecialArg];
            bool isCombatSkill = this._isCombatSkill;
            if (isCombatSkill)
            {
                bool flag2 = this.IsLockByApproveEnough(itemDisplayData);
                if (flag2)
                {
                    stringBuilder.Append(LocalStringManager.Get("LK_ExchangeBook_Tip_1"));
                    stringBuilder2.Append(LocalStringManager.Get("LK_ExchangeBook_Tag_1"));
                    flag = true;
                }
                bool flag3 = !this.IsTaiwuLearned(itemDisplayData);
                if (flag3)
                {
                    string text = LocalStringManager.Get("LK_ExchangeBook_Tip_2");
                    stringBuilder.Append(flag ? ("\n" + text) : (text ?? ""));
                    stringBuilder2.Append(flag ? "" : LocalStringManager.Get("LK_ExchangeBook_Tag_2"));
                    flag = true;
                }
            }
            else
            {
                stringBuilder.Append("请找拥有者进行换书");
                flag = true;
            }
            bool flag4 = flag;
            if (flag4)
            {
                itemView.SetLocked(true);
                itemView.GetComponent<PointerTrigger>().enabled = false;
                mouseTipDisplayer.PresetParam = new string[]
                {
                    stringBuilder.ToString().ColorReplace()
                };
                mouseTipDisplayer.Type = TipType.SingleDesc;
                mouseTipDisplayer.RuntimeParam = null;
                mouseTipDisplayer.transform.Find("Lable").GetComponent<TextMeshProUGUI>().text = stringBuilder2.ToString();
                mouseTipDisplayer.gameObject.SetActive(true);
                itemView.SetClickEvent(delegate
                {
                });
            }
            else
            {
                itemView.SetLocked(false);
                itemView.GetComponent<PointerTrigger>().enabled = true;
                mouseTipDisplayer.gameObject.SetActive(false);
                itemView.SetClickEvent(delegate
                {
                    this.PutShopItemToTrade(itemView);
                    this.UpdateAuthorityAndLoad();
                    this.UpdateConfirmButton();
                });
            }
            string arg = NameCenter.GetCharMonasticTitleOrNameByDisplayData(characterDisplayData, false, false).SetGradeColor((int)characterDisplayData.OrgInfo.Grade);
            itemView.CGet<TextMeshProUGUI>("Price").text = string.Format(string.Format("{0} {1}", itemDisplayData.Price, arg), Array.Empty<object>());
        }

        // Token: 0x0600001A RID: 26 RVA: 0x0000406C File Offset: 0x0000226C
        private bool IsTaiwuLearned(ItemDisplayData itemDisplayData)
        {
            return this._learnedBooks.Contains((short)(itemDisplayData.Key.TemplateId - 144));
        }

        // Token: 0x0600001B RID: 27 RVA: 0x0000409C File Offset: 0x0000229C
        private bool IsLockByApproveEnough(ItemDisplayData item)
        {
            return this._approveHighestGrave < ItemTemplateHelper.GetGrade(item.Key.ItemType, item.Key.TemplateId);
        }

        // Token: 0x0600001C RID: 28 RVA: 0x000040D4 File Offset: 0x000022D4
        private void OnRenderTradeNpcItem(ItemDisplayData itemDisplayData, ItemView itemView)
        {
            itemView.SetClickEvent(delegate
            {
                this.PutBookBack(itemView);
                this.UpdateAuthorityAndLoad();
                this.UpdateConfirmButton();
            });
            CharacterDisplayData characterDisplayData = this._npcDatas[itemDisplayData.SpecialArg];
            string arg = NameCenter.GetCharMonasticTitleOrNameByDisplayData(characterDisplayData, false, false).SetGradeColor((int)characterDisplayData.OrgInfo.Grade);
            itemView.CGet<TextMeshProUGUI>("Price").text = string.Format(string.Format("{0} {1}", itemDisplayData.Price, arg), Array.Empty<object>());
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00004170 File Offset: 0x00002370
        private void UpdateConfirmButton()
        {
            bool flag = this._totalAuthority == 0;
            if (flag)
            {
                this._confirm.interactable = false;
            }
            else
            {
                this._confirm.interactable = (this._authorities[this._taiwuId] >= this._totalAuthority);
            }
        }

        // Token: 0x0600001E RID: 30 RVA: 0x000041C8 File Offset: 0x000023C8
        private void PutBookBack(ItemView itemView)
        {
            bool flag = !itemView.IsLocked;
            if (flag)
            {
                ItemDisplayData data = itemView.Data;
                bool flag2 = this.ShouldShowItem(data);
                if (flag2)
                {
                    this._currItems.Add(data);
                    this.UpdateShopDisplay();
                }
                this._exchangeList.Remove(data);
                this.UpdateExchangeAreaDisplay();
            }
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00004224 File Offset: 0x00002424
        private void PutShopItemToTrade(ItemView itemView)
        {
            bool flag = !itemView.IsLocked;
            if (flag)
            {
                ItemDisplayData data = itemView.Data;
                this._exchangeList.Add(data);
                this._currItems.Remove(data);
                this.UpdateShopDisplay();
                this.UpdateExchangeAreaDisplay();
            }
        }

        // Token: 0x06000020 RID: 32 RVA: 0x00004270 File Offset: 0x00002470
        private void UpdateExchangeAreaDisplay()
        {
            this._exchangeNpcScroll.SetItemList(ref this._exchangeList, false, null, false, null);
            this._exchangeNpcBase.Find("NoPick").gameObject.SetActive(this._exchangeList.Count <= 0);
            RectTransform content = this._exchangeNpcScroll.GetComponent<CScrollRect>().Content;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, content.sizeDelta.y + 30f);
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000042EC File Offset: 0x000024EC
        private void SetBookAuthority(ItemDisplayData itemData)
        {
            sbyte behaviorType = this._npcDatas[itemData.SpecialArg].BehaviorType;
            int num = 100;
            switch (behaviorType)
            {
                case 0:
                    num = 200;
                    break;
                case 1:
                    num = 100;
                    break;
                case 2:
                    num = 150;
                    break;
                case 3:
                    num = 100;
                    break;
                case 4:
                    num = 200;
                    break;
            }
            itemData.Price = (int)((double)itemData.Price * (0.5 + 0.5 * (double)itemData.Durability / (double)itemData.MaxDurability) / 10.0 * (double)num / 100.0);
        }

        // Token: 0x06000022 RID: 34 RVA: 0x000043A0 File Offset: 0x000025A0
        public override void QuickHide()
        {
            this.ResetPage();
            foreach (KeyValuePair<int, List<ItemDisplayData>> keyValuePair in this._npcItems)
            {
                List<ItemKey> arg = keyValuePair.Value.ConvertAll<ItemKey>((ItemDisplayData element) => element.Key);
                // GameDataBridge.AddMethodCall<int, List<ItemKey>, bool>(this.Element.GameDataListenerId, 14, 4, keyValuePair.Key, arg, !this._isCombatSkill);
            }
            AudioManager.Instance.PlaySound("ui_default_cancel", false);
            base.QuickHide();
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00004464 File Offset: 0x00002664
        protected override void OnClick(CButton btn)
        {
            bool flag = btn.name == "Close";
            if (flag)
            {
                this.QuickHide();
            }
            else
            {
                bool flag2 = btn.name == "Confirm";
                if (flag2)
                {
                    this.ChangeBook();
                }
                else
                {
                    bool flag3 = btn.name == "Reset";
                    if (flag3)
                    {
                        this.ResetPage();
                    }
                }
            }
        }

        // Token: 0x06000024 RID: 36 RVA: 0x000044CD File Offset: 0x000026CD
        private void ResetPage()
        {
            this._exchangeList.Clear();
            this.UpdateCurrList();
            this.UpdateAuthorityAndLoad();
            this.UpdateExchangeAreaDisplay();
            this._confirm.interactable = false;
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00004500 File Offset: 0x00002700
        private void ChangeBook()
        {
            this._exchangeList.Sort((ItemDisplayData a, ItemDisplayData b) => a.SpecialArg.CompareTo(a.SpecialArg));
            List<ItemDisplayData> boughtItems = new List<ItemDisplayData>();
            int num = this._authorities[this._taiwuId];
            int num2 = 0;
            for (int i = 0; i < this._exchangeList.Count; i++)
            {
                boughtItems.Add(this._exchangeList[i]);
                num2 += this._exchangeList[i].Price;
                int characterId = this._npcDatas[this._exchangeList[i].SpecialArg].CharacterId;
                this._npcItems[characterId].Remove(this._exchangeList[i]);
                bool flag = i == this._exchangeList.Count - 1 || this._exchangeList[i].SpecialArg != this._exchangeList[i + 1].SpecialArg;
                if (flag)
                {
                    num -= num2;
                    int arg = this._authorities[characterId] + num2;
                    EventActorData eventActorData = new EventActorData();
                    eventActorData.AvatarData = new AvatarData();
                    TaiwuEventDomainHelper.MethodCall.StartNewDialog(this._taiwuId, characterId, "换书", "换书", eventActorData, eventActorData);
                    MerchantDomainHelper.MethodCall.ExchangeBook(characterId, boughtItems, new List<ItemDisplayData>(), num, arg);
                    num2 = 0;
                    boughtItems.Clear();
                }
            }
            this._exchangeList.Clear();
            this.UpdateExchangeAreaDisplay();
            this._confirm.interactable = false;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000046C8 File Offset: 0x000028C8
        private void GetBook(int index)
        {
            bool isCombatSkill = this._isCombatSkill;
            if (isCombatSkill)
            {
                MerchantDomainHelper.AsyncMethodCall.GetTradeBookDisplayData(null, this._npcDatas[index].CharacterId, false, delegate (int offset, RawDataPool dataPool)
                {
                    List<ItemDisplayData> list = new List<ItemDisplayData>();
                    Serializer.Deserialize(dataPool, offset, ref list);
                    this._npcItems.Add(this._npcDatas[index].CharacterId, list);
                    foreach (ItemDisplayData itemDisplayData in list)
                    {
                        itemDisplayData.SpecialArg = index;
                        this.SetBookAuthority(itemDisplayData);
                        ItemDomainHelper.AsyncMethodCall.GetSkillBookPagesInfo(null, itemDisplayData.Key, new AsyncMethodCallbackDelegate(this.OnGetPageInfo));
                    }
                });
            }
            else
            {
                MerchantDomainHelper.AsyncMethodCall.GetTradeBookDisplayData(null, this._npcDatas[index].CharacterId, true, delegate (int offset, RawDataPool dataPool)
                {
                    List<ItemDisplayData> list = new List<ItemDisplayData>();
                    Serializer.Deserialize(dataPool, offset, ref list);
                    this._npcItems.Add(this._npcDatas[index].CharacterId, list);
                    foreach (ItemDisplayData itemDisplayData in list)
                    {
                        bool flag = SkillBook.Instance[itemDisplayData.Key.TemplateId].ItemSubType == 1000;
                        if (flag)
                        {
                            itemDisplayData.SpecialArg = index;
                            this.SetBookAuthority(itemDisplayData);
                            ItemDomainHelper.AsyncMethodCall.GetSkillBookPagesInfo(null, itemDisplayData.Key, new AsyncMethodCallbackDelegate(this.OnGetPageInfo));
                        }
                    }
                });
            }
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00004753 File Offset: 0x00002953
        private IEnumerator CoroutineGetBook()
        {
            int num;
            for (int i = 0; i < this._npcDatas.Count; i = num + 1)
            {
                this.GetBook(i);
                num = i;
            }
            while (this._npcItems.Count < this._npcDatas.Count)
            {
                yield return null;
            }
            int totalBooks = 0;
            foreach (List<ItemDisplayData> list in this._npcItems.Values)
            {
                int num2;
                if (this._isCombatSkill)
                {
                    num2 = totalBooks + list.Count;
                }
                else
                {
                    num2 = totalBooks + list.Count((ItemDisplayData data) => SkillBook.Instance[data.Key.TemplateId].ItemSubType == 1000);
                }
                totalBooks = num2;
            }
            Dictionary<int, List<ItemDisplayData>>.ValueCollection.Enumerator enumerator = default(Dictionary<int, List<ItemDisplayData>>.ValueCollection.Enumerator);
            while (this._pagesDatas.Count < totalBooks)
            {
                yield return null;
            }
            this.UpdateCurrList();
            this.Element.ShowAfterRefresh();
            UIElement.FullScreenMask.Hide(false);
            yield break;
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00004764 File Offset: 0x00002964
        private void OnGetPageInfo(int offset, RawDataPool dataPool)
        {
            SkillBookPageDisplayData skillBookPageDisplayData = null;
            Serializer.Deserialize(dataPool, offset, ref skillBookPageDisplayData);
            this._pagesDatas.Add(skillBookPageDisplayData.ItemKey.Id, new ValueTuple<sbyte[], sbyte[]>(skillBookPageDisplayData.Type, skillBookPageDisplayData.State));
        }

        // Token: 0x04000008 RID: 8
        private static UIElement element;

        // Token: 0x04000009 RID: 9
        private int _organizationId;

        // Token: 0x0400000A RID: 10
        private int _taiwuId;

        // Token: 0x0400000B RID: 11
        private readonly List<CharacterDisplayData> _npcDatas = new List<CharacterDisplayData>();

        // Token: 0x0400000C RID: 12
        private ValueTuple<int, int> _taiwuLoads;

        // Token: 0x0400000D RID: 13
        private int _totalAuthority;

        // Token: 0x0400000E RID: 14
        private sbyte _approveHighestGrave;

        // Token: 0x0400000F RID: 15
        private readonly Dictionary<int, int> _authorities = new Dictionary<int, int>();

        // Token: 0x04000010 RID: 16
        private readonly Dictionary<int, ValueTuple<sbyte[], sbyte[]>> _pagesDatas = new Dictionary<int, ValueTuple<sbyte[], sbyte[]>>();

        // Token: 0x04000011 RID: 17
        private readonly Dictionary<int, List<ItemDisplayData>> _npcItems = new Dictionary<int, List<ItemDisplayData>>();

        // Token: 0x04000012 RID: 18
        private List<ItemDisplayData> _currItems = new List<ItemDisplayData>();

        // Token: 0x04000013 RID: 19
        private List<ItemDisplayData> _exchangeList = new List<ItemDisplayData>();

        // Token: 0x04000014 RID: 20
        private ItemScrollView _npcItemScroll;

        // Token: 0x04000015 RID: 21
        private ItemScrollView _exchangeNpcScroll;

        // Token: 0x04000016 RID: 22
        private List<short> _learnedBooks = new List<short>();

        // Token: 0x04000017 RID: 23
        private RectTransform _exchangeNpcBase;

        // Token: 0x04000018 RID: 24
        private CToggleGroup _gradeTogGroup;

        // Token: 0x04000019 RID: 25
        private CToggleGroup _skillTypeTogGroup;

        // Token: 0x0400001A RID: 26
        private CToggleGroup _firstPageTogGroup;

        // Token: 0x0400001B RID: 27
        private CToggleGroup _directPageTogGroup;

        // Token: 0x0400001C RID: 28
        private CToggleGroup _reversePageTogGroup;

        // Token: 0x0400001D RID: 29
        private CToggleGroup _completePageTogGroup;

        // Token: 0x0400001E RID: 30
        private CButton _confirm;

        // Token: 0x0400001F RID: 31
        private bool _isCombatSkill;
    }

}
