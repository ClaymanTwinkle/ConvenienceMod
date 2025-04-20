using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.AutoBreak;
using ConvenienceBackend.Utils;
using GameData.Domains.Combat;
using GameData.Domains.SpecialEffect.LegendaryBook.NpcEffect;
using GameData.Domains.Taiwu;
using GameData.Domains.Taiwu.LifeSkillCombat.Status;
using GameData.Utilities;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;

/*
0    起    StartPoint = 0    突破的起点。
1    终    EndPoint = 1    "突破的终点。只有连接此格时，突破才能成功。否则无法获得任何突破奖励。"
2    玄机    Bonus = 2    无说明
3    如常    Normal = 3    按部就班的突破。
4    相承    Special = 4    打通此格时，周围所有格子的打通成功率提高30%。    
5    蕴海    Special = 4    打通此格时，下次连接必须选择距离2的突破格为连接目标。    
6    魔障    Special = 4    打通此格时，此格周围的所有“如常”格变化为随机的“特殊突破格”。    
7    破立    Special = 4    "此格的打通成功率降低100%。此格周围所有格子的打通成功率提高60%。"    
8    不息    Special = 4    与此格相连的下1个格子必然可以打通。    
9    灵感    Special = 4    连接此格不会消耗天资上限或入魔上限。    
10    忘我    Special = 4    此格的打通成功率减少30%，且不会消耗天资上限或入魔上限。打通此格时，将周围随机1个“如常”格变化为“忘我”。    
11    逆行    Special = 4    "此格的打通成功率降低60%。打通此格时，天资上限+3。"    
12    循序    Special = 4    此格的打通成功率降低60%。此格周围每有1个打通的突破格，此格的打通成功率提高20%，且威力上限+1。    
13    苦功    Special = 4    打通此格时，周围突破格的打通成功率降低50%，威力上限+2，并且在打通失败时会重新变成可连接状态。    
14    通明    Special = 4    打通此格时，揭示多个未显示的格子。    
15    奇劫    Special = 4    打通此格时，将周围随机3个格子变为打通失败状态，并将它们的威力上限转移至此格上。    
16    归心    Special = 4    "此格的打通成功率提高30%。打通此格时，此格周围的所有“特殊突破格”变化为“如常”。"    
17    嫁衣    Special = 4    打通此格时，周围的突破格威力上限+1。    
18    寂灭    Special = 4    打通此格时，天资上限+3，并将周围突破格的威力上限变为0。    
19    焚心    Special = 4    当前天资超过天资上限的一半越多，此格及此格周围所有格子的打通成功率就提高越多。    
20    壮志    Special = 4    当前天资相比天资上限的一半越少，此格及此格周围所有格子的打通成功率就提高越多。    
21    诀窍    Special = 4    打通此格时，下次连接可以选择任意“诀窍”突破格为连接目标。    
22    完备    Normal = 3    "已熟知此格的要领。此格必然可以连接成功。"
23    覆辙    Normal = 3    "曾在此格遭受挫折。此格的打通成功率提高100%，但连接此格无论成功或失败均会损失一定的健康。"
*/
namespace ConvenienceBackend.AutoBreak
{
    public delegate bool IsMatch(SkillBreakPlateIndex index);

    public class MapCache
    {
        public readonly Dictionary<SkillBreakPlateIndex, List<SkillBreakPlateIndex>> NeighborsGeneralCache = new();
        public readonly Dictionary<SkillBreakPlateIndex, List<SkillBreakPlateIndex>> PureNeighborsCache = new();

        public readonly Dictionary<(SkillBreakPlateIndex, SkillBreakPlateIndex), int> DistanceCache = new();
    }

    public class PathFinder
    {
        private static readonly Logger _logger = LogManager.GetLogger("自动突破");

        private static readonly SkillBreakPlateAxial[] _neighborAxial = new SkillBreakPlateAxial[6]
{
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1),
            (-1, 1),
            (1, -1)
};

        private static readonly ObjectPool<HashSet<SkillBreakPlateIndex>> HashSetPool = new(10, 10000);
        private static readonly ObjectPool<List<SkillBreakPlateIndex>> ListPool = new(10, 10000);

