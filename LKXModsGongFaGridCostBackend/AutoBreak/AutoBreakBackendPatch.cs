using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ConvenienceBackend.MergeBookPanel;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.CombatSkill;
using GameData.Domains.Extra;
using GameData.Domains.Taiwu;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;
using NLog;

namespace ConvenienceBackend.AutoBreak
{
    internal class AutoBreakBackendPatch : BaseBackendPatch
    {
        private static Logger _logger = LogManager.GetLogger("自动突破");

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
        private static bool waitToFindPath = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SkillBreakPlate), "UpdateCanSelectGrids")]
        public static void Taiwu_UpdateCanSelectGrids_PostPatch(SkillBreakPlate __instance)
        {
            if (__instance.Finished)
            {
                return;
            }

            if (!__instance.CheckIndex(__instance.Current))
            {
                waitToFindPath = true;
            }
            else
            {
                if (!_cache.TryGetValue(__instance, out MapCache cache))
                {
                    cache = new MapCache();
                    _cache[__instance] = cache;
                }
                PathFinder finder = new(__instance, __instance.Current, cache);
                _logger.Info($"剩余可走步数是{finder.maxSteps}");
                (int maxScore, List<SkillBreakPlateIndex> bestPath) = finder.FindMaxScorePath();
                if (maxScore > 0 && bestPath != null && bestPath.Count > 0)
                {
                    ShowNextPoint(__instance, maxScore, bestPath);
                }
                else
                {
                    _logger.Info($"寻路失败");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "EnterSkillBreakPlate")]
        public static void Taiwu_EnterSkillBreakPlate_PostPatch(TaiwuDomain __instance, DataContext context, short skillId, ushort selectedPages)
        {
            bool flag4 = !DomainManager.Extra.TryGetElement_SkillBreakPlates(skillId, out SkillBreakPlate plate);
            if (flag4)
            {
                waitToFindPath = false;
                return;
            }

            if (waitToFindPath)
            {
                waitToFindPath = false;
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
                    ShowNextPoint(plate, maxScore, bestPath);
                }
                else
                {
                    _logger.Info($"寻路失败");
                }
            }
        }

        private static void ShowNextPoint(SkillBreakPlate plate, int maxScore, List<SkillBreakPlateIndex> bestPath)
        {
            _logger.Info($"预计能达到最大威力值{maxScore}");
            var nextPoint = bestPath[1];
            var canSelect = false;
            foreach (SkillBreakPlateIndex index in plate.GetIndexes())
            {
                if (plate[index].State == ESkillBreakGridState.CanSelect)
                {
                    if (index == nextPoint)
                    {
                        canSelect = true;
                        break;
                    }
                }
            }

            if (canSelect)
            {
                foreach (SkillBreakPlateIndex index in plate.GetIndexes())
                {
                    if (plate[index].State == ESkillBreakGridState.CanSelect)
                    {
                        if (index != nextPoint)
                        {
                            plate[index].State = ESkillBreakGridState.Showed;
                        }
                    }
                }
            }
            else
            {
                _logger.Info($"预测下一步{nextPoint}，但选不了");
            }
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
    }
}
