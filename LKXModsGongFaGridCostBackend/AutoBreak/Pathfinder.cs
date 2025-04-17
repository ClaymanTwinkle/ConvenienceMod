using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.AutoBreak;
using ConvenienceBackend.Utils;
using GameData.Domains.Combat;
using GameData.Domains.Taiwu;
using GameData.Domains.Taiwu.LifeSkillCombat.Status;
using Microsoft.VisualBasic;
using NLog;
using NLog.Fluent;

/*
0    起    StartPoint = 0    突破的起点。
1    终    EndPoint = 1    "突破的终点。只有连接此格时，突破才能成功。否则无法获得任何突破奖励。"
2    玄机    Bonus = 2    无说明
3    如常    Normal = 3    按部就班的突破。    lightgrey
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
    public class MapCache
    {
        public readonly Dictionary<SkillBreakPlateIndex, List<SkillBreakPlateIndex>> neighborsGeneralCache = new();
        public readonly Dictionary<SkillBreakPlateIndex, sbyte> neighborsGeneralIdCache = new();
    }

    public class PathFinder
    {
        private static Logger _logger = LogManager.GetLogger("自动突破");


        private static readonly SkillBreakPlateAxial[] _neighborAxial = new SkillBreakPlateAxial[6]
{
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1),
            (-1, 1),
            (1, -1)
};

        private static readonly SkillBreakPlateAxial[] _pureNeighbors = new SkillBreakPlateAxial[9]
        {
            (-1, -1), (-1, 0), (-1, 1), (1, -1), (1, 0), (1, 1), (0, -1), (0, 0), (0, 1)
        };


        private readonly SkillBreakPlate map;
        public readonly int maxSteps;
        private readonly SkillBreakPlateIndex start;
        private SkillBreakPlateIndex end;
        private List<SkillBreakPlateIndex> bonusPoints = new List<SkillBreakPlateIndex>();
        private readonly HashSet<sbyte> excludedTypes = new() { 15, 16 };
        private int allRequiredCount;

        private HashSet<SkillBreakPlateIndex> initialVisited = new();

        private MapCache _cache = null;

        private readonly Dictionary<sbyte, List<SkillBreakPlateIndex>> nextStepCanJumpToSameDict = new();

        public PathFinder(SkillBreakPlate map, SkillBreakPlateIndex start, MapCache cache = null)
        {
            this.map = map;
            this.start = start;
            this.maxSteps = map.StepGoneMad - map.StepCostedGoneMad + (map.StepNormal - map.StepCostedNormal);
            allRequiredCount = 0;

            _cache = cache ?? new MapCache();

            initialVisited.Add(start);
            GenerateCache();
        }

        private void GenerateCache()
        {
            for (int j = 0; j < map.Width; j++)
            {
                for (int k = 0; k < map.Height; k++)
                {
                    var grid = map[j, k];
                    if (grid == null) continue;
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
        }

        public (int score, List<SkillBreakPlateIndex>) FindMaxScorePath()
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

            var queue = new PriorityQueue<State, int>();
            queue.Enqueue(initialState, -initialState.Score);

            var best = new Dictionary<StateKey, int>();
            var stateKey = new StateKey(initialState.Index, initialState.RemainingSteps, initialState.RequiredMask);
            best[stateKey] = initialState.Score;

            int maxScore = -1;
            List<SkillBreakPlateIndex> bestPath = null;
            int remainingSteps = initialState.RemainingSteps;

            int loopCount = 0;
            int ignoreCount = 0;

            while (queue.Count > 0)
            {
                loopCount++;

                var current = queue.Dequeue();

                if (current.Index.Equals(end))
                {
                    if (current.RequiredMask == allRequiredCount)
                    {
                        if (current.Score > maxScore || (current.Score == maxScore && current.RemainingSteps > remainingSteps))
                        {
                            maxScore = current.Score;
                            bestPath = current.Path;
                        }
                    }

                    continue;
                }

                if (current.RequiredMask != allRequiredCount)
                { 
                    var forceContinue = false;
                    foreach (var bonusPoint in bonusPoints)
                    {
                        if (IsUnreachable(current, bonusPoint)) { forceContinue = true; break; }
                    }
                    if (forceContinue) continue;
                }

                foreach (var move in GenerateMoves(current))
                {
                    var newVisited = new HashSet<SkillBreakPlateIndex>(current.Visited) { move.NewIndex };
                    int newRequiredMask = current.RequiredMask + (move.TemplateId == 2 ? 1 : 0);

                    int newRemaining = current.RemainingSteps - move.Cost + move.AddSteps;
                    if (newRemaining < 0) continue;

                    int newScore = 0;
                    var newPath = new List<SkillBreakPlateIndex>(current.Path) { move.NewIndex };
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

                    var featureScore = CalcEstimateAddMaxPower(newState); // newScore

                    var key = new StateKey(newState.Index, newState.RemainingSteps, newState.RequiredMask);

                    if (best.TryGetValue(key, out int existing) && featureScore <= existing) continue;

                    best[key] = featureScore;
                    queue.Enqueue(newState, -featureScore);
                }
            }

            _logger.Info($"${map.Width}x{map.Height}循环次数{loopCount}，忽略测试{ignoreCount}");

            return (maxScore, bestPath);
        }

        private IEnumerable<Move> GenerateMoves(State state)
        {
            var moves = new List<Move>();

            IEnumerable<SkillBreakPlateIndex> neighbors = GetNeighborsGeneral(state.Index);
            foreach (SkillBreakPlateIndex neighbor in neighbors)
            {
                if (!state.Visited.Contains(neighbor))
                    AddMove(state, neighbor, moves);
            }
            return moves;
        }

        private void AddMove(State state, SkillBreakPlateIndex index, List<Move> moves)
        {
            var cell = map[index];
            if (IsExcludedGrid(cell)) return;

            int cost = map.CalcCostStep(index);
            if (state.RemainingSteps - cost < 0) return;

            int addSteps = cell.Template.AddStepNormal;
            int score = CalcAddMaxPower(index, state.Visited);

            moves.Add(new Move
            {
                NewIndex = index,
                Cost = cost,
                AddSteps = addSteps,
                Score = score,
                TemplateId = cell.TemplateId,
            });
        }

        private bool IsExcludedGrid(SkillBreakPlateGrid grid)
        {
            if (grid.State == ESkillBreakGridState.Failed) return true;
            if (grid.State == ESkillBreakGridState.Selected) return true;
            if (grid.Template.Type == ESkillBreakGridTypeType.StartPoint) return true;
            return excludedTypes.Contains(grid.TemplateId);
        }

        /// <summary>
        /// 预测分数
        /// </summary>
        /// <param name="index"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        private int CalcEstimateAddMaxPower(State state)
        {
            int maxScore = state.Score;
            var newVisited = new HashSet<SkillBreakPlateIndex>(state.Visited);
            foreach (var move in GenerateMoves(state))
            {
                int newScore = 0;
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

        private int CalcAddMaxPower(SkillBreakPlateIndex index, HashSet<SkillBreakPlateIndex> visited)
        {
            int value = this.CalcAddMaxPowerBase(visited, index);
            bool ignoreEffectAddMaxPower = map[index].Template.IgnoreEffectAddMaxPower;
            int result;
            if (ignoreEffectAddMaxPower)
            {
                result = value;
            }
            else
            {
                int successNeighborCount = 0;
                var neighbors = GetPureNeighbors(index, 1);
                foreach (var neighbor in neighbors)
                {
                    if (map.CheckIndex(neighbor) && map.CalcDistance(index, neighbor) <= 1)
                    {
                        if (neighbor != index && visited.Contains(neighbor))
                        {
                            if (map[neighbor].Template.ClearNeighborMaxPower && map[index].TemplateId != 2)
                            {
                                return 0;
                            }
                            successNeighborCount++;
                            value += map[neighbor].Template.NeighborAddMaxPowerWhenActive;
                        }
                    }
                }
                result = value + successNeighborCount * map[index].Template.SucceedNeighborAddMaxPower;
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
                    result = this.CalcAddMaxPowerAsBonus(visited, index, 3);
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
                int value = (neighbor.TemplateId == 2) ? 0 : this.CalcAddMaxPower(neighborIndex, visited);
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

            List<SkillBreakPlateIndex> list = null;

            if (_cache.neighborsGeneralIdCache.ContainsKey(pos) && _cache.neighborsGeneralIdCache[pos] == grid.TemplateId) 
            {
                list = _cache.neighborsGeneralCache.GetValueOrDefault(pos);
            }
            if (list != null) return list;
            list = new List<SkillBreakPlateIndex>();
            _cache.neighborsGeneralCache[pos] = list;
            _cache.neighborsGeneralIdCache[pos] = grid.TemplateId;

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
                if (map[otherIndex].TemplateId == grid.TemplateId && !(pos == otherIndex) && map.CalcDistance(pos, otherIndex) != grid.Template.NextStepOffset && map[otherIndex].State.CanInteract())
                {
                    list.Add(otherIndex);
                }
            }

            return list;
        }

        private List<SkillBreakPlateIndex> GetPureNeighbors(SkillBreakPlateIndex pos, int distance = 1)
        {
            List<SkillBreakPlateIndex> points = new();

            for (int x = -distance; x <= distance; x++)
            {
                for (int y = -distance; y <= distance; y++)
                {
                    SkillBreakPlateIndex neighborPos = pos + (x, y);

                    if (map.CheckIndex(neighborPos) && map.CalcDistance(pos, neighborPos) <= distance)
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
                if (map[pos].State == ESkillBreakGridState.CanSelect) return false;
                if (grid.State == ESkillBreakGridState.Selected) continue;
                if (neighborPos == pos) continue;

                if (map.CalcDistance(pos, neighborPos) == grid.Template.NextStepOffset && (!state.Visited.Contains(neighborPos) || state.Index == neighborPos)) return false;

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

        private struct State
        {
            public SkillBreakPlateIndex Index;
            public int RemainingSteps;
            public int Score;
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
            public int Score;
            public sbyte TemplateId;
        }
    }
}