        private readonly SkillBreakPlate map;
        public readonly int maxSteps;
        private readonly List<SkillBreakPlateIndex> startList;
        private SkillBreakPlateIndex end;
        private readonly List<SkillBreakPlateIndex> bonusPoints = new();
        private readonly int[,] bonusRangeArea;
        private readonly HashSet<sbyte> excludedTypes = new() { 15, 16 };
        private readonly float[,] scoreMap;
        private int allRequiredCount;

        private readonly int maxBonusImpactRange = 3;

        private readonly HashSet<SkillBreakPlateIndex> initialVisited = new();

        private readonly MapCache _cache = null;

        private readonly Dictionary<sbyte, List<SkillBreakPlateIndex>> nextStepCanJumpToSameDict = new();

        private readonly float bonusAddMaxPowerFactor;
        private readonly float bonusAddMaxPowerNormalFactor;
        private readonly float bonusAddMaxPowerGoneMadFactor;

        private readonly Stopwatch sw = new();
        private readonly Stopwatch sw2 = new();
        private int watchCount = 0;


        public PathFinder(SkillBreakPlate map, SkillBreakPlateIndex start, MapCache cache = null) :
            this(map, new List<SkillBreakPlateIndex> { start }, cache)
        {
        }

        public PathFinder(SkillBreakPlate map, List<SkillBreakPlateIndex> startList, MapCache cache = null)
        {
            this.map = map;
            this.scoreMap = new float[map.Width,map.Height];
            this.bonusRangeArea = new int[map.Width,map.Height];
            this.startList = startList;
            this.maxSteps = map.StepGoneMad - map.StepCostedGoneMad + (map.StepNormal - map.StepCostedNormal);
            allRequiredCount = 0;

            this.bonusAddMaxPowerFactor = (map.OutlineConfig.BonusAddMaxPower + 100) * GlobalConfig.Instance.BreakoutBonusAddPowerCorrectionFactor / 10000f;
            this.bonusAddMaxPowerNormalFactor = map.OutlineConfig.BonusAddMaxPowerNormal * GlobalConfig.Instance.BreakoutBonusAddPowerCorrectionFactor / 10000f;
            this.bonusAddMaxPowerGoneMadFactor = map.OutlineConfig.BonusAddMaxPowerGoneMad * GlobalConfig.Instance.BreakoutBonusAddPowerCorrectionFactor / 10000f;

            _cache = cache ?? new MapCache();

            foreach (var start in startList)
            {
                initialVisited.Add(start);
            }
            GenerateCache();
        }

