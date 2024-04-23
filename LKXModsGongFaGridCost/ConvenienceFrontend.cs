using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ConvenienceFrontend.AutoBreak;
using ConvenienceFrontend.Bandit;
using ConvenienceFrontend.BatchMakeItem;
using ConvenienceFrontend.BetterReading;
using ConvenienceFrontend.CombatSimulator;
using ConvenienceFrontend.CombatStrategy;
using ConvenienceFrontend.CricketCombatOptimize;
using ConvenienceFrontend.CustomSteal;
using ConvenienceFrontend.CustomWeapon;
using ConvenienceFrontend.ExchangeBook;
using ConvenienceFrontend.IgnoreReadFinishBook;
using ConvenienceFrontend.InscribeChar;
using ConvenienceFrontend.ManualArchive;
using ConvenienceFrontend.MergeBookPanel;
using ConvenienceFrontend.QuicklyCreateCharacter;
using ConvenienceFrontend.ShopShortcut;
using ConvenienceFrontend.TaiwuBuildingManager;
using ConvenienceFrontend.TongdaoComabt;
using ConvenienceFrontend.TongdaoEquipCombatSkill;
using ConvenienceFrontend.Utils;
using GameData.Domains.Mod;
using GameData.GameDataBridge;
using GameData.Utilities;
using HarmonyLib;
using Newtonsoft.Json;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine;

namespace ConvenienceFrontend
{
    [PluginConfig("ConvenienceFrontend", "kesar", "1.0.0")]
    public class ConvenienceFrontend : TaiwuRemakePlugin
    {
        private const string CONFIG_FILE_NAME = "ModConfig.json";

        private const int LOAD_CONFIG_METHOD_ID = 1994;

        // Token: 0x04000001 RID: 1
        private Harmony harmony;

        // Token: 0x04000002 RID: 2
        private static bool bool_Toggle_Total;

        public static Dictionary<string, System.Object> Config = new Dictionary<string, object>();

        public static string _modIdStr = "1_";

        private readonly List<BaseFrontPatch> allPatchList = new List<BaseFrontPatch>()
        {
            // 较艺必胜
            new ComparativeArtFrontPatch(),
            // 自动战斗
            new CombatStrategyMod(),
            // 自定义偷窃
            new CustomStealFrontPatch(),
            // 修改武器的式
            new CustomWeaponFrontPatch(),
            // 手动存档
            // new ManualArchiveFrontendPatch(),
            // roll角色属性
            new QuicklyCreateCharacterFrontend(),
            // 太吾村管家
            new TaiwuBuildingManagerFrontPatch(),
            // 隐藏已读书籍
            new IgnoreReadFinishBookFrontPatch(),
            // 平衡装备
            // new BetterArmorFrontPatch(),
            // 合并书页
            new MergeBookPanelFrontPatch(),
            // 批量制作物品
            new BatchMakeItemFrontPatch(),
        };

        private static readonly List<BaseFrontPatch> extraPatchList = new List<BaseFrontPatch>()
        {
            // 模拟对战
            // new CombatSimulatorFrontPatch(),
            // 蛐蛐战斗优化
            new CricketCombatOptimizeFrontPatch(),
            // 一键突破
            new AutoBreakFrontPatch(),
            // 铭刻自己
            new InscribeCharFrontPatch(),
            // 土匪
            new BanditFrontPatch(),
            // 商店快捷操作
            new ShopShortcutFrontPatch(),
            // 同道战斗
            new TongdaoFrontPatch(),
            // 更好的读书
            new BetterReadingFrontPatch(),
            // 重置分辨率
            new ResetResolutionFrontPatch(),
            // 门派换书
            new ExchangeBookFrontPatch(),
            // 同道换功法
            new TongDaoEquipCombatSkillFrontPatch(),
        };

