using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using GameData.Common;
using GameData.Domains.Building;
using GameData.Domains.Map;
using GameData.Domains;
using GameData.Utilities;
using Newtonsoft.Json.Linq;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class AutoWorkHelper
    {
        private static bool[] _ToggleWorkMode = new bool[2] { false, false };

        public static void UpdateConfig(Dictionary<string, System.Object> config)
        {
            _ToggleWorkMode = ((JArray)config.GetValueOrDefault("Toggle_WorkMode", new JArray(false, false))).ToList().ConvertAll(x => (bool)x).ToArray();
        }

        /// <summary>
        /// 分配所有村民工作
        /// </summary>
        public static void AssignAllVillagersJobs(DataContext context)
        {
            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);
            // 所有建筑
            var allBuildingBlockList = BuildingFinder.FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.Building);
            AdaptableLog.Info("allBuildingBlockList = " + allBuildingBlockList.Count);
            foreach (var buildingBlockKey in allBuildingBlockList)
            {
                AutoWork(context, buildingBlockKey);
            }
        }

        /// <summary>
        /// 建筑分配工作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="buildingBlockKey"></param>
        public static void AutoWork(DataContext context, BuildingBlockKey buildingBlockKey)
        {
            // 清理选择
            for (sbyte i = 0; i < 3; i++)
            {
                DomainManager.Building.SetShopManager(context, buildingBlockKey, i, -1);
            }

            // 重新选择
            BuildingBlockData element_BuildingBlocks = DomainManager.Building.GetElement_BuildingBlocks(buildingBlockKey);
            var workers = WorkerSelector.SelectWorkersByLifeSkillAttainment(element_BuildingBlocks.TemplateId, buildingBlockKey, !_ToggleWorkMode[1]);
            if (workers.All(x => x < 0))
            {
                return;
            }
            for (sbyte i = 0; i < workers.Length; i++)
            {
                if (workers[i] > -1)
                {
                    AdaptableLog.Info("SetShopManager = " + workers[i]);
                    DomainManager.Building.SetShopManager(context, buildingBlockKey, i, workers[i]);
                }
            }
        }
    }
}
