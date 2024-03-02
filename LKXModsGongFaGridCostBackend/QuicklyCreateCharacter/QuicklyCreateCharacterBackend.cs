using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using GameData.Common;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character;
using GameData.Domains.Global.Inscription;
using GameData.Domains.Map;
using GameData.Domains.Organization;
using GameData.Domains.World;
using GameData.Domains;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using GameData.Domains.Item;
using Redzen.Random;

namespace ConvenienceBackend.QuicklyCreateCharacter
{
    internal class QuicklyCreateCharacterBackend : BaseBackendPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_SLRole", ref QuicklyCreateCharacterBackend.bool_Toggle_Total);
        }

        // Token: 0x06000002 RID: 2 RVA: 0x000021B9 File Offset: 0x000003B9
        public override void OnEnterNewWorld()
        {
            QuicklyCreateCharacterBackend.bool_IsEnterNewSave = true;
            QuicklyCreateCharacterBackend.bool_IsCreatWorld = false;
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000021C8 File Offset: 0x000003C8
        public override void OnLoadedArchiveData()
        {
            QuicklyCreateCharacterBackend.bool_IsCreatWorld = false;
            QuicklyCreateCharacterBackend.bool_IsEnterNewSave = false;
        }

        public override void Initialize(Harmony harmony, string modIdStr)
        {
            base.Initialize(harmony, modIdStr);
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002214 File Offset: 0x00000414
        public override void Dispose()
        {
            QuicklyCreateCharacterBackend.bool_IsCreatWorld = false;
            QuicklyCreateCharacterBackend.bool_IsEnterNewSave = false;
        }

        public override void OnConfigUpdate(Dictionary<string, object> config)
        {
            base.OnConfigUpdate(config);
            _keepGoodResult = config.GetTypedValue<bool>("Toggle_KeepGoodResult");
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002278 File Offset: 0x00000478
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDomain), "CallMethod")]
        public static bool CharacterDomain_CallMethod_PrePatch(CharacterDomain __instance, int __result, Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context)
        {
            if(!QuicklyCreateCharacterBackend.bool_Toggle_Total) return true;

            if (operation.DomainId == 4 && operation.MethodId == GameDataBridgeConst.MethodId && QuicklyCreateCharacterBackend.bool_IsEnterNewSave && !QuicklyCreateCharacterBackend.bool_IsCreatWorld)
            {
                int num = operation.ArgsOffset;
                ushort flag = 0;
                num += Serializer.Deserialize(argDataPool, num, ref flag);
                if (operation.ArgsCount == 2)
                {
                    switch (flag)
                    {
                        case GameDataBridgeConst.Flag.Flag_RollCharacter:
                            {
                                KeyValuePair<bool, ProtagonistCreationInfo> result = default;

                                QuicklyCreateCharacterBackend.protagonistCreationInfo = null;
                                Serializer.DeserializeDefault<KeyValuePair<bool, ProtagonistCreationInfo>>(argDataPool, num, ref result);
                                QuicklyCreateCharacterBackend.protagonistCreationInfo = result.Value;

                                if (result.Key)
                                {
                                    QuicklyCreateCharacterBackend.characterData = null;
                                }

                                __result = GameData.Serializer.Serializer.SerializeDefault(new KeyValuePair<int, KeyValuePair<bool, string>>(flag, QuicklyCreateCharacterBackend.GetCharacterDataByInfo(QuicklyCreateCharacterBackend.protagonistCreationInfo)),returnDataPool);
                            }
                            break;
                        default:
                            __result = -1;
                            break;
                    }
                }

                return false;
            }

            return true;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000022FC File Offset: 0x000004FC
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WorldDomain), "CallMethod")]
        public static void WorldDomain_CallMethod_PostPatch(WorldDomain __instance, int __result, Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context)
        {
            if (!QuicklyCreateCharacterBackend.bool_Toggle_Total) return;

            QuicklyCreateCharacterBackend.bool_IsCreatWorld = true;
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002320 File Offset: 0x00000520
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "OfflineCreateProtagonist")]
        public static void Character_OfflineCreateProtagonist_PostPatch(Character __instance, Character.ProtagonistFeatureRelatedStatus __result, DataContext context, ProtagonistCreationInfo info)
        {
            if (!QuicklyCreateCharacterBackend.bool_Toggle_Total) return;

            if (QuicklyCreateCharacterBackend.bool_IsEnterNewSave && QuicklyCreateCharacterBackend.bool_IsCreatWorld)
            {
                if (info.InscribedChar == null)
                {
                    // 覆盖roll属性
                    QuicklyCreateCharacterBackend.OverwriteCharacterAttribute(__instance, __result);
                }
                else
                {
                    AdaptableLog.Info("铭刻角色忽略");
                }
                QuicklyCreateCharacterBackend.bool_IsCreatWorld = false;
                QuicklyCreateCharacterBackend.bool_IsEnterNewSave = false;
                QuicklyCreateCharacterBackend.protagonistCreationInfo = null;
                QuicklyCreateCharacterBackend.characterData = null;
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002390 File Offset: 0x00000590
        private unsafe static KeyValuePair<bool, string> GetCharacterDataByInfo(ProtagonistCreationInfo info)
        {
            sbyte sectOrgTemplateIdByStateTemplateId = MapDomain.GetSectOrgTemplateIdByStateTemplateId(info.TaiwuVillageStateTemplateId);
            short memberId = OrganizationDomain.GetMemberId(sectOrgTemplateIdByStateTemplateId, 8);
            sbyte gender = info.Gender;
            short characterTemplateId = MapDomain.GetCharacterTemplateId(info.TaiwuVillageStateTemplateId, gender);
            Character.ProtagonistFeatureRelatedStatus statusValue;

            // 模拟生成
            Character character = QuicklyCreateCharacterBackend.CreateTempCharacter(info, characterTemplateId, memberId, out statusValue);

            Traverse traverse = Traverse.Create(character);
            List<short> featureIds = character.GetFeatureIds();
            sbyte lifeSkillQualificationGrowthType = character.GetLifeSkillQualificationGrowthType();
            LifeSkillShorts lifeSkill__ForOverwrite = character.GetBaseLifeSkillQualifications();
            sbyte combatSkillQualificationGrowthType = character.GetCombatSkillQualificationGrowthType();
            CombatSkillShorts combatSkill_ForOverwrite = character.GetBaseCombatSkillQualifications();
            MainAttributes baseMainAttributes = character.GetBaseMainAttributes();
            LifeSkillShorts lifeSkill__ForDisplay = (LifeSkillShorts)traverse.Method("CalcLifeSkillQualifications", Array.Empty<object>()).GetValue();
            CombatSkillShorts combatSkill_ForDisplay = (CombatSkillShorts)traverse.Method("CalcCombatSkillQualifications", Array.Empty<object>()).GetValue();
            MainAttributes maxMainAttributes = character.GetMaxMainAttributes();
            Inventory inventory = character.GetInventory();
            TempIteamData itemDataValue = new(inventory);

            // 返回结果
            var characterData = new TempCharacterData(character, statusValue, lifeSkillQualificationGrowthType, combatSkillQualificationGrowthType, featureIds, lifeSkill__ForOverwrite, combatSkill_ForOverwrite, baseMainAttributes, lifeSkill__ForDisplay, combatSkill_ForDisplay, maxMainAttributes, itemDataValue);

            var matchScore = CharacterDataChecker.CheckCharacterDataScore(characterData, ConvenienceBackend.Config);
            var isOk = matchScore == 100;
            if ( QuicklyCreateCharacterBackend.characterData == null || !_keepGoodResult || isOk)
            {
                QuicklyCreateCharacterBackend.characterData = characterData;
            }
            else if (_keepGoodResult)
            {
                var currentMatchScore = CharacterDataChecker.CheckCharacterDataScore(QuicklyCreateCharacterBackend.characterData, ConvenienceBackend.Config);

                if (matchScore == currentMatchScore &&
                    characterData.CalcScope() > QuicklyCreateCharacterBackend.characterData.CalcScope())
                {
                    QuicklyCreateCharacterBackend.characterData = characterData;
                }
                else if (matchScore > currentMatchScore)
                {
                    QuicklyCreateCharacterBackend.characterData = characterData;
                }
            }

            string text = JsonSerializer.Serialize<List<string>>(QuicklyCreateCharacterBackend.characterData.displayList, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return new KeyValuePair<bool, string>(isOk, text);
        }

        /// <summary>
        /// 重新随机创建角色数据
        /// </summary>
        /// <param name="info"></param>
        /// <param name="charTemplateId"></param>
        /// <param name="orgMemberId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static Character CreateTempCharacter(ProtagonistCreationInfo info, short charTemplateId, short orgMemberId, out Character.ProtagonistFeatureRelatedStatus status)
        {
            Character character = new(charTemplateId);
            DataContext currentThreadDataContext = DataContextManager.GetCurrentThreadDataContext();
            status = character.OfflineCreateProtagonist(charTemplateId, orgMemberId, info, currentThreadDataContext);
            character.OfflineSetId(-1);
            ObjectCollectionDataStates objectCollectionDataStates = (ObjectCollectionDataStates)Traverse.Create(DomainManager.Character).Field("_dataStatesObjects").GetValue();
            character.CollectionHelperData = DomainManager.Character.HelperDataObjects;
            character.DataStatesOffset = objectCollectionDataStates.Create();
            character.SetCurrMainAttributes(character.GetMaxMainAttributes(), currentThreadDataContext);
            return character;
        }

        // Token: 0x0600000B RID: 11 RVA: 0x000025B4 File Offset: 0x000007B4
        public unsafe static void OverwriteCharacterAttribute(Character oldCharacter, Character.ProtagonistFeatureRelatedStatus status)
        {
            if (QuicklyCreateCharacterBackend.characterData == null) return;

            Traverse traverse = Traverse.Create(oldCharacter);
            traverse.Field("_lifeSkillQualificationGrowthType").SetValue(QuicklyCreateCharacterBackend.characterData.lifeSkillQualificationGrowthType);
            traverse.Field("_baseLifeSkillQualifications").SetValue(QuicklyCreateCharacterBackend.characterData.lifeSkillQualifications_ForOverwrite);
            traverse.Field("_combatSkillQualificationGrowthType").SetValue(QuicklyCreateCharacterBackend.characterData.combatSkillQualificationGrowthType);
            traverse.Field("_baseCombatSkillQualifications").SetValue(QuicklyCreateCharacterBackend.characterData.combatSkillQualifications_ForOverwrite);
            traverse.Field("_featureIds").SetValue(QuicklyCreateCharacterBackend.characterData.featureIds);
            traverse.Field("_baseMainAttributes").SetValue(QuicklyCreateCharacterBackend.characterData.mainAttributes_ForOverwrite);
            traverse.Field("_skillQualificationBonuses").SetValue(QuicklyCreateCharacterBackend.characterData.character.GetSkillQualificationBonuses());
            traverse.Field("_inventory").SetValue(QuicklyCreateCharacterBackend.characterData.character.GetInventory());
            traverse.Field("_learnedLifeSkills").SetValue(QuicklyCreateCharacterBackend.characterData.character.GetLearnedLifeSkills());
            traverse.Field("_lifeSkillQualifications").SetValue(QuicklyCreateCharacterBackend.characterData.character.GetLifeSkillQualifications());
            traverse.Field("_learnedCombatSkills").SetValue(QuicklyCreateCharacterBackend.characterData.character.GetLearnedCombatSkills());
            traverse.Field("_combatSkillQualifications").SetValue(QuicklyCreateCharacterBackend.characterData.character.GetCombatSkillQualifications());
            status.ReadLifeSkillTemplateId = QuicklyCreateCharacterBackend.characterData.status.ReadLifeSkillTemplateId;
            status.ReadCombatSkillTemplateId = QuicklyCreateCharacterBackend.characterData.status.ReadCombatSkillTemplateId;
            status.CombatSkillBookPageTypes = QuicklyCreateCharacterBackend.characterData.status.CombatSkillBookPageTypes;
            status.CombatSkills = QuicklyCreateCharacterBackend.characterData.status.CombatSkills;
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GenerateRandomBasicFeatures")]
        public static void Character_GenerateRandomBasicFeatures_PrePatch(DataContext context, Dictionary<short, short> featureGroup2Id, bool isProtagonist, ref bool allGoodBasicFeatures)
        {
            if (RollAttributeConfigReader.EnableAllBlueFeature(ConvenienceBackend.Config))
            {
                if (isProtagonist)
                {
                    allGoodBasicFeatures = true;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "OfflineApplyProtagonistFeature_SkillBooks")]
        public static unsafe bool Character_OfflineApplyProtagonistFeature_SkillBooks_PrePatch(Character __instance, DataContext context, ProtagonistCreationInfo info, Character.ProtagonistFeatureRelatedStatus status)
        {
            var useOri = true;
            var combatSkillBookName = RollAttributeConfigReader.GetCombatSkillBookName(ConvenienceBackend.Config);

            if (!String.IsNullOrEmpty(combatSkillBookName))
            {
                useOri = false;

                var traverse = Traverse.Create(__instance);
                var ProtagonistLifeSkillBookIds = traverse.Field<short[]>("ProtagonistLifeSkillBookIds").Value;
                var ProtagonistCombatSkillBookIds = traverse.Field<short[][]>("ProtagonistCombatSkillBookIds").Value;
                var _inventory = traverse.Field<Inventory>("_inventory").Value;
                List<LifeSkillItem> _learnedLifeSkills = traverse.Field<List<LifeSkillItem>>("_learnedLifeSkills").Value;
                List<short> _learnedCombatSkills = traverse.Field<List<short>>("_learnedCombatSkills").Value;
                LifeSkillShorts _baseLifeSkillQualifications = traverse.Field<LifeSkillShorts>("_baseLifeSkillQualifications").Value;
                CombatSkillShorts _baseCombatSkillQualifications = traverse.Field<CombatSkillShorts>("_baseCombatSkillQualifications").Value;


                IRandomSource random = context.Random;
                short num = ProtagonistLifeSkillBookIds[random.Next(ProtagonistLifeSkillBookIds.Length)];
                ItemKey itemKey = DomainManager.Item.CreateSkillBook(context, num, 5, 0, -1, 50);
                _inventory.OfflineAdd(itemKey, 1);
                short lifeSkillTemplateIdFromSkillBook = ItemTemplateHelper.GetLifeSkillTemplateIdFromSkillBook(num);
                Config.LifeSkillItem lifeSkillItem = Config.LifeSkill.Instance[lifeSkillTemplateIdFromSkillBook];
                LifeSkillItem item = new LifeSkillItem(lifeSkillTemplateIdFromSkillBook);
                _learnedLifeSkills.Add(item);
                status.ReadLifeSkillTemplateId = lifeSkillTemplateIdFromSkillBook;
                int num2 = 90 + random.Next(10);
                if (_baseLifeSkillQualifications.Items[lifeSkillItem.Type] < num2)
                {
                    _baseLifeSkillQualifications.Items[lifeSkillItem.Type] = (short)num2;
                }

                sbyte sectOrgTemplateIdByStateTemplateId = MapDomain.GetSectOrgTemplateIdByStateTemplateId(info.TaiwuVillageStateTemplateId);
                sbyte largeSectIndex = OrganizationDomain.GetLargeSectIndex(sectOrgTemplateIdByStateTemplateId);
                short[] combatSkillBookIdArray = ProtagonistCombatSkillBookIds[largeSectIndex];
                short skillBookId = combatSkillBookIdArray[random.Next(combatSkillBookIdArray.Length)];
                foreach (var itemId in combatSkillBookIdArray)
                {
                    Config.SkillBookItem skillBookItem = Config.SkillBook.Instance[itemId];
                    if (skillBookItem.Name.Contains(combatSkillBookName))
                    {
                        skillBookId = itemId;
                        break;
                    }
                }
                itemKey = DomainManager.Item.CreateSkillBook(context, skillBookId, 5, 0, -1, 50);
                _inventory.OfflineAdd(itemKey, 1);
                short combatSkillTemplateIdFromSkillBook = ItemTemplateHelper.GetCombatSkillTemplateIdFromSkillBook(skillBookId);
                Config.CombatSkillItem combatSkillItem = Config.CombatSkill.Instance[combatSkillTemplateIdFromSkillBook];
                GameData.Domains.CombatSkill.CombatSkill item2 = new GameData.Domains.CombatSkill.CombatSkill(-1, combatSkillTemplateIdFromSkillBook, 50, 0);
                status.CombatSkills.Add(item2);
                _learnedCombatSkills.Add(combatSkillTemplateIdFromSkillBook);
                status.ReadCombatSkillTemplateId = combatSkillTemplateIdFromSkillBook;
                GameData.Domains.Item.SkillBook element_SkillBooks = DomainManager.Item.GetElement_SkillBooks(itemKey.Id);
                status.CombatSkillBookPageTypes = element_SkillBooks.GetPageTypes();
                int num4 = 90 + random.Next(10);
                if (_baseCombatSkillQualifications.Items[combatSkillItem.Type] < num4)
                {
                    _baseCombatSkillQualifications.Items[combatSkillItem.Type] = (short)num4;
                }
            }

            return useOri;
        }

        // Token: 0x04000003 RID: 3
        public static bool bool_Toggle_Total = false;

        // Token: 0x04000005 RID: 5
        public static bool bool_IsEnterNewSave = false;

        // Token: 0x04000006 RID: 6
        public static bool bool_IsCreatWorld = false;

        // Token: 0x04000007 RID: 7
        public static ProtagonistCreationInfo protagonistCreationInfo = new();

        public static TempCharacterData characterData;

        private static bool _keepGoodResult = false;
    }
}
