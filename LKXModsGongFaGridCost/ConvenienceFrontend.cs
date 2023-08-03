using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ConvenienceFrontend.CombatSimulator;
using ConvenienceFrontend.CombatStrategy;
using ConvenienceFrontend.CustomSteal;
using ConvenienceFrontend.CustomWeapon;
using ConvenienceFrontend.IgnoreReadFinishBook;
using ConvenienceFrontend.ManualArchive;
using ConvenienceFrontend.ModifyCombatSkill;
using ConvenienceFrontend.QuicklyCreateCharacter;
using ConvenienceFrontend.TaiwuBuildingManager;
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
        private static string _config_file_path = CONFIG_FILE_NAME;

        // Token: 0x04000001 RID: 1
        private Harmony harmony;

        // Token: 0x04000002 RID: 2
        private static bool bool_Toggle_Total;

        public static Dictionary<string, System.Object> Config = new Dictionary<string, object>();

        private static string _modIdStr = "1_";

        private readonly List<BaseFrontPatch> allPatchList = new List<BaseFrontPatch>()
        {
            // 较艺必胜
            // new ComparativeArtFrontPatch(),
            // 自动战斗
            new CombatStrategyMod(),
            // 自定义偷窃
            new CustomStealFrontPatch(),
            // 修改武器的式
            new CustomWeaponFrontPatch(),
            // 手动存档
            // new ManualArchiveFrontendPatch(),
            // 修改功法
            new ModifyCombatSkillFrontPatch(),
            // roll角色属性
            new QuicklyCreateCharacterFrontend(),
            // 太吾村管家
            new TaiwuBuildingManagerFrontPatch(),
            // 隐藏已读书籍
            new IgnoreReadFinishBookFrontPatch(),
            // 平衡装备
            // new BetterArmorFrontPatch(),
        };

        private static readonly List<BaseFrontPatch> extraPatchList = new List<BaseFrontPatch>()
        {
            // 模拟对战
            new CombatSimulatorFrontPatch(),
        };

        public override void OnModSettingUpdate()
        {
            Debug.Log("OnModSettingUpdate " + _modIdStr);

            ModManager.GetSetting(base.ModIdStr, "Toggle_Total", ref ConvenienceFrontend.bool_Toggle_Total);

            allPatchList.ForEach((BaseFrontPatch patch) => patch.OnModSettingUpdate(base.ModIdStr));
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

            SendLoadSettings();
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002088 File Offset: 0x00000288
        public override void Dispose()
        {
            allPatchList.ForEach((BaseFrontPatch patch) => patch.Dispose());

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
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000020F0 File Offset: 0x000002F0
        public override void OnLoadedArchiveData()
        {
            allPatchList.ForEach((BaseFrontPatch patch) => patch.OnLoadedArchiveData());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIManager), "ShowUI")]
        public static void UIManager_ShowUI_Postfix(UIElement elem)
        {
            Debug.Log("ShowUI: " + elem.Name);
        }

        private void InitConfig()
        {
            string directoryName = ModManager.GetModInfo(base.ModIdStr).DirectoryName;
            _config_file_path = Path.Combine(directoryName, CONFIG_FILE_NAME);

            LoadConfig();
        }

        public static void SaveConfig(bool send = true)
        {
            JsonFileUtils.WriteFile(_config_file_path, Config);
            if (send) 
            {
                SendLoadSettings();
            }
        }

        public static void LoadConfig()
        {
            Config = JsonFileUtils.ReadFile<Dictionary<string, System.Object>>(_config_file_path);
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
