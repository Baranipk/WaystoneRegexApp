using ReaLTaiizor.Colors;
using ReaLTaiizor.Controls;
using ReaLTaiizor.Manager;
using ReaLTaiizor.Util;
using MaterialForm = ReaLTaiizor.Forms.MaterialForm;
using TabPage = System.Windows.Forms.TabPage;
using Panel = System.Windows.Forms.Panel;
using Label = System.Windows.Forms.Label;
using ComboBox = System.Windows.Forms.ComboBox;
using CheckBox = System.Windows.Forms.CheckBox;
using Button = System.Windows.Forms.Button;
using TrackBar = System.Windows.Forms.TrackBar;

namespace WaystoneRegex
{
    public partial class Form1 : MaterialForm
    {
        private readonly MaterialSkinManager _skin = MaterialSkinManager.Instance;
        private readonly AppSettings _settings = AppSettings.Load();

        private MaterialTabControl _tabs = null!;
        private TabPage _tabBuilder = null!, _tabSaved = null!, _tabSettings = null!;
        private TextBox _queryBox = null!;
        private Label _summaryLabel = null!;
        private Label _modSummaryLabel = null!;
        private Label _outLabel = null!, _hintLabel = null!;
        private Button _copyBtn = null!;
        private Button _clearBtn = null!;
        private MaterialButton _modsBtn = null!;
        private readonly System.Windows.Forms.Timer _copyFlashTimer = new() { Interval = 900 };
        private ComboBox _themeCombo = null!;
        private ComboBox _fontCombo = null!;
        private ComboBox _langCombo = null!;
        private Label _fontLbl = null!, _langLbl = null!, _themeLbl = null!;
        private Button _tagSettingsBtn = null!;
        private Button _openFolderBtn = null!;

        private record StatRow(CheckBox Enabled, NumericUpDown Min, NumericUpDown Max, string Prefix, CheckBox Exclude);
        private readonly List<StatRow> _statRows = new();
        private CheckBox _effEnabled = null!;
        private CheckBox _effExclude = null!;
        private NumericUpDown _effMin = null!;
        private NumericUpDown _effMax = null!;
        private readonly List<CheckBox> _checks = new();
        private bool _suppressBuild;

        private readonly List<ModifierState> _mods = ModifierState.Defaults();

        private static readonly string[] FontChoices =
        {
            "Bahnschrift", "Segoe UI", "Verdana", "Tahoma", "Arial", "Calibri", "Trebuchet MS",
        };
        private string _fontFamily;
        private readonly List<Label> _bigLabels = new();
        private readonly List<Label> _smallLabels = new();

        private readonly List<SavedRegex> _saved = SavedRegexStore.Load();
        private FlowLayoutPanel _savedPanel = null!;
        private Button _saveBtn = null!;
        private TextBox _savedSearchBox = null!;
        private ComboBox _savedSortCombo = null!;
        private ComboBox _savedTagCombo = null!;
        private Button _savedImportBtn = null!;
        private Button _savedRefreshBtn = null!;
        private Button _savedFolderBtn = null!;
        private Label _savedSearchLbl = null!, _savedSortLbl = null!, _savedTagLbl = null!;
        private bool _suppressSavedRefresh;

        private int _corruptMode;
        private MaterialButton _corruptBtn = null!;

        public Form1()
        {
            InitializeComponent();

            Loc.Turkish = _settings.Turkish;
            _fontFamily = FontChoices.Contains(_settings.Font) ? _settings.Font : FontChoices[0];
            Themes.Current = Themes.Find(_settings.ThemeName);

            _skin.AddFormToManage(this);
            ApplySkinTheme();

            BuildUi();
            ApplyLanguage();
            Recalculate();
            ApplyFonts();
            StyleRawInputs();
        }

        private bool IsDark => Themes.Current.DarkSkin;

        /// <summary>Material başlık çubuğu ve sekme rengini seçili temaya göre ayarlar.</summary>
        private void ApplySkinTheme()
        {
            var t = Themes.Current;
            _skin.Theme = t.DarkSkin ? MaterialSkinManager.Themes.DARK : MaterialSkinManager.Themes.LIGHT;
            _skin.ColorScheme = new MaterialColorScheme(
                t.Primary, t.Primary, t.Primary, t.MaterialAccent, MaterialTextShade.WHITE);
        }

        /// <summary>İki rengi karıştırır (rozet gibi ikincil yazılar için).</summary>
        private static Color Mix(Color a, Color b, double ratio)
            => Color.FromArgb(
                (int)(a.R * ratio + b.R * (1 - ratio)),
                (int)(a.G * ratio + b.G * (1 - ratio)),
                (int)(a.B * ratio + b.B * (1 - ratio)));

        private string CorruptText() => "☠ CORRUPT: " + _corruptMode switch
        {
            1 => Loc.T("ON", "AÇIK"),
            2 => Loc.T("HIDE (!)", "GİZLE (!)"),
            _ => Loc.T("OFF", "KAPALI"),
        };

        private void BuildUi()
        {
            Text = "PoE2 WAYSTONE REGEX";
            MinimumSize = new Size(1214, 870);
            Size = new Size(1214, 870);

            var tabSelector = new MaterialTabSelector { Dock = DockStyle.Top, Height = 48 };
            _tabs = new MaterialTabControl { Dock = DockStyle.Fill };
            tabSelector.BaseTabControl = _tabs;

            _tabBuilder = new TabPage();
            _tabSaved = new TabPage();
            _tabSettings = new TabPage();
            _tabs.TabPages.AddRange(new[] { _tabBuilder, _tabSaved, _tabSettings });

            Controls.Add(_tabs);
            Controls.Add(tabSelector);

            BuildBuilderTab(_tabBuilder);
            BuildSavedTab(_tabSaved);
            BuildSettingsTab(_tabSettings);
        }

        private void BuildBuilderTab(TabPage tab)
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(16, 10, 16, 10) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // ===== OUTPUT =====
            _outLabel = new Label { AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
            _smallLabels.Add(_outLabel);
            _queryBox = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                Height = 96,
                ScrollBars = ScrollBars.Vertical,
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle,
            };
            _queryBox.TextChanged += (_, _) => Recalculate();
            var outPanel = new Panel { Dock = DockStyle.Top, AutoSize = true };
            outPanel.Controls.Add(_queryBox);
            outPanel.Controls.Add(_outLabel);
            _outLabel.Dock = DockStyle.Top;
            root.Controls.Add(outPanel, 0, 0);

