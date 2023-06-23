using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GameData.Domains.Mod;
using Newtonsoft.Json;
using ConvenienceFrontend.CombatStrategy.config.data;
using FrameWork.ModSystem;
using static Spine.Unity.MeshGenerator;
using UnityEngine;

namespace ConvenienceFrontend.CombatStrategy.config
{
    public class ConfigManager
    {
        // Token: 0x0400000B RID: 11
        private static string _settingFile;

        // Token: 0x0400000C RID: 12
        private static string _strategyFile;

        private static string _programmesFile;

        // Token: 0x0400000D RID: 13
        // private static string _strategyJson;
        private static string _programmesJson;

        // Token: 0x0400000E RID: 14
        private static string _settingsJson;

        public static Settings GlobalSettings;

        private static int _originSelectStrategyIndex = 0;

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
            string directoryName = modInfo.DirectoryName;
            _settingFile = Path.Combine(directoryName, "Settings.json");
            _strategyFile = Path.Combine(directoryName, "Strategy.json");
            _programmesFile = Path.Combine(directoryName, "Programmes.json");
        }

        public static void ReadJsons()
        {
            if (_settingFile == string.Empty || !File.Exists(_settingFile))
            {
                GlobalSettings = new Settings();
                _settingsJson = null;
            }
            else
            {
                _settingsJson = File.ReadAllText(_settingFile);
                GlobalSettings = JsonConvert.DeserializeObject<Settings>(_settingsJson);
            }

            if (_programmesFile == string.Empty || !File.Exists(_programmesFile))
            {
                var programmes = new StrategyProgramme();
                programmes.name = "默认方案";
                // 没有创建过文件
                if (_strategyFile != string.Empty && File.Exists(_strategyFile))
                {
                    // 旧文件兼容
                    var strategyJson = File.ReadAllText(_strategyFile);
                    programmes.strategies = JsonConvert.DeserializeObject<List<Strategy>>(strategyJson);
                }
                Programmes = new List<StrategyProgramme> { programmes };
            }
            else
            {
                _programmesJson = File.ReadAllText(_programmesFile);
                Programmes = JsonConvert.DeserializeObject<List<StrategyProgramme>>(_programmesJson);
            }

            // 兼容Settings
            Programmes.ForEach(programmes => {
                if (programmes.settings == null)
                {
                    programmes.settings = (BackendSettings)GlobalSettings.CreateDeepCopy();
                }
                else
                {
                    programmes.settings.isEnable = GlobalSettings.isEnable;
                }
            });

            if (GlobalSettings.SelectStrategyIndex >= Programmes.Count || GlobalSettings.SelectStrategyIndex < 0)
            {
                ChangeStrategyProgramme(0);
            }
            _originSelectStrategyIndex = GlobalSettings.SelectStrategyIndex;
        }

        public static ValueTuple<bool, bool> SaveJsons()
        {
            bool settingsChanged = false;
            bool strategiesChanged = false;
            if (_settingFile != string.Empty)
            {
                string text = JsonConvert.SerializeObject(GlobalSettings);
                if (!text.Equals(_settingsJson))
                {
                    _settingsJson = text;
                    File.WriteAllText(_settingFile, text);
                    settingsChanged = true;
                }
            }

            if (_originSelectStrategyIndex != GlobalSettings.SelectStrategyIndex)
            {
                _originSelectStrategyIndex = GlobalSettings.SelectStrategyIndex;
                strategiesChanged = true;
            }

            if (_programmesFile != string.Empty)
            {
                string text2 = JsonConvert.SerializeObject(Programmes);
                if (!text2.Equals(_programmesJson))
                {
                    _programmesJson = text2;

                    File.WriteAllText(_programmesFile, text2);
                    strategiesChanged = true;
                }
            }
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
            var copyStrategy = (StrategyProgramme)ConfigManager.CurrentStrategyProgramme.CreateDeepCopy();
            ConfigManager.Programmes.Add(copyStrategy);
            return copyStrategy;
        }

        public static StrategyProgramme CreateNewStrategyProgramme(string name)
        {
            var programme = new StrategyProgramme
            {
                name = name,
                settings = (BackendSettings)CurrentStrategyProgramme.settings.CreateDeepCopy(),
            };
            ConfigManager.Programmes.Add(programme);

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
                    ConfigManager.Programmes.Add(programme);
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
