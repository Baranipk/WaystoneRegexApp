using System.Text.RegularExpressions;

namespace WaystoneRegex
{
    /// <summary>
    /// PoE2 stash arama kutusu semantiği:
    /// - Boşluk terimleri ayırır (AND mantığı)
    /// - Çift tırnak içindeki ifade tek terim sayılır (boşluk içerebilir)
    /// - '!' öneki negasyon yapar (terim eşleşen item hariç tutulur)
    /// - Her terim case-insensitive regex olarak item metninde aranır
    /// </summary>
    public class SearchTerm
    {
        public string Pattern { get; init; } = "";
        public bool Negated { get; init; }
        public bool WasQuoted { get; init; }
        public Regex? Regex { get; init; }
        public string? Error { get; init; }
    }

    public static class SearchQuery
    {
        public static List<SearchTerm> Parse(string query)
        {
            var terms = new List<SearchTerm>();
            int i = 0;
            while (i < query.Length)
            {
                if (char.IsWhiteSpace(query[i])) { i++; continue; }

                bool negated = false;
                if (query[i] == '!') { negated = true; i++; }
                if (i >= query.Length) break;

                string pattern;
                bool quoted = false;
                if (query[i] == '"')
                {
                    quoted = true;
                    int end = query.IndexOf('"', i + 1);
                    if (end < 0) { pattern = query[(i + 1)..]; i = query.Length; }
                    else { pattern = query[(i + 1)..end]; i = end + 1; }
                }
                else
                {
                    int start = i;
                    while (i < query.Length && !char.IsWhiteSpace(query[i])) i++;
                    pattern = query[start..i];
                }

                if (pattern.Length == 0) continue;

                Regex? rx = null;
                string? error = null;
                try
                {
                    rx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
                catch (ArgumentException ex)
                {
                    error = ex.Message;
                }

                terms.Add(new SearchTerm { Pattern = pattern, Negated = negated, WasQuoted = quoted, Regex = rx, Error = error });
            }
            return terms;
        }

        /// <summary>Tüm terimler sağlanıyorsa true (hatalı regex'li terimler yok sayılır).</summary>
        public static bool Matches(IEnumerable<SearchTerm> terms, string itemText)
            => terms.All(t => t.Regex is null || t.Regex.IsMatch(itemText) != t.Negated);
    }
}
