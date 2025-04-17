using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ConvenienceBackend.MergeBookPanel;
using ConvenienceBackend.TaiwuBuildingManager;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.Extra;
using GameData.Domains.Taiwu;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json.Linq;
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
        private static readonly object lockObj = new();

        public override void OnModSettingUpdate(string modIdStr)
        {
        }

        private static bool is_new_plate = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuDomain), "EnterSkillBreakPlate")]
        public static void Taiwu_Init_PrePatch(TaiwuDomain __instance, DataContext context, short skillId, ushort selectedPages)
        {
            is_new_plate = !DomainManager.Extra.TryGetElement_SkillBreakPlates(skillId, out var value);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "EnterSkillBreakPlate")]
        public static void Taiwu_Init_PostPatch(TaiwuDomain __instance, DataContext context, short skillId, ushort selectedPages, ref GameData.Domains.Taiwu.SkillBreakPlate __result)
        {
            // if (!_isAutoBreak) return;

            if (is_new_plate)
            {
                Config.CombatSkillItem combatSkillItem = Config.CombatSkill.Instance[skillId];

                var lastPlate = __result;
                DomainManager.Extra.RemoveSkillBreakPlate(context, skillId);
                int maxScore = CalcIdealMaximumScore(lastPlate);
                for (int i =0;i<10;i++)
                {
                    Config.SkillBreakPlateItem config = combatSkillItem.SkillBreakPlate;
                    if (DomainManager.TutorialChapter.InGuiding && skillId == Config.CombatSkill.DefKey.TianshuXuanji)
                    {
                        config = Config.SkillBreakPlate.Instance[(sbyte)Config.SkillBreakPlate.DefKey.Tutorial];
                    }

                    (int succeedCount, int failCount) clearedSkillPlateStepInfo = DomainManager.Extra.GetClearedSkillPlateStepInfo(skillId);
                    int item = clearedSkillPlateStepInfo.succeedCount;
                    int item2 = clearedSkillPlateStepInfo.failCount;
                    var plate = new SkillBreakPlate(context.Random, config, selectedPages, item, item2);
                    DomainManager.Extra.RemoveClearedSkillPlateStepInfo(context, skillId);

                    plate.StepBase = __instance.GetTaiwu().GetSkillBreakoutAvailableStepsCount(skillId);
                    plate.BaseSuccessRate = __instance.CallPrivateMethod<byte>("CalcTaiwuBreakBaseSuccessRate", combatSkillItem);
                
                    int score = CalcIdealMaximumScore(plate);
                    if (score > maxScore)
                    {
                        maxScore = score;
                        lastPlate = plate;
                    }
                }

                __result = lastPlate;
                DomainManager.Extra.SetOrAddSkillBreakPlate(context, skillId, __result);


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
                List<Thread> threads = new();

                foreach (var startPoint in startPointList)
                {
                    Thread thread = new(() => {
                        PathFinder finder = new(plate, startPoint);
                        _logger.Info($"剩余可走步数是{finder.maxSteps}");
                        (int score, List<SkillBreakPlateIndex> path) = finder.FindMaxScorePath();
                        _logger.Info($"预计最大分数是{score}");
                        lock (lockObj) // 线程安全操作
                        {
                            if (score > maxScore)
                            {
                                maxScore = score;
                                bestPath = path;
                            }
                        }
                    });
                    threads.Add(thread);
                    thread.Start();
                }

                foreach (Thread t in threads) t.Join();

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

        private static int bonusScore = 0;

        private static int CalcIdealMaximumScore(SkillBreakPlate plate)
        {
            bonusScore = 0;

            var maxScore = 0;
            for (int j = 0; j < plate.Width; j++)
            {
                for (int k = 0; k < plate.Height; k++)
                {
                    var index = (j, k);
                    maxScore += CalcAddMaxPower(plate, index);
                }
            }

            return maxScore;
        }

        public static int CalcAddMaxPower(SkillBreakPlate plate, SkillBreakPlateIndex index)
        {
            int value = CalcAddMaxPowerBase(plate, index);
            if (plate[index].Template.IgnoreEffectAddMaxPower)
            {
                return value;
            }

            int successNeighborCount = 0;
            foreach (SkillBreakPlateIndex pureNeighbor in GetPureNeighbors(plate, index))
            {
                if (!(pureNeighbor == index))
                {
                    if (plate[pureNeighbor].Template.ClearNeighborMaxPower && plate[index].TemplateId != 2)
                    {
                        continue;
                    }

                    successNeighborCount++;
                    value += plate[pureNeighbor].Template.NeighborAddMaxPowerWhenActive;
                }
            }

            return value + successNeighborCount * plate[index].Template.SucceedNeighborAddMaxPower;
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
                    result = CalcAddMaxPowerAsBonus(plate, index, 3);
                    bonusScore += result;
                }
            }
            else
            {
                result = 0;
            }
            return result;
        }

        private static int CalcAddMaxPowerAsBonus(SkillBreakPlate plate, SkillBreakPlateIndex index, int impactRange)
        {
            int total = 0;
            int totalNormal = 0;
            int totalGoneMad = 0;
            foreach (SkillBreakPlateIndex neighborIndex in plate.CallPrivateMethod<IEnumerable<SkillBreakPlateIndex>>("GetPureNeighbors", index, impactRange))
            {
                SkillBreakPlateGrid neighbor = plate[neighborIndex];
                int value = (neighbor.TemplateId == 2) ? 0 : CalcAddMaxPower(plate, neighborIndex);
                if (value != 0)
                {
                    total += value;
                    bool recordedStepIsGoneMad = neighbor.RecordedStepIsGoneMad;
                    if (recordedStepIsGoneMad)
                    {
                        totalGoneMad += value;
                    }
                    else
                    {
                        totalNormal += value;
                    }
                }
            }
            _logger.Debug($"total={total}");
            int result = total * (CValuePercentBonus)plate.OutlineConfig.BonusAddMaxPower;
            _logger.Debug($"result={result}");
            result += totalNormal * (CValuePercent)plate.OutlineConfig.BonusAddMaxPowerNormal;
            result += totalGoneMad * (CValuePercent)plate.OutlineConfig.BonusAddMaxPowerGoneMad;
            CValuePercent correctionFactor = (int)GlobalConfig.Instance.BreakoutBonusAddPowerCorrectionFactor;
            _logger.Debug($"CalcAddMaxPowerAsBonus={result}*{correctionFactor}");
            return result * correctionFactor;
        }

        private static List<SkillBreakPlateIndex> GetPureNeighbors(SkillBreakPlate plate, SkillBreakPlateIndex pos, int distance = 1)
        {
            var neighbors = new List<SkillBreakPlateIndex>();
            foreach (var (dx, dy) in _pureNeighbors)
            {
                int x = pos.X + dx;
                int y = pos.Y + dy;
                SkillBreakPlateIndex index = (x, y);
                if (plate.CheckIndex(x, y) && plate.CalcDistance(pos, index) <= distance)
                {
                    neighbors.Add(index);
                }
            }
            return neighbors;
        }
    }
}
