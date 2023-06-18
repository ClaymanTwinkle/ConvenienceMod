using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvenienceFrontend.CombatStrategy.config;
using GameData.Domains.Item.Display;
using GameData.Domains.Mod;
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
            ConfigManager.Initialize(modInfo);
            CombatStrategyMod._modId = modInfo.ModId;
            harmony.PatchAll(typeof(CombatStrategyMod));
            harmony.PatchAll(typeof(UI_CombatPatch));
            UIUtils.PrepareMaterial();
            ConfigManager.ReadJsons();
        }

        // Token: 0x06000006 RID: 6 RVA: 0x0000217E File Offset: 0x0000037E
        public override void OnModSettingUpdate(string modIdStr)
        {
            ModManager.GetSetting(modIdStr, "ReplaceAI", ref ReplaceAI);
        }

        // Token: 0x0600000A RID: 10 RVA: 0x0000236C File Offset: 0x0000056C
        public static string GetStrategiesJson()
        {
            List<Strategy> list = Strategies.FindAll((Strategy strategy) => strategy.enabled && strategy.IsComplete());
            return JsonConvert.SerializeObject(list);
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

        // Token: 0x04000005 RID: 5
        public static Settings Settings => ConfigManager.Settings;

        // Token: 0x04000006 RID: 6
        public static List<Strategy> Strategies => ConfigManager.Strategies;

        public static bool ReplaceAI;

        // Token: 0x04000009 RID: 9
        public const ushort MethodId = 1957;

        // Token: 0x0400000A RID: 10
        public static ModId _modId;
    }
}
