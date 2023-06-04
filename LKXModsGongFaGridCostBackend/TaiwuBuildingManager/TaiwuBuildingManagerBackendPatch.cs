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

namespace ConvenienceBackend.TaiwuBuildingManager
{
    internal class TaiwuBuildingManagerBackendPatch : BaseBackendPatch
    {
        private static bool _enableMod = false;

        // 过月自动拆资源
        private static bool _enableRemoveUselessResource = false;
        // 过月自动采资源
        private static bool _enableCollectResource = false;
        // 建筑建完后自动工作
        private static bool _enableBuildingAutoWork = false;

        private static bool[] _IntUselessResourceType = new bool[3] { false, false, false };

        private static bool[] _IntCollectResourceType = new bool[6] { false, false, false, false, false, false };

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
            _enableRemoveUselessResource = (bool)config.GetValueOrDefault("Toggle_EnableRemoveUselessResource", false);
            _enableCollectResource = (bool)config.GetValueOrDefault("Toggle_EnableCollectResource", false);
            _IntUselessResourceType = ((JArray)config.GetValueOrDefault("Toggle_IntUselessResourceType", new JArray(false, false, false))).ToList().ConvertAll(x => (bool)x).ToArray();
            _IntCollectResourceType = ((JArray)config.GetValueOrDefault("Toggle_IntCollectResourceType", new JArray(false, false, false, false, false, false))).ToList().ConvertAll(x => (bool)x).ToArray();

            _enableBuildingAutoWork = (bool)config.GetValueOrDefault("Toggle_EnableBuildingAutoWork", false);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuDomain), "CallMethod")]
        public static bool TaiwuDomain_CallMethod_Prefix(TaiwuDomain __instance, Operation operation, RawDataPool argDataPool, ref int __result)
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
            if (_enableCollectResource)
            {
                AssignIdlePeopleToCollectResources(context);
            }
        }

        /// <summary>
        /// 结束过月
        /// </summary>
        /// <param name="context"></param>

        private void OnAdvanceMonthFinish(DataContext context)
        {
            if (!_enableMod) return;

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
            var resourceNames = new string[6] { "食材", "木材", "金铁", "玉石", "织物", "药材" };
            // 查找最少资源项
            sbyte collectResourceType = 0;
            for (sbyte i = 0; i < 6; i++)
            {
                if (DomainManager.Taiwu.GetTaiwu().GetResource(i) < DomainManager.Taiwu.GetTaiwu().GetResource(collectResourceType))
                {
                    collectResourceType = i;
                }
            }
            // 如果不是最小项，则选用户选择的
            for (sbyte i = 0; i < _IntCollectResourceType.Length; i++)
            {
                if (_IntCollectResourceType[i])
                {
                    collectResourceType = i;
                    break;
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
