using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.MergeBookPanel;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.AvatarSystem;
using GameData.Domains.Character.Creation;
using GameData.Domains.Combat;
using GameData.Domains.Organization;
using HarmonyLib;
using NLog;
using TaiwuModdingLib.Core.Utils;

namespace ConvenienceBackend.Gigolos
{
    internal class GigolosBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("软饭硬吃");

        private static bool _changeFemale = false;

        public override void OnModSettingUpdate(string modIdStr)
        {

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OrganizationDomain), "CreateCoreCharacter")]
        public static void OrganizationDomain_SetPlayerAutoCombat_Prefix(OrganizationDomain __instance, SettlementMembersCreationInfo info)
        {
            if (Ignore(info)) return;

            _changeFemale = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OrganizationDomain), "CreateCoreCharacter")]
        public static void OrganizationDomain_SetPlayerAutoCombat_Postfix(OrganizationDomain __instance, SettlementMembersCreationInfo info)
        {
            if (Ignore(info)) return;
            _changeFemale = false;

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Gender), "GetRandom")]
        public static bool Gender_GetRandom_Prefix(ref sbyte __result)
        {
            if (!_changeFemale) return true;
            _logger.Info("性别默认女");

            __result = Gender.Female;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OrganizationDomain), "CreateSpouseAndChildren")]
        public static bool OrganizationDomain_CreateSpouseAndChildren_Prefix(OrganizationDomain __instance, SettlementMembersCreationInfo info)
        {
            if (Ignore(info)) return true;

            _logger.Info("跳过配偶和孩子");

            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDomain), "CreateIntelligentCharacter")]
        public static void CharacterDomain_CreateIntelligentCharacter_Prefix(
            DataContext context, 
            ref IntelligentCharacterCreationInfo info
        )
        {
            if (Ignore(info)) return;
            info.Age = 28;
        }

            [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "OfflineCreateAttractionAndAvatar")]
        public static void Character_OfflineCreateAttractionAndAvatar_Postfix(
            Character __instance, 
            DataContext context, 
            sbyte bodyType, 
            ref IntelligentCharacterCreationInfo info
        )
        {
            if (Ignore(info)) return;

            if (!__instance.GetTransgender() && __instance.GetAvatar().GetBaseCharm() > 500) return;

            if (__instance.GetTransgender())
            {
                _logger.Info("纠正人妖");
                __instance.SetPrivateField("_transgender", false);
            }

            var avatar = __instance.GetAvatar();

            while (avatar.GetBaseCharm() < 500) 
            {
                if (info.BaseAttraction >= 0)
                {
                    if (info.Avatar != null)
                    {
                        break;
                    }
                    else
                    {
                        avatar = AvatarManager.Instance.GetRandomAvatar(context.Random, __instance.GetGender(), __instance.GetTransgender(), bodyType, info.BaseAttraction);
                    }
                }
                else
                {
                    short baseAttraction = (short)__instance.CallMethod("GenerateRandomAttraction", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, context.Random);
                    avatar = AvatarManager.Instance.GetRandomAvatar(context.Random, __instance.GetGender(), __instance.GetTransgender(), bodyType, baseAttraction);
                }
            }

            __instance.SetPrivateField("_avatar", avatar);
        }

        private static bool Ignore(SettlementMembersCreationInfo info)
        {
            if (info.OrgTemplateId != Config.Organization.DefKey.WalledTown || info.AreaId != 138) return true;
            if (info.CoreMemberConfig == null) return true;
            if (info.CoreMemberConfig.Grade != 8 && info.CoreMemberConfig.Grade != 7) return true;

            return false;
        }

        private static bool Ignore(IntelligentCharacterCreationInfo info)
        {
            if (info.OrgInfo.OrgTemplateId != Config.Organization.DefKey.WalledTown || info.Location.AreaId != 138) return true;
            if (info.OrgInfo.Grade != 8 && info.OrgInfo.Grade != 7) return true;

            return false;
        }
    }
}
