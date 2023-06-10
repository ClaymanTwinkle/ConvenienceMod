using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
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
using System.Diagnostics;
using GameData.Domains.Character.Display;

namespace ConvenienceBackend.QuicklyCreateCharacter
{
    internal class QuicklyCreateCharacterBackend : BaseBackendPatch
    {
        public override void OnModSettingUpdate(string modIdStr)
        {
            // DomainManager.Mod.GetSetting(modIdStr, "Toggle_Total", ref QuicklyCreateCharacterBackend.bool_Toggle_Total);
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_BackendCustomized", ref QuicklyCreateCharacterBackend.bool_Toggle_BackendCustomized);
            int dropdown_LifeSkillGrowthType = 0;
            int dropdown_CombatSkillGrowthType = 0;
            int dropdown_LifeSkillType = 0;
            int slider_LifeSkillQualification = 0;
            int dropdown_CombatSkillType = 0;
            int slider_CombatSkillQualification = 0;
            int dropdown_MainAttributeType = 0;
            int slider_MainAttribute = 0;
            int slider_RollCountLimit = 1;
            DomainManager.Mod.GetSetting(modIdStr, "Dropdown_LifeSkillGrowthType", ref dropdown_LifeSkillGrowthType);
            DomainManager.Mod.GetSetting(modIdStr, "Dropdown_CombatSkillGrowthType", ref dropdown_CombatSkillGrowthType);
            DomainManager.Mod.GetSetting(modIdStr, "Dropdown_LifeSkillType", ref dropdown_LifeSkillType);
            DomainManager.Mod.GetSetting(modIdStr, "Slider_LifeSkillQualification", ref slider_LifeSkillQualification);
            DomainManager.Mod.GetSetting(modIdStr, "Dropdown_CombatSkillType", ref dropdown_CombatSkillType);
            DomainManager.Mod.GetSetting(modIdStr, "Slider_CombatSkillQualification", ref slider_CombatSkillQualification);
            DomainManager.Mod.GetSetting(modIdStr, "Dropdown_MainAttributeType", ref dropdown_MainAttributeType);
            DomainManager.Mod.GetSetting(modIdStr, "Slider_MainAttribute", ref slider_MainAttribute);
            DomainManager.Mod.GetSetting(modIdStr, "Slider_RollCountLimit", ref slider_RollCountLimit);
            bool flag = QuicklyCreateCharacterBackend.bool_Toggle_BackendCustomized;
            if (flag)
            {
                QuicklyCreateCharacterBackend.customizedInfo = new CustomizedAttributeInfo(dropdown_LifeSkillGrowthType, dropdown_CombatSkillGrowthType, dropdown_LifeSkillType, slider_LifeSkillQualification, dropdown_CombatSkillType, slider_CombatSkillQualification, dropdown_MainAttributeType, slider_MainAttribute, slider_RollCountLimit);
            }
            else
            {
                QuicklyCreateCharacterBackend.customizedInfo = null;
            }
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
            bool flag = QuicklyCreateCharacterBackend.mappedFile != null;
            if (flag)
            {
                QuicklyCreateCharacterBackend.mappedFile.Dispose();
            }
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
            bool flag2 = QuicklyCreateCharacterBackend.mappedFile != null;
            if (flag2)
            {
                QuicklyCreateCharacterBackend.mappedFile.Dispose();
            }
            bool flag3 = QuicklyCreateCharacterBackend.customizedInfo != null;
            if (flag3)
            {
                QuicklyCreateCharacterBackend.customizedInfo = null;
            }
            QuicklyCreateCharacterBackend.bool_IsCreatWorld = false;
            QuicklyCreateCharacterBackend.bool_IsEnterNewSave = false;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002278 File Offset: 0x00000478
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDomain), "CallMethod")]
        public static bool CharacterDomain_CallMethod_PrePatch(CharacterDomain __instance, int __result, Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context)
        {
            if(!QuicklyCreateCharacterBackend.bool_Toggle_Total) return true;

            if (operation.DomainId == 4 && operation.MethodId == GameDataBridgeConst.MethodId && operation.ArgsCount == 1 && QuicklyCreateCharacterBackend.bool_IsEnterNewSave && !QuicklyCreateCharacterBackend.bool_IsCreatWorld)
            {
                int num = operation.ArgsOffset;
                QuicklyCreateCharacterBackend.protagonistCreationInfo = null;
                num += Serializer.DeserializeDefault<ProtagonistCreationInfo>(argDataPool, num, ref QuicklyCreateCharacterBackend.protagonistCreationInfo);
                __result = GameData.Serializer.Serializer.Serialize(QuicklyCreateCharacterBackend.GetCharacterDataByInfo(QuicklyCreateCharacterBackend.protagonistCreationInfo), returnDataPool);

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
                QuicklyCreateCharacterBackend.OverwriteCharacterAttribute(__instance, __result);
                QuicklyCreateCharacterBackend.bool_IsCreatWorld = false;
                QuicklyCreateCharacterBackend.bool_IsEnterNewSave = false;
                QuicklyCreateCharacterBackend.protagonistCreationInfo = null;
                QuicklyCreateCharacterBackend.characterData = null;
                if (QuicklyCreateCharacterBackend.mappedFile != null)
                {
                    QuicklyCreateCharacterBackend.mappedFile.Dispose();
                }
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002390 File Offset: 0x00000590
        private unsafe static string GetCharacterDataByInfo(ProtagonistCreationInfo info)
        {
            bool flag = QuicklyCreateCharacterBackend.mappedFile != null;
            if (flag)
            {
                QuicklyCreateCharacterBackend.mappedFile.Dispose();
            }
            sbyte sectOrgTemplateIdByStateTemplateId = MapDomain.GetSectOrgTemplateIdByStateTemplateId(info.TaiwuVillageStateTemplateId);
            short memberId = OrganizationDomain.GetMemberId(sectOrgTemplateIdByStateTemplateId, 8);
            InscribedCharacter inscribedChar = info.InscribedChar;
            sbyte gender = info.Gender;
            short characterTemplateId = MapDomain.GetCharacterTemplateId(info.TaiwuVillageStateTemplateId, gender);
            Character.ProtagonistFeatureRelatedStatus statusValue;
            Character character = QuicklyCreateCharacterBackend.CreateTempCharacter(info, characterTemplateId, memberId, out statusValue);
            bool flag2 = QuicklyCreateCharacterBackend.bool_Toggle_BackendCustomized && QuicklyCreateCharacterBackend.customizedInfo != null;
            if (flag2)
            {
                bool flag3 = QuicklyCreateCharacterBackend.customizedInfo.CheckIsPassed(character);
                int num = 1;
                while (!flag3)
                {
                    character = QuicklyCreateCharacterBackend.CreateTempCharacter(info, characterTemplateId, memberId, out statusValue);
                    flag3 = QuicklyCreateCharacterBackend.customizedInfo.CheckIsPassed(character);
                    num++;
                    if (num >= QuicklyCreateCharacterBackend.customizedInfo.rollCountLimit)
                    {
                        break;
                    }
                }
            }
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

            QuicklyCreateCharacterBackend.characterData = new TempCharacterData(character, statusValue, lifeSkillQualificationGrowthType, combatSkillQualificationGrowthType, featureIds, lifeSkill__ForOverwrite, combatSkill_ForOverwrite, baseMainAttributes, lifeSkill__ForDisplay, combatSkill_ForDisplay, maxMainAttributes, itemDataValue);
            string text = JsonSerializer.Serialize<List<string>>(QuicklyCreateCharacterBackend.characterData.displayList, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return text;
            // QuicklyCreateCharacterBackend.TransferCharacterAttributeList();
        }

        // Token: 0x0600000A RID: 10 RVA: 0x00002534 File Offset: 0x00000734
        public static Character CreateTempCharacter(ProtagonistCreationInfo info, short charTemplateId, short orgMemberId, out Character.ProtagonistFeatureRelatedStatus status)
        {
            Character character = new Character(charTemplateId);
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
            bool flag = QuicklyCreateCharacterBackend.characterData == null;
            if (!flag)
            {
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
        }

        // Token: 0x0600000C RID: 12 RVA: 0x000027C0 File Offset: 0x000009C0
        public static void TransferCharacterAttributeList()
        {
            string text = JsonSerializer.Serialize<List<string>>(QuicklyCreateCharacterBackend.characterData.displayList, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            bool flag = QuicklyCreateCharacterBackend.mappedFile != null;
            if (flag)
            {
                QuicklyCreateCharacterBackend.mappedFile.Dispose();
            }
            QuicklyCreateCharacterBackend.mappedFile = MemoryMappedFile.CreateOrOpen("QuicklyCreateCharacterData", (long)bytes.Length);
            using (MemoryMappedViewAccessor memoryMappedViewAccessor = QuicklyCreateCharacterBackend.mappedFile.CreateViewAccessor())
            {
                memoryMappedViewAccessor.WriteArray<byte>(0L, bytes, 0, bytes.Length);
            }
        }

        // Token: 0x04000002 RID: 2
        public static MemoryMappedFile mappedFile;

        // Token: 0x04000003 RID: 3
        public static bool bool_Toggle_Total = true;

        // Token: 0x04000004 RID: 4
        public static bool bool_Toggle_BackendCustomized;

        // Token: 0x04000005 RID: 5
        public static bool bool_IsEnterNewSave = false;

        // Token: 0x04000006 RID: 6
        public static bool bool_IsCreatWorld = false;

        // Token: 0x04000007 RID: 7
        public static ProtagonistCreationInfo protagonistCreationInfo = new();

        // Token: 0x04000008 RID: 8
        public static TempCharacterData characterData;

        // Token: 0x04000009 RID: 9
        public static CustomizedAttributeInfo customizedInfo;
    }
}
