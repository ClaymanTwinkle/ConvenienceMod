using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using Config.ConfigCells.Character;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character.Ai;
using GameData.Domains.Character.AvatarSystem;
using GameData.Domains.Item;
using GameData.Domains.LifeRecord;
using GameData.Domains.Taiwu.Profession;
using GameData.Domains.Taiwu.Profession.SkillsData;
using GameData.Utilities;
using HarmonyLib;
using static GameData.DomainEvents.Events;

namespace ConvenienceBackend.ProfessionOptimize
{
    internal class ProfessionOptimizeBackend : BaseBackendPatch
    {
        private static bool _ReallocateFeatures = false;

        public override void OnModSettingUpdate(string modIdStr)
        {
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_ReallocateFeatures", ref _ReallocateFeatures);
        }

        public override void OnEnterNewWorld()
        {
            base.OnEnterNewWorld();

            if (!_ReallocateFeatures) return;

            UnregisterHandlers();
            RegisterHandlers();
        }

        public override void OnLoadedArchiveData()
        {
            base.OnLoadedArchiveData();

            if (!_ReallocateFeatures) return;

            UnregisterHandlers();
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            Events.RegisterHandler_AdvanceMonthBegin(OnAdvanceMonthBegin);
            Events.RegisterHandler_AdvanceMonthFinish(OnAdvanceMonthFinish);
        }

        private void UnregisterHandlers()
        {
            Events.UnRegisterHandler_AdvanceMonthBegin(OnAdvanceMonthBegin);
            Events.UnRegisterHandler_AdvanceMonthFinish(OnAdvanceMonthFinish);
        }

        private void OnAdvanceMonthBegin(DataContext context)
        {
            // 将没执行的志向主动1技能执行掉，防止忘记了
            var professionList = new List<int> {
                Profession.DefKey.Savage, // 山人
                Profession.DefKey.TaoistMonk, // 道长
                Profession.DefKey.BuddhistMonk, // 高僧
                Profession.DefKey.Traveler, // 旅人
                Profession.DefKey.TravelingBuddhistMonk, // 云游僧
                Profession.DefKey.TravelingTaoistMonk // 云游道
            };
            var skillIndex = 0;
            foreach (var professionId in professionList)
            {
                if (DomainManager.Extra.GetTaiwuCurrProfessionId() == professionId && DomainManager.Extra.CanExecuteProfessionSkill(professionId, skillIndex))
                {
                    ProfessionData professionData = DomainManager.Extra.GetProfessionData(professionId);
                    ProfessionSkillItem skillConfig = professionData.GetSkillConfig(skillIndex);
                    DomainManager.TaiwuEvent.OnEvent_ProfessionSkillClicked(skillConfig.TemplateId);
                    if (skillConfig.Instant && professionData.Seniority < 10000)
                    {
                        ProfessionSkillArg professionSkillArg = default;
                        professionSkillArg.ProfessionId = professionId;
                        professionSkillArg.SkillId = skillConfig.TemplateId;
                        professionSkillArg.IsSuccess = true;
                        ProfessionSkillHandle.OnSkillExecuted(context, ref professionSkillArg);
                        ProfessionSkillHandle.OnActiveSkillExecuted(context, ref professionSkillArg);
                    }
                }
            }
        }

        private unsafe void OnAdvanceMonthFinish(DataContext context)
        { 
            
        }

