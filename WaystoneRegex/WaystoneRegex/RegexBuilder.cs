namespace WaystoneRegex
{
    /// <summary>"En az N%" koşulunu sağlayan sayı regex'i üretir (örn. 35 → (3[5-9]|[4-9]\d|\d{3})).</summary>
    public static class RegexBuilder
    {
        public static string MinNumberPattern(int min)
        {
            if (min <= 0) return @"\d+";

            // Oyun içi arama kutusu ~50 karakterle sınırlı olduğu için
            // topluluk standardı kısa yazım kullanılır: \d yerine . ve 100+ için 1..
            var parts = new List<string>();
            if (min >= 200)
            {
                return "([2-9]..)";
            }
            if (min >= 100)
            {
                // 100-199 aralığı: yüzler basamağı 1, kalan iki hane için eşik
                int rem = min - 100;
                int tens = rem / 10, ones = rem % 10;
                if (ones == 0)
                    parts.Add($"1[{tens}-9].");
                else
                {
                    parts.Add($"1{tens}[{ones}-9]");
                    if (tens < 9) parts.Add($"1[{tens + 1}-9].");
                }
                parts.Add("[2-9].."); // 200% ve üzeri
            }
            else
            {
                if (min < 10)
                {
                    parts.Add($"[{min}-9]");
                    parts.Add("[1-9].");
                }
                else
                {
                    int tens = min / 10, ones = min % 10;
                    if (ones == 0)
                        parts.Add($"[{tens}-9].");
                    else
                    {
                        parts.Add($"{tens}[{ones}-9]");
                        if (tens < 9) parts.Add($"[{tens + 1}-9].");
                    }
                }
                parts.Add("1.."); // 100% ve üzeri
            }
            return "(" + string.Join("|", parts) + ")";
        }

        /// <summary>
        /// Verilen stat öneki için tam arama terimi üretir.
        /// max > 0 ise min–max aralığı, değilse "min ve üzeri".
        /// </summary>
        public static string StatTerm(string prefix, int min, int max = 0)
            => $"\"{prefix}\\+{(max > 0 && max >= min ? RangeNumberPattern(min, Math.Min(max, 199)) : MinNumberPattern(min))}%\"";

        /// <summary>min–max (0–199) aralığını kapsayan sayı kalıbı, örn. (20,45) → (2[0-9]|3[0-9]|4[0-5]).</summary>
        public static string RangeNumberPattern(int min, int max)
        {
            var parts = new List<string>();

            // Tek haneli bölüm
            if (min <= 9)
                parts.Add(D(min, Math.Min(max, 9)));

            // İki haneli bölüm
            if (max >= 10 && min <= 99)
                parts.AddRange(TwoDigit(Math.Max(min, 10), Math.Min(max, 99), leadingZero: false));

            // 100-199 bölümü ("1" + son iki hane)
            if (max >= 100)
                parts.AddRange(TwoDigit(Math.Max(min, 100) - 100, max - 100, leadingZero: true).Select(p => "1" + p));

            return parts.Count == 1 && !parts[0].Contains('|') ? "(" + parts[0] + ")" : "(" + string.Join("|", parts) + ")";
        }

        private static string D(int x, int y) => x == y ? x.ToString() : $"[{x}-{y}]";

        /// <summary>İki haneli [a,b] aralığı için kalıp parçaları (leadingZero: 0-99, "05" gibi).</summary>
        private static IEnumerable<string> TwoDigit(int a, int b, bool leadingZero)
        {
            if (a > b) yield break;
            int ta = a / 10, oa = a % 10, tb = b / 10, ob = b % 10;

            if (ta == tb)
            {
                yield return ta.ToString() + D(oa, ob);
                yield break;
            }

            int midLo = ta, midHi = tb;
            if (oa > 0) { yield return ta.ToString() + D(oa, 9); midLo = ta + 1; }
            if (ob < 9) midHi = tb - 1;
            if (midLo <= midHi) yield return D(midLo, midHi) + ".";
            if (ob < 9) yield return tb.ToString() + D(0, ob);
        }

        /// <summary>
        /// Negasyon terimi. Oyun içi arama !"tırnaklı ifade" biçimini desteklemediği için
        /// boşluklar '.' ile değiştirilip tırnaksız tek parça terim üretilir: !extra.chaos
        /// </summary>
        public static string NegTerm(string pattern)
            => "!" + pattern.TrimEnd().Replace(" ", ".");
    }
}
