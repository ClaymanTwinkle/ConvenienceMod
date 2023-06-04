using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ConvenienceFrontend.CombatStrategy;
using ConvenienceFrontend.CustomSteal;
using ConvenienceFrontend.CustomWeapon;
using ConvenienceFrontend.IgnoreReadFinishBook;
using ConvenienceFrontend.ManualArchive;
using ConvenienceFrontend.ModifyCombatSkill;
using ConvenienceFrontend.RollCreateRole;
using ConvenienceFrontend.TaiwuBuildingManager;
using ConvenienceFrontend.Utils;
using GameData.Domains.Mod;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine;

namespace ConvenienceFrontend
{
    [PluginConfig("ConvenienceFrontend", "kesar", "1.0.0")]
    public class ConvenienceFrontend : TaiwuRemakePlugin
    {
        private const string CONFIG_FILE_NAME = "ModConfig.json";
        private static string _config_file_path = CONFIG_FILE_NAME;

        // Token: 0x04000001 RID: 1
        private Harmony harmony;

        // Token: 0x04000002 RID: 2
        private static bool bool_Toggle_Total;

        public static Dictionary<string, System.Object> Config = new Dictionary<string, object>();

        private List<BaseFrontPatch> allPatch = new List<BaseFrontPatch>()
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
            // 修改功法
            new ModifyCombatSkillFrontPatch(),
            // roll角色属性
            // new RollCreateRoleFrontPatch(),
            // 太吾村管家
            new TaiwuBuildingManagerFrontPatch(),
            // 隐藏已读书籍
            new IgnoreReadFinishBookFrontPatch()
        };

        public override void OnModSettingUpdate()
        {
            ModManager.GetSetting(base.ModIdStr, "Toggle_Total", ref ConvenienceFrontend.bool_Toggle_Total);

            allPatch.ForEach((BaseFrontPatch patch) => patch.OnModSettingUpdate(base.ModIdStr));
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002069 File Offset: 0x00000269
        public override void Initialize()
        {
            InitConfig();

            this.harmony = Harmony.CreateAndPatchAll(typeof(ConvenienceFrontend), null);

            allPatch.ForEach((BaseFrontPatch patch) => this.harmony.PatchAll(patch.GetType()));
            allPatch.ForEach((BaseFrontPatch patch) => patch.Initialize(harmony, base.ModIdStr));
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002088 File Offset: 0x00000288
        public override void Dispose()
        {
            allPatch.ForEach((BaseFrontPatch patch) => patch.Dispose());

            if (this.harmony != null)
            {
                this.harmony.UnpatchSelf();
            }

            this.harmony = null;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000020F0 File Offset: 0x000002F0
        public override void OnLoadedArchiveData()
        {
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

        public static void SaveConfig()
        {
            JsonFileUtils.WriteFile(_config_file_path, Config);
        }

        public static void LoadConfig()
        {
            Config = JsonFileUtils.ReadFile<Dictionary<string, System.Object>>(_config_file_path);
            if (Config == null)
            { 
                Config = new Dictionary<string, System.Object>();
            }
        }
    }
}
