# for opening program click .exe in publish folder 

# Programı başlatmak için publish klasöründeki .exe uzantılı programı çalıştırın


# PoE2 Waystone Regex

**EN** | A regex builder for the *Path of Exile 2* stash search box — build, test, save and share waystone filters without writing regex by hand.
**TR** | *Path of Exile 2* stash arama kutusu için regex oluşturucu — elle regex yazmadan waystone filtreleri üret, kaydet ve paylaş.

---

## English

### What is this?

In PoE2 you filter waystones by typing search terms into the stash search box. The search box supports regex, but writing patterns like `"size: \+(2[0-9]|[3-9].|1..)%"` by hand is error-prone (quoting rules, the ~50 character limit, negation quirks). This app generates correct, game-tested patterns from simple inputs.

It targets the **patch 0.5** waystone stat lines:
`Item Rarity`, `Pack Size`, `Monster Rarity`, `Monster Effectiveness`, `Waystone Drop Chance`.

### Getting started

1. Run `WaystoneRegex.exe` — no installation, no .NET required (Windows 10/11 x64).
   Windows SmartScreen may warn on first run (unsigned exe): *More info → Run anyway*.
2. Your data lives in `%AppData%\WaystoneRegex` (settings + saved regexes).

### Regex Builder tab

| Control | What it does |
|---|---|
| **MIN %** | Lowest value to match. E.g. PACK SIZE MIN 20 matches `Pack Size: +20%` and above. Entering a value auto-activates the row. |
| **MAX %** | Optional upper bound (0 = unlimited). MIN 20 / MAX 45 matches +20%..+45%. |
| **ACTIVE** | Toggles the row on/off. |
| **EXCLUDE (!)** | Inverts the row: waystones *with* that stat are filtered out. |
| **⚙ WAYSTONE MODIFIERS** | Per-modifier filters (Extra Chaos/Fire/Cold/Lightning, Reduced Player Resistances, Temporal Chains, ...): **HIDE (!)** removes them, **SHOW** keeps only them. |
| **☠ CORRUPT** | OFF / ON (corrupted only) / HIDE (!). Corrupted waystones are the 6–8 mod pool. |
| **📋 COPY** | Copies the generated regex; paste it into the in-game search box. |
| **💾 SAVE REGEX** | Saves the current setup with a name and tags. |

The status line warns when the query exceeds the in-game **50 character limit**.

### Saved Regexes tab

- Each entry shows its name, tags, a stat badge line (`IR 30+  PS 20-45  MR —  DROP 80+  EFF !`) and the generated query.
- **COPY** / **SHOW** (load into the builder output) / **EXPORT** (save as `.wsrx` file) / **⚙** (edit every value, tags, delete).
- **REACH slider (0–20%)** lowers all MIN thresholds by that percentage for more matches — only minimums are relaxed.
- **Search box** filters by name, tag or query text; **TAG** dropdown filters by tag; **SORT** orders by name or any stat.
- Sharing: every saved regex is a `.wsrx` file in `%AppData%\WaystoneRegex\Regexes` (open with **📁**). Send the file to a friend; they drop it onto the list, use **📂 LOAD FROM FILE**, or copy it into their folder and press **🔄 REFRESH**.

### Settings tab

- **THEME**: 10 palettes — WAYSTONE (default), BREACH, ABYSS, EXPEDITION, VAAL TEMPLE, DELIRIUM, RITUAL, DARK, LIGHT, UBER DARK.
- **FONT**: pick the UI font.
- **LANGUAGE**: English (default) or Turkish. Game/regex terms always stay in English.
- **🏷 TAG SETTINGS**: add/remove the tags offered when saving (defaults: SIMULACRUM, BOSS RUSH, JUICE).
- **📁 OPEN REGEX FOLDER**: opens the `.wsrx` folder.

---

## Türkçe

### Bu program ne işe yarar?

PoE2'de waystone'ları stash arama kutusuna terim yazarak filtrelersin. Arama kutusu regex destekler ama `"size: \+(2[0-9]|[3-9].|1..)%"` gibi kalıpları elle yazmak hataya açıktır (tırnak kuralları, ~50 karakter sınırı, negasyon incelikleri). Bu uygulama basit girdilerden doğru, oyunda test edilmiş kalıplar üretir.

**0.5 yaması** waystone satırlarını hedefler:
`Item Rarity`, `Pack Size`, `Monster Rarity`, `Monster Effectiveness`, `Waystone Drop Chance`.

### Başlangıç

1. `WaystoneRegex.exe`'yi çalıştır — kurulum ve .NET gerekmez (Windows 10/11 x64).
   İlk açılışta SmartScreen uyarabilir (imzasız exe): *Daha fazla bilgi → Yine de çalıştır*.
