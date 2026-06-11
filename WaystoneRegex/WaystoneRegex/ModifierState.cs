namespace WaystoneRegex
{
    public enum ModMode { Off, Hide, Only }

    public class ModifierState
    {
        public string Label { get; }
        public string Pattern { get; }
        public ModMode Mode { get; set; } = ModMode.Off;

        public ModifierState(string label, string pattern)
        {
            Label = label;
            Pattern = pattern;
        }

        // Oyun terimleri — dilden bağımsız, her zaman orijinal İngilizce
        public static List<ModifierState> Defaults() => new()
        {
            new("Extra Chaos Damage",            "extra chaos"),
            new("Extra Fire Damage",             "extra fire"),
            new("Extra Cold Damage",             "extra cold"),
            new("Extra Lightning Damage",        "extra lightning"),
            new("Reduced Player Resistances",    "maximum player resistances"),
            new("Less Recovery (Life/ES)",       "less recovery"),
            new("Temporal Chains",               "temporal chains"),
            new("Enfeeble",                      "enfeeble"),
            new("Additional Projectiles",        "additional projectiles"),
            new("Increased Monster Speed",       "movement speed"),
            new("Critical Hits",                 "critical"),
            new("Reduced Flask Charges",         "flask charges"),
        };
    }
}
