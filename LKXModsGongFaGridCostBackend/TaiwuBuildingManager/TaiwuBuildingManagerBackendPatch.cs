using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehTree;
using Config;
using Config.Common;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Building;
using GameData.Domains.Character;
using GameData.Domains.Map;
using GameData.Domains.World;
using GameData.Utilities;
using HarmonyLib;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class TaiwuBuildingManagerBackendPatch : BaseBackendPatch
    {
        // 过月自动拆资源
        private static bool _enableRemoveUselessResource = false;
        // 过月自动采资源
        private static bool _enableCollectResource = false;

        private static List<Location> _collectResourceLocations = new List<Location>();

        public override void OnModSettingUpdate(string modIdStr)
        {
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_EnableRemoveUselessResource", ref _enableRemoveUselessResource);
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_EnableCollectResource", ref _enableCollectResource);
        }

        public override void OnEnterNewWorld()
        {
            base.OnEnterNewWorld();
            Events.UnRegisterHandler_AdvanceMonthBegin(OnAdvanceMonthBegin);
            Events.UnRegisterHandler_AdvanceMonthFinish(OnAdvanceMonthFinish);
            Events.RegisterHandler_AdvanceMonthBegin(OnAdvanceMonthBegin);
            Events.RegisterHandler_AdvanceMonthFinish(OnAdvanceMonthFinish);
        }

        public override void OnLoadedArchiveData()
        {
            base.OnLoadedArchiveData();
            Events.UnRegisterHandler_AdvanceMonthBegin(OnAdvanceMonthBegin);
            Events.UnRegisterHandler_AdvanceMonthFinish(OnAdvanceMonthFinish);
            Events.RegisterHandler_AdvanceMonthBegin(OnAdvanceMonthBegin);
            Events.RegisterHandler_AdvanceMonthFinish(OnAdvanceMonthFinish);
        }

        private void OnAdvanceMonthBegin(DataContext context)
        {
            if (_enableRemoveUselessResource)
            {
                CleanAllUselessResource(context);
            }
            if (_enableCollectResource)
            {
                AssignIdlePeopleToCollectResources(context);
            }
        }

        private void OnAdvanceMonthFinish(DataContext context)
        {
            if (_enableCollectResource)
            {
                DemobilizePeopleToCollectResources(context);
            }
        }

        private void AssignIdlePeopleToCollectResources(DataContext context)
        {
            _collectResourceLocations.Clear();
            // 0 Food
            // 1 Wood
            // 2 Stone
            // 3 Jade
            // 4 Silk
            // 5 Herbal
            var resourceNames = new string[6] { "食材", "木材", "石头", "玉石", "织物", "药材" };
            // 查找最少资源项
            sbyte collectResourceType = 0;
            for (sbyte i = 1; i < 6; i++)
            {
                if (DomainManager.Taiwu.GetTaiwu().GetResource(i) < DomainManager.Taiwu.GetTaiwu().GetResource(collectResourceType))
                {
                    collectResourceType = i;
                }
            }

            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var allAreaBlocks = DomainManager.Map.GetAreaBlocks(taiwuVillageLocation.AreaId).ToArray().FindAll(x=>x.CurrResources.Get(collectResourceType)>0 && !DomainManager.Taiwu.TryGetElement_VillagerWorkLocations(x.GetLocation(), out var xx));
            allAreaBlocks.Sort((a, b) => b.CurrResources.Get(collectResourceType) - a.CurrResources.Get(collectResourceType));
            foreach (var mapBlockData in allAreaBlocks)
            {
                var charId = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true).FirstOrDefault(-1);

                if (charId > -1)
                {
                    _collectResourceLocations.Add(mapBlockData.GetLocation());

                    var hasMark = DomainManager.Extra.TryGetElement_LocationMarkHashSet(mapBlockData.GetLocation(), out var ignore);
                    // 派遣工作
                    DomainManager.Taiwu.SetVillagerCollectResourceWork(context, charId, mapBlockData.AreaId, mapBlockData.BlockId, collectResourceType);
                    // 移动到目的地
                    GameData.Domains.Character.Character character = DomainManager.Character.GetElement_Objects(charId);
                    Location srcLocation = character.GetLocation();
                    if (srcLocation.AreaId != mapBlockData.AreaId || srcLocation.BlockId != mapBlockData.BlockId)
                    {
                        DomainManager.Character.LeaveGroup(context, character, true);
                        DomainManager.Character.GroupMove(context, character, mapBlockData.GetLocation());
                    }
                    // 不要标记
                    if (!hasMark)
                    {
                        DomainManager.Extra.RemoveLocationMark(context, mapBlockData.GetLocation());
                    }
                }
                else
                {
                    break;
                }
            }
            AdaptableLog.Info("派遣闲人采资源团" + _collectResourceLocations.Count + "人，去采集" + resourceNames[collectResourceType] + "，预计能采集到" + DomainManager.Taiwu.CalcResourceChangeByVillageWork(context)[collectResourceType]);
        }

        private void DemobilizePeopleToCollectResources(DataContext context)
        {
            foreach (var location in _collectResourceLocations)
            {
                DomainManager.Taiwu.StopVillagerWork(context, location.AreaId, location.BlockId, 10);
            }

            AdaptableLog.Info("遣散闲人采资源团" + _collectResourceLocations.Count + "人");
            _collectResourceLocations.Clear();
        }

        private void CleanAllUselessResource(DataContext context)
        {
            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();

            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);
            // 1. 废墟
            CleanAllResourceById(context, Config.BuildingBlock.DefKey.Ruins);

            // 2. 杂草堆
            CleanAllResourceById(context, Config.BuildingBlock.DefKey.UselessResourceBegin);

            // 3. 乱石堆
            CleanAllResourceById(context, Config.BuildingBlock.DefKey.UselessResourceBegin + 1);
        }

        private void CleanAllResourceById(DataContext context, short buildingTemplateId)
        {
            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return;

            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);

            var buildingBlockDataList = DomainManager.Building.FindAllBuildingsWithSameTemplate(taiwuVillageLocation, buildingAreaData, buildingTemplateId).ConvertAll(x => ValueTuple.Create(x, DomainManager.Building.GetBuildingBlockData(x))).FindAll(x => x.Item2.OperationType == BuildingOperationType.Invalid).OrderByDescending(x => x.Item2.Level).ToList();

            var buildingBlockItem = Config.BuildingBlock.Instance[buildingTemplateId];
            AdaptableLog.Info(buildingBlockItem.Name + " " + buildingBlockDataList.Count);

            foreach (var buildingBlockDataKV in buildingBlockDataList)
            {
                CleanResource(context, buildingBlockDataKV.Item1, buildingBlockDataKV.Item2);
            }
        }

        private void CleanResource(DataContext context, BuildingBlockKey blockKey, BuildingBlockData buildingBlockData)
        {
            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return;

            var buildingBlockItem = Config.BuildingBlock.Instance[buildingBlockData.TemplateId];
            Dictionary<int, short> _propertyValueDict = new Dictionary<int, short>();
            foreach (var villager in villagers)
            {
                if (buildingBlockItem.RequireLifeSkillType >= 0)
                {
                    var attainment = DomainManager.Character.GetLifeSkillAttainment(villager, buildingBlockItem.RequireLifeSkillType);
                    if (!_propertyValueDict.ContainsKey(attainment.Item1))
                    {
                        _propertyValueDict.Add(attainment.Item1, (short)attainment.Item2);
                    }
                }

                if (buildingBlockItem.RequireCombatSkillType >= 0)
                {
                    var attainment = DomainManager.Character.GetCombatSkillAttainment(villager, buildingBlockItem.RequireCombatSkillType);
                    if (!_propertyValueDict.ContainsKey(attainment.Item1))
                    {
                        _propertyValueDict.Add(attainment.Item1, (short)attainment.Item2);
                    }
                }
            }
            villagers.Sort((int workerA, int workerB) => _propertyValueDict[workerA].CompareTo(_propertyValueDict[workerB]));

            var costProgress = buildingBlockItem.OperationTotalProgress[BuildingOperationType.Remove];
            var expectProgress = 0;
            var wokers = new int[3] { -1, -1, -1 };
            var maxCharCount = Math.Min(villagers.Count, wokers.Length);
            var index = 0;
            if (costProgress > 0)
            {
                while (index < maxCharCount)
                {
                    wokers[index] = villagers[index];
                    expectProgress += 100 + _propertyValueDict.GetValueSafe(wokers[index++]);
                    if (expectProgress >= costProgress)
                    {
                        break;
                    }
                }
                DomainManager.Building.Remove(context, blockKey, wokers);
            }
        }
    }
}