            // ===== Butonlar =====
            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
            _copyBtn = new Button
            {
                Size = new Size(220, 56),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 12, 0),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            _copyBtn.FlatAppearance.BorderSize = 0;
            _copyBtn.Click += (_, _) =>
            {
                if (_queryBox.Text.Length == 0) return;
                Clipboard.SetText(_queryBox.Text);
                // Tıklama efekti: her temada görünür — parlak yeşil onay + metin değişimi
                _copyFlashTimer.Stop();
                _copyBtn.BackColor = Color.FromArgb(40, 160, 70);
                _copyBtn.Text = Loc.T("✔ COPIED!", "✔ KOPYALANDI!");
                _copyFlashTimer.Start();
            };
            _copyFlashTimer.Tick += (_, _) =>
            {
                _copyFlashTimer.Stop();
                _copyBtn.BackColor = Themes.Current.Accent;
                _copyBtn.Text = Loc.T("📋 COPY", "📋 KOPYALA");
            };
            _clearBtn = new Button
            {
                Size = new Size(180, 56),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 8, 0),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            _clearBtn.FlatAppearance.BorderSize = 2;
            _clearBtn.Click += (_, _) => ClearInputs();
            btnFlow.Controls.AddRange(new Control[] { _copyBtn, _clearBtn });
            root.Controls.Add(btnFlow, 0, 1);