        public override void OnModSettingUpdate()
        {
            Debug.Log("OnModSettingUpdate " + _modIdStr);

            ModManager.GetSetting(base.ModIdStr, "Toggle_Total", ref ConvenienceFrontend.bool_Toggle_Total);

            allPatchList.ForEach((BaseFrontPatch patch) => patch.OnModSettingUpdate(base.ModIdStr));
            if (IsLocalTest())
            {
                extraPatchList.ForEach((BaseFrontPatch patch) => patch.OnModSettingUpdate(base.ModIdStr));
            }
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002069 File Offset: 0x00000269
        public override void Initialize()
        {
            _modIdStr = ModIdStr;
            AdaptableLog.Info("Initialize " + _modIdStr);

            InitConfig();

            this.harmony = Harmony.CreateAndPatchAll(typeof(ConvenienceFrontend), null);

            allPatchList.ForEach((BaseFrontPatch patch) => this.harmony.PatchAll(patch.GetType()));
            allPatchList.ForEach((BaseFrontPatch patch) => patch.Initialize(harmony, base.ModIdStr));

            if (IsLocalTest())
            {
                extraPatchList.ForEach((BaseFrontPatch patch) => this.harmony.PatchAll(patch.GetType()));
                extraPatchList.ForEach((BaseFrontPatch patch) => patch.Initialize(harmony, base.ModIdStr));
            }

            SendLoadSettings();
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002088 File Offset: 0x00000288
        public override void Dispose()
        {
            allPatchList.ForEach((BaseFrontPatch patch) => patch.Dispose());
            if (IsLocalTest()) 
            {
                extraPatchList.ForEach((BaseFrontPatch patch) => patch.Dispose());
            }

            if (this.harmony != null)
            {
                this.harmony.UnpatchSelf();
            }

            this.harmony = null;
        }

        public override void OnEnterNewWorld()
        {
            base.OnEnterNewWorld();
            allPatchList.ForEach((BaseFrontPatch patch) => patch.OnEnterNewWorld());
            if (IsLocalTest())
                extraPatchList.ForEach((BaseFrontPatch patch) => patch.OnEnterNewWorld());
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000020F0 File Offset: 0x000002F0
        public override void OnLoadedArchiveData()
        {
            allPatchList.ForEach((BaseFrontPatch patch) => patch.OnLoadedArchiveData());
            if (IsLocalTest())
                extraPatchList.ForEach((BaseFrontPatch patch) => patch.OnLoadedArchiveData());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIManager), "ShowUI")]
        public static void UIManager_ShowUI_Postfix(UIElement elem)
        {
            Debug.Log("ShowUI: " + elem.Name);
        }

        private void InitConfig()
        {
            LoadConfig();
        }

        public static void SaveConfig(bool send = true)
        {
            GlobalConfigManager.SaveConfig(CONFIG_FILE_NAME, Config);
            if (send) 
            {
                SendLoadSettings();
            }
        }

        public static void LoadConfig()
        {
            Config = GlobalConfigManager.LoadConfig<Dictionary<string, System.Object>>(CONFIG_FILE_NAME);
            if (Config == null)
            { 
                Config = new Dictionary<string, System.Object>();
            }
        }

        public static void SendLoadSettings()
        {
            GameDataBridge.AddMethodCall<string>(-1, 0, LOAD_CONFIG_METHOD_ID, JsonConvert.SerializeObject(ConvenienceFrontend.Config));
        }

        /// <summary>
        /// 本地mod
        /// </summary>
        /// <returns></returns>
        public static bool IsLocalTest()
        {
            return _modIdStr.StartsWith("0_");
        }

        /// <summary>
        /// 判断当前是否是测试版本
        /// </summary>
        /// <returns></returns>
        public static bool IsTestVersion()
        {
            return Game.Instance.GameVersion.Contains("test");
        }

        /// <summary>
        /// 判断当前是否进入了游戏中
        /// </summary>
        /// <returns></returns>
        public static bool IsInGame()
        {
            return Game.Instance.GetCurrentGameStateName() == EGameState.InGame;
        }
    }
}