2. Verilerin `%AppData%\WaystoneRegex` klasöründe tutulur (ayarlar + kayıtlı regexler).

### Regex Builder sekmesi

| Kontrol | İşlevi |
|---|---|
| **MIN %** | Eşleşecek en düşük değer. Örn. PACK SIZE MIN 20 → `Pack Size: +20%` ve üzeri eşleşir. Değer girince satır otomatik aktifleşir. |
| **MAX %** | İsteğe bağlı üst sınır (0 = sınırsız). MIN 20 / MAX 45 → +20%..+45% arası. |
| **ACTIVE** | Satırı açar/kapatır. |
| **EXCLUDE (!)** | Satırı tersine çevirir: o stat'a *sahip* waystone'lar elenir. |
| **⚙ WAYSTONE MODIFIERS** | Mod bazlı filtreler (Extra Chaos/Fire/Cold/Lightning, Reduced Player Resistances, Temporal Chains, ...): **HIDE (!)** o modluları eler, **SHOW** yalnız onları bırakır. |
| **☠ CORRUPT** | OFF / ON (sadece corrupt) / HIDE (!). Corrupt waystone'lar 6–8 mod havuzudur. |
| **📋 COPY** | Üretilen regex'i panoya kopyalar; oyundaki arama kutusuna yapıştır. |
| **💾 SAVE REGEX** | Mevcut ayarları isim ve etiketlerle kaydeder. |

Durum satırı, sorgu oyun içi **50 karakter sınırını** aşarsa uyarır.

### Saved Regexes sekmesi

- Her kayıtta isim, etiketler, stat rozeti satırı (`IR 30+  PS 20-45  MR —  DROP 80+  EFF !`) ve üretilen sorgu görünür.
- **COPY** / **SHOW** (oluşturucuya yükle) / **EXPORT** (`.wsrx` dosyası olarak kaydet) / **⚙** (tüm değerleri ve etiketleri düzenle, sil).
- **REACH kaydırıcısı (0–20%)** tüm MIN eşiklerini o oranda düşürür, daha çok waystone eşleşir — yalnız minimumlar gevşer.
- **Arama kutusu** isim/etiket/sorgu metninde arar; **TAG** listesi etikete göre filtreler; **SORT** isme veya stat'a göre sıralar.
- Paylaşım: her kayıt `%AppData%\WaystoneRegex\Regexes` klasöründe bir `.wsrx` dosyasıdır (**📁** ile aç). Dosyayı arkadaşına gönder; o da listeye sürükleyip bırakır, **📂 LOAD FROM FILE** kullanır ya da kendi klasörüne kopyalayıp **🔄 REFRESH**'e basar.

### Settings sekmesi

- **THEME**: 10 palet — WAYSTONE (varsayılan), BREACH, ABYSS, EXPEDITION, VAAL TEMPLE, DELIRIUM, RITUAL, DARK, LIGHT, UBER DARK.
- **FONT**: arayüz fontunu seç.
- **LANGUAGE**: İngilizce (varsayılan) veya Türkçe. Oyun/regex terimleri her zaman İngilizce kalır.
- **🏷 TAG SETTINGS**: kaydederken sunulan etiketleri ekle/sil (varsayılanlar: SIMULACRUM, BOSS RUSH, JUICE).
- **📁 OPEN REGEX FOLDER**: `.wsrx` klasörünü açar.

---

### Notes / Notlar

- Negation uses the game-tested unquoted form (e.g. `!extra.chaos`) because the in-game search does not support `!"quoted phrases"`.
  Negasyonlar oyunda test edilmiş tırnaksız biçimi kullanır (örn. `!extra.chaos`); oyun `!"tırnaklı ifade"` desteklemez.
- The in-game search cannot count mods; the CORRUPT filter is the practical way to separate the 6–8 mod pool.
  Oyun içi arama mod sayamaz; 6–8 mod havuzunu ayırmanın pratik yolu CORRUPT filtresidir.

---

### License / Lisans

Released under the **MIT License** — see [LICENSE](LICENSE). Free to use, modify and redistribute; keep the copyright notice.
**MIT Lisansı** ile yayınlanmıştır — bkz. [LICENSE](LICENSE). Kullanımı, değiştirilmesi ve dağıtımı serbesttir; telif notunu koruyun.

### Disclaimer / Feragatname

This is an unofficial fan-made tool. It is not affiliated with, endorsed or sponsored by Grinding Gear Games. *Path of Exile* and *Path of Exile 2* are trademarks of Grinding Gear Games.
Bu, resmi olmayan bir hayran aracıdır. Grinding Gear Games ile bağlantılı değildir, onaylanmamış veya desteklenmemiştir. *Path of Exile* ve *Path of Exile 2*, Grinding Gear Games'in ticari markalarıdır.
