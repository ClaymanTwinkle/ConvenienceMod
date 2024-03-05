using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.ArchiveData;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Item;
using GameData.Domains.Taiwu;
using GameData.Domains;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Redzen.Random;

namespace ConvenienceBackend.MergeBookPanel
{
    internal class MergeBookPanelBackendPatch : BaseBackendPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDomain), "CallMethod")]
        public static bool CallMethodCharacterPrefix(Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context, CharacterDomain __instance, ref int __result)
        {
            int num = operation.ArgsOffset;
            if (operation.MethodId == 151)
            {
                int argsCount = operation.ArgsCount;
                if (argsCount == 6)
                {
                    ItemKey itemKey = default(ItemKey);
                    num += Serializer.Deserialize(argDataPool, num, ref itemKey);
                    short num2 = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref num2);
                    ushort num3 = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref num3);
                    byte b = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref b);
                    ushort num4 = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref num4);
                    short num5 = 0;
                    Serializer.Deserialize(argDataPool, num, ref num5);
                    GameData.Domains.Character.Character element_Objects = __instance.GetElement_Objects(DomainManager.Taiwu.GetTaiwuCharId());
                    ItemDomain item = DomainManager.Item;
                    IRandomSource random = context.Random;
                    int num6 = item.CallPrivateMethod<int>("GenerateNextItemId", new object[]
                    {
                        context
                    });
                    GameData.Domains.Item.SkillBook skillBook = new GameData.Domains.Item.SkillBook(random, itemKey.TemplateId, num6, -1, -1, -1, 50, true);
                    skillBook.SetPrivateField("_pageTypes", b);
                    skillBook.SetPrivateField("MaxDurability", num2);
                    skillBook.SetPrivateField("CurrDurability", num2);
                    skillBook.SetPrivateField("_pageIncompleteState", num3);
                    item.CallPrivateMethod("AddElement_SkillBooks", new object[]
                    {
                        num6,
                        skillBook
                    });
                    element_Objects.AddInventoryItem(context, skillBook.GetItemKey(), 1);
                    if (SkillGroup.FromItemSubType(skillBook.GetItemSubType()) != 0)
                    {
                        int num7 = item.CallPrivateMethod<int>("GenerateNextItemId", new object[]
                        {
                            context
                        });
                        GameData.Domains.Item.SkillBook skillBook2 = new GameData.Domains.Item.SkillBook(random, itemKey.TemplateId, num7, -1, -1, -1, 50, true);
                        skillBook2.SetPrivateField("_pageTypes", SkillBookStateHelper.SetOutlinePageType((byte)~b, (sbyte)(b & 7)));
                        skillBook2.SetPrivateField("MaxDurability", num5);
                        skillBook2.SetPrivateField("CurrDurability", num5);
                        skillBook2.SetPrivateField("_pageIncompleteState", num4);
                        item.CallPrivateMethod("AddElement_SkillBooks", new object[]
                        {
                            num7,
                            skillBook2
                        });
                        element_Objects.AddInventoryItem(context, skillBook2.GetItemKey(), 1);
                    }
                }
                else if (argsCount == 4)
                {
                    ItemKey itemKey2 = default(ItemKey);
                    num += Serializer.Deserialize(argDataPool, num, ref itemKey2);
                    short num8 = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref num8);
                    ushort num9 = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref num9);
                    byte b2 = 0;
                    Serializer.Deserialize(argDataPool, num, ref b2);
                    GameData.Domains.Character.Character element_Objects2 = __instance.GetElement_Objects(DomainManager.Taiwu.GetTaiwuCharId());
                    ItemDomain item2 = DomainManager.Item;
                    IRandomSource random2 = context.Random;
                    int num10 = item2.CallPrivateMethod<int>("GenerateNextItemId", new object[]
                    {
                        context
                    });
                    GameData.Domains.Item.SkillBook skillBook3 = new GameData.Domains.Item.SkillBook(random2, itemKey2.TemplateId, num10, -1, -1, -1, 50, true);
                    skillBook3.SetPrivateField("_pageTypes", b2);
                    skillBook3.SetPrivateField("MaxDurability", num8);
                    skillBook3.SetPrivateField("CurrDurability", num8);
                    skillBook3.SetPrivateField("_pageIncompleteState", num9);
                    item2.CallPrivateMethod("AddElement_SkillBooks", new object[]
                    {
                        num10,
                        skillBook3
                    });
                    element_Objects2.AddInventoryItem(context, skillBook3.GetItemKey(), 1);
                }
                return false;
            }
            if (operation.MethodId == 154)
            {
                if (operation.ArgsCount == 5)
                {
                    ItemKey itemKey3 = default(ItemKey);
                    num += Serializer.Deserialize(argDataPool, num, ref itemKey3);
                    short durability = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref durability);
                    short maxDurability = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref maxDurability);
                    byte pageType = 0;
                    num += Serializer.Deserialize(argDataPool, num, ref pageType);
                    bool isCustom = false;
                    Serializer.Deserialize(argDataPool, num, ref isCustom);
                    TransformBook(itemKey3, durability, maxDurability, pageType, isCustom, context);
                }
                return false;
            }
            if (operation.MethodId == 155)
            {
                Dictionary<short, List<ItemKey>> dictionary = new Dictionary<short, List<ItemKey>>();
                GameData.Domains.Character.Character element_Objects3 = DomainManager.Character.GetElement_Objects(DomainManager.Taiwu.GetTaiwuCharId());
                foreach (ItemKey itemKey4 in element_Objects3.GetInventory().Items.Keys)
                {
                    if (itemKey4.ItemType == 6)
                    {
                        if (!dictionary.ContainsKey(itemKey4.TemplateId))
                        {
                            dictionary.Add(itemKey4.TemplateId, new List<ItemKey>());
                        }
                        dictionary[itemKey4.TemplateId].Add(itemKey4);
                    }
                }
                foreach (List<ItemKey> list in dictionary.Values)
                {
                    ItemKey itemKey5 = DomainManager.Item.CreateItem(context, list[0].ItemType, list[0].TemplateId);
                    short num11 = 0;
                    short num12 = 0;
                    foreach (ItemKey itemKey6 in list)
                    {
                        ItemBase baseItem = DomainManager.Item.GetBaseItem(itemKey6);
                        num11 = (short)((num11 + baseItem.GetCurrDurability()) % 1000);
                        num12 = (short)((num12 + baseItem.GetMaxDurability()) % 1000);
                    }
                    GameData.Domains.Item.CraftTool element_CraftTools = DomainManager.Item.GetElement_CraftTools(itemKey5.Id);
                    element_CraftTools.SetMaxDurability(num12, context);
                    element_CraftTools.SetCurrDurability(num11, context);
                    element_Objects3.AddInventoryItem(context, itemKey5, 1);
                    DomainManager.Item.DiscardItemList(context, element_Objects3.GetId(), list);
                }
                return false;
            }
            if (operation.MethodId == 156)
            {
                new List<ItemKey>();
                Dictionary<short, List<ItemKey>> dictionary2 = new Dictionary<short, List<ItemKey>>();
                GameData.Domains.Character.Character element_Objects4 = DomainManager.Character.GetElement_Objects(DomainManager.Taiwu.GetTaiwuCharId());
                foreach (ItemKey itemKey7 in element_Objects4.GetInventory().Items.Keys)
                {
                    if (itemKey7.ItemType == 10 && Config.SkillBook.Instance[itemKey7.TemplateId].ItemSubType == 1000)
                    {
                        if (!dictionary2.ContainsKey(itemKey7.TemplateId))
                        {
                            dictionary2.Add(itemKey7.TemplateId, new List<ItemKey>());
                        }
                        dictionary2[itemKey7.TemplateId].Add(itemKey7);
                    }
                }
                foreach (List<ItemKey> list2 in dictionary2.Values)
                {
                    SCBookInfo scbookInfo = CalcuBookState(list2);
                    ItemKey itemKey8 = DomainManager.Item.CreateItem(context, list2[0].ItemType, list2[0].TemplateId);
                    GameData.Domains.Item.SkillBook element_SkillBooks = DomainManager.Item.GetElement_SkillBooks(itemKey8.Id);
                    short maxDurability2 = DomainManager.Item.GetElement_SkillBooks(list2[0].Id).GetMaxDurability();
                    element_SkillBooks.SetMaxDurability((list2.Count == 1) ? maxDurability2 : scbookInfo.durability, context);
                    element_SkillBooks.SetCurrDurability(scbookInfo.durability, context);
                    element_SkillBooks.SetPageIncompleteState(scbookInfo.completeState, context);
                    element_Objects4.AddInventoryItem(context, itemKey8, 1);
                    DomainManager.Item.DiscardItemList(context, element_Objects4.GetId(), list2);
                }
                return false;
            }
            return true;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000027E8 File Offset: 0x000009E8
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemDomain), "CallMethod")]
        public static bool CallMethodItemPrefix(Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context, ItemDomain __instance, ref int __result)
        {
            int argsOffset = operation.ArgsOffset;
            if (operation.MethodId == 157)
            {
                ItemKey itemKey = default(ItemKey);
                Serializer.Deserialize(argDataPool, argsOffset, ref itemKey);
                sbyte[] bookReadingProgress = GetBookReadingProgress(itemKey);
                __result = Serializer.Serialize(bookReadingProgress, returnDataPool);
                return false;
            }
            return true;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002830 File Offset: 0x00000A30
        private static SCBookInfo CalcuBookState(List<ItemKey> bookList)
        {
            SCBookInfo result = default(SCBookInfo);
            ItemDomain item = DomainManager.Item;
            sbyte[] array = new sbyte[]
            {
                2,
                2,
                2,
                2,
                2
            };
            List<sbyte[]> list = new List<sbyte[]>();
            List<short> list2 = new List<short>();
            if (bookList.Count == 1)
            {
                GameData.Domains.Item.SkillBook element_SkillBooks = item.GetElement_SkillBooks(bookList[0].Id);
                return new SCBookInfo
                {
                    completeState = element_SkillBooks.GetPageIncompleteState(),
                    durability = element_SkillBooks.GetCurrDurability()
                };
            }
            for (int i = 0; i < bookList.Count; i++)
            {
                GameData.Domains.Item.SkillBook element_SkillBooks2 = item.GetElement_SkillBooks(bookList[i].Id);
                ushort pageIncompleteState = element_SkillBooks2.GetPageIncompleteState();
                list2.Add(element_SkillBooks2.GetCurrDurability());
                sbyte[] array2 = new sbyte[5];
                for (int j = 0; j < 5; j++)
                {
                    array2[j] = SkillBookStateHelper.GetPageIncompleteState(pageIncompleteState, (byte)j);
                    array[j] = ((array[j] > array2[j]) ? array2[j] : array[j]);
                }
                list.Add(array2);
            }
            ushort num = 0;
            for (int k = 0; k < 5; k++)
            {
                num |= (ushort)(array[k] << k * 2);
            }
            result.completeState = num;
            float num2 = list2.Sum((short d) => (float)d);
            for (int l = 0; l < list.Count; l++)
            {
                for (int m = 0; m < 5; m++)
                {
                    if (array[m] == 0 && list[l][m] == 1)
                    {
                        num2 -= (float)list2[l] * 0.1f;
                    }
                    else if (array[m] == 0 && list[l][m] == 2)
                    {
                        num2 -= (float)list2[l] * 0.18f;
                    }
                    else if (array[m] == 1 && list[l][m] == 2)
                    {
                        num2 -= (float)list2[l] * 0.08f;
                    }
                }
            }
            result.durability = (short)num2.Clamp(1f, 999f);
            return result;
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002A60 File Offset: 0x00000C60
        private static sbyte[] GetBookReadingProgress(ItemKey itemKey)
        {
            GameData.Domains.Item.SkillBook element_SkillBooks = DomainManager.Item.GetElement_SkillBooks(itemKey.Id);
            sbyte[] array = new sbyte[15];
            if (SkillGroup.FromItemSubType(element_SkillBooks.GetItemSubType()) == 0)
            {
                short lifeSkillTemplateId = element_SkillBooks.GetLifeSkillTemplateId();
                TaiwuLifeSkill taiwuLifeSkill;
                TaiwuLifeSkill taiwuLifeSkill2;
                if (DomainManager.Taiwu.TryGetTaiwuLifeSkill(lifeSkillTemplateId, out taiwuLifeSkill))
                {
                    sbyte[] allBookPageReadingProgress = taiwuLifeSkill.GetAllBookPageReadingProgress();
                    if (allBookPageReadingProgress != null)
                    {
                        allBookPageReadingProgress.CopyTo(array, 0);
                    }
                }
                else if (DomainManager.Taiwu.TryGetNotLearnLifeSkillReadingProgress(lifeSkillTemplateId, out taiwuLifeSkill2))
                {
                    sbyte[] allBookPageReadingProgress2 = taiwuLifeSkill2.GetAllBookPageReadingProgress();
                    if (allBookPageReadingProgress2 != null)
                    {
                        allBookPageReadingProgress2.CopyTo(array, 0);
                    }
                }
            }
            else
            {
                short combatSkillTemplateId = element_SkillBooks.GetCombatSkillTemplateId();
                TaiwuCombatSkill taiwuCombatSkill;
                TaiwuCombatSkill taiwuCombatSkill2;
                if (DomainManager.Taiwu.TryGetElement_CombatSkills(combatSkillTemplateId, out taiwuCombatSkill))
                {
                    sbyte[] allBookPageReadingProgress3 = taiwuCombatSkill.GetAllBookPageReadingProgress();
                    if (allBookPageReadingProgress3 != null)
                    {
                        allBookPageReadingProgress3.CopyTo(array, 0);
                    }
                }
                else if (DomainManager.Taiwu.TryGetNotLearnCombatSkillReadingProgress(combatSkillTemplateId, out taiwuCombatSkill2))
                {
                    sbyte[] allBookPageReadingProgress4 = taiwuCombatSkill2.GetAllBookPageReadingProgress();
                    if (allBookPageReadingProgress4 != null)
                    {
                        allBookPageReadingProgress4.CopyTo(array, 0);
                    }
                }
            }
            return array;
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002B3C File Offset: 0x00000D3C
        private unsafe static void TransformBook(ItemKey itemKey, short durability, short maxDurability, byte pageType, bool isCustom, DataContext context)
        {
            GameData.Domains.Item.SkillBook element_SkillBooks = DomainManager.Item.GetElement_SkillBooks(itemKey.Id);
            if (SkillGroup.FromItemSubType(element_SkillBooks.GetItemSubType()) == 0)
            {
                sbyte[] array = new sbyte[5];
                short lifeSkillTemplateId = element_SkillBooks.GetLifeSkillTemplateId();
                TaiwuLifeSkill taiwuLifeSkill;
                TaiwuLifeSkill taiwuLifeSkill2;
                if (DomainManager.Taiwu.TryGetTaiwuLifeSkill(lifeSkillTemplateId, out taiwuLifeSkill))
                {
                    array = taiwuLifeSkill.GetAllBookPageReadingProgress();
                }
                else if (DomainManager.Taiwu.TryGetNotLearnLifeSkillReadingProgress(lifeSkillTemplateId, out taiwuLifeSkill2))
                {
                    array = taiwuLifeSkill2.GetAllBookPageReadingProgress();
                }
                ushort num = element_SkillBooks.GetPageIncompleteState();
                for (int i = 0; i < array.Length; i++)
                {
                    int num2 = (array[i] == 100) ? (~(3 << i * 2)) : -1;
                    num = (ushort)((int)num & num2);
                }
                if (isCustom)
                {
                    num = 0;
                }
                element_SkillBooks.SetPageIncompleteState(num, context);
                return;
            }
            sbyte[] array2 = new sbyte[15];
            short combatSkillTemplateId = element_SkillBooks.GetCombatSkillTemplateId();
            TaiwuCombatSkill taiwuCombatSkill;
            TaiwuCombatSkill taiwuCombatSkill2;
            if (DomainManager.Taiwu.TryGetElement_CombatSkills(combatSkillTemplateId, out taiwuCombatSkill))
            {
                array2 = taiwuCombatSkill.GetAllBookPageReadingProgress();
            }
            else if (DomainManager.Taiwu.TryGetNotLearnCombatSkillReadingProgress(combatSkillTemplateId, out taiwuCombatSkill2))
            {
                array2 = taiwuCombatSkill2.GetAllBookPageReadingProgress();
            }
            ushort num3 = element_SkillBooks.GetPageIncompleteState();
            for (int j = 1; j < 6; j++)
            {
                int num4 = (int)(5 + SkillBookStateHelper.GetNormalPageType(pageType, (byte)j) * 5) + j - 1;
                int num5 = (array2[num4] == 100) ? (~(3 << j * 2)) : -1;
                num3 = (ushort)((int)num3 & num5);
            }
            if (isCustom)
            {
                num3 = 0;
            }
            element_SkillBooks.SetPrivateField("_pageTypes", pageType);
            element_SkillBooks.CallPrivateMethod("SetModifiedAndInvalidateInfluencedCache", new object[]
            {
                6,
                context
            });
            if (element_SkillBooks.CollectionHelperData.IsArchive)
            {
                *OperationAdder.DynamicObjectCollection_SetFixedField<int>(element_SkillBooks.CollectionHelperData.DomainId, element_SkillBooks.CollectionHelperData.DataId, element_SkillBooks.GetId(), 11U, 1) = pageType;
            }
            element_SkillBooks.SetPageIncompleteState(num3, context);
        }

        // Token: 0x04000001 RID: 1
        public const ushort MethodIDCreateBook = 151;

        // Token: 0x04000002 RID: 2
        public const ushort MethodIDTransformBook = 154;

        // Token: 0x04000003 RID: 3
        public const ushort MethodIDMergeAllTools = 155;

        // Token: 0x04000004 RID: 4
        public const ushort MethodIDMergeAllLifeBooks = 156;

        // Token: 0x04000005 RID: 5
        public const ushort MethodIDGetBookReadingProgress = 157;

        // Token: 0x04000006 RID: 6
        public const sbyte Invalid = -1;

        // Token: 0x04000007 RID: 7
        public const sbyte Complete = 0;

        // Token: 0x04000008 RID: 8
        public const sbyte Incomplete = 1;

        // Token: 0x04000009 RID: 9
        public const sbyte Lost = 2;

        // Token: 0x02000007 RID: 7
        private struct SCBookInfo
        {
            // Token: 0x0400000B RID: 11
            public ushort completeState;

            // Token: 0x0400000C RID: 12
            public short durability;
        }
    }
}
