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
using GameData.Domains.Combat;
using GameData.Domains.Map;
using GameData.Domains.Taiwu;
using GameData.Domains.TaiwuEvent;
using GameData.Domains.World;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class TaiwuBuildingManagerBackendPatch : BaseBackendPatch
    {
        private static bool _enableMod = false;

        // 过月自动拆资源
        private static bool _enableRemoveUselessResource = false;

        // 建筑建完后自动工作
        private static bool _enableBuildingAutoWork = false;


        private static bool[] _IntUselessResourceType = new bool[3] { false, false, false };

        private static List<Location> _collectResourceLocations = new List<Location>();

        public override void OnModSettingUpdate(string modIdStr)
        {
            DomainManager.Mod.GetSetting(modIdStr, "Toggle_EnableBuildManager", ref _enableMod);
        }

        public override void OnEnterNewWorld()
        {
            base.OnEnterNewWorld();
            UnregisterHandlers();
            RegisterHandlers();
        }

        public override void OnLoadedArchiveData()
        {
            base.OnLoadedArchiveData();
            UnregisterHandlers();
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            Events.RegisterHandler_AdvanceMonthBegin(OnAdvanceMonthBegin);
            Events.RegisterHandler_AdvanceMonthFinish(OnAdvanceMonthFinish);
        }

        private void UnregisterHandlers()
        {
            Events.UnRegisterHandler_AdvanceMonthBegin(OnAdvanceMonthBegin);
            Events.UnRegisterHandler_AdvanceMonthFinish(OnAdvanceMonthFinish);
        }

        private static void InitConfig(Dictionary<string, System.Object> config)
        {
            AutoCollectResourcesHelper.UpdateConfig(config);
            AutoHarvestHelper.UpdateConfig(config);

            _enableRemoveUselessResource = (bool)config.GetValueOrDefault("Toggle_EnableRemoveUselessResource", false);
            _IntUselessResourceType = ((JArray)config.GetValueOrDefault("Toggle_IntUselessResourceType", new JArray(false, false, false))).ToList().ConvertAll(x => (bool)x).ToArray();

            _enableBuildingAutoWork = (bool)config.GetValueOrDefault("Toggle_EnableBuildingAutoWork", false);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuDomain), "CallMethod")]
        public static bool TaiwuDomain_CallMethod_Prefix(TaiwuDomain __instance, Operation operation, RawDataPool argDataPool, DataContext context, ref int __result)
        {
            if (operation.MethodId == GameDataBridgeConst.MethodId)
            {
                int num = operation.ArgsOffset;
                ushort num2 = 0;
                num += Serializer.Deserialize(argDataPool, num, ref num2);
                if (operation.ArgsCount == 2)
                {
                    switch (num2)
                    {
                        case GameDataBridgeConst.Flag.Flag_Load_Settings:
                            {
                                string json = null;
                                Serializer.Deserialize(argDataPool, num, ref json);
                                try
                                {
                                    if (json != null)
                                    {
                                        InitConfig(JsonConvert.DeserializeObject<Dictionary<string, System.Object>>(json));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    AdaptableLog.Warning("TaiwuBuildingManager Backend: Deserialize settings Json Failed:" + ex.Message, false);
                                }
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_Assign_Jobs:
                            {
                                string text = null;
                                Serializer.Deserialize(argDataPool, num, ref text);

                                AssignAllVillagersJobs(context);
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_Upgrade_buildings:
                            {
                                string text = null;
                                Serializer.Deserialize(argDataPool, num, ref text);

                                UpgradeAllBuildings(context);
                                break;
                            }
                    }
                }

                __result = -1;
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuEventDomain), "OnEvent_ConstructComplete")]
        public static void TaiwuEventDomain_OnEvent_ConstructComplete_Postfix(BuildingBlockKey arg0, short arg1, sbyte arg2)
        {
            if (!_enableMod) return;
            if (!_enableBuildingAutoWork) return;
            DomainManager.Building.SetBuildingAutoWork(DomainManager.TaiwuEvent.MainThreadDataContext, arg0.BuildingBlockIndex, true);
        }

        /// <summary>
        /// 开始过月
        /// </summary>
        /// <param name="context"></param>
        private void OnAdvanceMonthBegin(DataContext context)
        {
            if (!_enableMod) return;

            if (_enableRemoveUselessResource)
            {
                CleanAllUselessResource(context);
            }
            AutoCollectResourcesHelper.AssignIdlePeopleToCollectResources(context);
        }

        /// <summary>
        /// 结束过月
        /// </summary>
        /// <param name="context"></param>

        private void OnAdvanceMonthFinish(DataContext context)
        {
            if (!_enableMod) return;

            AutoCollectResourcesHelper.DemobilizePeopleToCollectResources(context);
            AutoHarvestHelper.HandleAutoHarvest(context);
        }

        private void CleanAllUselessResource(DataContext context)
        {
            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();

            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);

            for (short i = 0; i < _IntUselessResourceType.Length; i++)
            {
                // 1. 杂草堆
                // 2. 乱石堆
                // 3. 废墟
                CleanAllResourceById(context, Config.BuildingBlock.DefKey.UselessResourceBegin + i);
            }
        }

        private void CleanAllResourceById(DataContext context, int buildingTemplateId)
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

        private void CleanResource(DataContext context, BuildingBlockKey blockKey, BuildingBlockData buildingBlockData)
        {
            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return;

            var wokers = SelectWorkersByPropertyValue(buildingBlockData.TemplateId, BuildingOperationType.Remove);

            if (wokers.All(x => x == -1)) return;

            DomainManager.Building.Remove(context, blockKey, wokers);
        }

        private static int[] SelectWorkersByPropertyValue(int templateId, int buildingOperationType)
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

        private static int[] SelectWorkersByLifeSkillAttainment(int templateId, BuildingBlockKey buildingBlockKey)
        {
            var _availableWorker = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);

            var _configData = Config.BuildingBlock.Instance[templateId];

            sbyte lifeSkillType = GetOperationNeedSkillType(_configData, buildingBlockKey);

            AdaptableLog.Info("SelectWorkersByLifeSkillAttainment " + _configData.Name);

            _availableWorker.Sort((int workerA, int workerB) => DomainManager.Character.GetAllLifeSkillAttainment(workerA)[lifeSkillType].CompareTo(DomainManager.Character.GetAllLifeSkillAttainment(workerB)[lifeSkillType]));

            AdaptableLog.Info("SelectWorkersByLifeSkillAttainment " + _availableWorker.Count);

            int num9 = _availableWorker.Count - 1;
            int maxProduceValue = _configData.MaxProduceValue;

            var wokers = new int[3] { -1, -1, -1 };

            AdaptableLog.Info("SelectWorkersByLifeSkillAttainment " + maxProduceValue);

            if (maxProduceValue > 0)
            {
                int produceValue = 0;
                var _selectingShopManagerIndex = 0;
                while (_selectingShopManagerIndex < wokers.Length && num9 >= 0 && (_configData.TemplateId == 105 || produceValue < _configData.MaxProduceValue))
                {
                    int num12 = -1;
                    for (int num13 = 0; num13 < _availableWorker.Count; num13++)
                    {
                        if (DomainManager.Character.GetAllLifeSkillAttainment(_availableWorker[num13])[lifeSkillType] + 100 + produceValue >= maxProduceValue)
                        {
                            num12 = num13;
                            break;
                        }
                    }

                    if (num12 == -1)
                    {
                        wokers[_selectingShopManagerIndex] = _availableWorker[num9];
                        produceValue += 100 + DomainManager.Character.GetAllLifeSkillAttainment(_availableWorker[num9])[lifeSkillType];
                        if (produceValue >= maxProduceValue)
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

        /// <summary>
        /// 分配所有村民工作
        /// </summary>
        private static void AssignAllVillagersJobs(DataContext context)
        {
            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);
            // 所有建筑
            var allBuildingBlockList = FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.Building);
            AdaptableLog.Info("allBuildingBlockList = " + allBuildingBlockList.Count);
            foreach ( var buildingBlockKey in allBuildingBlockList )
            {
                // 清理选择
                for (sbyte i = 0; i < 3; i++)
                {
                    DomainManager.Building.SetShopManager(context, buildingBlockKey, i, -1);
                }

                // 重新选择
                BuildingBlockData element_BuildingBlocks = DomainManager.Building.GetElement_BuildingBlocks(buildingBlockKey);
                var workers = SelectWorkersByLifeSkillAttainment(element_BuildingBlocks.TemplateId, buildingBlockKey);
                if (workers.All(x => x < 0))
                {
                    continue;
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

        /// <summary>
        /// 升级所有建筑
        /// </summary>
        private static void UpgradeAllBuildings(DataContext context)
        {
            var villagers = DomainManager.Taiwu.GetAllVillagersAvailableForWork(true);
            if (villagers.Count == 0) return;

            Location taiwuVillageLocation = DomainManager.Taiwu.GetTaiwuVillageLocation();
            var buildingAreaData = DomainManager.Building.GetBuildingAreaData(taiwuVillageLocation);

            // 优先building
            FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.Building).ForEach(x => {
                BuildingBlockData buildingBlockData = DomainManager.Building.GetElement_BuildingBlocks(x);
                var wokers = SelectWorkersByPropertyValue(buildingBlockData.TemplateId, BuildingOperationType.Upgrade);
                if (wokers.Any(x => x > -1)) 
                {
                    if (DomainManager.Building.CanUpgrade(x))
                    {
                        DomainManager.Building.Upgrade(context, x, wokers);
                    }
                }
            });

            // 优先main building
            FindBuildingsByType(taiwuVillageLocation, buildingAreaData, EBuildingBlockType.MainBuilding).ForEach(x => {
                BuildingBlockData buildingBlockData = DomainManager.Building.GetElement_BuildingBlocks(x);
                var wokers = SelectWorkersByPropertyValue(buildingBlockData.TemplateId, BuildingOperationType.Upgrade);
                if (wokers.Any(x => x > -1))
                {
                    if (DomainManager.Building.CanUpgrade(x))
                    {
                        DomainManager.Building.Upgrade(context, x, wokers);
                    }
                }
            });
        }

        private static List<BuildingBlockKey> FindBuildingsByType(Location location, BuildingAreaData buildingArea, EBuildingBlockType type)
        {
            List<BuildingBlockKey> list = new List<BuildingBlockKey>();
            for (short num = 0; num < buildingArea.Width * buildingArea.Width; num = (short)(num + 1))
            {
                BuildingBlockKey buildingBlockKey = new(location.AreaId, location.BlockId, num);
                BuildingBlockData element_BuildingBlocks = DomainManager.Building.GetElement_BuildingBlocks(buildingBlockKey);
                var buildingBlockItem = Config.BuildingBlock.Instance[element_BuildingBlocks.TemplateId];
                if (buildingBlockItem != null && buildingBlockItem.Type == type && element_BuildingBlocks.CanUse())
                {
                    list.Add(buildingBlockKey);
                }
            }

            return list;
        }
    }
}
