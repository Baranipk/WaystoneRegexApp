using ReaLTaiizor.Colors;
using ReaLTaiizor.Util;

namespace WaystoneRegex
{
    /// <summary>
    /// Tema paleti: pencere zemini, girdi zemini, yazı, vurgu (butonlar) ve regex çıktısı rengi.
    /// MaterialPrimary/Accent yalnız başlık çubuğu ve sekme çizgisi için kullanılır.
    /// </summary>
    public record ThemeDef(
        string Name,
        bool DarkSkin,
        Color Back,
        Color InputBack,
        Color Fore,
        Color Accent,
        Color Query,
        MaterialPrimary Primary,
        MaterialAccent MaterialAccent);

    public static class Themes
    {
        public static readonly ThemeDef[] All =
        {
            // Varsayılan: yanık bronz + kızıl + kor (waystone görseli)
            new("WAYSTONE",   true,  Color.FromArgb(34, 24, 18),  Color.FromArgb(44, 32, 24),  Color.FromArgb(235, 215, 185), Color.FromArgb(160, 30, 30),   Color.FromArgb(255, 170, 60),  MaterialPrimary.Red900,      MaterialAccent.Amber400),
            // Kapalı mor
            new("BREACH",     true,  Color.FromArgb(26, 18, 36),  Color.FromArgb(38, 28, 52),  Color.FromArgb(225, 210, 240), Color.FromArgb(120, 45, 170),  Color.FromArgb(205, 130, 255), MaterialPrimary.Purple900,   MaterialAccent.Purple400),
            // Kapalı yeşil
            new("ABYSS",      true,  Color.FromArgb(14, 26, 20),  Color.FromArgb(22, 38, 30),  Color.FromArgb(200, 230, 210), Color.FromArgb(28, 110, 65),   Color.FromArgb(95, 220, 145),  MaterialPrimary.Green900,    MaterialAccent.Green400),
            // Koyu mavi
            new("EXPEDITION", true,  Color.FromArgb(15, 23, 40),  Color.FromArgb(23, 33, 56),  Color.FromArgb(200, 215, 240), Color.FromArgb(40, 90, 180),   Color.FromArgb(110, 175, 255), MaterialPrimary.Indigo900,   MaterialAccent.Blue400),
            // Sarı-kırmızı koyu tonlar
            new("VAAL TEMPLE", true, Color.FromArgb(38, 22, 12),  Color.FromArgb(52, 30, 16),  Color.FromArgb(240, 220, 170), Color.FromArgb(180, 45, 30),   Color.FromArgb(255, 200, 60),  MaterialPrimary.DeepOrange900, MaterialAccent.Amber400),
            // Koyu + açık gri tonlar
            new("DELIRIUM",   true,  Color.FromArgb(38, 38, 42),  Color.FromArgb(54, 54, 60),  Color.FromArgb(228, 228, 233), Color.FromArgb(120, 120, 135), Color.FromArgb(240, 240, 248), MaterialPrimary.BlueGrey800, MaterialAccent.Blue400),
            // Koyu kırmızı tonlar
            new("RITUAL",     true,  Color.FromArgb(30, 12, 12),  Color.FromArgb(44, 18, 18),  Color.FromArgb(235, 200, 200), Color.FromArgb(170, 30, 40),   Color.FromArgb(255, 95, 95),   MaterialPrimary.Red900,      MaterialAccent.Red400),
            // Nötr koyu
            new("DARK",       true,  Color.FromArgb(32, 32, 36),  Color.FromArgb(46, 46, 52),  Color.FromArgb(225, 225, 225), Color.FromArgb(95, 95, 110),   Color.FromArgb(255, 200, 90),  MaterialPrimary.BlueGrey900, MaterialAccent.Amber400),
            // Beyaz tonlar
            new("LIGHT",      false, Color.FromArgb(246, 246, 248), Color.White,               Color.FromArgb(40, 40, 45),    Color.FromArgb(178, 34, 34),   Color.FromArgb(140, 70, 10),   MaterialPrimary.BlueGrey500, MaterialAccent.Amber400),
            // Son derece karanlık
            new("UBER DARK",  true,  Color.FromArgb(10, 10, 12),  Color.FromArgb(18, 18, 22),  Color.FromArgb(200, 200, 205), Color.FromArgb(70, 70, 80),    Color.FromArgb(235, 235, 240), MaterialPrimary.BlueGrey900, MaterialAccent.Blue400),
        };

        public static ThemeDef Current { get; set; } = All[0];

        public static ThemeDef Find(string name)
            => All.FirstOrDefault(t => t.Name == name) ?? All[0];
    }
}
