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

        // 建筑自动分配人员工作
        private static bool _enableBuildingAutoWork = false;

        // 建筑自动分配人员升级
        private static bool _enableBuildingAutoUpdate = false;

        // 自动刮胡子
        private static bool _enableAutoShave = false;

        // 资源可建造
        private static bool _enableBuildResource = false;
        // 移动建造不消耗耐久
        private static bool _enableMoveBuildNoDurability = false;

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WorldDomain), "AdvanceMonth")]
        public static void WorldDomain_AdvanceMonth_Postfix(DataContext context)
        {
            // 自动收获建筑资源
            AutoHarvestHelper.HandleAutoHarvest(context);
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


        public override void OnConfigUpdate(Dictionary<string, object> config)
        {
            base.OnConfigUpdate(config);
            InitConfig(config);
        }

        private static void InitConfig(Dictionary<string, System.Object> config)
        {
            AutoCollectResourcesHelper.UpdateConfig(config);
            AutoHarvestHelper.UpdateConfig(config);
            AutoWorkHelper.UpdateConfig(config);
            UselessResourceCleaner.UpdateConfig(config);
            ResidenceAutoLiveHelper.UpdateConfig(config);

            _enableRemoveUselessResource = (bool)config.GetValueOrDefault("Toggle_EnableRemoveUselessResource", false);

            _enableBuildingAutoWork = (bool)config.GetValueOrDefault("Toggle_EnableBuildingAutoWork", false);
            _enableBuildingAutoUpdate = (bool)config.GetValueOrDefault("Toggle_EnableBuildingAutoUpdate", false);

            _enableAutoShave = (bool)config.GetValueOrDefault("Toggle_EnableAutoShave", false);

            // 可以建造自然资源
            _enableBuildResource = (bool)config.GetValueOrDefault("Toggle_EnableBuildResource", false);
            // 移动建造不消耗耐久
            _enableMoveBuildNoDurability = (bool)config.GetValueOrDefault("Toggle_EnableMoveBuildNoDurability", false);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuDomain), "CallMethod")]
        public static bool TaiwuDomain_CallMethod_Prefix(TaiwuDomain __instance, Operation operation, RawDataPool argDataPool, DataContext context, ref int __result)
        {
            if (operation.MethodId == GameDataBridgeConst.MethodId)
            {
                int num = operation.ArgsOffset;
                ushort flag = 0;
                num += Serializer.Deserialize(argDataPool, num, ref flag);
                if (operation.ArgsCount == 2)
                {
                    switch (flag)
                    {
                        case GameDataBridgeConst.Flag.Flag_Assign_Jobs:
                            {
                                string text = null;
                                Serializer.Deserialize(argDataPool, num, ref text);

                                AutoWorkHelper.AssignAllVillagersJobs(context);
                                break;
                            }
                        case GameDataBridgeConst.Flag.Flag_Upgrade_buildings:
                            {
                                string text = null;
                                Serializer.Deserialize(argDataPool, num, ref text);

                                BuildingUpgradeHelper.UpgradeAllBuildings(context);
                                break;
                            }
                    }
                }

                __result = -1;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 建造建筑
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="context"></param>
        /// <param name="blockKey"></param>
        /// <param name="buildingTemplateId"></param>
        /// <param name="workers"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingDomain), "Build")]
        public static void BuildingDomain_Build_Patch(BuildingDomain __instance, ref DataContext context, ref BuildingBlockKey blockKey, ref short buildingTemplateId, ref int[] workers)
        {
            BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[buildingTemplateId];
            if (buildingBlockItem.Type == EBuildingBlockType.NormalResource || buildingBlockItem.Type == EBuildingBlockType.SpecialResource)
            {
                BuildingBlockData element_BuildingBlocks = DomainManager.Building.GetElement_BuildingBlocks(blockKey);
                DomainManager.Building.GmCmd_BuildImmediately(context, buildingTemplateId, blockKey, element_BuildingBlocks.Level);
            }
        }

        /// <summary>
        /// 判断建筑能否建造
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="blockKey"></param>
        /// <param name="buildingTemplateId"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildingDomain), "CanBuild")]
        public static bool BuildingDomain_CanBuild_Patch(BuildingDomain __instance, ref BuildingBlockKey blockKey, ref short buildingTemplateId, ref bool __result)
        {
            if (!_enableBuildResource) return true;

            BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[buildingTemplateId];

            if (!(buildingBlockItem.Type == EBuildingBlockType.NormalResource || buildingBlockItem.Type == EBuildingBlockType.SpecialResource)) return true;

            Location location = new Location(blockKey.AreaId, blockKey.BlockId);
            if (!__instance.GetTaiwuBuildingAreas().Contains(location)) return true;

            BuildingBlockData element_BuildingBlocks = __instance.GetElement_BuildingBlocks(blockKey);
            if (element_BuildingBlocks.TemplateId != 0) return true;

            BuildingAreaData element_BuildingAreas = __instance.GetElement_BuildingAreas(location);
            sbyte width = BuildingBlock.Instance[buildingTemplateId].Width;
            __instance.IsBuildingBlocksEmpty(blockKey.AreaId, blockKey.BlockId, blockKey.BuildingBlockIndex, element_BuildingAreas.Width, width);
            List<short> item = ObjectPool<List<short>>.Instance.Get();
            bool canBuild = true;
            if (canBuild && buildingTemplateId >= 0)
            {
                canBuild = ((!buildingBlockItem.isUnique || !BuildingDomain.HasBuilt(location, element_BuildingAreas, buildingTemplateId, true)) && __instance.AllDependBuildingAvailable(blockKey, buildingTemplateId, out sbyte b));
                if (canBuild)
                {
                    ushort[] baseBuildCost = buildingBlockItem.BaseBuildCost;
                    GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
                    for (sbyte b2 = 0; b2 < 8; b2 += 1)
                    {
                        if (taiwu.GetResource(b2) < (int)baseBuildCost[(int)b2])
                        {
                            canBuild = false;
                            break;
                        }
                    }
                }
            }
            ObjectPool<List<short>>.Instance.Return(item);
            __result = canBuild;
            return false;
        }

        /// <summary>
        /// 移动建造消耗经验
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="context"></param>
        /// <param name="originalBlockKey"></param>
        /// <param name="nowBlockKey"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildingDomain), "ExchangeBlockData")]
        public static bool BuildingDomain_ExchangeBlockData_Patch(BuildingDomain __instance, ref DataContext context, ref BuildingBlockKey originalBlockKey, ref BuildingBlockKey nowBlockKey)
        {
            if (!_enableMoveBuildNoDurability) return true;

            MapBlockItem mapBlockItem = MapBlock.Instance[DomainManager.Map.GetBlock(originalBlockKey.AreaId, originalBlockKey.BlockId).TemplateId];
            sbyte buildingAreaWidth = mapBlockItem.BuildingAreaWidth;
            BuildingBlockData buildingBlockData = __instance.GetElement_BuildingBlocks(originalBlockKey).Clone();
            BuildingBlockItem item = BuildingBlock.Instance.GetItem(buildingBlockData.TemplateId);
            buildingBlockData.Durability = item.MaxDurability;
            for (int i = 0; i < (int)item.Width; i++)
            {
                for (int j = 0; j < (int)item.Width; j++)
                {
                    short num = (short)((int)originalBlockKey.BuildingBlockIndex + i * (int)buildingAreaWidth + j);
                    BuildingBlockKey buildingBlockKey = new BuildingBlockKey(originalBlockKey.AreaId, originalBlockKey.BlockId, num);
                    BuildingBlockData buildingBlockData2 = new BuildingBlockData(num, 0, -1, -1);
                    Traverse.Create(__instance).Method("SetElement_BuildingBlocks", new object[]
                    {
                            buildingBlockKey,
                            buildingBlockData2,
                            context
                    }).GetValue();
                }
            }
            for (int k = 0; k < (int)item.Width; k++)
            {
                for (int l = 0; l < (int)item.Width; l++)
                {
                    short num2 = (short)((int)nowBlockKey.BuildingBlockIndex + k * (int)buildingAreaWidth + l);
                    BuildingBlockKey buildingBlockKey2 = new BuildingBlockKey(nowBlockKey.AreaId, nowBlockKey.BlockId, num2);
                    BuildingBlockData buildingBlockData3 = (num2 == nowBlockKey.BuildingBlockIndex) ? buildingBlockData : new BuildingBlockData(num2, 0, -1, nowBlockKey.BuildingBlockIndex);
                    buildingBlockData3.BlockIndex = num2;
                    Traverse.Create(__instance).Method("SetElement_BuildingBlocks", new object[]
                    {
                            buildingBlockKey2,
                            buildingBlockData3,
                            context
                    }).GetValue();
                }
            }

            return false;
        }

        /// <summary>
        /// 开始过月
        /// </summary>
        /// <param name="context"></param>
        private void OnAdvanceMonthBegin(DataContext context)
        {
            if (!_enableMod) return;

            if (_enableBuildingAutoWork)
            {
                // 村民自动工作
                AutoWorkHelper.AssignAllVillagersJobs(context);
            }

            if (_enableBuildingAutoUpdate)
            {
                // 建筑自动升级
                BuildingUpgradeHelper.UpgradeAllBuildings(context);
            }

            if (_enableRemoveUselessResource)
            {
                // 自动拆资源
                UselessResourceCleaner.CleanAllUselessResource(context);
            }

            // 居所自动住人
            ResidenceAutoLiveHelper.AutoLive(context);

            // 自动外出收集资源
            AutoCollectResourcesHelper.AssignIdlePeopleToCollectResources(context);
        }

        /// <summary>
        /// 结束过月
        /// </summary>
        /// <param name="context"></param>
        private unsafe void OnAdvanceMonthFinish(DataContext context)
        {
            if (!_enableMod) return;

            // 取消外出收集资源
            AutoCollectResourcesHelper.DemobilizePeopleToCollectResources(context);

            // 禁用过月刮胡子
            // ClearBeard(context);
        }

        private unsafe void ClearBeard(DataContext context)
        {
            if (_enableAutoShave)
            {
                var taiwuId = DomainManager.Taiwu.GetTaiwuCharId();
                var avatar = DomainManager.Taiwu.GetTaiwu().GetAvatar();

                var hadBeard = false;

                if (avatar.Beard1Id != 0)
                {
                    hadBeard = true;
                    avatar.Beard1Id = 0;
                    DomainManager.Character.InitializeAvatarElementGrowthProgress(context, taiwuId, 1);
                }


                if (avatar.Beard2Id != 0)
                {
                    hadBeard = true;
                    avatar.Beard2Id = 0;
                    DomainManager.Character.InitializeAvatarElementGrowthProgress(context, taiwuId, 2);
                }

                if (hadBeard)
                {
                    DomainManager.Taiwu.GetTaiwu().SetAvatar(avatar, context);
                    AdaptableLog.Info("刮胡子啦");
                }
            }
        }
    }
}