            // ===== Açıklama =====
            _hintLabel = new Label { AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
            _smallLabels.Add(_hintLabel);
            root.Controls.Add(_hintLabel, 0, 2);

            // ===== Stat satırları (oyun terimleri — çevrilmez) =====
            var statTable = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 5, Margin = new Padding(0, 6, 0, 0) };
            statTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            statTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195));
            statTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195));
            statTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            statTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // MIN / MAX başlıkları
            var minHead = new Label { Text = "MIN %", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(4, 0, 0, 0) };
            var maxHead = new Label { Text = "MAX % (0 = ∞)", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(4, 0, 0, 0) };
            _smallLabels.Add(minHead);
            _smallLabels.Add(maxHead);
            statTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            statTable.Controls.Add(new Label { Text = "" }, 0, 0);
            statTable.Controls.Add(minHead, 1, 0);
            statTable.Controls.Add(maxHead, 2, 0);

            int rowIdx = 1;
            foreach (var (label, prefix, def) in WaystoneStats.Items)
            {
                var lbl = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 12, 0, 12) };
                _bigLabels.Add(lbl);
                var num = new NumericUpDown { Minimum = 0, Maximum = 200, Value = def, Width = 170, TextAlign = HorizontalAlignment.Center, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 8), BorderStyle = BorderStyle.FixedSingle };
                var maxNum = new NumericUpDown { Minimum = 0, Maximum = 199, Value = 0, Width = 170, TextAlign = HorizontalAlignment.Center, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 8), BorderStyle = BorderStyle.FixedSingle };
                var cb = MakeToggle("active", new Padding(16, 6, 0, 6));
                var excl = MakeToggle("exclude", new Padding(8, 6, 0, 6));
                excl.Enabled = false;
                excl.Visible = label != "WAYSTONE DROP CHANCE";

                cb.CheckedChanged += (_, _) =>
                {
                    excl.Enabled = cb.Checked;
                    if (!cb.Checked && excl.Checked) excl.Checked = false;
                    else BuildQueryFromInputs();
                };
                excl.CheckedChanged += (_, _) =>
                {
                    num.Enabled = !excl.Checked;
                    maxNum.Enabled = !excl.Checked;
                    if (excl.Checked && !cb.Checked) cb.Checked = true;
                    else BuildQueryFromInputs();
                };
                num.ValueChanged += (_, _) => { if (!cb.Checked) cb.Checked = true; else BuildQueryFromInputs(); };
                maxNum.ValueChanged += (_, _) => { if (!cb.Checked) cb.Checked = true; else BuildQueryFromInputs(); };

                statTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                statTable.Controls.Add(lbl, 0, rowIdx);
                statTable.Controls.Add(num, 1, rowIdx);
                statTable.Controls.Add(maxNum, 2, rowIdx);
                statTable.Controls.Add(cb, 3, rowIdx);
                statTable.Controls.Add(excl, 4, rowIdx);
                _statRows.Add(new StatRow(cb, num, maxNum, prefix, excl));
                rowIdx++;
            }

            var effLbl = new Label { Text = "EFFECTIVENESS", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 12, 0, 12) };
            _bigLabels.Add(effLbl);
            _effMin = new NumericUpDown { Minimum = 0, Maximum = 200, Value = 0, Width = 170, TextAlign = HorizontalAlignment.Center, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 8), BorderStyle = BorderStyle.FixedSingle };
            _effMax = new NumericUpDown { Minimum = 0, Maximum = 199, Value = 0, Width = 170, TextAlign = HorizontalAlignment.Center, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 8), BorderStyle = BorderStyle.FixedSingle };
            _effEnabled = MakeToggle("active", new Padding(16, 6, 0, 6));
            _effExclude = MakeToggle("exclude", new Padding(8, 6, 0, 6));
            _effExclude.Enabled = false;
            _effEnabled.CheckedChanged += (_, _) =>
            {
                _effExclude.Enabled = _effEnabled.Checked;
                if (!_effEnabled.Checked && _effExclude.Checked) _effExclude.Checked = false;
                else BuildQueryFromInputs();
            };
            _effExclude.CheckedChanged += (_, _) =>
            {
                _effMin.Enabled = !_effExclude.Checked;
                _effMax.Enabled = !_effExclude.Checked;
                if (_effExclude.Checked && !_effEnabled.Checked) _effEnabled.Checked = true;
                else BuildQueryFromInputs();
            };
            _effMin.ValueChanged += (_, _) => { if (!_effEnabled.Checked) _effEnabled.Checked = true; else BuildQueryFromInputs(); };
            _effMax.ValueChanged += (_, _) => { if (!_effEnabled.Checked) _effEnabled.Checked = true; else BuildQueryFromInputs(); };
            statTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            statTable.Controls.Add(effLbl, 0, rowIdx);
            statTable.Controls.Add(_effMin, 1, rowIdx);
            statTable.Controls.Add(_effMax, 2, rowIdx);
            statTable.Controls.Add(_effEnabled, 3, rowIdx);
            statTable.Controls.Add(_effExclude, 4, rowIdx);

            root.Controls.Add(statTable, 0, 3);

            // ===== Modifiers + Corrupt =====
            var modFlow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
            _modsBtn = new MaterialButton { Text = "⚙ WAYSTONE MODIFIERS", Type = MaterialButton.MaterialButtonType.Contained, AutoSize = true, Margin = new Padding(0, 0, 8, 0) };
            _modsBtn.Click += (_, _) => OpenModifiersDialog();
            _corruptBtn = new MaterialButton { Type = MaterialButton.MaterialButtonType.Outlined, UseAccentColor = true, AutoSize = true };
            _corruptBtn.Click += (_, _) =>
            {
                _corruptMode = (_corruptMode + 1) % 3;
                _corruptBtn.Text = CorruptText();
                BuildQueryFromInputs();
            };
            modFlow.Controls.AddRange(new Control[] { _modsBtn, _corruptBtn });
            root.Controls.Add(modFlow, 0, 4);

            // ===== Özetler + sağ altta büyük KAYDET =====
            var bottomPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 10, 0, 0) };
            var sumPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown };
            _summaryLabel = new Label { AutoSize = true };
            _modSummaryLabel = new Label { AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
            _smallLabels.Add(_summaryLabel);
            _smallLabels.Add(_modSummaryLabel);
            sumPanel.Controls.Add(_summaryLabel);
            sumPanel.Controls.Add(_modSummaryLabel);

            _saveBtn = new Button
            {
                Size = new Size(320, 80),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            };
            _saveBtn.FlatAppearance.BorderSize = 0;
            _saveBtn.Click += (_, _) => SaveCurrentRegex();
            bottomPanel.Resize += (_, _) =>
                _saveBtn.Location = new Point(bottomPanel.ClientSize.Width - _saveBtn.Width - 8, bottomPanel.ClientSize.Height - _saveBtn.Height - 8);

            bottomPanel.Controls.Add(sumPanel);
            bottomPanel.Controls.Add(_saveBtn);
            root.Controls.Add(bottomPanel, 0, 5);

            tab.Controls.Add(root);
        }

        private void BuildSavedTab(TabPage tab)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // ===== Üst şerit: arama + sıralama + dosyadan yükleme =====
            var header = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12, 10, 12, 4) };

            _savedSearchLbl = new Label { Text = "🔍", AutoSize = true, Margin = new Padding(0, 12, 4, 0) };
            _smallLabels.Add(_savedSearchLbl);
            _savedSearchBox = new TextBox { Width = 240, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 8, 16, 0) };
            _savedSearchBox.TextChanged += (_, _) => RefreshSavedTab();

            _savedTagLbl = new Label { Text = "TAG:", AutoSize = true, Margin = new Padding(0, 12, 4, 0) };
            _smallLabels.Add(_savedTagLbl);
            _savedTagCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 8, 16, 0) };
            _savedTagCombo.SelectedIndexChanged += (_, _) => RefreshSavedTab();

            _savedSortLbl = new Label { AutoSize = true, Margin = new Padding(0, 12, 4, 0) };
            _smallLabels.Add(_savedSortLbl);
            _savedSortCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 8, 16, 0) };
            _savedSortCombo.SelectedIndexChanged += (_, _) => RefreshSavedTab();

            _savedImportBtn = new Button { Size = new Size(200, 40), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 6, 8, 0) };
            _savedImportBtn.Click += (_, _) => ImportFromFiles();

            _savedRefreshBtn = new Button { Size = new Size(140, 40), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 6, 8, 0) };
            _savedRefreshBtn.Click += (_, _) => ReloadFromFolder();

            _savedFolderBtn = new Button { Text = "📁", Size = new Size(48, 40), FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 6, 0, 0), Font = new Font("Segoe UI", 14f) };
            _savedFolderBtn.Click += (_, _) =>
            {
                Directory.CreateDirectory(SavedRegexStore.FolderPath);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = SavedRegexStore.FolderPath,
                    UseShellExecute = true,
                });
            };

            header.Controls.AddRange(new Control[] { _savedSearchLbl, _savedSearchBox, _savedTagLbl, _savedTagCombo, _savedSortLbl, _savedSortCombo, _savedImportBtn, _savedRefreshBtn, _savedFolderBtn });
            layout.Controls.Add(header, 0, 0);

            // ===== Kayıt listesi =====
            _savedPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(16, 4, 16, 16),
            };
            _savedPanel.Resize += (_, _) =>
            {
                foreach (Control c in _savedPanel.Controls) c.Width = _savedPanel.ClientSize.Width - 40;
            };

            // .wsrx dosyalarını pencereye sürükle-bırak ile içe aktarma
            _savedPanel.AllowDrop = true;
            _savedPanel.DragEnter += (_, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy;
            };
            _savedPanel.DragDrop += (_, e) =>
            {
                if (e.Data?.GetData(DataFormats.FileDrop) is string[] files) ImportFilesCore(files);
            };

            layout.Controls.Add(_savedPanel, 0, 1);

            tab.Controls.Add(layout);
        }

        /// <summary>Regexes klasörünü yeniden okur: içine atılan/değiştirilen dosyalar uygulamaya yansır.</summary>
        private void ReloadFromFolder()
        {
            _saved.Clear();
            foreach (var saved in SavedRegexStore.Load())
            {
                saved.RebuildQuery();
                _saved.Add(saved);
            }
            SavedRegexStore.Save(_saved); // dosya adlarını/içerikleri normalize et
            RefreshSavedTab();
        }

        /// <summary>Sıralama seçeneklerini dile göre kurar (stat adları orijinal kalır).</summary>
        private void RebuildSortItems()
        {
            int keep = Math.Max(_savedSortCombo.SelectedIndex, 0);
            _savedSortCombo.Items.Clear();
            _savedSortCombo.Items.AddRange(new object[]
            {
                Loc.T("Name", "İsim"),
                "ITEM RARITY",
                "PACK SIZE",
                "MONSTER RARITY",
                "WAYSTONE DROP CHANCE",
                "EFFECTIVENESS",
            });
            _savedSortCombo.SelectedIndex = Math.Min(keep, _savedSortCombo.Items.Count - 1);
        }

        /// <summary>Etiket filtresi seçeneklerini günceller: ALL + kayıtlardaki tüm etiketler.</summary>
        private void RebuildTagItems()
        {
            string? keep = _savedTagCombo.SelectedIndex > 0 ? _savedTagCombo.SelectedItem as string : null;
            _suppressSavedRefresh = true;
            _savedTagCombo.Items.Clear();
            _savedTagCombo.Items.Add(Loc.T("ALL", "TÜMÜ"));
            foreach (var tag in _saved.SelectMany(s => s.Tags).Distinct().OrderBy(t => t))
                _savedTagCombo.Items.Add(tag);
            int idx = keep is null ? 0 : _savedTagCombo.Items.IndexOf(keep);
            _savedTagCombo.SelectedIndex = idx < 0 ? 0 : idx;
            _suppressSavedRefresh = false;
        }

        /// <summary>Arama + etiket filtresi + sıralama uygulanmış kayıt listesi.</summary>
        private List<SavedRegex> FilteredSaved()
        {
            IEnumerable<SavedRegex> list = _saved;

            if (_savedTagCombo?.SelectedIndex > 0)
            {
                string tag = (string)_savedTagCombo.SelectedItem!;
                list = list.Where(s => s.Tags.Contains(tag));
            }

            string q = _savedSearchBox?.Text.Trim() ?? "";
            if (q.Length > 0)
                list = list.Where(s =>
                    s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    s.Query.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    s.Tags.Any(t => t.Contains(q, StringComparison.OrdinalIgnoreCase)));

            int sort = _savedSortCombo?.SelectedIndex ?? 0;
            list = sort switch
            {
                1 => list.OrderByDescending(s => s.StatSortValue(0)),
                2 => list.OrderByDescending(s => s.StatSortValue(1)),
                3 => list.OrderByDescending(s => s.StatSortValue(2)),
                4 => list.OrderByDescending(s => s.StatSortValue(3)),
                5 => list.OrderByDescending(s => s.EffSortValue()),
                _ => list.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase),
            };
            return list.ToList();
        }

        private void BuildSettingsTab(TabPage tab)
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(24, 20, 24, 20) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 4; i++) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

            _themeLbl = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 14, 0, 0) };
            _bigLabels.Add(_themeLbl);
            _themeCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240, Anchor = AnchorStyles.Left, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 14, 0, 0) };
            _themeCombo.Items.AddRange(Themes.All.Select(t => (object)t.Name).ToArray());
            _themeCombo.SelectedIndex = Array.FindIndex(Themes.All, t => t.Name == Themes.Current.Name);
            _themeCombo.SelectedIndexChanged += (_, _) =>
            {
                Themes.Current = Themes.All[_themeCombo.SelectedIndex];
                _settings.ThemeName = Themes.Current.Name;
                _settings.Dark = Themes.Current.DarkSkin;
                _settings.Save();
                ApplySkinTheme();
                StyleRawInputs();
            };
            panel.Controls.Add(_themeLbl, 0, 0);
            panel.Controls.Add(_themeCombo, 1, 0);

            _fontLbl = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 14, 0, 0) };
            _bigLabels.Add(_fontLbl);
            _fontCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240, Anchor = AnchorStyles.Left, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 14, 0, 0) };
            _fontCombo.Items.AddRange(FontChoices);
            _fontCombo.SelectedIndex = Array.IndexOf(FontChoices, _fontFamily);
            _fontCombo.SelectedIndexChanged += (_, _) =>
            {
                _fontFamily = (string)_fontCombo.SelectedItem!;
                _settings.Font = _fontFamily;
                _settings.Save();
                ApplyFonts();
                RefreshSavedTab();
            };
            panel.Controls.Add(_fontLbl, 0, 1);
            panel.Controls.Add(_fontCombo, 1, 1);

            _langLbl = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 14, 0, 0) };
            _bigLabels.Add(_langLbl);
            _langCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240, Anchor = AnchorStyles.Left, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 14, 0, 0) };
            _langCombo.Items.AddRange(new object[] { "English", "Türkçe" });
            _langCombo.SelectedIndex = _settings.Turkish ? 1 : 0;
            _langCombo.SelectedIndexChanged += (_, _) =>
            {
                Loc.Turkish = _langCombo.SelectedIndex == 1;
                _settings.Turkish = Loc.Turkish;
                _settings.Save();
                ApplyLanguage();
                Recalculate();
                RefreshSavedTab();
            };
            panel.Controls.Add(_langLbl, 0, 2);
            panel.Controls.Add(_langCombo, 1, 2);

            _tagSettingsBtn = new Button { Size = new Size(240, 44), FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Left, Margin = new Padding(0, 10, 0, 0) };
            _tagSettingsBtn.Click += (_, _) =>
            {
                using var dlg = new TagManagerDialog(_settings.Tags, IsDark, _fontFamily, _settings.Save);
                dlg.ShowDialog(this);
                RefreshSavedTab();
            };
            panel.Controls.Add(new Label { Text = "" }, 0, 3);
            panel.Controls.Add(_tagSettingsBtn, 1, 3);

            panel.RowCount = 5;
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            _openFolderBtn = new Button { Size = new Size(240, 44), FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Left, Margin = new Padding(0, 10, 0, 0) };
            _openFolderBtn.Click += (_, _) =>
            {
                Directory.CreateDirectory(SavedRegexStore.FolderPath);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = SavedRegexStore.FolderPath,
                    UseShellExecute = true,
                });
            };
            panel.Controls.Add(new Label { Text = "" }, 0, 4);
            panel.Controls.Add(_openFolderBtn, 1, 4);

            tab.Controls.Add(panel);
        }

        /// <summary>Tüm arayüz metinlerini seçili dile göre ayarlar (oyun terimleri hariç).</summary>
        private void ApplyLanguage()
        {
            _tabBuilder.Text = Loc.T("REGEX BUILDER", "REGEX OLUŞTURUCU");
            _tabSaved.Text = Loc.T("SAVED REGEXES", "KAYDEDİLMİŞ REGEXLER");
            _tabSettings.Text = Loc.T("SETTINGS", "AYARLAR");

            _outLabel.Text = Loc.T("GENERATED REGEX — paste into the in-game search box", "OLUŞTURULAN REGEX — oyundaki arama kutusuna yapıştır");
            _copyBtn.Text = Loc.T("📋 COPY", "📋 KOPYALA");
            _clearBtn.Text = Loc.T("CLEAR", "TEMİZLE");
            _hintLabel.Text = Loc.T(
                "MIN = lowest value to match; MAX optional (0 = no upper limit). E.g. 20–45 matches +20%..+45%",
                "MIN = eşleşecek en düşük değer; MAX isteğe bağlı (0 = üst sınır yok). Örn. 20–45 → +20%..+45% arası eşleşir");
            _corruptBtn.Text = CorruptText();
            _saveBtn.Text = Loc.T("💾  SAVE REGEX", "💾  REGEX'İ KAYDET");

            _themeLbl.Text = Loc.T("THEME", "TEMA");
            _fontLbl.Text = "FONT";
            _langLbl.Text = Loc.T("LANGUAGE", "DİL");

            _savedSortLbl.Text = Loc.T("SORT:", "SIRALA:");
            _savedImportBtn.Text = Loc.T("📂 LOAD FROM FILE", "📂 DOSYADAN YÜKLE");
            _savedRefreshBtn.Text = Loc.T("🔄 REFRESH", "🔄 YENİLE");
            _tagSettingsBtn.Text = "🏷 TAG SETTINGS";
            _openFolderBtn.Text = Loc.T("📁 OPEN REGEX FOLDER", "📁 REGEX KLASÖRÜNÜ AÇ");
            RebuildSortItems();

            foreach (var cb in _checks)
                cb.Text = (string)cb.Tag! == "exclude" ? Loc.T("EXCLUDE (!)", "HARİÇ TUT (!)") : Loc.T("ACTIVE", "AKTİF");

            RefreshSavedTab();
        }

        // ===== Tema uyumlu basmalı aç/kapa düğmesi =====
        private CheckBox MakeToggle(string kind, Padding margin)
        {
            var cb = new CheckBox
            {
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                AutoSize = false,
                Size = new Size(150, 42),
                Anchor = AnchorStyles.Left,
                Margin = margin,
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = kind,
            };
            cb.FlatAppearance.BorderSize = 1;
            cb.CheckedChanged += (_, _) => StyleToggle(cb);
            cb.EnabledChanged += (_, _) => StyleToggle(cb);
            _checks.Add(cb);
            return cb;
        }

        private void StyleToggle(CheckBox cb)
        {
            var t = Themes.Current;

            cb.FlatAppearance.CheckedBackColor = t.Accent;
            cb.FlatAppearance.MouseOverBackColor = cb.Checked ? t.Accent : Mix(t.Accent, t.Back, 0.25);
            cb.FlatAppearance.MouseDownBackColor = t.Accent;

            if (cb.Checked)
            {
                cb.BackColor = t.Accent;
                cb.ForeColor = Color.White;
                cb.FlatAppearance.BorderColor = t.Accent;
            }
            else
            {
                cb.BackColor = t.Back;
                cb.ForeColor = cb.Enabled ? t.Fore : Mix(t.Fore, t.Back, 0.6);
                cb.FlatAppearance.BorderColor = Mix(t.Accent, t.Fore, 0.55);
            }
        }

        private void ApplyFonts()
        {
            var big = new Font(_fontFamily, 15f, FontStyle.Bold);
            var input = new Font(_fontFamily, 22f, FontStyle.Bold);
            var small = new Font(_fontFamily, 11f, FontStyle.Bold);

            foreach (var lbl in _bigLabels) lbl.Font = big;
            foreach (var lbl in _smallLabels) lbl.Font = small;
            foreach (var cb in _checks) cb.Font = small;
            foreach (var row in _statRows) { row.Min.Font = input; row.Max.Font = input; }
            _effMin.Font = input;
            _effMax.Font = input;
            _fontCombo.Font = new Font(_fontFamily, 11f);
            _langCombo.Font = new Font(_fontFamily, 11f);
            _savedSearchBox.Font = new Font(_fontFamily, 12f, FontStyle.Bold);
            _savedSortCombo.Font = new Font(_fontFamily, 11f);
            _savedTagCombo.Font = new Font(_fontFamily, 11f);
            _savedImportBtn.Font = new Font(_fontFamily, 10f, FontStyle.Bold);
            _savedRefreshBtn.Font = new Font(_fontFamily, 10f, FontStyle.Bold);
            _tagSettingsBtn.Font = new Font(_fontFamily, 11f, FontStyle.Bold);
            _openFolderBtn.Font = new Font(_fontFamily, 11f, FontStyle.Bold);
            if (_saveBtn is not null) _saveBtn.Font = new Font(_fontFamily, 17f, FontStyle.Bold);
            _copyBtn.Font = new Font(_fontFamily, 14f, FontStyle.Bold);
            _clearBtn.Font = new Font(_fontFamily, 14f, FontStyle.Bold);
            _queryBox.Font = new Font("Consolas", 19f, FontStyle.Bold); // regex çıktısı sabit monospace
        }

        private void StyleRawInputs()
        {
            var t = Themes.Current;
            Color back = t.InputBack;
            Color fore = t.Fore;

            _queryBox.BackColor = back;
            _queryBox.ForeColor = t.Query;
            foreach (var row in _statRows)
            {
                row.Min.BackColor = back; row.Min.ForeColor = fore;
                row.Max.BackColor = back; row.Max.ForeColor = fore;
            }
            _effMin.BackColor = back;
            _effMin.ForeColor = fore;
            _effMax.BackColor = back;
            _effMax.ForeColor = fore;
            _fontCombo.BackColor = back;
            _fontCombo.ForeColor = fore;
            _langCombo.BackColor = back;
            _langCombo.ForeColor = fore;
            _savedSearchBox.BackColor = back;
            _savedSearchBox.ForeColor = fore;
            _savedSortCombo.BackColor = back;
            _savedSortCombo.ForeColor = fore;
            _savedTagCombo.BackColor = back;
            _savedTagCombo.ForeColor = fore;
            _savedImportBtn.BackColor = back;
            _savedImportBtn.ForeColor = fore;
            _savedImportBtn.FlatAppearance.BorderColor = Themes.Current.Accent;
            _savedRefreshBtn.BackColor = back;
            _savedRefreshBtn.ForeColor = fore;
            _savedRefreshBtn.FlatAppearance.BorderColor = Themes.Current.Accent;
            _savedFolderBtn.BackColor = back;
            _savedFolderBtn.ForeColor = fore;
            _savedFolderBtn.FlatAppearance.BorderColor = Themes.Current.Accent;
            _tagSettingsBtn.BackColor = back;
            _tagSettingsBtn.ForeColor = fore;
            _tagSettingsBtn.FlatAppearance.BorderColor = Themes.Current.Accent;
            _openFolderBtn.BackColor = back;
            _openFolderBtn.ForeColor = fore;
            _openFolderBtn.FlatAppearance.BorderColor = Themes.Current.Accent;

            foreach (var lbl in _bigLabels) { lbl.ForeColor = fore; lbl.BackColor = Color.Transparent; }
            foreach (var lbl in _smallLabels) { lbl.ForeColor = fore; lbl.BackColor = Color.Transparent; }
            foreach (var cb in _checks) StyleToggle(cb);

            if (_saveBtn is not null)
            {
                _saveBtn.BackColor = t.Accent;
                _saveBtn.ForeColor = Color.White;
            }
            _copyBtn.BackColor = t.Accent;
            _copyBtn.ForeColor = Color.White;
            _copyBtn.FlatAppearance.MouseOverBackColor = Mix(t.Accent, Color.White, 0.8);
            _copyBtn.FlatAppearance.MouseDownBackColor = Mix(t.Accent, Color.Black, 0.7);
            _clearBtn.BackColor = t.Back;
            _clearBtn.ForeColor = t.Fore;
            _clearBtn.FlatAppearance.BorderColor = t.Accent;
            _clearBtn.FlatAppearance.MouseOverBackColor = Mix(t.Accent, t.Back, 0.25);
            _clearBtn.FlatAppearance.MouseDownBackColor = Mix(t.Accent, t.Back, 0.5);
            RefreshSavedTab();
        }

        // ===== Regex kaydetme =====
        private SavedRegex CaptureState(string name)
        {
            var saved = new SavedRegex
            {
                Name = name,
                Stats = _statRows.Select(r => new StatSetting
                {
                    Enabled = r.Enabled.Checked,
                    Min = (int)r.Min.Value,
                    Max = (int)r.Max.Value,
                    Exclude = r.Exclude.Checked,
                }).ToList(),
                Effectiveness = new StatSetting { Enabled = _effEnabled.Checked, Min = (int)_effMin.Value, Max = (int)_effMax.Value, Exclude = _effExclude.Checked },
                OnlyMods = _mods.Where(m => m.Mode == ModMode.Only).Select(m => m.Pattern).ToList(),
                HiddenMods = _mods.Where(m => m.Mode == ModMode.Hide).Select(m => m.Pattern).ToList(),
                CorruptMode = _corruptMode,
            };
            saved.RebuildQuery();
            return saved;
        }

        private void SaveCurrentRegex()
        {
            var result = PromptSave($"Regex {_saved.Count + 1}");
            if (result is null) return;
            var saved = CaptureState(result.Value.Name);
            saved.Tags = result.Value.Tags;
            _saved.Add(saved);
            SavedRegexStore.Save(_saved);
            RefreshSavedTab();
            _tabs.SelectedIndex = 1;
        }

        /// <summary>İsim + etiket seçtiren kaydetme penceresi.</summary>
        private (string Name, List<string> Tags)? PromptSave(string defaultName)
        {
            bool dark = IsDark;
            var t = Themes.Current;
            Color back = t.Back;
            Color inputBack = t.InputBack;
            Color fore = t.Fore;
            Color accent = t.Accent;

            using var dlg = new Form
            {
                Text = Loc.T("SAVE REGEX", "REGEX'İ KAYDET"),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(560, 300),
                Font = new Font(_fontFamily, 11f, FontStyle.Bold),
                BackColor = back,
                ForeColor = fore,
            };
            dlg.Controls.Add(new Label { Text = Loc.T("Name:", "İsim:"), Location = new Point(20, 22), AutoSize = true, ForeColor = fore });
            var box = new TextBox { Location = new Point(110, 18), Width = 420, Text = defaultName, BackColor = inputBack, ForeColor = fore, BorderStyle = BorderStyle.FixedSingle };
            dlg.Controls.Add(box);

            // Etiketler: açılır seçici
            var selectedTags = new List<string>();
            dlg.Controls.Add(new Label { Text = "TAGS", Location = new Point(20, 68), AutoSize = true, ForeColor = fore });
            var tagsLbl = new Label { Location = new Point(110, 68), AutoSize = false, Size = new Size(280, 28), ForeColor = fore, AutoEllipsis = true, Text = Loc.T("(none)", "(yok)") };
            dlg.Controls.Add(tagsLbl);
            var tagsBtn = new Button { Text = Loc.T("SELECT ▾", "SEÇ ▾"), Location = new Point(400, 62), Size = new Size(130, 36), FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore };
            tagsBtn.FlatAppearance.BorderColor = accent;
            tagsBtn.Click += (_, _) =>
            {
                using var picker = new TagPickerDialog(_settings.Tags, selectedTags, dark, _fontFamily, _settings.Save);
                if (picker.ShowDialog(dlg) == DialogResult.OK)
                {
                    selectedTags = picker.Selected;
                    tagsLbl.Text = selectedTags.Count == 0 ? Loc.T("(none)", "(yok)") : string.Join("  ", selectedTags.Select(t => "[" + t + "]"));
                }
            };
            dlg.Controls.Add(tagsBtn);

            var ok = new Button { Text = Loc.T("SAVE", "KAYDET"), Location = new Point(400, 116), Size = new Size(130, 40), FlatStyle = FlatStyle.Flat, BackColor = accent, ForeColor = Color.White, DialogResult = DialogResult.OK };
            ok.FlatAppearance.BorderSize = 0;
            dlg.Controls.Add(ok);
            dlg.AcceptButton = ok;
            dlg.ClientSize = new Size(560, 172);
            box.SelectAll();

            if (dlg.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(box.Text)) return null;
            return (box.Text.Trim(), selectedTags);
        }

        /// <summary>Kaydedilmiş regexler sekmesini yeniden kurar.</summary>
        private void RefreshSavedTab()
        {
            if (_savedPanel is null || _suppressSavedRefresh) return;
            RebuildTagItems();
            var t = Themes.Current;
            Color rowBack = t.InputBack;
            Color fore = t.Fore;
            Color queryColor = t.Query;
            Color accent = t.Accent;
            Color badgeColor = Mix(t.Fore, t.Back, 0.7);
            Color tagColor = Mix(t.Query, t.Fore, 0.7);

            _savedPanel.SuspendLayout();
            _savedPanel.Controls.Clear();

            if (_saved.Count == 0)
            {
                _savedPanel.Controls.Add(new Label
                {
                    Text = Loc.T("No saved regexes yet. Use 💾 SAVE REGEX in the builder.", "Henüz kayıt yok. Oluşturucudan 💾 REGEX'İ KAYDET ile ekle."),
                    AutoSize = true,
                    ForeColor = fore,
                    Font = new Font(_fontFamily, 12f, FontStyle.Bold),
                });
            }

            foreach (var saved in FilteredSaved())
            {
                var row = new Panel
                {
                    Height = 132,
                    Width = Math.Max(_savedPanel.ClientSize.Width - 40, 600),
                    BackColor = rowBack,
                    Margin = new Padding(0, 0, 0, 10),
                };
                var nameLbl = new Label { Text = saved.Name, Location = new Point(14, 8), AutoSize = true, ForeColor = fore, Font = new Font(_fontFamily, 13f, FontStyle.Bold) };

                // Etiketler — ismin yanında turuncu
                var tagsLbl = new Label
                {
                    Text = saved.Tags.Count == 0 ? "" : string.Join("  ", saved.Tags.Select(t => "[" + t + "]")),
                    AutoSize = true,
                    ForeColor = tagColor,
                    Font = new Font(_fontFamily, 10f, FontStyle.Bold),
                };

                // Stat rozetleri: IR / PS / MR / DROP / EFF
                var badgeLbl = new Label
                {
                    Text = $"IR {saved.StatBadge(0)}    PS {saved.StatBadge(1)}    MR {saved.StatBadge(2)}    DROP {saved.StatBadge(3)}    EFF {saved.EffBadge()}",
                    Location = new Point(14, 40),
                    AutoSize = true,
                    ForeColor = badgeColor,
                    Font = new Font(_fontFamily, 10.5f, FontStyle.Bold),
                };

                var queryLbl = new Label { Text = saved.Query.Length == 0 ? "—" : saved.Query, Location = new Point(14, 66), AutoSize = false, Size = new Size(row.Width - 480, 26), ForeColor = queryColor, Font = new Font("Consolas", 10.5f, FontStyle.Bold), AutoEllipsis = true, Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right };

                var btnFont = new Font(_fontFamily, 10f, FontStyle.Bold);

                var copyBtn = new Button { Text = Loc.T("COPY", "KOPYALA"), Size = new Size(110, 48), Location = new Point(row.Width - 450, 26), FlatStyle = FlatStyle.Flat, BackColor = accent, ForeColor = Color.White, Anchor = AnchorStyles.Right | AnchorStyles.Top, Font = btnFont };
                copyBtn.FlatAppearance.BorderSize = 0;
                copyBtn.Click += (_, _) => { if (saved.Query.Length > 0) Clipboard.SetText(saved.Query); };

                var showBtn = new Button { Text = Loc.T("SHOW", "GÖSTER"), Size = new Size(110, 48), Location = new Point(row.Width - 332, 26), FlatStyle = FlatStyle.Flat, BackColor = rowBack, ForeColor = fore, Anchor = AnchorStyles.Right | AnchorStyles.Top, Font = btnFont };
                showBtn.FlatAppearance.BorderColor = accent;
                showBtn.Click += (_, _) => { _queryBox.Text = saved.Query; _tabs.SelectedIndex = 0; };

                var exportBtn = new Button { Text = Loc.T("EXPORT", "İNDİR"), Size = new Size(100, 48), Location = new Point(row.Width - 214, 26), FlatStyle = FlatStyle.Flat, BackColor = rowBack, ForeColor = fore, Anchor = AnchorStyles.Right | AnchorStyles.Top, Font = btnFont };
                exportBtn.FlatAppearance.BorderColor = accent;
                exportBtn.Click += (_, _) => ExportSaved(saved);

                var gearBtn = new Button { Text = "⚙", Size = new Size(48, 48), Location = new Point(row.Width - 58, 26), FlatStyle = FlatStyle.Flat, BackColor = rowBack, ForeColor = fore, Anchor = AnchorStyles.Right | AnchorStyles.Top, Font = new Font("Segoe UI", 18f) };
                gearBtn.FlatAppearance.BorderColor = accent;
                gearBtn.Click += (_, _) => OpenSavedSettings(saved);

                // Erişilebilirlik kaydırıcısı: min eşikleri %0-20 düşürür → daha çok eşleşme
                var relaxLbl = new Label
                {
                    Location = new Point(14, 102),
                    AutoSize = true,
                    ForeColor = badgeColor,
                    Font = new Font(_fontFamily, 9.5f, FontStyle.Bold),
                };
                var relaxBar = new TrackBar
                {
                    Minimum = 0,
                    Maximum = 20,
                    Value = Math.Clamp(saved.Relax, 0, 20),
                    TickFrequency = 5,
                    SmallChange = 1,
                    LargeChange = 5,
                    AutoSize = false,
                    Size = new Size(220, 28),
                    Location = new Point(150, 98),
                    BackColor = rowBack,
                };
                void UpdateRelaxLabel() => relaxLbl.Text =
                    Loc.T($"REACH -{relaxBar.Value}%", $"ERİŞİM -%{relaxBar.Value}");
                UpdateRelaxLabel();
                relaxBar.ValueChanged += (_, _) =>
                {
                    saved.Relax = relaxBar.Value;
                    saved.RebuildQuery();
                    queryLbl.Text = saved.Query.Length == 0 ? "—" : saved.Query;
                    UpdateRelaxLabel();
                };
                relaxBar.MouseUp += (_, _) => SavedRegexStore.Save(_saved);

                row.Controls.Add(nameLbl);
                row.Controls.Add(tagsLbl);
                row.Controls.Add(badgeLbl);
                row.Controls.Add(queryLbl);
                row.Controls.Add(relaxLbl);
                row.Controls.Add(relaxBar);
                row.Controls.Add(copyBtn);
                row.Controls.Add(showBtn);
                row.Controls.Add(exportBtn);
                row.Controls.Add(gearBtn);
                tagsLbl.Location = new Point(nameLbl.PreferredWidth + 26, 11);
                _savedPanel.Controls.Add(row);
            }

            _savedPanel.ResumeLayout();
        }

        // ===== Dosya paylaşımı (.wsrx) =====
        private void ExportSaved(SavedRegex saved)
        {
            using var dlg = new SaveFileDialog
            {
                Title = Loc.T("Export regex to file", "Regex'i dosya olarak kaydet"),
                FileName = string.Join("_", saved.Name.Split(Path.GetInvalidFileNameChars())) + ".wsrx",
                Filter = "Waystone Regex (*.wsrx)|*.wsrx|JSON (*.json)|*.json",
                DefaultExt = "wsrx",
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            SavedRegexStore.ExportToFile(saved, dlg.FileName);
        }

        private void ImportFromFiles()
        {
            using var dlg = new OpenFileDialog
            {
                Title = Loc.T("Load regex file", "Regex dosyası yükle"),
                Filter = "Waystone Regex (*.wsrx;*.json)|*.wsrx;*.json|All files (*.*)|*.*",
                Multiselect = true,
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            ImportFilesCore(dlg.FileNames);
        }

        private void ImportFilesCore(string[] files)
        {
            int added = 0;
            var failed = new List<string>();
            foreach (var file in files)
            {
                try
                {
                    foreach (var saved in SavedRegexStore.ImportFromFile(file))
                    {
                        saved.RebuildQuery();
                        _saved.Add(saved);
                        added++;
                    }
                }
                catch
                {
                    failed.Add(Path.GetFileName(file));
                }
            }

            if (added > 0)
            {
                SavedRegexStore.Save(_saved);
                RefreshSavedTab();
            }
            if (failed.Count > 0)
                MessageBox.Show(this,
                    Loc.T("Could not read: ", "Okunamayan dosyalar: ") + string.Join(", ", failed),
                    Loc.T("Import error", "Yükleme hatası"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void OpenSavedSettings(SavedRegex saved)
        {
            using var dlg = new SavedRegexDialog(saved, IsDark, _fontFamily, _settings.Tags, _settings.Save);
            var result = dlg.ShowDialog(this);
            if (result == DialogResult.Abort) _saved.Remove(saved);
            if (result is DialogResult.OK or DialogResult.Abort)
            {
                SavedRegexStore.Save(_saved);
                RefreshSavedTab();
            }
        }

        private void OpenModifiersDialog()
        {
            using var dlg = new ModifiersDialog(_mods, BuildQueryFromInputs, IsDark, _fontFamily);
            dlg.ShowDialog(this);
            BuildQueryFromInputs();
        }

        // ===== Sorgu üretimi =====
        private void BuildQueryFromInputs()
        {
            if (_suppressBuild) return;
            var terms = new List<string>();

            foreach (var row in _statRows)
                if (row.Enabled.Checked)
                    terms.Add(row.Exclude.Checked
                        ? RegexBuilder.NegTerm(row.Prefix)
                        : RegexBuilder.StatTerm(row.Prefix, (int)row.Min.Value, (int)row.Max.Value));

            if (_effEnabled.Checked)
                terms.Add(_effExclude.Checked
                    ? "!effectiveness"
                    : RegexBuilder.StatTerm("effectiveness: ", (int)_effMin.Value, (int)_effMax.Value));

            foreach (var mod in _mods)
            {
                if (mod.Mode == ModMode.Hide) terms.Add(RegexBuilder.NegTerm(mod.Pattern));
                else if (mod.Mode == ModMode.Only) terms.Add($"\"{mod.Pattern}\"");
            }

            if (_corruptMode == 1) terms.Add("corrupted");
            else if (_corruptMode == 2) terms.Add("!corrupted");

            _queryBox.Text = string.Join(" ", terms);
        }

        private void ClearInputs()
        {
            _suppressBuild = true;
            foreach (var row in _statRows) { row.Enabled.Checked = false; row.Exclude.Checked = false; }
            _effEnabled.Checked = false;
            _effExclude.Checked = false;
            foreach (var mod in _mods) mod.Mode = ModMode.Off;
            _corruptMode = 0;
            _corruptBtn.Text = CorruptText();
            _suppressBuild = false;
            _queryBox.Text = "";
        }

        private void Recalculate()
        {
            if (_summaryLabel is null) return;
            var terms = SearchQuery.Parse(_queryBox.Text);
            var errors = terms.Where(t => t.Error != null).ToList();

            _summaryLabel.Text = errors.Count > 0
                ? Loc.T("⚠ Invalid regex: ", "⚠ Hatalı regex: ") + string.Join("; ", errors.Select(e => $"'{e.Pattern}' → {e.Error}"))
                : terms.Count == 0
                    ? Loc.T("Pick filters or type a query", "Filtre seç veya sorgu yaz")
                    : $"{terms.Count} {Loc.T("terms", "terim")}  •  {_queryBox.Text.Length} {Loc.T("chars", "karakter")}"
                      + (_queryBox.Text.Length > 50 ? Loc.T("  ⚠ exceeds the in-game 50 char limit!", "  ⚠ oyun içi 50 karakter sınırını aşıyor!") : "");

            var active = _mods.Where(m => m.Mode != ModMode.Off)
                              .Select(m => $"{m.Label}: {(m.Mode == ModMode.Hide ? Loc.T("HIDE", "GİZLE") : Loc.T("SHOW", "GÖSTER"))}")
                              .ToList();
            _modSummaryLabel.Text = active.Count == 0
                ? Loc.T("No active modifier filters", "Aktif modifier filtresi yok")
                : Loc.T("Active modifier filters:  ", "Aktif modifier filtreleri:  ") + string.Join("   |   ", active);
        }
    }
}
