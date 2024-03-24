using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ConvenienceBackend.AutoBreak;
using ConvenienceBackend.CombatSimulator;
using ConvenienceBackend.CombatStrategy;
using ConvenienceBackend.ComparativeArt;
using ConvenienceBackend.CricketCombatOptimize;
using ConvenienceBackend.CustomSteal;
using ConvenienceBackend.CustomWeapon;
using ConvenienceBackend.ManualArchive;
using ConvenienceBackend.MergeBookPanel;
using ConvenienceBackend.NotNTR;
using ConvenienceBackend.ProfessionOptimize;
using ConvenienceBackend.QuicklyCreateCharacter;
using ConvenienceBackend.TaiwuBuildingManager;
using ConvenienceBackend.TongdaoCombat;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Global;
using GameData.Domains.Global.Inscription;
using GameData.Domains.Map;
using GameData.Domains.Mod;
using GameData.Domains.Organization;
using GameData.Domains.SpecialEffect;
using GameData.Domains.Taiwu;
using GameData.Domains.World;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json;
using NLog;
using TaiwuModdingLib.Core.Plugin;

namespace ConvenienceBackend
{
    // Token: 0x02000002 RID: 2
    [PluginConfig("ConvenienceBackend", "kesar", "1.0.0")]
    public class ConvenienceBackend : TaiwuRemakePlugin
    {
        private static readonly List<BaseBackendPatch> allPatchList = new()
        {
            // 较艺必胜
            new ComparativeArtBackendPatch(),
            // sl偷窃、哄骗
            new CustomStealBackendPatch(),
            // 自动战斗
            new CombatStrategyBackendPatch(),
            // 拒绝NTR
            new NotNTRBackendPatch(),
            // 修改武器的式
            new CustomWeaponBackendPatch(),
            // 手动存档
            // new ManualArchiveBackendPatch(),
            // 太吾村管家
            new TaiwuBuildingManagerBackendPatch(),
            // 开局Roll属性
            new QuicklyCreateCharacterBackend(),
            // 平衡装备
            // new BetterArmorBackendPatch(),
            // 志向优化
            new ProfessionOptimizeBackend(),
            // 合并书页
            new MergeBookPanelBackendPatch(),
        };

        private static string _modIdStr = "1_";

        private static readonly List<BaseBackendPatch> extraPatchList = new()
        {
            // 模拟对战
            // new CombatSimulatorBackendPatch(),
            // 促织优化
            new CricketCombatOptimizeBackendPatch(),
            // 自动突破
            new AutoBreakBackendPatch(),
            // 同道战斗
            new TongdaoCombatBackendPatch(),

        };

        public override void Initialize()
        {
            _modIdStr = ModIdStr;
            AdaptableLog.Info("Initialize " + _modIdStr);

            harmony = Harmony.CreateAndPatchAll(typeof(ConvenienceBackend), null);

            allPatchList.ForEach((BaseBackendPatch patch) => this.harmony.PatchAll(patch.GetType()));
            allPatchList.ForEach((BaseBackendPatch patch) => patch.Initialize(harmony, ModIdStr));
            if (IsLocalTest())
            {
                extraPatchList.ForEach((BaseBackendPatch patch) => this.harmony.PatchAll(patch.GetType()));
                extraPatchList.ForEach((BaseBackendPatch patch) => patch.Initialize(harmony, ModIdStr));
            }
        }

        public override void OnModSettingUpdate()
        {
            AdaptableLog.Info("OnModSettingUpdate " + ModIdStr);

            DomainManager.Mod.GetSetting(ModIdStr, "Toggle_Total", ref bool_Toggle_Total);

            allPatchList.ForEach((BaseBackendPatch patch) => patch.OnModSettingUpdate(ModIdStr));

            if (IsLocalTest())
            {
                extraPatchList.ForEach((BaseBackendPatch patch) => patch.OnModSettingUpdate(ModIdStr));
            }
        }

        public override void Dispose()
        {
            AdaptableLog.Info("Dispose");

            allPatchList.ForEach((BaseBackendPatch patch) => patch.Dispose());

            if (IsLocalTest())
            {
                extraPatchList.ForEach((BaseBackendPatch patch) => patch.Dispose());
            }

            harmony?.UnpatchSelf();
            harmony = null;
        }

        // Token: 0x06000002 RID: 2 RVA: 0x000021B9 File Offset: 0x000003B9
        public override void OnEnterNewWorld()
        {
            AdaptableLog.Info("OnEnterNewWorld");

            allPatchList.ForEach((BaseBackendPatch patch) => patch.OnEnterNewWorld());

            if (IsLocalTest())
            {
                extraPatchList.ForEach((BaseBackendPatch patch) => patch.OnEnterNewWorld());
            }
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000021C8 File Offset: 0x000003C8
        public override void OnLoadedArchiveData()
        {
            AdaptableLog.Info("OnLoadedArchiveData");

            allPatchList.ForEach((BaseBackendPatch patch) => patch.OnLoadedArchiveData());

            if (IsLocalTest())
            {
                extraPatchList.ForEach((BaseBackendPatch patch) => patch.OnLoadedArchiveData());
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GlobalDomain), "CallMethod")]
        public static bool GlobalDomain_CallMethod_Prefix(TaiwuDomain __instance, Operation operation, RawDataPool argDataPool, DataContext context, ref int __result)
        {
            if (operation.MethodId == LOAD_CONFIG_METHOD_ID)
            {
                AdaptableLog.Info("CallMethod Load Config");
                string json = null;
                int num = operation.ArgsOffset;
                num += Serializer.Deserialize(argDataPool, num, ref json);
                Config = JsonConvert.DeserializeObject<Dictionary<string, System.Object>>(json);
                allPatchList.ForEach((BaseBackendPatch patch) => patch.OnConfigUpdate(Config));
                if (IsLocalTest())
                {
                    extraPatchList.ForEach((BaseBackendPatch patch) => patch.OnConfigUpdate(Config));
                }
                __result = -1;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 本地mod
        /// </summary>
        /// <returns></returns>
        public static bool IsLocalTest()
        {
            return _modIdStr.StartsWith("0_");
        }

        // Token: 0x04000001 RID: 1
        private Harmony harmony;

        // Token: 0x04000003 RID: 3
        public static bool bool_Toggle_Total;

        private const int LOAD_CONFIG_METHOD_ID = 1994;

        public static Dictionary<string, System.Object> Config = new Dictionary<string, object>();
    }

}
