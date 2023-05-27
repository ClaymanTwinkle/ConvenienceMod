using System;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Ai;
using GameData.Domains.Character.Relation;
using GameData.Domains.TaiwuEvent.EventHelper;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace ConvenienceBackend.NotNTR
{
    internal class NotNTRBackendPatch: BaseBackendPatch
    {
        // Token: 0x06000002 RID: 2 RVA: 0x00002060 File Offset: 0x00000260
        public override void OnModSettingUpdate(string ModIdStr)
        {
            DomainManager.Mod.GetSetting(ModIdStr, "BN_RELATION", ref BN_RELATION);
            DomainManager.Mod.GetSetting(ModIdStr, "BN_TEAM", ref BN_TEAM);
            if (BN_RELATION && BN_TEAM)
            {
                BN_TEAM = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "OfflineExecuteFixedAction_MakeLove_Mutual")]
        public static bool PrefixOfflineExecuteFixedAction_MakeLove_Mutual(Character __instance, int targetCharId)
        {
            Character element_Objects = DomainManager.Character.GetElement_Objects(targetCharId);
            return isAllowedWithoutTargetRestriction(__instance, element_Objects);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AiHelper.Relation), "GetStartRelationSuccessRate_HusbandOrWife")]
        public static void PostfixGetStartRelationSuccessRate_HusbandOrWife(Character selfChar, Character targetChar, ref int __result)
        {
            if (!isAllowedWithTargetRestriction(selfChar, targetChar))
            {
                __result = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AiHelper.Relation), "GetStartRelationSuccessRate_BoyOrGirlFriend")]
        public static void PostfixGetStartRelationSuccessRate_BoyOrGirlFriend(Character selfChar, Character targetChar, ref int __result)
        {
            if (!isAllowedWithTargetRestriction(selfChar, targetChar))
            {
                __result = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AiHelper.Relation), "GetStartRelationSuccessRate_Adored")]
        public static void PostfixGetStartRelationSuccessRate_Adored(Character selfChar, Character targetChar, ref int __result)
        {
            if (!isAllowedWithoutTargetRestriction(selfChar, targetChar))
            {
                __result = 0;
            }
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000020B8 File Offset: 0x000002B8
        public static bool isAllowedWithoutTargetRestriction(Character self, Character target)
        {
            if (self == null || target == null)
            {
                return true;
            }
            int taiwuCharId = DomainManager.Taiwu.GetTaiwuCharId();
            int id = self.GetId();
            if (id == taiwuCharId || target.GetId() == taiwuCharId)
            {
                return true;
            }
            if (BN_RELATION)
            {
                RelatedCharacter relatedCharacter;
                if (DomainManager.Character.TryGetRelation(id, taiwuCharId, out relatedCharacter) && (RelationType.HasRelation(relatedCharacter.RelationType, 16384) || RelationType.HasRelation(relatedCharacter.RelationType, 1024)))
                {
                    return false;
                }
            }
            else if (BN_TEAM && DomainManager.Taiwu.IsInGroup(id))
            {
                return false;
            }
            return true;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002144 File Offset: 0x00000344
        public static bool isAllowedWithTargetRestriction(Character self, Character target)
        {
            if (self == null || target == null)
            {
                return true;
            }
            int taiwuCharId = DomainManager.Taiwu.GetTaiwuCharId();
            int id = self.GetId();
            int id2 = target.GetId();
            if (id == taiwuCharId || id2 == taiwuCharId)
            {
                return true;
            }
            if (BN_RELATION)
            {
                RelatedCharacter relatedCharacter;
                if (DomainManager.Character.TryGetRelation(id, taiwuCharId, out relatedCharacter) && (RelationType.HasRelation(relatedCharacter.RelationType, 16384) || RelationType.HasRelation(relatedCharacter.RelationType, 1024)))
                {
                    return false;
                }
                RelatedCharacter relatedCharacter2;
                if (DomainManager.Character.TryGetRelation(id2, taiwuCharId, out relatedCharacter2) && (RelationType.HasRelation(relatedCharacter2.RelationType, 16384) || RelationType.HasRelation(relatedCharacter2.RelationType, 1024)))
                {
                    return false;
                }
            }
            else if (BN_TEAM && (DomainManager.Taiwu.IsInGroup(id) || DomainManager.Taiwu.IsInGroup(id2)))
            {
                return false;
            }
            return true;
        }

        // Token: 0x04000002 RID: 2
        public static bool BN_RELATION;

        // Token: 0x04000003 RID: 3
        public static bool BN_TEAM;
    }
}
