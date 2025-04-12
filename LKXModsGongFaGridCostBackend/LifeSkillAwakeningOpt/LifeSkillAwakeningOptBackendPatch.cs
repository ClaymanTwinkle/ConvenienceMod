using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Ai.GeneralAction.LifeSkillRandom;
using GameData.Domains.Character.ParallelModifications;
using HarmonyLib;
using NLog;

namespace ConvenienceBackend.LifeSkillAwakeningOpt
{
    internal class LifeSkillAwakeningOptBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("技艺提升");

        public override void OnModSettingUpdate(string modIdStr)
        {
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(LifeSkillAwakeningAction), "CheckValid")]
        public static unsafe void LifeSkillAwakeningAction_CheckValid_PostPatch(LifeSkillAwakeningAction __instance, Character selfChar, Character targetChar)
        {
            if (targetChar.GetId() != DomainManager.Taiwu.GetTaiwuCharId()) return;
            LifeSkillShorts lifeSkillQualifications = targetChar.GetLifeSkillQualifications();
            var value = lifeSkillQualifications.Items[__instance.IncreasedLifeSkillType];

            _logger.Info(__instance.IncreasedLifeSkillType + " = " + value);

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "OfflineCalcGeneralAction_Awakening")]
        public static unsafe bool Character_OfflineCalcGeneralAction_Awakening_PrePatch(Character __instance, DataContext context, PeriAdvanceMonthGeneralActionModification mod, HashSet<int> currBlockChars, HashSet<int> caringCharIds)
        {
            var _actionEnergies = __instance.GetActionEnergies();

            LifeSkillShorts lifeSkillAttainments = __instance.GetLifeSkillAttainments();
            short selfBuddhismAttainment = lifeSkillAttainments.Items[13];
            short selfTaoismAttainment = lifeSkillAttainments.Items[12];
            if (selfBuddhismAttainment < 200 && selfTaoismAttainment < 200)
            {
                return false;
            }

            Character targetChar = null;
            if (caringCharIds.Count > 0)
            {
                targetChar = __instance.SelectMaxPriorityActionTarget(context, caringCharIds, delegate(Character targetChar) { return IsValidForLifeSkillAwakening(__instance, targetChar); });
            }

            if (targetChar == null)
            {
                targetChar = __instance.SelectMaxPriorityActionTarget(context, currBlockChars, delegate (Character targetChar) { return IsValidForLifeSkillAwakening(__instance, targetChar); });
            }

            if (targetChar == null)
            {
                return false;
            }

            LifeSkillShorts targetLifeSkillAttainments = targetChar.GetLifeSkillAttainments();
            LifeSkillShorts canImproveLifeSkillAttainments = targetChar.IsTaiwu() ? targetChar.GetLifeSkillQualifications() : targetLifeSkillAttainments;
            Span<sbyte> canImproveLifeSkillTypes = stackalloc sbyte[16];
            int canImproveCount = 0;
            for (sbyte lifeSkillType = 0; lifeSkillType < 16; lifeSkillType = (sbyte)(lifeSkillType + 1))
            {
                if (canImproveLifeSkillAttainments.Items[lifeSkillType] < 90)
                {
                    canImproveLifeSkillTypes[canImproveCount] = lifeSkillType;
                    canImproveCount++;
                }
            }

            if (canImproveCount == 0)
            {
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(77, 2);
                defaultInterpolatedStringHandler.AppendLiteral("Selected character ");
                defaultInterpolatedStringHandler.AppendFormatted(targetChar);
                defaultInterpolatedStringHandler.AppendLiteral(" does not have a life skill type that can be awakened by ");
                defaultInterpolatedStringHandler.AppendFormatted(__instance);
                defaultInterpolatedStringHandler.AppendLiteral(".");
                throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
            }

            sbyte increasedLifeSkillType = canImproveLifeSkillTypes[context.Random.Next(canImproveCount)];
            Span<sbyte> religiousLifeSkillTypes = stackalloc sbyte[LifeSkillType.ReligiousTypes.Length];
            int religiousLifeSkillTypeCount = 0;
            sbyte[] religiousTypes = LifeSkillType.ReligiousTypes;
            foreach (sbyte b2 in religiousTypes)
            {
                if (lifeSkillAttainments.Items[b2] > targetLifeSkillAttainments.Items[b2] && lifeSkillAttainments.Items[b2] >= 200)
                {
                    religiousLifeSkillTypes[religiousLifeSkillTypeCount] = b2;
                    religiousLifeSkillTypeCount++;
                }
            }

            if (religiousLifeSkillTypeCount == 0)
            {
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 2);
                defaultInterpolatedStringHandler.AppendFormatted(__instance);
                defaultInterpolatedStringHandler.AppendLiteral(" cannot perform awakening action to ");
                defaultInterpolatedStringHandler.AppendFormatted(targetChar);
                defaultInterpolatedStringHandler.AppendLiteral(" due to insufficient buddhism or taoism attainment.");
                throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
            }

            sbyte awakeningLifeSkillType = religiousLifeSkillTypes[context.Random.Next(religiousLifeSkillTypeCount)];
            mod.PerformedActions.Add((targetChar, new LifeSkillAwakeningAction
            {
                AwakeningLifeSkillType = awakeningLifeSkillType,
                IncreasedLifeSkillType = increasedLifeSkillType
            }));
            _actionEnergies.SpendEnergyOnAction(4);

            return false;
        }

        private static unsafe bool IsValidForLifeSkillAwakening(Character __instance, Character targetChar)
        {
            LifeSkillShorts selfAttainments = __instance.GetLifeSkillAttainments();
            LifeSkillShorts targetAttainments = targetChar.GetLifeSkillAttainments();

            LifeSkillShorts validLifeSkillAttainments = targetChar.IsTaiwu() ? targetChar.GetLifeSkillQualifications() : targetAttainments;

            bool hasValidLifeSkillType = false;
            for (sbyte b = 0; b < 16; b = (sbyte)(b + 1))
            {
                if (validLifeSkillAttainments.Items[b] < 90)
                {
                    hasValidLifeSkillType = true;
                    break;
                }
            }

            if (!hasValidLifeSkillType)
            {
                return false;
            }

            short selfBuddhismAttainment = selfAttainments.Items[13];
            short targetBuddhismAttainment = targetAttainments.Items[13];
            if (selfBuddhismAttainment > targetBuddhismAttainment && selfBuddhismAttainment >= 200)
            {
                return true;
            }

            short selfTaoismAttainment = selfAttainments.Items[12];
            short targetTaoismAttainment = targetAttainments.Items[12];
            if (selfTaoismAttainment > targetTaoismAttainment && selfTaoismAttainment >= 200)
            {
                return true;
            }

            return false;
        }
    }
}
