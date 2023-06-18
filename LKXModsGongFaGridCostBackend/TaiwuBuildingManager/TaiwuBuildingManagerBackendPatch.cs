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

        private void OnAdvanceMonthFinish(DataContext context)
        {
            if (!_enableMod) return;

            // 取消外出收集资源
            AutoCollectResourcesHelper.DemobilizePeopleToCollectResources(context);
        }
    }
}
