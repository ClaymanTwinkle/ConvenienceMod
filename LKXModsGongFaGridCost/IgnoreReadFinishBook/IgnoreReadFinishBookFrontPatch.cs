using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using GameData.Domains.Item.Display;
using GameData.Domains.Item;
using HarmonyLib;

namespace ConvenienceFrontend.IgnoreReadFinishBook
{
    internal class IgnoreReadFinishBookFrontPatch : BaseFrontPatch
    {
        private static bool _enableFilterReadFinishBook = false;

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
        }

        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "Toggle_EnableFilterReadFinishBook", ref _enableFilterReadFinishBook);
            // UpdateReadingProgressOnMonthChange

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UI_Reading), "RefreshBookList")]
        public static bool UI_Reading_RefreshBookList_Prefix(
            bool isRef, 
            bool isOnlyShowReadFinished, 
            List<ItemKey> ____availableReferenceBookList, 
            List<ItemKey> ____availableBookList,
            CToggleGroup ____skillTypeTogGroup,
            CToggleGroup ____skillSubTypeTogGroup,
            CToggleGroup ____refSkillTypeTogGroup,
            CToggleGroup ____refSkillSubTypeTogGroup,
            List<ItemDisplayData> ____combatSkillBooks,
            List<ItemDisplayData> ____lifeSkillBooks,
            Dictionary<int, sbyte> ____allBookReadingProgressList,
            ItemKey ____curReadingBook,
            ItemKey[] ____referenceBooks
        )
        {
            if (!_enableFilterReadFinishBook) return true;

            List< ItemKey> list = (isRef ? ____availableReferenceBookList : ____availableBookList);
            list.Clear();
            int num = (isRef ? ____refSkillSubTypeTogGroup.GetActive().Key : ____skillSubTypeTogGroup.GetActive().Key);
            bool flag = (isRef ? ____refSkillTypeTogGroup.GetActive().Key == 0 : ____skillTypeTogGroup.GetActive().Key == 0);
            List<ItemDisplayData> list2 = (flag ? ____combatSkillBooks : ____lifeSkillBooks);
            foreach (ItemDisplayData item in list2)
            {
                SkillBookItem skillBookItem = SkillBook.Instance[item.Key.TemplateId];
                if (____allBookReadingProgressList.ContainsKey(item.Key.Id) && (!isRef || !____curReadingBook.Equals(item.Key)) && (isRef || !____referenceBooks.Contains(item.Key)))
                {
                    bool flag2 = !isOnlyShowReadFinished || ((isRef && ____allBookReadingProgressList[item.Key.Id] >= 100) || (!isRef && ____allBookReadingProgressList[item.Key.Id] < 100));
                    bool flag3 = ((!flag) ? (skillBookItem.LifeSkillType == num || num == -1) : (skillBookItem.CombatSkillType == num || num == -1));
                    if (flag3 && flag2)
                    {
                        list.Add(item.Key);
                    }
                }
            }
            return false;
        }
    }
}
