using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using FrameWork;
using GameData.Domains.Item;
using GameData.Domains.Item.Display;
using GameData.GameDataBridge;
using GameData.Utilities;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceFrontend.MergeBookPanel
{
    public class MergeAll
    {
        // Token: 0x06000006 RID: 6 RVA: 0x00002114 File Offset: 0x00000314
        public static void MergeAllTools()
        {
            MergeAll.isMerging = true;
            GameLog.LogMessage("MergeAllTools Start isMerging = true");
            UI_CharacterMenuItems itemMenu = UIElement.CharacterMenuItems.UiBaseAs<UI_CharacterMenuItems>();
            UI_CharacterMenu characterMenu = UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>();
            List<ItemDisplayData> toollist = (from item in itemMenu.GetFieldValue<List<ItemDisplayData>>("_inventoryItems")
                                              where item.Key.ItemType == 6
                                              select item).ToList<ItemDisplayData>();
            if (toollist.Count == 0)
            {
                MergeAll.isMerging = false;
                return;
            }
            MergeAll.RemoveMergedItems(toollist);
            SingletonObject.getInstance<AsyncMethodDispatcher>().AsyncMethodCall(4, ModMono.MethodIDMergeAllTools, delegate (int offset, RawDataPool dataPool) {
                itemMenu.SetPrivateField("_needRefresh", true);
                GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 28, characterMenu.CurCharacterId);
                GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 30, characterMenu.CurCharacterId);
                MergeAll.isMerging = false;
            });

            //itemMenu.AsynchMethodCall(4, 155, delegate (int p0, RawDataPool p1)
            //{
            //    itemMenu.SetPrivateField("_needRefresh", true);
            //    GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 28, characterMenu.CurCharacterId);
            //    GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 30, characterMenu.CurCharacterId);
            //    MergeAll.isMerging = false;
            //});
            MergeAll.ShowDialog("合并结果", "合并成功", null);
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000021D4 File Offset: 0x000003D4
        public static void MergeAllLifeBooks()
        {
            MergeAll.isMerging = true;
            GameLog.LogMessage("MergeAllLifeBooks Start isMerging = true");
            UI_CharacterMenu characterMenu = UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>();
            UI_CharacterMenuItems itemMenu = (UI_CharacterMenuItems)characterMenu.AllSubPages[3];
            itemMenu.GetFieldValue("_itemScroll");
            List<ItemDisplayData> booklist = (from item in itemMenu.GetFieldValue<List<ItemDisplayData>>("_inventoryItems")
                                              where item.Key.ItemType == 10 && SkillBook.Instance[item.Key.TemplateId].ItemSubType == 1000
                                              select item).ToList<ItemDisplayData>();
            if (booklist.Count == 0)
            {
                MergeAll.isMerging = false;
                return;
            }
            MergeAll.RemoveMergedItems(booklist);

            SingletonObject.getInstance<AsyncMethodDispatcher>().AsyncMethodCall(4, ModMono.MethodIDMergeAllLifeBooks, delegate (int offset, RawDataPool dataPool) {
                itemMenu.SetPrivateField("_needRefresh", true);
                GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 28, characterMenu.CurCharacterId);
                GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 30, characterMenu.CurCharacterId);
                MergeAll.isMerging = false;
            });

            //itemMenu.AsynchMethodCall(4, 156, delegate (int p0, RawDataPool p1)
            //{
            //    itemMenu.SetPrivateField("_needRefresh", true);
            //    GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 28, characterMenu.CurCharacterId);
            //    GameDataBridge.AddMethodCall<int>(itemMenu.Element.GameDataListenerId, 4, 30, characterMenu.CurCharacterId);
            //    MergeAll.isMerging = false;
            //});
            MergeAll.ShowDialog("合并结果", "合并成功", null);
        }

        // Token: 0x06000008 RID: 8 RVA: 0x000022AC File Offset: 0x000004AC
        private static void RemoveMergedItems(List<ItemDisplayData> itemList)
        {
            UI_CharacterMenuItems itemMenu = (UI_CharacterMenuItems)UIElement.CharacterMenu.UiBaseAs<UI_CharacterMenu>().AllSubPages[3];
            ItemScrollView itemScroll = itemMenu.GetFieldValue<ItemScrollView>("_itemScroll");
            List<ItemDisplayData> inventoryItems = itemMenu.GetFieldValue<List<ItemDisplayData>>("_inventoryItems");
            for (int i = 0; i < itemList.Count; i++)
            {
                ItemDisplayData item = itemList[i];
                inventoryItems.Remove(item);
                itemMenu.CallPrivateMethod("ClearItemUsingState", new object[]
                {
                    item
                });
            }
            itemScroll.SetItemList(ref inventoryItems, false, null, false, null);
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002330 File Offset: 0x00000530
        public static void ShowDialog(string title, string message, Action onYes)
        {
            DialogCmd dialogCmd = new DialogCmd();
            dialogCmd.Type = 1;
            dialogCmd.Title = title;
            dialogCmd.Content = message;
            if (onYes != null)
            {
                dialogCmd.Yes = onYes;
            }
            UIElement.Dialog.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("Cmd", dialogCmd));
            UIManager.Instance.ShowUI(UIElement.Dialog);
        }

        // Token: 0x04000001 RID: 1
        public static bool isMerging;

        // Token: 0x04000002 RID: 2
        public const int TogKeyItem = 3;
    }
}
