using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace ConvenienceFrontend.Utils
{
    internal class JsonFileUtils
    {

        public static T ReadFile<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default;
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void WriteFile<T>(string path, T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            File.WriteAllText(path, json);
        }
    }
}
