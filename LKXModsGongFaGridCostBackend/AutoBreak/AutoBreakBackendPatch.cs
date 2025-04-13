using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ConvenienceBackend.MergeBookPanel;
using ConvenienceBackend.TaiwuBuildingManager;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.CombatSkill;
using GameData.Domains.Extra;
using GameData.Domains.Taiwu;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using NLog;

namespace ConvenienceBackend.AutoBreak
{
    internal class AutoBreakBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("自动突破");
        private static readonly SkillBreakPlateAxial[] _pureNeighbors = new SkillBreakPlateAxial[9]
{
            (-1, -1), (-1, 0), (-1, 1), (1, -1), (1, 0), (1, 1), (0, -1), (0, 0), (0, 1)
};
        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "EnterSkillBreakPlate")]
        public static void Taiwu_Init_PostPatch(TaiwuDomain __instance, ref GameData.Domains.Taiwu.SkillBreakPlate __result)
        {
            // if (!_isAutoBreak) return;

            for (int j = 0; j < __result.Width; j++)
            {
                for (int k = 0; k < __result.Height; k++)
                {
                    var grid = __result[j, k];
                    if (grid.State == ESkillBreakGridState.Invisible)
                    {
                        grid.State = ESkillBreakGridState.Showed; // 显示
                    }
                    // grid.SuccessRateFix = 100;
                }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillBreakPlate), "CalcSuccessRate")]
        public static bool Taiwu_CalcSuccessRate_PostPatch(SkillBreakPlate __instance, ref short __result)
        {
            __result = 100;
            return false;
        }

        private static Dictionary<SkillBreakPlate, MapCache> _cache = new();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtraDomain), "CallMethod")]
        public static bool ExtraDomain_CallMethod_Prefix(ExtraDomain __instance, Operation operation, RawDataPool argDataPool, RawDataPool returnDataPool, DataContext context, ref int __result)
        {
            if (operation.MethodId == 1333)
            {
                int argsOffset = operation.ArgsOffset;
                if (operation.ArgsCount == 1)
                {
                    short skillId = -1;
                    argsOffset += GameData.Serializer.Serializer.Deserialize(argDataPool, argsOffset, ref skillId);

                    if (skillId > -1)
                    {
                        (int maxScore, List<SkillBreakPlateIndex> bestPath) = FindPath(skillId);

                        __result = GameData.Serializer.Serializer.Serialize(bestPath, returnDataPool);
                        GameData.Serializer.Serializer.Serialize(maxScore, returnDataPool);
                        return false;
                    }
                }

                __result = -1;
                return false;
            }
            else if (operation.MethodId == 1334)
            {
                int argsOffset = operation.ArgsOffset;
                if (operation.ArgsCount == 1)
                {
                    short skillId = -1;
                    argsOffset += GameData.Serializer.Serializer.Deserialize(argDataPool, argsOffset, ref skillId);

                    if (skillId > -1)
                    {
                        var maxScore = 0;
                        if (DomainManager.Extra.TryGetElement_SkillBreakPlates(skillId, out SkillBreakPlate plate))
                        {
                            maxScore = CalcIdealMaximumScore(plate);
                        }
                        __result = GameData.Serializer.Serializer.Serialize(maxScore, returnDataPool);
                        return false;
                    }
                }

                __result = -1;
                return false;
            }

            return true;
        }

        private static (int score, List<SkillBreakPlateIndex>) FindPath(short skillId)
        {
            if (!DomainManager.Extra.TryGetElement_SkillBreakPlates(skillId, out SkillBreakPlate plate))
            {
                return (0, new List<SkillBreakPlateIndex>());
            }

            if (!plate.CheckIndex(plate.Current))
            {
                // 刚开始，4取1
                var startPointList = (from pos in plate.GetIndexes()
                                      where plate.CallPrivateMethod<bool>("IsStartPoint", pos.X, pos.Y)
                                      select pos);

                int maxScore = -1;
                List<SkillBreakPlateIndex> bestPath = null;
                foreach (var startPoint in startPointList)
                {
                    if (!_cache.TryGetValue(plate, out MapCache cache))
                    {
                        cache = new MapCache();
                        _cache[plate] = cache;
                    }
                    PathFinder finder = new(plate, startPoint);
                    _logger.Info($"剩余可走步数是{finder.maxSteps}");
                    (int score, List<SkillBreakPlateIndex> path) = finder.FindMaxScorePath();
                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestPath = path;
                    }
                }
                if (maxScore > 0 && bestPath != null && bestPath.Count > 1)
                {
                    // ShowNextPoint(plate, maxScore, bestPath);
                    return (maxScore, bestPath);
                }
                else
                {
                    _logger.Info($"寻路失败");
                }
            }
            else
            {
                if (!_cache.TryGetValue(plate, out MapCache cache))
                {
                    cache = new MapCache();
                    _cache[plate] = cache;
                }
                PathFinder finder = new(plate, plate.Current, cache);
                _logger.Info($"剩余可走步数是{finder.maxSteps}");
                (int maxScore, List<SkillBreakPlateIndex> bestPath) = finder.FindMaxScorePath();
                if (maxScore > 0 && bestPath != null && bestPath.Count > 0)
                {
                    // ShowNextPoint(plate, maxScore, bestPath);
                    return(maxScore, bestPath);
                }
                else
                {
                    _logger.Info($"寻路失败");
                }
            }
            return (0, new List<SkillBreakPlateIndex>());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtraDomain), "RemoveElement_SkillBreakPlates")]
        public static void ExtraDomain_RemoveElement_SkillBreakPlates_PrePatch(ExtraDomain __instance, short elementId)
        {
            if (__instance.TryGetElement_SkillBreakPlates(elementId, out SkillBreakPlate plate))
            { 
                _cache.Remove(plate);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtraDomain), "SetElement_SkillBreakPlates")]
        public static void ExtraDomain_SetElement_SkillBreakPlates_PrePatch(ExtraDomain __instance, short elementId, GameData.Domains.Taiwu.SkillBreakPlate value)
        {
            if (__instance.TryGetElement_SkillBreakPlates(elementId, out SkillBreakPlate plate) && plate != value)
            {
                _cache.Remove(plate);
            }
        }


        private static int CalcIdealMaximumScore(SkillBreakPlate plate)
        { 
            var maxScore = 0;
            for (int j = 0; j < plate.Width; j++)
            {
                for (int k = 0; k < plate.Height; k++)
                {
                    var index = (j, k);
                    var grid = plate[index];

                    maxScore += CalcAddMaxPowerBase(plate, index);

                    int extraCount = 0;
                    int extraScore = 0;
                    foreach (SkillBreakPlateAxial offset in _pureNeighbors)
                    {
                        SkillBreakPlateAxial neighborAxial = index + offset * grid.Template.NextStepOffset;
                        SkillBreakPlateIndex neighborPos = (SkillBreakPlateIndex)neighborAxial;
                        if (plate.CheckIndex(neighborPos))
                        {
                            if (!(neighborPos == index))
                            {
                                extraCount++;
                                if (plate[neighborPos].Template.ClearNeighborMaxPower && plate[index].TemplateId != 2)
                                {
                                    continue;
                                }

                                extraScore += Math.Max(0, plate[neighborPos].Template.NeighborAddMaxPowerWhenActive);
                            }
                        }
                    }
                    extraScore += (extraCount * Math.Max(0, plate[index].Template.SucceedNeighborAddMaxPower));

                    maxScore += extraScore;
                }
            }

            return maxScore;
        }

        private static int CalcAddMaxPowerBase(SkillBreakPlate plate, SkillBreakPlateIndex index)
        {
            SkillBreakPlateGrid grid = plate[index];
            sbyte templateId = grid.TemplateId;
            int result;
            if (templateId > 1)
            {
                if (templateId != 2)
                {
                    result = grid.AddMaxPower;
                }
                else
                {
                    result = 0; // this.CalcAddMaxPowerAsBonus(index, this.GetBonus(index).ImpactRange);
                }
            }
            else
            {
                result = 0;
            }
            return result;
        }
    }
}
