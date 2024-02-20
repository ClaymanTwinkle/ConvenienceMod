using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CombatStrategy.config;
using GameData.Domains.Item.Display;
using GameData.Domains.Mod;
using GameData.GameDataBridge;
using HarmonyLib;
using Newtonsoft.Json;
using TaiwuModdingLib.Core.Plugin;
using TMPro;
using UnityEngine;

namespace ConvenienceFrontend.CombatStrategy
{
    internal class CombatStrategyMod : BaseFrontPatch
    {
        // Token: 0x06000005 RID: 5 RVA: 0x000020D4 File Offset: 0x000002D4
        public override void Initialize(Harmony harmony, string ModIdStr)
        {
            ModManager.GetSetting(ModIdStr, "ReplaceAI", ref ReplaceAI);

            ModInfo modInfo = ModManager.GetModInfo(ModManager.EnabledMods.Find((ModId mod) => mod.ToString() == ModIdStr));
            CombatStrategyConfigManager.Initialize(modInfo);
            CombatStrategyMod._modId = modInfo.ModId;
            harmony.PatchAll(typeof(CombatStrategyMod));
            harmony.PatchAll(typeof(UI_CombatPatch));
            UIUtils.PrepareMaterial();
            CombatStrategyConfigManager.ReadJsons();
        }

        // Token: 0x06000006 RID: 6 RVA: 0x0000217E File Offset: 0x0000037E
        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "ReplaceAI", ref ReplaceAI);
        }

        /// <summary>
        /// 设置中打开战斗策略设置面板
        /// </summary>
        /// <param name="modInfo"></param>
        /// <param name="____settingEntriesList"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_ModPanel), "UpdateSettings")]
        public static void UI_ModPanel_UpdateSettings_Postfix(ModInfo modInfo, List<Refers> ____settingEntriesList)
        {
            if (modInfo == null) return;
            
            if (!modInfo.ModId.Equals(_modId)) return;
            if (____settingEntriesList.Count == 0) return;

            var settingRefers =  ____settingEntriesList.Find((Refers entry) => {
                var text = entry.CGet<TextMeshProUGUI>("Label").text;
                return text!= null && text.Contains("战斗策略设置");
            });

            if (settingRefers == null) return;

            CToggle ctoggle = settingRefers.CGet<CToggle>("Toggle");
            ctoggle.onValueChanged.RemoveAllListeners();
            ctoggle.isOn = false;
            ctoggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                if (isOn)
                {
                    UIManager.Instance.ShowUI(UI_CombatStrategySetting.GetUI());
                    UIElement ui = UI_CombatStrategySetting.GetUI();
                    Action onHide;
                    onHide = (delegate ()
                    {
                        ctoggle.isOn = false;
                    });
                    ui.OnHide = onHide;
                }
            });
        }

        public static void SendSettings()
        {
            GameDataBridge.AddMethodCall<ushort, string>(-1, 8, GameDataBridgeConst.MethodId, GameDataBridgeConst.Flag.Flag_UpdateSettingsJson, CombatStrategyConfigManager.GetBackendSettingsJson());
        }

        // Token: 0x04000005 RID: 5
        public static BackendSettings ProgrammeSettingsSettings => CombatStrategyConfigManager.ProgrammeSettings;

        public static config.data.GlobalSettings GlobalSettings => CombatStrategyConfigManager.GlobalSettings;

        // Token: 0x04000006 RID: 6
        public static List<Strategy> Strategies => CombatStrategyConfigManager.Strategies;

        public static bool ReplaceAI;

        // Token: 0x04000009 RID: 9
        public const ushort MethodId = 1957;

        // Token: 0x0400000A RID: 10
        public static ModId _modId;
    }
}
