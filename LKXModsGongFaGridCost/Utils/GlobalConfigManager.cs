using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.Utils
{
    internal class GlobalConfigManager
    {
        private static string GetRootDirectory()
        {
            return ModManager.GetModInfo(ConvenienceFrontend._modIdStr).DirectoryName;
        }

        public static void SaveConfig<T>(string filename, T config)
        {
            JsonFileUtils.WriteFile(Path.Combine(GetRootDirectory(), filename), config);
        }

        public static T LoadConfig<T>(string filename) 
        {
            return JsonFileUtils.ReadFile<T>(Path.Combine(GetRootDirectory(), filename));
        }
    }
}
