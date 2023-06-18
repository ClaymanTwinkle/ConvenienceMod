using System;
using System.Collections.Generic;
using System.Linq;
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
            var removeOperationResReturn = new int[8];
            for (short i = 0; i < _IntUselessResourceType.Length; i++)
            {
                if (_IntUselessResourceType[i])
                {
                    // 1. 杂草堆
                    // 2. 乱石堆
                    // 3. 废墟
                    CleanAllResourceById(context, Config.BuildingBlock.DefKey.UselessResourceBegin + i, ref removeOperationResReturn);
                }
            }
        }

        /// <summary>
        /// 检查增加的资源是否超重
        /// </summary>
        /// <param name="removeOperationResReturn"></param>
        /// <param name="addRes"></param>
        /// <returns></returns>
        private static bool CheckResourceIsOverload(int[] removeOperationResReturn, int[] addRes)
        {
            for (int i = 0; i < 6; i++)
            {
                if (addRes[i] == 0) continue;

                if (BuildingFinder.CheckAddResourceIsOverload((sbyte)i, removeOperationResReturn[i] + addRes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static void CleanAllResourceById(DataContext context, int buildingTemplateId, ref int[] removeOperationResReturn)
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
                if (Config.BuildingBlock.DefKey.Ruins == buildingTemplateId)
                {
                    // 废墟直接拆
                    CleanResource(context, buildingBlockDataKV.Item1, buildingBlockDataKV.Item2);
                }
                else
                {
                    var addRes = BuildingFinder.GetRemoveOperationResReturn(buildingBlockDataKV.Item2);
                    if (CheckResourceIsOverload(removeOperationResReturn, addRes))
                    {
                        break;
                    }

                    if (!CleanResource(context, buildingBlockDataKV.Item1, buildingBlockDataKV.Item2)) break;

                    for (int i = 0; i < Math.Min(removeOperationResReturn.Length, addRes.Length); i++)
                    {
                        removeOperationResReturn[i] += addRes[i];
                    }
                }
            }
        }

        public static bool CleanResource(DataContext context, BuildingBlockKey blockKey, BuildingBlockData buildingBlockData)
        {
            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return false;

            var wokers = WorkerSelector.SelectWorkersByPropertyValue(buildingBlockData.TemplateId, BuildingOperationType.Remove);

            if (wokers.All(x => x == -1)) return false;

            DomainManager.Building.Remove(context, blockKey, wokers);
            
            return true;
        }
    }
}
