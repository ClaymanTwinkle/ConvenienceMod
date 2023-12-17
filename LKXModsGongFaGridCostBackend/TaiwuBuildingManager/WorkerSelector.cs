using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Building;
using GameData.Domains;
using GameData.Utilities;
using Config;
using HarmonyLib;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    /// <summary>
    /// 工人选择器
    /// </summary>
    internal class WorkerSelector
    {
        public static int[] SelectWorkersByLifeSkillAttainment(int templateId, BuildingBlockKey buildingBlockKey, bool useEfficiency = true)
        {
            var _availableWorker = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);

            var _configData = Config.BuildingBlock.Instance[templateId];

            sbyte lifeSkillType = GetOperationNeedSkillType(_configData, buildingBlockKey);
            // AdaptableLog.Info("SelectWorkersByLifeSkillAttainment " + _configData.Name);

            if (lifeSkillType > -1)
            {
                _availableWorker.Sort((int workerA, int workerB) => DomainManager.Character.GetAllLifeSkillAttainment(workerA)[lifeSkillType].CompareTo(DomainManager.Character.GetAllLifeSkillAttainment(workerB)[lifeSkillType]));
            }

            // AdaptableLog.Info("SelectWorkersByLifeSkillAttainment " + _availableWorker.Count);

            int num9 = _availableWorker.Count - 1;
            int maxProduceValue = _configData.TemplateId == BuildingBlock.DefKey.BookCollectionRoom ? 100 : _configData.MaxProduceValue;

            var specialBlock = _configData.TemplateId == BuildingBlock.DefKey.MakeupRoom || _configData.TemplateId == BuildingBlock.DefKey.PhoenixPlatform;
            if (_configData.TemplateId == BuildingBlock.DefKey.MakeupRoom || _configData.TemplateId == BuildingBlock.DefKey.PhoenixPlatform)
            {
                useEfficiency = false;
            }

            var wokers = new int[3] { -1, -1, -1 };

            // AdaptableLog.Info("SelectWorkersByLifeSkillAttainment " + maxProduceValue);

            if (maxProduceValue > 0 || specialBlock)
            {
                int produceValue = 0;
                var _selectingShopManagerIndex = 0;
                while (_selectingShopManagerIndex < wokers.Length && num9 >= 0 && (_configData.TemplateId == BuildingBlock.DefKey.BookCollectionRoom || produceValue < maxProduceValue || !useEfficiency))
                {
                    int num12 = -1;

                    if (useEfficiency)
                    {
                        for (int num13 = 0; num13 < _availableWorker.Count; num13++)
                        {
                            if (DomainManager.Character.GetAllLifeSkillAttainment(_availableWorker[num13])[lifeSkillType] + 100 + produceValue >= maxProduceValue)
                            {
                                num12 = num13;
                                break;
                            }
                        }
                    }

                    if (num12 == -1)
                    {
                        wokers[_selectingShopManagerIndex] = _availableWorker[num9];
                        produceValue += 100 + DomainManager.Character.GetAllLifeSkillAttainment(_availableWorker[num9])[lifeSkillType];
                        if (useEfficiency && produceValue >= maxProduceValue)
                        {
                            break;
                        }

                        num9--;
                        _selectingShopManagerIndex++;
                        continue;
                    }
                    wokers[_selectingShopManagerIndex] = _availableWorker[num12];
                    break;
                }
            }

            return wokers;
        }

        private static sbyte GetOperationNeedSkillType(BuildingBlockItem _configData, BuildingBlockKey buildingBlockKey)
        {
            sbyte b = 15;
            if (_configData.IsCollectResourceBuilding)
            {
                sbyte collectBuildingResourceType = DomainManager.Building.GetCollectBuildingResourceType(buildingBlockKey);
                return (sbyte)((collectBuildingResourceType < 6) ? Config.ResourceType.Instance[collectBuildingResourceType].LifeSkillType : 9);
            }

            return _configData.RequireLifeSkillType;
        }

        public static int[] SelectWorkersByPropertyValue(int templateId, int buildingOperationType)
        {
            var wokers = new int[3] { -1, -1, -1 };

            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return wokers;

            var buildingBlockItem = Config.BuildingBlock.Instance[templateId];
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

            var costProgress = buildingBlockItem.OperationTotalProgress[buildingOperationType];
            var expectProgress = 0;
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
            }

            return wokers;
        }
    }
}
