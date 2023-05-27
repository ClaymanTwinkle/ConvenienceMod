using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GameData.Domains.Mod;
using Newtonsoft.Json;
using ConvenienceFrontend.CombatStrategy.config.data;

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

        private static JsonSerializerSettings _backendJsonSerializerSettings;

        public static Settings Settings;

        private static int _originSelectStrategyIndex = 0;

        public static List<StrategyProgramme> Programmes;

        // Token: 0x04000006 RID: 6
        public static List<Strategy> Strategies
        {
            get
            {
                return Programmes[Settings.SelectStrategyIndex].strategies;
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
                Settings = new Settings();
                _settingsJson = null;
            }
            else
            {
                _settingsJson = File.ReadAllText(_settingFile);
                Settings = JsonConvert.DeserializeObject<Settings>(_settingsJson);
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

            if (Settings.SelectStrategyIndex >= Programmes.Count || Settings.SelectStrategyIndex < 0)
            {
                Settings.SelectStrategyIndex = 0;
            }
            _originSelectStrategyIndex = Settings.SelectStrategyIndex;
        }

        public static ValueTuple<bool, bool> SaveJsons()
        {
            bool settingsChanged = false;
            bool strategiesChanged = false;
            if (_settingFile != string.Empty)
            {
                string text = JsonConvert.SerializeObject(Settings);
                if (!text.Equals(_settingsJson))
                {
                    _settingsJson = text;
                    File.WriteAllText(_settingFile, text);
                    settingsChanged = true;
                }
            }

            if (_originSelectStrategyIndex != Settings.SelectStrategyIndex)
            {
                _originSelectStrategyIndex = Settings.SelectStrategyIndex;
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

        public static String GetSettingsJson()
        {
            return _settingsJson;
        }

        public static string GetStrategiesJson()
        {
            List<Strategy> list = Strategies.FindAll((Strategy strategy) => strategy.enabled && strategy.IsComplete());
            return JsonConvert.SerializeObject(list);
        }

        public static string GetBackendSettingsJson()
        {
            if (_backendJsonSerializerSettings == null)
            {
                _backendJsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = BaseTypeContractResolver.Instance
                };
            }
            return JsonConvert.SerializeObject(Settings, _backendJsonSerializerSettings);
        }
    }
}
