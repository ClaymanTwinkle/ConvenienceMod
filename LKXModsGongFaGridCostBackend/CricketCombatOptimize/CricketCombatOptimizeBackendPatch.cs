using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.Utils;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Extra;
using GameData.Domains.Item;
using GameData.Domains.Taiwu;
using GameData.Domains.TaiwuEvent;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Domains.TaiwuEvent.EventOption;
using GameData.Utilities;
using HarmonyLib;
using NLog;
using Redzen.Random;

namespace ConvenienceBackend.CricketCombatOptimize
{
    internal class CricketCombatOptimizeBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("蛐蛐优化");

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        /*        [HarmonyPrefix]
                [HarmonyPatch(typeof(OptionConditionMatcher), "InteractionOffCooldown")]
                public static bool OptionConditionMatcher_InteractionOffCooldown_PrePatch(int arg0, short arg1, ref bool __result)
                {
                    if (7 == arg1)
                    {
                        // 无限制斗促织
                        __result = true;
                        return false;
                    }

                    return true;
                }*/

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemDomain), "CalcEnemyWagers")]
        public static bool ItemDomain_CalcEnemyWagers_PrePatch(ItemDomain __instance, IRandomSource random, GameData.Domains.Character.Character character, ref IEnumerable<Wager> __result)
        {
            var returnResult = new List<Wager>();

            sbyte charGrade = character.GetOrganizationInfo().Grade;
            sbyte taiwuFame = DomainManager.Taiwu.GetTaiwu().GetFame();
            (sbyte, sbyte) tuple = CricketSpecialConstants.CalcWagerGradeRange(charGrade, taiwuFame);
            sbyte minGrade = tuple.Item1;
            sbyte maxGrade = tuple.Item2;
            List<ItemKey> itemPool = ObjectPool<List<ItemKey>>.Instance.Get();
            Dictionary<ItemKey, int> items = character.GetInventory().Items;
            foreach (Func<ItemKey, bool> matcher in CricketSpecialConstants.WagerItemMatchers)
            {
                itemPool.Clear();
                itemPool.AddRange(items.Keys.Where(matcher).Where(new Func<ItemKey, bool>(GradeMatcher)));
                itemPool.RemoveAll((ItemKey x) => __instance.GetBaseItem(x).GetPrice() < 1);
                if (itemPool.Count != 0)
                {
                    sbyte highestGrade = itemPool.Max((ItemKey x) => ItemTemplateHelper.GetGrade(x.ItemType, x.TemplateId));
                    // itemPool.RemoveAll((ItemKey x) => ItemTemplateHelper.GetGrade(x.ItemType, x.TemplateId) < highestGrade);
                    itemPool.ForEach(itemKey2 => returnResult.Add(Wager.CreateItem(itemKey2, 1)));
                }
            }
            var equipment = character.GetEquipment();
            foreach (Func<ItemKey, bool> matcher in CricketSpecialConstants.WagerItemMatchers)
            {
                itemPool.Clear();
                itemPool.AddRange(equipment.Where(matcher).Where(new Func<ItemKey, bool>(GradeMatcher)));
                itemPool.RemoveAll((ItemKey x) => __instance.GetBaseItem(x).GetPrice() < 1);
                if (itemPool.Count != 0)
                {
                    sbyte highestGrade = itemPool.Max((ItemKey x) => ItemTemplateHelper.GetGrade(x.ItemType, x.TemplateId));
                    // itemPool.RemoveAll((ItemKey x) => ItemTemplateHelper.GetGrade(x.ItemType, x.TemplateId) < highestGrade);
                    itemPool.ForEach(itemKey2 => returnResult.Add(Wager.CreateItem(itemKey2, 1)));
                }
            }

            ObjectPool<List<ItemKey>>.Instance.Return(itemPool);
            List<sbyte> resourcePool = ObjectPool<List<sbyte>>.Instance.Get();
            List<sbyte> resourceGrades = ObjectPool<List<sbyte>>.Instance.Get();
            for (sbyte resourceType2 = 0; resourceType2 < 8; resourceType2 = (sbyte)(resourceType2 + 1))
            {
                int resourceCount = character.GetResource(resourceType2);
                for (sbyte resourceGrade = maxGrade; resourceGrade >= minGrade; resourceGrade = (sbyte)(resourceGrade - 1))
                {
                    int gradeCount = CricketSpecialConstants.GradeToPriceResource(resourceType2, resourceGrade);
                    if (gradeCount <= resourceCount)
                    {
                        resourcePool.Add(resourceType2);
                        resourceGrades.Add(resourceGrade);
                        break;
                    }
                }
            }

            foreach (sbyte resourceType in resourcePool)
            {
                int index = resourcePool.IndexOf(resourceType);
                sbyte grade = resourceGrades[index];
                int count = CricketSpecialConstants.GradeToPriceResource(resourceType, grade);
                returnResult.Add(Wager.CreateResource(resourceType, count));
            }

            ObjectPool<List<sbyte>>.Instance.Return(resourcePool);
            ObjectPool<List<sbyte>>.Instance.Return(resourceGrades);
            int exp = CricketSpecialConstants.GradeToPriceExp((sbyte)(maxGrade / 2));
            returnResult.Add(Wager.CreateExp(exp));
            bool GradeMatcher(ItemKey itemKey)
            {
                sbyte grade2 = ItemTemplateHelper.GetGrade(itemKey.ItemType, itemKey.TemplateId);
                return minGrade <= grade2 && grade2 <= maxGrade;
            }

            __result = returnResult;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemDomain), "TransferWager")]
        public static void ItemDomain_TransferWager_PrePatch(ItemDomain __instance, DataContext context, GameData.Domains.Character.Character srcChar, GameData.Domains.Character.Character destChar, Wager wager)
        {
            if (wager.Type == 1 && wager.Count > 0)
            {
                var equipment = srcChar.GetEquipment();
                for (sbyte i = 0; i < equipment.Length; i++)
                {
                    if (equipment[i] == wager.ItemKey)
                    {
                        srcChar.ChangeEquipment(context, i, -1, wager.ItemKey);
                        break;
                    }
                }
            }
        }

        private static List<short> typeRandomPool = null;
        private static ECricketPartsType cricketType = ECricketPartsType.Trash;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtraDomain), "UpgradeCricket")]
        public static void ItemDomain_UpgradeCricket_Prefix(ItemDomain __instance, DataContext context, int charId, int cricketId)
        {
            GameData.Domains.Item.Cricket cricket = DomainManager.Item.GetElement_Crickets(cricketId);
            if (cricket == null)
            {
                typeRandomPool = null;
                cricketType = ECricketPartsType.Trash;
                return;
            }

            var cricketUpgradeRandomPool = typeof(ExtraDomain).GetStaticFieldValue<Dictionary<ECricketPartsType, List<short>>>("CricketUpgradeRandomPool");
            cricketType = cricket.GetColorData().Type;
            typeRandomPool = cricketUpgradeRandomPool[cricketType];

            if (typeRandomPool.Contains(Config.CricketParts.DefKey.SanDuanJin))
            {
                cricketUpgradeRandomPool[cricketType] = new List<short>() { Config.CricketParts.DefKey.SanDuanJin };
            }
            else if (typeRandomPool.Contains(Config.CricketParts.DefKey.BaBai))
            {
                cricketUpgradeRandomPool[cricketType] = new List<short>() { Config.CricketParts.DefKey.BaBai };
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtraDomain), "UpgradeCricket")]
        public static void ItemDomain_UpgradeCricket_Postfix(ItemDomain __instance, DataContext context, int charId, int cricketId)
        {
            if (cricketType == ECricketPartsType.Trash)
            {
                return;
            }
            var cricketUpgradeRandomPool = typeof(ExtraDomain).GetStaticFieldValue<Dictionary<ECricketPartsType, List<short>>>("CricketUpgradeRandomPool");
            if (typeRandomPool != null)
            { 
                cricketUpgradeRandomPool[cricketType] = typeRandomPool;
            }
        }
    }
}
