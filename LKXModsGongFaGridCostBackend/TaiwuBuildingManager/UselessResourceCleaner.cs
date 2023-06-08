using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Common;
using GameData.Domains.Building;
using GameData.Domains.Map;
using GameData.Domains;
using GameData.Utilities;
using Newtonsoft.Json.Linq;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class UselessResourceCleaner
    {
        private static bool[] _IntUselessResourceType = new bool[3] { false, false, false };

        public static void UpdateConfig(Dictionary<string, System.Object> config)
        {
            _IntUselessResourceType = ((JArray)config.GetValueOrDefault("Toggle_IntUselessResourceType", new JArray(false, false, false))).ToList().ConvertAll(x => (bool)x).ToArray();
        }

        public static void CleanAllUselessResource(DataContext context)
        {
            for (short i = 0; i < _IntUselessResourceType.Length; i++)
            {
                // 1. 杂草堆
                // 2. 乱石堆
                // 3. 废墟
                CleanAllResourceById(context, Config.BuildingBlock.DefKey.UselessResourceBegin + i);
            }
        }

        public static void CleanAllResourceById(DataContext context, int buildingTemplateId)
        {
            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return;

            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);

            var buildingBlockDataList = DomainManager.Building.FindAllBuildingsWithSameTemplate(taiwuVillageLocation, buildingAreaData, (short)buildingTemplateId).ConvertAll(x => ValueTuple.Create(x, DomainManager.Building.GetBuildingBlockData(x))).FindAll(x => x.Item2.OperationType == BuildingOperationType.Invalid).OrderByDescending(x => x.Item2.Level).ToList();

            var buildingBlockItem = Config.BuildingBlock.Instance[buildingTemplateId];
            AdaptableLog.Info(buildingBlockItem.Name + " " + buildingBlockDataList.Count);

            foreach (var buildingBlockDataKV in buildingBlockDataList)
            {
                CleanResource(context, buildingBlockDataKV.Item1, buildingBlockDataKV.Item2);
            }
        }

        public static void CleanResource(DataContext context, BuildingBlockKey blockKey, BuildingBlockData buildingBlockData)
        {
            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return;

            var wokers = WorkerSelector.SelectWorkersByPropertyValue(buildingBlockData.TemplateId, BuildingOperationType.Remove);

            if (wokers.All(x => x == -1)) return;

            DomainManager.Building.Remove(context, blockKey, wokers);
        }
    }
}