        /// <summary>
        /// 云游道-易天改命-优先交换最高级的正面特性过来
        /// </summary>
        /// <param name="context"></param>
        /// <param name="targetChar"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProfessionSkillHandle), "TravelingTaoistMonkSkill_ReallocateFeatures")]
        public static bool PrefixTravelingTaoistMonkSkill_ReallocateFeatures(DataContext context, GameData.Domains.Character.Character targetChar)
        {
            if (!_ReallocateFeatures) return true;

            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            List<short> list = new List<short>();
            // 保存同源特性
            Dictionary<short, List<short>> dictionary = new Dictionary<short, List<short>>();
            List<short> featureIds = taiwu.GetFeatureIds();
            int count = featureIds.Count;
            for (int num = count - 1; num >= 0; num--)
            {
                short num2 = featureIds[num];
                CharacterFeatureItem characterFeatureItem = CharacterFeature.Instance[num2];
                if (characterFeatureItem.CanBeExchanged)
                {
                    list.Add(num2);
                    featureIds.RemoveAt(num);
                    if (!dictionary.ContainsKey(characterFeatureItem.MutexGroupId))
                    {
                        dictionary.Add(characterFeatureItem.MutexGroupId, new List<short>());
                    }

                    AdaptableLog.Info(characterFeatureItem.Name + "-" + characterFeatureItem.Level);

                    dictionary[characterFeatureItem.MutexGroupId].Add(num2);
                }
            }
            // 太吾不可变的继续设置回来
            taiwu.SetFeatureIds(featureIds, context);
            List<short> featureIds2 = targetChar.GetFeatureIds();
            int count2 = featureIds2.Count;
            for (int num3 = count2 - 1; num3 >= 0; num3--)
            {
                short num4 = featureIds2[num3];
                CharacterFeatureItem characterFeatureItem2 = CharacterFeature.Instance[num4];
                if (characterFeatureItem2.CanBeExchanged)
                {
                    list.Add(num4);
                    featureIds2.RemoveAt(num3);
                    if (!dictionary.ContainsKey(characterFeatureItem2.MutexGroupId))
                    {
                        dictionary.Add(characterFeatureItem2.MutexGroupId, new List<short>());
                    }

                    AdaptableLog.Info(characterFeatureItem2.Name + "-" + characterFeatureItem2.Level);

                    dictionary[characterFeatureItem2.MutexGroupId].Add(num4);
                }
            }
            // 目标角色不可变的继续设置回来
            targetChar.SetFeatureIds(featureIds2, context);
            bool flag = false;
            bool flag2 = false;
            foreach (KeyValuePair<short, List<short>> item in dictionary)
            {
                if (item.Value.Count > 1)
                {
                    var first = item.Value[0];
                    var second = item.Value[1];

                    if (CharacterFeature.Instance[first].Level >= CharacterFeature.Instance[second].Level)
                    {
                        taiwu.AddFeature(context, first);
                        targetChar.AddFeature(context, second);
                    }
                    else
                    {
                        taiwu.AddFeature(context, second);
                        targetChar.AddFeature(context, first);
                    }

                    flag = true;
                    flag2 = true;
                    list.Remove(first);
                    list.Remove(second);
                }
            }
            // 洗牌剩下的特性
            CollectionUtils.Shuffle(context.Random, list);
            list.Sort((x, y) => {
                var yFeature = CharacterFeature.Instance[y];
                var xFeature = CharacterFeature.Instance[x];
                if (yFeature.Level != xFeature.Level) return yFeature.Level - xFeature.Level;
                Func<FeatureMedals, int> selector = a =>
                {
                    if (a.Values != null)
                    {
                        return a.Values.Sum(b =>
                        {
                            if (b == 0)
                            {
                                return 1;
                            }
                            else if (b == 1)
                            {
                                return -1;
                            }
                            else
                            {
                                return 0;
                            }
                        });
                    }
                    else
                    {
                        return 0;
                    }
                };

                return yFeature.FeatureMedals.Sum(selector) - xFeature.FeatureMedals.Sum(selector);
            });
            foreach (short item2 in list)
            {
                if (featureIds.Count < count && taiwu.AddFeature(context, item2))
                {
                    flag = true;
                }
                else if (featureIds2.Count < count2 && targetChar.AddFeature(context, item2))
                {
                    flag2 = true;
                }
            }

            if (!flag)
            {
                DomainManager.Extra.GenerateCharTeammateCommands(context, taiwu.GetId());
            }

            if (!flag2)
            {
                DomainManager.Extra.GenerateCharTeammateCommands(context, targetChar.GetId());
            }
            return false;
        }

        /// <summary>
        /// 道长-渡劫-只消耗99张天劫符箓
        /// </summary>
        /// <param name="context"></param>
        /// <param name="professionData"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProfessionSkillHandle), "TaoistMonkSkill_ConfirmTribulationSucceed")]
        public static bool PrefixTaoistMonkSkill_ConfirmTribulationSucceed(DataContext context, ProfessionData professionData)
        {
            if (!_ReallocateFeatures) return true;

            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            List<short> featureIds = taiwu.GetFeatureIds();
            TaoistMonkSkillsData skillsData = professionData.GetSkillsData<TaoistMonkSkillsData>();
            List<ItemKey> itemKeys = context.AdvanceMonthRelatedData.ItemKeys;
            itemKeys.Clear();
            Dictionary<ItemKey, int> items = taiwu.GetInventory().Items;
            foreach (KeyValuePair<ItemKey, int> item2 in items)
            {
                item2.Deconstruct(out var key, out var value);
                ItemKey item = key;
                int num = value;
                if (item.ItemType == 12 && item.TemplateId == 234)
                {
                    itemKeys.Add(item);
                }
            }

            foreach (ItemKey item3 in itemKeys)
            {
                taiwu.RemoveInventoryItem(context, item3, Math.Max(items[item3], 99), deleteItem: true);
            }

            if (featureIds.Contains(393))
            {
                for (short num2 = 387; num2 <= 393; num2 = (short)(num2 + 1))
                {
                    taiwu.RemoveFeature(context, num2);
                }

                skillsData.SurvivedTribulationCount = 8;
                DomainManager.Extra.SetProfessionData(context, professionData);
            }
            else
            {
                for (short num3 = 387; num3 <= 393; num3 = (short)(num3 + 1))
                {
                    if (taiwu.AddFeature(context, num3))
                    {
                        sbyte b = (skillsData.SurvivedTribulationCount = (sbyte)(num3 - 387 + 1));
                        DomainManager.Extra.SetProfessionData(context, professionData);
                        break;
                    }
                }
            }

            LifeRecordCollection lifeRecordCollection = DomainManager.LifeRecord.GetLifeRecordCollection();
            int currDate = DomainManager.World.GetCurrDate();
            lifeRecordCollection.AddTribulationSucceeded(taiwu.GetId(), currDate, taiwu.GetLocation());
            if (taiwu.GetCurrAge() > 16)
            {
                short leftMaxHealth = taiwu.GetLeftMaxHealth();
                taiwu.SetCurrAge(16, context);
                short leftMaxHealth2 = taiwu.GetLeftMaxHealth();
                taiwu.ChangeHealth(context, (leftMaxHealth2 - leftMaxHealth) * 12);
                AvatarData avatar = taiwu.GetAvatar();
                if (avatar.UpdateGrowableElementsShowingAbilities(taiwu))
                {
                    taiwu.SetAvatar(avatar, context);
                }
            }

            return false;
        }
    }
}
