using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConvenienceBackend.AutoBreak;
using ConvenienceBackend.Utils;
using GameData.Domains.Taiwu;
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
        private readonly HashSet<sbyte> excludedTypes = new() { 15, 16 };
        private int allRequiredCount;

        private HashSet<SkillBreakPlateIndex> initialVisited = new HashSet<SkillBreakPlateIndex>();

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
                    else if (grid.Template.Type == ESkillBreakGridTypeType.Bonus && grid.State != ESkillBreakGridState.Selected)
                    {
                        allRequiredCount++;
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

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.RequiredMask == allRequiredCount && current.Index.Equals(end))
                {
                    if (current.Score > maxScore || (current.Score == maxScore && current.RemainingSteps > remainingSteps))
                    {
                        maxScore = current.Score;
                        bestPath = current.Path;
                    }
                    continue;
                }

                foreach (var move in GenerateMoves(current))
                {
                    var newVisited = new HashSet<SkillBreakPlateIndex>(current.Visited) { (move.NewX, move.NewY) };
                    int newRequiredMask = current.RequiredMask + (move.TemplateId == 2 ? 1 : 0);

                    int newRemaining = current.RemainingSteps - move.Cost + move.AddSteps;
                    if (newRemaining < 0) continue;

                    int newScore = 0;
                    var newPath = new List<SkillBreakPlateIndex>(current.Path) { (move.NewX, move.NewY) };
                    foreach (var node in newPath)
                    {
                        newScore += CalcAddMaxPower(node, newVisited);
                    }

                    var newState = new State
                    {
                        Index = (move.NewX, move.NewY),
                        RemainingSteps = newRemaining,
                        Score = newScore,
                        Visited = newVisited,
                        RequiredMask = newRequiredMask,
                        Path = newPath
                    };

                    var key = new StateKey(newState.Index, newState.RemainingSteps, newState.RequiredMask);

                    if (best.TryGetValue(key, out int existing) && newScore <= existing) continue;

                    best[key] = newScore;
                    queue.Enqueue(newState, -newScore);
                }
            }
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
                NewX = index.X,
                NewY = index.Y,
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


        private int CalcAddMaxPower(SkillBreakPlateIndex index, HashSet<SkillBreakPlateIndex> visited)
        {
            int value = this.CalcAddMaxPowerBase(index);
            bool ignoreEffectAddMaxPower = map[index].Template.IgnoreEffectAddMaxPower;
            int result;
            if (ignoreEffectAddMaxPower)
            {
                result = value;
            }
            else
            {
                int successNeighborCount = 0;
                foreach (var (dx, dy) in _pureNeighbors)
                {
                    int x = index.X + dx;
                    int y = index.Y + dy;
                    SkillBreakPlateIndex neighbor = (x, y);
                    if (map.CheckIndex(x, y) && map.CalcDistance(index, neighbor) <= 1)
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

        private int CalcAddMaxPowerBase(SkillBreakPlateIndex index)
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
                    result = 0; // this.CalcAddMaxPowerAsBonus(index, this.GetBonus(index).ImpactRange);
                }
            }
            else
            {
                result = 0;
            }
            return result;
        }


        private List<SkillBreakPlateIndex> GetPureNeighbors(SkillBreakPlateIndex pos, int distance = 1)
        {
            var neighbors = new List<SkillBreakPlateIndex>();
            foreach (var (dx, dy) in _pureNeighbors)
            {
                int x = pos.X + dx;
                int y = pos.Y + dy;
                SkillBreakPlateIndex index = (x, y);
                if (map.CheckIndex(x, y) && map.CalcDistance(pos, index) <= distance)
                {
                    neighbors.Add(index);
                }
            }
            return neighbors;
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
            public int NewX;
            public int NewY;
            public int Cost;
            public int AddSteps;
            public int Score;
            public sbyte TemplateId;
        }
    }
}
