using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamImageSorter
{
    public class AppSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Directory { get; set; } = string.Empty;
    }


    public class JsonConfigManager
    {
        private readonly string _filePath;

        public JsonConfigManager(string filePath)
        {
            _filePath = filePath;
        }

        public AppSettings ReadConfig()
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings(); // Return default settings if the file doesn't exist
            }

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public void WriteConfig(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}