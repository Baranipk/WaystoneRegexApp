using System.Text.Json;

namespace WaystoneRegex
{
    /// <summary>
    /// Basit yerelleştirme: varsayılan İngilizce, Türkçe seçenek.
    /// Oyun/regex terimleri (stat adları, modifier adları, CORRUPT vb.) her dilde orijinal kalır.
    /// </summary>
    public static class Loc
    {
        public static bool Turkish;
        public static string T(string en, string tr) => Turkish ? tr : en;
    }

    /// <summary>Uygulama ayarları — %AppData%\WaystoneRegex\settings.json</summary>
    public class AppSettings
    {
        public bool Dark { get; set; } = true; // eski sürüm uyumu; artık ThemeName kullanılır
        public string ThemeName { get; set; } = "WAYSTONE";
        public string Font { get; set; } = "Bahnschrift";
        public bool Turkish { get; set; } = false;
        public List<string> Tags { get; set; } = new() { "SIMULACRUM", "BOSS RUSH", "JUICE" };

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WaystoneRegex", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new();
            }
            catch { }
            return new();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
