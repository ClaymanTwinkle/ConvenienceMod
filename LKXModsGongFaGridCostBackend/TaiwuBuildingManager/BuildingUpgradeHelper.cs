using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains.Building;
using GameData.Domains.Map;
using GameData.Domains;
using Config;
using NLog;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class BuildingUpgradeHelper
    {
        private static Logger _logger = LogManager.GetLogger("太吾管家");

        public static void UpdateConfig(Dictionary<string, System.Object> config)
        {
        }

        /// <summary>
        /// 升级所有建筑
        /// </summary>
        public static void UpgradeAllBuildings(DataContext context)
        {
            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return;

            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);

            void cleanAction(BuildingBlockKey x)
            {
                BuildingBlockData buildingBlockData = DomainManager.Building.GetElement_BuildingBlocks(x);
                if (IgnoreBuilding(buildingBlockData.TemplateId)) return;
                BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[buildingBlockData.TemplateId];
                if (buildingBlockData.OperationType == BuildingOperationType.Upgrade)
                {
                    if (DomainManager.Building.TryGetElement_BuildingOperatorDict(x, out var characterList))
                    {
                        var hasChar = false;
                        for (int i = 0; i < characterList.GetCount(); i++)
                        {
                            if (characterList[i] > -1)
                            {
                                hasChar = true;
                                break;
                            }
                        }
                        if (!hasChar)
                        {
                            _logger.Info("发现[" + buildingBlockItem.Name + "]没人升级，先取消升级");
                            DomainManager.Building.SetStopOperation(context, x, true);
                        }
                    }
                    else
                    {
                        _logger.Info("发现[" + buildingBlockItem.Name + "]没人升级，先取消升级");
                        DomainManager.Building.SetStopOperation(context, x, true);
                    }
                }
            }
            // 优先building
            BuildingFinder.FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.Building, true).ForEach(cleanAction);

            // 优先main building
            BuildingFinder.FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.MainBuilding, true).ForEach(cleanAction);

            void upgradeAction(BuildingBlockKey x)
            {
                BuildingBlockData buildingBlockData = DomainManager.Building.GetElement_BuildingBlocks(x);
                if (IgnoreBuilding(buildingBlockData.TemplateId)) return;
                var wokers = WorkerSelector.SelectWorkersByPropertyValue(buildingBlockData.TemplateId, BuildingOperationType.Upgrade);
                if (wokers.Any(x => x > -1))
                {
                    BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[buildingBlockData.TemplateId];
                    if (DomainManager.Building.CanUpgrade(x) && DomainManager.Building.UpgradeIsHaveEnoughResource(buildingBlockData) && buildingBlockData.Level < buildingBlockItem.MaxLevel)
                    {
                        DomainManager.Building.Upgrade(context, x, wokers);
                    }
                }
            }

            // 优先building
            BuildingFinder.FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.Building).ForEach(upgradeAction);

            // 优先main building
            BuildingFinder.FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.MainBuilding).ForEach(upgradeAction);
        }

        private static bool IgnoreBuilding(short templateId)
        {
            return BuildingBlock.DefKey.ChickenCoop == templateId;
        }
    }
}
