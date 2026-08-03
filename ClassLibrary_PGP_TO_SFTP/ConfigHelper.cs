using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassLibrary_PGP_TO_SFTP
{
    public class ConfigHelper
    {
        private static JObject _config;

        private static string GetConfigFilePath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(basePath, "appsettings.json");

            // Busca el archivo en el directorio actual de ejecución
            //return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            return path;
            //return Path.Combine(Directory.GetParent(basePath).Parent.Parent.FullName, "appsettings.json");
        }

        private static JObject LoadConfig()
        {
            if (_config != null)
                return _config;

            string path = GetConfigFilePath();

            if (!File.Exists(path))
                throw new FileNotFoundException("No se encontró el archivo appsettings.json en: " + path);

            string json = File.ReadAllText(path);
            _config = JObject.Parse(json);
            return _config;
        }

        public static T GetSection<T>(string sectionName)
        {
            var config = LoadConfig();
            var section = config[sectionName];
            if (section == null)
                throw new Exception($"La sección '{sectionName}' no existe en appsettings.json.");

            return section.ToObject<T>();
        }

        //public static void UpdateSection<T>(string sectionName, T newValue)
        //{
        //    var config = LoadConfig();
        //    config[sectionName] = JObject.FromObject(newValue);

        //    string path = GetConfigFilePath();
        //    File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));

        //    _config = null; // refrescar
        //}
    }
}
