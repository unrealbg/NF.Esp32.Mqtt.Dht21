using System.Collections;
using System.IO;

namespace ESP32_NF_MQTT_DHT.Configs
{
    public static class ConfigManager
    {
        private static Hashtable _settings;

        static ConfigManager()
        {
            _settings = new Hashtable();
            LoadConfig();
        }

        private static void LoadConfig()
        {
            try
            {
                string filePath = "config.txt";

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            _settings[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            
        }

        public static string GetSetting(string key)
        {
            return _settings[key] as string;
        }
    }
}
