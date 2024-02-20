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
        private const string ROOT_DIR = "ConvenienceModConfig";

        private static string GetRootDirectory()
        {
            return ModManager.GetModInfo(ConvenienceFrontend._modIdStr).DirectoryName;
        }

        private static string GetNewRootDirectory()
        {
            string archiveDir = Game.GetArchiveDirPath(false);
            if (!Directory.Exists(archiveDir))
            {
                Directory.CreateDirectory(archiveDir);
            }

            string newRootDir = Path.Combine(archiveDir, ROOT_DIR);
            if (!Directory.Exists(newRootDir))
            {
                Directory.CreateDirectory(newRootDir);
            }
            return newRootDir;
        }

        public static void SaveConfig<T>(string filename, T config)
        {
            JsonFileUtils.WriteFile(Path.Combine(GetNewRootDirectory(), filename), config);
        }

        public static T LoadConfig<T>(string filename) 
        {
            var newFilePath = Path.Combine(GetNewRootDirectory(), filename);
            if (File.Exists(newFilePath)) 
            {
                return JsonFileUtils.ReadFile<T>(newFilePath);
            }
            return JsonFileUtils.ReadFile<T>(Path.Combine(GetRootDirectory(), filename));
        }
    }
}