        private void GenerateCache()
        {
            List<SkillBreakPlateIndex> specialPoints = new();
            for (int j = 0; j < map.Width; j++)
            {
                for (int k = 0; k < map.Height; k++)
                {
                    var grid = map[j, k];
                    if (grid == null) continue;
                    scoreMap[j, k] = grid.AddMaxPower;
                    SkillBreakPlateIndex index = (j, k);
                    if (grid.State == ESkillBreakGridState.Selected)
                    {
                        initialVisited.Add(index);
                    }
                    if (grid.Template.Type == ESkillBreakGridTypeType.EndPoint)
                    {
                        end = index;
                    } 
                    else if (grid.Template.Type == ESkillBreakGridTypeType.Bonus)
                    {
                        if (grid.State != ESkillBreakGridState.Selected)
                        {
                            allRequiredCount++;
                        }
                        bonusPoints.Add(index);
                    }
                    else if (grid.Template.Type == ESkillBreakGridTypeType.Special)
                    { 
                        specialPoints.Add(index);
                    }
                    if (grid.Template.NextStepCanJumpToSame)
                    {
                        var list = nextStepCanJumpToSameDict.GetValueOrDefault(grid.TemplateId);
                        if (list == null)
                        {
                            list = new List<SkillBreakPlateIndex>();
                            nextStepCanJumpToSameDict[grid.TemplateId] = list;
                        }
                        list.Add(index);
                    }
                }
            }
            foreach (var specialPoint in specialPoints)
            {
                var specialGrid = map[specialPoint];
                int successNeighborCount = 0;
                var neighbors = GetPureNeighbors(specialPoint, 1);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor != specialPoint)
                    {
                        if (specialGrid.Template.ClearNeighborMaxPower)
                        {
                            continue;
                        }
                        successNeighborCount++;

                        if (map[neighbor].Template.Type > ESkillBreakGridTypeType.Bonus)
                        {
                            scoreMap[neighbor.X, neighbor.Y] += specialGrid.Template.NeighborAddMaxPowerWhenActive;

                            //scoreMap[specialPoint.X, specialPoint.Y] += specialGrid.Template.NeighborAddMaxPowerWhenActive;
                        }
                    }
                }
                scoreMap[specialPoint.X, specialPoint.Y] = scoreMap[specialPoint.X, specialPoint.Y] + successNeighborCount * specialGrid.Template.SucceedNeighborAddMaxPower;
            }
            foreach (var bonusPoint in bonusPoints)
            {
                var pureNeighbors = GetPureNeighbors(bonusPoint, maxBonusImpactRange);
                foreach (SkillBreakPlateIndex neighborIndex in pureNeighbors)
                {
                    SkillBreakPlateGrid neighbor = map[neighborIndex];
                    if (neighbor!= null && neighbor.TemplateId > 2)
                    {
                        bonusRangeArea[neighborIndex.X, neighborIndex.Y]++;
                        scoreMap[neighborIndex.X, neighborIndex.Y] += (scoreMap[neighborIndex.X, neighborIndex.Y] * (map.OutlineConfig.BonusAddMaxPower + map.OutlineConfig.BonusAddMaxPowerNormal + map.OutlineConfig.BonusAddMaxPowerGoneMad) * GlobalConfig.Instance.BreakoutBonusAddPowerCorrectionFactor / 100);
                    }
                }
            }
        }

        public (int score, List<SkillBreakPlateIndex>) FindMaxScorePath()
        {
            var queue = new PriorityQueue<State, float>();
            var best = new Dictionary<StateKey, float>();

            foreach (var start in startList)
            {
                var initialState = new State
                {
                    Index = start,
                    RemainingSteps = maxSteps,
                    Score = CalcAddMaxPower(start, initialVisited),
                    Visited = initialVisited,
                    RequiredMask = 0,
                    Path = new List<SkillBreakPlateIndex> { start }
                };

                queue.Enqueue(initialState, -initialState.Score);

                var stateKey = new StateKey(initialState.Index, initialState.RemainingSteps, initialState.RequiredMask);
                best[stateKey] = initialState.Score;
            }

            float maxScore = -1;
            List<SkillBreakPlateIndex> bestPath = null;
            int remainingSteps = 0;

            int loopCount = 0;
            int ignoreCount = 0;

            while (queue.Count > 0)
            {

                var current = queue.Dequeue();

                if (end == current.Index)
                {
                    if (current.RequiredMask == allRequiredCount)
                    {
                        if (current.Score > maxScore || (current.Score == maxScore && current.RemainingSteps > remainingSteps))
                        {
                            maxScore = current.Score;
                            bestPath = current.Path;
                            remainingSteps = current.RemainingSteps;

                            _logger.Info($"${map.Width}x{map.Height}循环次数{loopCount}，分数{maxScore}，剩余步数{remainingSteps}");
                            continue;
                        }
                    }
                    // 回收current
                    RecycleState(current);
                    continue;
                }

                if (current.RequiredMask != allRequiredCount)
                {
                    var forceContinue = false;
                    foreach (var bonusPoint in bonusPoints)
                    {
                        if (IsUnreachable(current, bonusPoint)) { forceContinue = true; break; }
                    }
                    if (forceContinue)
                    {
                        // 回收current
                        ignoreCount++;
                        RecycleState(current);
                        continue;
                    }
                }

                if (IsUnreachable(current, end))
                {
                    // 回收current
                    ignoreCount++;
                    RecycleState(current);
                    continue;
                }

                loopCount++;
                foreach (var move in GenerateMoves(current))
                {
                    int newRemaining = current.RemainingSteps - move.Cost + move.AddSteps;
                    if (newRemaining < 0) {
                        ignoreCount++;
                        continue;
                    }
                    var newVisited = HashSetPool.Get();
                    newVisited.UnionWith(current.Visited);
                    newVisited.Add(move.NewIndex);
                    int newRequiredMask = current.RequiredMask + (move.TemplateId == 2 ? 1 : 0);
                    float newScore = 0;
                    var newPath = ListPool.Get();
                    newPath.AddRange(current.Path);
                    newPath.Add(move.NewIndex);
                    foreach (var node in newPath)
                    {
                        newScore += CalcAddMaxPower(node, newVisited);
                    }

                    var newState = new State
                    {
                        Index = move.NewIndex,
                        RemainingSteps = newRemaining,
                        Score = newScore,
                        Visited = newVisited,
                        RequiredMask = newRequiredMask,
                        Path = newPath
                    };

                    sw.Start();
                    var featureScore = CalcEstimateAddMaxPower(newState); // newScore
                    watchCount++;
                    sw.Stop();
                    var key = new StateKey(newState.Index, newState.RemainingSteps, newState.RequiredMask);

                    if (best.TryGetValue(key, out float existing) && featureScore < existing) 
                    {
                        ignoreCount++;
                        continue;
                    }

                    best[key] = featureScore;
                    queue.Enqueue(newState, -newScore);
                }
                // 回收current
                RecycleState(current);

            }

            _logger.Info($"${map.Width}x{map.Height}循环次数{loopCount}忽略次数{ignoreCount}，分数{maxScore}，剩余步数{remainingSteps}, 耗时{sw.ElapsedMilliseconds}ms，, 耗时2{sw2.ElapsedMilliseconds}ms, 耗时次数{watchCount}");

            return ((int)maxScore, bestPath);
        }

        private List<Move> GenerateMoves(State state)
        {
            var moves = new List<Move>();

            List<SkillBreakPlateIndex> neighbors = GetNeighborsGeneral(state.Index);
            foreach (SkillBreakPlateIndex neighbor in neighbors)
            {
                if (!state.Visited.Contains(neighbor))
                {
                    AddMove(state, neighbor, moves);
                }
            }
            return moves;
        }

        private void AddMove(State state, SkillBreakPlateIndex index, List<Move> moves)
        {
            var cell = map[index];
            if (IsExcludedGrid(index, cell)) return;

            int cost = cell.Template.CostBreakCount;
            if (state.RemainingSteps - cost < 0) return;

            int addSteps = cell.Template.AddStepNormal;

            moves.Add(new Move
            {
                NewIndex = index,
                Cost = cost,
                AddSteps = addSteps,
                TemplateId = cell.TemplateId,
            });
        }

        private bool IsExcludedGrid(SkillBreakPlateIndex index, SkillBreakPlateGrid grid)
        {
            if (grid.State == ESkillBreakGridState.Failed) return true;
            if (grid.State == ESkillBreakGridState.Selected) return true;

            return excludedTypes.Contains(grid.TemplateId);
        }

        /// <summary>
        /// 预测分数
        /// </summary>
        /// <param name="index"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        private float CalcEstimateAddMaxPower(State state)
        {
            //float score = 0;

            //foreach (var pos in state.Path)
            //{
            //    score += scoreMap[pos.X, pos.X];
            //}

            //return score;

            float maxScore = state.Score;

            var newVisited = HashSetPool.Get();
            newVisited.UnionWith(state.Visited);
            foreach (var move in GenerateMoves(state))
            {
                float newScore = 0;
                newVisited.Add(move.NewIndex);

                foreach (var node in state.Path)
                {
                    newScore += CalcAddMaxPower(node, newVisited);
                }

                if (newScore >= maxScore)
                {
                    maxScore = newScore;
                }
                else
                {
                    newVisited.Remove(move.NewIndex);
                }
            }

            return maxScore;
        }

        private float CalcAddMaxPower(SkillBreakPlateIndex index, HashSet<SkillBreakPlateIndex> visited)
        {
            float value = this.CalcAddMaxPowerBase(visited, index);
            bool ignoreEffectAddMaxPower = map[index].Template.IgnoreEffectAddMaxPower;
            float result = value;
            if (ignoreEffectAddMaxPower)
            {
                // result = value;
            }
            else
            {
                int successNeighborCount = 0;
                var neighbors = GetPureNeighbors(index, 1);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor != index && visited.Contains(neighbor))
                    {
                        if (map[neighbor].Template.ClearNeighborMaxPower && map[index].TemplateId != 2)
                        {
                            return 0;
                        }
                        successNeighborCount++;
                        result += map[neighbor].Template.NeighborAddMaxPowerWhenActive;
                    }
                }

                result += successNeighborCount * map[index].Template.SucceedNeighborAddMaxPower;

                if (result > value)
                {
                    var bonusFactor = bonusRangeArea[index.X, index.Y];
                    if (bonusFactor > 0)
                    {
                        var newResult = result;
                        newResult += (bonusFactor * (result - value) * bonusAddMaxPowerFactor);
                        newResult += (bonusFactor * result * bonusAddMaxPowerNormalFactor);
                        newResult += (bonusFactor * result * bonusAddMaxPowerGoneMadFactor);

                        result = newResult;
                    }
                }
            }

            return result;
        }

        private int CalcAddMaxPowerBase(HashSet<SkillBreakPlateIndex> visited, SkillBreakPlateIndex index)
        {
            SkillBreakPlateGrid grid = map[index];
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
                    result = 0; // this.CalcAddMaxPowerAsBonus(visited, index, maxBonusImpactRange);
                }
            }
            else
            {
                result = 0;
            }
            return result;
        }

        private int CalcAddMaxPowerAsBonus(HashSet<SkillBreakPlateIndex> visited, SkillBreakPlateIndex index, int impactRange)
        {
            int total = 0;
            int totalNormal = 0;
            int totalGoneMad = 0;
            foreach (SkillBreakPlateIndex neighborIndex in GetPureNeighbors(index, impactRange))
            {
                SkillBreakPlateGrid neighbor = map[neighborIndex];
                int value = (neighbor.TemplateId == 2) ? 0 : (int)this.CalcAddMaxPower(neighborIndex, visited);
                if (value != 0)
                {
                    total += value;
                    if (visited.Contains(neighborIndex))
                    {
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
            }
            int result = total * (CValuePercentBonus)map.OutlineConfig.BonusAddMaxPower;
            result += totalNormal * (CValuePercent)map.OutlineConfig.BonusAddMaxPowerNormal;
            result += totalGoneMad * (CValuePercent)map.OutlineConfig.BonusAddMaxPowerGoneMad;
            CValuePercent correctionFactor = (int)GlobalConfig.Instance.BreakoutBonusAddPowerCorrectionFactor;
            return result * correctionFactor;
        }

        private List<SkillBreakPlateIndex> GetNeighborsGeneral(SkillBreakPlateIndex pos)
        {
            SkillBreakPlateGrid grid = map[pos];

            List<SkillBreakPlateIndex> list;
            if (_cache.NeighborsGeneralCache.ContainsKey(pos))
            {
                list = _cache.NeighborsGeneralCache.GetValueOrDefault(pos);
                if (list != null) return list;
            }
            list = new();
            _cache.NeighborsGeneralCache[pos] = list;

            SkillBreakPlateAxial axial = pos;
            foreach (SkillBreakPlateAxial offset in _neighborAxial)
            {
                SkillBreakPlateAxial neighborAxial = axial + offset * grid.Template.NextStepOffset;
                SkillBreakPlateIndex neighborPos = (SkillBreakPlateIndex)neighborAxial;
                if (map.CheckIndex(neighborPos) && map[neighborPos].State.CanInteract())
                {
                    list.Add(neighborPos);
                }
            }

            if (!grid.Template.NextStepCanJumpToSame)
            {
                return list;
            }

            var nextStepCanJumpList = nextStepCanJumpToSameDict[grid.TemplateId];
            foreach (SkillBreakPlateIndex otherIndex in nextStepCanJumpList)
            {
                if (map[otherIndex].TemplateId == grid.TemplateId && !(pos == otherIndex) && CalcDistance(pos, otherIndex) != grid.Template.NextStepOffset && map[otherIndex].State.CanInteract())
                {
                    list.Add(otherIndex);
                }
            }

            return list;
        }

        private List<SkillBreakPlateIndex> GetPureNeighbors(SkillBreakPlateIndex pos, int distance = 1)
        {
            List<SkillBreakPlateIndex> points;
            if (distance == 1)
            {
                if (_cache.PureNeighborsCache.ContainsKey(pos))
                {
                    points = _cache.PureNeighborsCache.GetValueOrDefault(pos);
                    if (points != null) 
                    {
                        return points;
                    }
                }
            }

            points = new();
            if (distance == 1)
            {
                _cache.PureNeighborsCache[pos] = points;
            }

            for (int x = -distance; x <= distance; x++)
            {
                for (int y = -distance; y <= distance; y++)
                {
                    SkillBreakPlateIndex neighborPos = pos + (x, y);

                    if (map.CheckIndex(neighborPos) && CalcDistance(pos, neighborPos) <= distance)
                    {
                        points.Add(neighborPos);
                    }
                }
            }

            return points;
        }

        private bool IsUnreachable(State state, SkillBreakPlateIndex pos)
        {
            if (state.Visited.Contains(pos)) return false;

            var neighborPosList = GetPureNeighbors(pos, 2);
            foreach (var neighborPos in neighborPosList)
            {
                SkillBreakPlateGrid grid = map[neighborPos];
                if (grid.State == ESkillBreakGridState.Failed) continue;
                //if (map[pos].State == ESkillBreakGridState.CanSelect) return false;
                if (grid.State == ESkillBreakGridState.Selected) continue;
                if (neighborPos == pos) continue;

                if (CalcDistance(pos, neighborPos) == grid.Template.NextStepOffset && (!state.Visited.Contains(neighborPos) || state.Index == neighborPos))
                {
                    return false;
                }

                //if (!grid.Template.NextStepCanJumpToSame)
                //{
                //    continue;
                //}
                //var nextStepCanJumpList = nextStepCanJumpToSameDict[grid.TemplateId];
                //foreach (SkillBreakPlateIndex otherIndex in nextStepCanJumpList)
                //{
                //    SkillBreakPlateGrid otherGrid = map[otherIndex];
                //    if (otherGrid.TemplateId != grid.TemplateId) continue;
                //    if (neighborPos == otherIndex) continue;
                //    if (otherGrid.State == ESkillBreakGridState.Failed) continue;
                //    if (state.Visited.Contains(otherIndex) && otherIndex != state.Index) continue;

                //    return false;
                //}
            }
            return true;
        }

        private int CalcDistance(SkillBreakPlateIndex a, SkillBreakPlateIndex b)
        {
            // var key = (a, b);
            int distance;
            //if (_cache.DistanceCache.ContainsKey(key))
            //{
            //    distance = _cache.DistanceCache[key];
            //}
            //else
            {
                distance = map.CalcDistance(a, b);
                //_cache.DistanceCache[key] = distance;
                //_cache.DistanceCache[(b, a)] = distance;
            }
            return distance;
        }

        private void RecycleState(State state)
        {
            RecycleHashSet(ref state.Visited);
            RecycleList(ref state.Path);
        }

        private static void RecycleHashSet(ref HashSet<SkillBreakPlateIndex> set)
        {
            set.Clear();
            HashSetPool.Return(set);
            set = null;
        }

        private static void RecycleList(ref List<SkillBreakPlateIndex> list)
        {
            list.Clear();
            ListPool.Return(list);
            list = null;
        }

        private struct State
        {
            public SkillBreakPlateIndex Index;
            public int RemainingSteps;
            public float Score;
            public HashSet<SkillBreakPlateIndex> Visited;
            public int RequiredMask;
            public List<SkillBreakPlateIndex> Path;
        }

        private struct StateKey : IEquatable<StateKey>
        {
            public int X;
            public int Y;
            public int RemainingSteps;
            public int RequiredMask;

            public SkillBreakPlateIndex Index;

            public StateKey(SkillBreakPlateIndex index, int steps, int mask)
            {
                this.Index = index;
                X = index.X;
                Y = index.Y;
                RemainingSteps = steps;
                RequiredMask = mask;
            }

            public bool Equals(StateKey other) => X == other.X && Y == other.Y &&
                RemainingSteps == other.RemainingSteps && RequiredMask == other.RequiredMask;

            public override int GetHashCode() => HashCode.Combine(X, Y, RemainingSteps, RequiredMask);
        }

        private struct Move
        {
            public SkillBreakPlateIndex NewIndex;
            public int Cost;
            public int AddSteps;
            public sbyte TemplateId;
        }
    }
}
