using System;
using System.Collections.Generic;
using GameData.Domains.Mod;
using Newtonsoft.Json;
using ConvenienceFrontend.CombatStrategy.config.data;
using FrameWork.ModSystem;
using UnityEngine;
using GameData.Utilities;
using ConvenienceFrontend.Utils;

namespace ConvenienceFrontend.CombatStrategy.config
{
    public class CombatStrategyConfigManager
    {
        private const string SETTING_FILE_NAME = "GlobalSettings.json";
        private const string PROGRAMMES_FILE_NAME = "Programmes.json";

        public static data.GlobalSettings GlobalSettings;

        public static List<StrategyProgramme> Programmes;

        public static StrategyProgramme CurrentStrategyProgramme
        {
            get
            {
                return Programmes[GlobalSettings.SelectStrategyIndex];
            }
        }

        public static BackendSettings ProgrammeSettings
        {
            get
            {
                return CurrentStrategyProgramme.settings;
            }
        }

        public static List<Strategy> Strategies
        {
            get
            {
                return CurrentStrategyProgramme.strategies;
            }
        }

        public static void Initialize(ModInfo modInfo)
        {
        }

        public static void ReadJsons()
        {
            GlobalSettings = GlobalConfigManager.LoadConfig<data.GlobalSettings>(SETTING_FILE_NAME) ?? new data.GlobalSettings();

            Programmes = GlobalConfigManager.LoadConfig<List<StrategyProgramme>>(PROGRAMMES_FILE_NAME) ?? new List<StrategyProgramme> 
            { 
                new StrategyProgramme
                {
                    name = "默认方案"
                }
            };

            // 兼容Settings
            Programmes.ForEach(programmes => {
                if (programmes.settings == null)
                {
                    programmes.settings = new BackendSettings();
                }
                programmes.settings.isEnable = GlobalSettings.isEnable;
            });

            if (GlobalSettings.SelectStrategyIndex >= Programmes.Count || GlobalSettings.SelectStrategyIndex < 0)
            {
                ChangeStrategyProgramme(0);
            }
        }

        public static ValueTuple<bool, bool> SaveJsons()
        {
            bool settingsChanged = true;
            bool strategiesChanged = true;

            GlobalConfigManager.SaveConfig(SETTING_FILE_NAME, GlobalSettings);
            GlobalConfigManager.SaveConfig(PROGRAMMES_FILE_NAME, Programmes);
            return new ValueTuple<bool, bool>(settingsChanged, strategiesChanged);
        }

        public static string GetEnableStrategiesJson()
        {
            List<Strategy> list = Strategies.FindAll((Strategy strategy) => strategy.enabled && strategy.IsComplete());
            return JsonConvert.SerializeObject(list);
        }

        public static string GetAllStrategiesJson()
        {
            return JsonConvert.SerializeObject(Strategies);
        }

        public static string GetBackendSettingsJson()
        {
            ProgrammeSettings.isEnable = GlobalSettings.isEnable;
            
            var json = JsonConvert.SerializeObject(ProgrammeSettings);
            return json;
        }

        public static string GetCurrentStrategyProgrammeJson()
        { 
            return JsonConvert.SerializeObject(CurrentStrategyProgramme);
        }

        public static void ChangeStrategyProgramme(int index)
        { 
            GlobalSettings.SelectStrategyIndex = index;
        }

        public static StrategyProgramme CopyStrategyProgramme()
        {
            var copyStrategy = (StrategyProgramme)CombatStrategyConfigManager.CurrentStrategyProgramme.CreateDeepCopy();
            CombatStrategyConfigManager.Programmes.Add(copyStrategy);
            return copyStrategy;
        }

        public static StrategyProgramme CreateNewStrategyProgramme(string name)
        {
            var programme = new StrategyProgramme
            {
                name = name,
                settings = (BackendSettings)CurrentStrategyProgramme.settings.CreateDeepCopy(),
            };
            CombatStrategyConfigManager.Programmes.Add(programme);

            return programme;
        }

        public static StrategyProgramme CreateNewStrategyProgrammeFromClipboard()
        {
            try
            {
                var programme = JsonConvert.DeserializeObject<StrategyProgramme>(GUIUtility.systemCopyBuffer);
                if (programme != null)
                {
                    programme.name += "(导入)";
                    CombatStrategyConfigManager.Programmes.Add(programme);
                }
                return programme;
            }
            catch 
            { 
            
            }

            return null;

        }
    }
}
