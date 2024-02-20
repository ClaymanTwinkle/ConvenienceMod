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

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class BuildingUpgradeHelper
    {
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


            Action<BuildingBlockKey> action = x => {
                BuildingBlockData buildingBlockData = DomainManager.Building.GetElement_BuildingBlocks(x);
                var wokers = WorkerSelector.SelectWorkersByPropertyValue(buildingBlockData.TemplateId, BuildingOperationType.Upgrade);
                if (wokers.Any(x => x > -1))
                {
                    BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[buildingBlockData.TemplateId];
                    if (DomainManager.Building.CanUpgrade(x) && DomainManager.Building.UpgradeIsHaveEnoughResource(buildingBlockData) && buildingBlockData.Level < buildingBlockItem.MaxLevel)
                    {
                        DomainManager.Building.Upgrade(context, x, wokers);
                    }
                }
            };

            // 优先building
            BuildingFinder.FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.Building).ForEach(action);

            // 优先main building
            BuildingFinder.FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.MainBuilding).ForEach(action);
        }
    }
}
