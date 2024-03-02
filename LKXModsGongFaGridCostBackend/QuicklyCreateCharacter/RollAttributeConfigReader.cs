using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace ConvenienceBackend.QuicklyCreateCharacter
{
    public class RollAttributeConfigReader
    {
        public static bool EnableAllBlueFeature(Dictionary<string, System.Object> _config)
        {
            return _config.GetTypedValue<bool>("Toggle_FilterEnableAllBlueFeature");
        }

        public static string GetCombatSkillBookName(Dictionary<string, System.Object> _config)
        {
            return (_config.GetTypedValue<string>("InputField_FilterCombatSkillBookName") ?? "").Trim();
        }

        public static JArray GetLifeSkillBookTypes(Dictionary<string, System.Object> _config)
        {
            return (JArray)_config.GetTypedValue<JArray>("ToggleGroup_FilterLifeSkillBookName") ?? new JArray();
        }
    }
}
