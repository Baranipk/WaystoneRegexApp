using System.Text.Json;

namespace WaystoneRegex
{
    /// <summary>Tek bir stat satırının ayarı.</summary>
    public class StatSetting
    {
        public bool Enabled { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }   // 0 = üst sınır yok
        public bool Exclude { get; set; }
    }

    /// <summary>Kaydedilmiş bir regex'in tüm ayarları.</summary>
    public class SavedRegex
    {
        public string Name { get; set; } = "";
        public List<StatSetting> Stats { get; set; } = new();      // WaystoneStats.Items sırasıyla
        public StatSetting Effectiveness { get; set; } = new();
        public List<string> OnlyMods { get; set; } = new();        // gösterilecek modifier kalıpları
        public List<string> HiddenMods { get; set; } = new();      // gizlenecek modifier kalıpları (!)
        public int CorruptMode { get; set; }                       // 0 kapalı, 1 açık, 2 gizle
        public List<string> Tags { get; set; } = new();

        /// <summary>Erişilebilirlik (%0-20): tüm minimum eşikleri bu oranda düşürür → daha çok eşleşme.</summary>
        public int Relax { get; set; }

        public string Query { get; set; } = "";

        /// <summary>Stat sütununun kısa gösterimi: "—" pasif, "!" hariç, "30+" min, "20-45" aralık.</summary>
        public string StatBadge(int index)
        {
            var s = index < Stats.Count ? Stats[index] : null;
            return Badge(s);
        }

        public string EffBadge() => Badge(Effectiveness);

        private static string Badge(StatSetting? s)
        {
            if (s is null || !s.Enabled) return "—";
            if (s.Exclude) return "!";
            return s.Max > 0 ? $"{s.Min}-{s.Max}" : $"{s.Min}+";
        }

        /// <summary>Sıralama için sayısal değer: pasif -1, hariç -2, aktif Min.</summary>
        public int StatSortValue(int index)
        {
            var s = index < Stats.Count ? Stats[index] : null;
            return SortValue(s);
        }

        public int EffSortValue() => SortValue(Effectiveness);

        private static int SortValue(StatSetting? s)
            => s is null || !s.Enabled ? -1 : (s.Exclude ? -2 : s.Min);

        /// <summary>Relax oranı uygulanmış minimum: %20 → 30 yerine 24.</summary>
        private int RelaxedMin(int min)
            => Relax <= 0 ? min : Math.Max(0, (int)Math.Round(min * (100 - Math.Min(Relax, 20)) / 100.0));

        /// <summary>Ayarlardan sorgu metnini yeniden üretir (Relax dahil).</summary>
        public void RebuildQuery()
        {
            var terms = new List<string>();
            for (int i = 0; i < Stats.Count && i < WaystoneStats.Items.Length; i++)
            {
                var s = Stats[i];
                if (!s.Enabled) continue;
                var prefix = WaystoneStats.Items[i].Prefix;
                terms.Add(s.Exclude ? RegexBuilder.NegTerm(prefix) : RegexBuilder.StatTerm(prefix, RelaxedMin(s.Min), s.Max));
            }
            if (Effectiveness.Enabled)
                terms.Add(Effectiveness.Exclude ? "!effectiveness" : RegexBuilder.StatTerm("effectiveness: ", RelaxedMin(Effectiveness.Min), Effectiveness.Max));
            foreach (var p in OnlyMods)
                terms.Add($"\"{p}\"");
            foreach (var p in HiddenMods)
                terms.Add(RegexBuilder.NegTerm(p));
            if (CorruptMode == 1) terms.Add("corrupted");
            else if (CorruptMode == 2) terms.Add("!corrupted");
            Query = string.Join(" ", terms);
        }
    }

    /// <summary>0.5 yamasındaki waystone stat satırları (tek doğruluk kaynağı).</summary>
    public static class WaystoneStats
    {
        public static readonly (string Label, string Prefix, int Default)[] Items =
        {
            ("ITEM RARITY",           "m rarity: ",  0),
            ("PACK SIZE",             "size: ",      0),
            ("MONSTER RARITY",        "r rarity: ",  0),
            ("WAYSTONE DROP CHANCE",  "chance: ",    0),
        };
    }

    /// <summary>
    /// Kayıtları %AppData%\WaystoneRegex\Regexes klasöründe TEK TEK .wsrx dosyaları olarak saklar.
    /// Klasör paylaşım noktasıdır: dosyalar dışarı sürüklenebilir, içine atılanlar YENİLE ile yüklenir.
    /// </summary>
    public static class SavedRegexStore
    {
        private static string BaseDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WaystoneRegex");

        public static string FolderPath => Path.Combine(BaseDir, "Regexes");

        private static string LegacyJsonPath => Path.Combine(BaseDir, "saved.json");

        /// <summary>Klasördeki tüm .wsrx dosyalarını okur (eski saved.json varsa bir kez taşır).</summary>
        public static List<SavedRegex> Load()
        {
            Directory.CreateDirectory(FolderPath);

            // Eski tek-dosya formatından geçiş
            if (!Directory.EnumerateFiles(FolderPath, "*.wsrx").Any() && File.Exists(LegacyJsonPath))
            {
                try
                {
                    var old = JsonSerializer.Deserialize<List<SavedRegex>>(File.ReadAllText(LegacyJsonPath)) ?? new();
                    Save(old);
                    File.Move(LegacyJsonPath, LegacyJsonPath + ".bak", overwrite: true);
                }
                catch { }
            }

            var list = new List<SavedRegex>();
            foreach (var file in Directory.EnumerateFiles(FolderPath, "*.wsrx").OrderBy(f => f))
            {
                try { list.AddRange(ImportFromFile(file)); }
                catch { /* bozuk dosya atlanır */ }
            }
            return list;
        }

        /// <summary>Listeyi klasöre yazar: her kayıt kendi dosyasında, listede olmayan dosyalar silinir.</summary>
        public static void Save(List<SavedRegex> list)
        {
            Directory.CreateDirectory(FolderPath);
            var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var saved in list)
            {
                string baseName = string.Join("_", saved.Name.Split(Path.GetInvalidFileNameChars()));
                if (baseName.Length == 0) baseName = "regex";
                string path = Path.Combine(FolderPath, baseName + ".wsrx");
                int n = 2;
                while (keep.Contains(path))
                    path = Path.Combine(FolderPath, $"{baseName}_{n++}.wsrx");
                ExportToFile(saved, path);
                keep.Add(path);
            }

            foreach (var file in Directory.EnumerateFiles(FolderPath, "*.wsrx"))
                if (!keep.Contains(file))
                    File.Delete(file);
        }

        /// <summary>Paylaşım dosyası (.wsrx): tek kaydın JSON hali.</summary>
        public static void ExportToFile(SavedRegex saved, string path)
            => File.WriteAllText(path, JsonSerializer.Serialize(saved, new JsonSerializerOptions { WriteIndented = true }));

        /// <summary>.wsrx dosyasından kayıt(lar) okur; tek kayıt veya liste kabul eder.</summary>
        public static List<SavedRegex> ImportFromFile(string path)
        {
            string json = File.ReadAllText(path);
            try
            {
                var one = JsonSerializer.Deserialize<SavedRegex>(json);
                if (one is not null && (one.Name.Length > 0 || one.Query.Length > 0 || one.Stats.Count > 0))
                    return new List<SavedRegex> { one };
            }
            catch { /* liste olabilir */ }
            return JsonSerializer.Deserialize<List<SavedRegex>>(json) ?? new();
        }
    }
}
