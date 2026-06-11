namespace WaystoneRegex
{
    /// <summary>
    /// Kaydedilmiş bir regex'in tüm değerlerini düzenleme penceresi.
    /// DialogResult.OK = kaydet, DialogResult.Abort = kaydı sil.
    /// </summary>
    public class SavedRegexDialog : Form
    {
        private readonly SavedRegex _saved;
        private readonly TextBox _nameBox;
        private readonly List<(CheckBox On, NumericUpDown Min, NumericUpDown Max, CheckBox Excl)> _statInputs = new();
        private readonly (CheckBox On, NumericUpDown Min, NumericUpDown Max, CheckBox Excl) _effInputs;
        private readonly List<(CheckBox Hide, CheckBox Only, string Pattern)> _modInputs = new();
        private readonly ComboBox _corruptCombo;
        private List<string> _tags;

        public SavedRegexDialog(SavedRegex saved, bool dark, string fontFamily, List<string> availableTags, Action saveTags)
        {
            _saved = saved;
            _tags = new List<string>(saved.Tags);

            Text = Loc.T("REGEX SETTINGS — ", "REGEX AYARLARI — ") + saved.Name;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font(fontFamily, 11f, FontStyle.Bold);

            var theme = Themes.Current;
            Color back = theme.Back;
            Color inputBack = theme.InputBack;
            Color fore = theme.Fore;
            Color accent = theme.Accent;
            BackColor = back;
            ForeColor = fore;

            var mods = ModifierState.Defaults();
            int width = 640;
            int y = 16;

            // ===== İsim =====
            Controls.Add(new Label { Text = Loc.T("NAME", "İSİM"), Location = new Point(24, y + 4), AutoSize = true, ForeColor = fore });
            _nameBox = new TextBox { Location = new Point(140, y), Width = width - 180, Text = saved.Name, BackColor = inputBack, ForeColor = fore, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(_nameBox);
            y += 44;

            // MIN / MAX başlıkları
            Controls.Add(new Label { Text = "MIN", Location = new Point(370, y), AutoSize = true, ForeColor = fore });
            Controls.Add(new Label { Text = "MAX (0=∞)", Location = new Point(458, y), AutoSize = true, ForeColor = fore });
            y += 28;

            // ===== Stat satırları =====
            for (int i = 0; i < WaystoneStats.Items.Length; i++)
            {
                var (label, _, def) = WaystoneStats.Items[i];
                var s = i < saved.Stats.Count ? saved.Stats[i] : new StatSetting { Min = def };
                y = AddStatRow(label, s, fore, inputBack, accent, y, out var inputs, allowExclude: label != "WAYSTONE DROP CHANCE");
                _statInputs.Add(inputs);
            }
            y = AddStatRow("EFFECTIVENESS", saved.Effectiveness, fore, inputBack, accent, y, out _effInputs, allowExclude: true);

            // ===== Modifiers (GİZLE / GÖSTER) =====
            y += 8;
            Controls.Add(new Label { Text = "MODIFIERS", Location = new Point(24, y + 2), AutoSize = true, ForeColor = fore });
            Controls.Add(new Label { Text = Loc.T("HIDE (!)", "GİZLE (!)"), Location = new Point(360, y + 2), AutoSize = true, ForeColor = fore });
            Controls.Add(new Label { Text = Loc.T("SHOW", "GÖSTER"), Location = new Point(480, y + 2), AutoSize = true, ForeColor = fore });
            y += 32;
            foreach (var mod in mods)
            {
                Controls.Add(new Label { Text = mod.Label, AutoSize = true, Location = new Point(24, y + 2), ForeColor = fore, Font = new Font(fontFamily, 10f, FontStyle.Bold) });
                var hide = new CheckBox { AutoSize = true, Location = new Point(380, y), Checked = saved.HiddenMods.Contains(mod.Pattern) };
                var only = new CheckBox { AutoSize = true, Location = new Point(495, y), Checked = saved.OnlyMods.Contains(mod.Pattern) };
                hide.CheckedChanged += (_, _) => { if (hide.Checked) only.Checked = false; };
                only.CheckedChanged += (_, _) => { if (only.Checked) hide.Checked = false; };
                Controls.Add(hide);
                Controls.Add(only);
                _modInputs.Add((hide, only, mod.Pattern));
                y += 30;
            }

            // ===== Etiketler: açılır seçici =====
            y += 8;
            Controls.Add(new Label { Text = "TAGS", Location = new Point(24, y + 8), AutoSize = true, ForeColor = fore });
            var tagsLbl = new Label
            {
                Location = new Point(140, y + 8),
                AutoSize = false,
                Size = new Size(330, 28),
                ForeColor = fore,
                AutoEllipsis = true,
                Text = TagsText(),
            };
            Controls.Add(tagsLbl);
            var tagsBtn = new Button { Text = Loc.T("SELECT ▾", "SEÇ ▾"), Location = new Point(width - 150, y), Size = new Size(130, 36), FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore };
            tagsBtn.FlatAppearance.BorderColor = accent;
            tagsBtn.Click += (_, _) =>
            {
                using var picker = new TagPickerDialog(availableTags, _tags, dark, fontFamily, saveTags);
                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    _tags = picker.Selected;
                    tagsLbl.Text = TagsText();
                }
            };
            Controls.Add(tagsBtn);
            y += 46;

            // ===== Corrupt =====
            y += 8;
            Controls.Add(new Label { Text = "CORRUPT", Location = new Point(24, y + 4), AutoSize = true, ForeColor = fore });
            _corruptCombo = new ComboBox { Location = new Point(140, y), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = inputBack, ForeColor = fore, FlatStyle = FlatStyle.Flat };
            _corruptCombo.Items.AddRange(new object[]
            {
                Loc.T("OFF", "KAPALI"),
                Loc.T("ON (corrupted only)", "AÇIK (sadece corrupt)"),
                Loc.T("HIDE (!)", "GİZLE (!)"),
            });
            _corruptCombo.SelectedIndex = Math.Clamp(saved.CorruptMode, 0, 2);
            Controls.Add(_corruptCombo);
            y += 52;

            // ===== Butonlar =====
            var saveBtn = MakeButton(Loc.T("SAVE", "KAYDET"), accent, Color.White, new Point(width - 150, y));
            saveBtn.Click += (_, _) => { ApplyToSaved(); DialogResult = DialogResult.OK; };
            var delBtn = MakeButton(Loc.T("DELETE", "SİL"), back, fore, new Point(24, y));
            delBtn.FlatAppearance.BorderColor = accent;
            delBtn.Click += (_, _) =>
            {
                if (MessageBox.Show(this,
                        Loc.T($"Delete '{saved.Name}'?", $"'{saved.Name}' silinsin mi?"),
                        Loc.T("Delete", "Sil"), MessageBoxButtons.YesNo, MessageBoxIcon.None) == DialogResult.Yes)
                    DialogResult = DialogResult.Abort;
            };
            var cancelBtn = MakeButton(Loc.T("CANCEL", "İPTAL"), back, fore, new Point(width - 290, y));
            cancelBtn.FlatAppearance.BorderColor = dark ? Color.FromArgb(120, 90, 60) : Color.FromArgb(150, 120, 90);
            cancelBtn.DialogResult = DialogResult.Cancel;
            Controls.Add(saveBtn);
            Controls.Add(delBtn);
            Controls.Add(cancelBtn);
            AcceptButton = saveBtn;
            CancelButton = cancelBtn;

            ClientSize = new Size(width + 24, y + 60);
        }

        private int AddStatRow(string label, StatSetting s, Color fore, Color inputBack, Color accent, int y,
            out (CheckBox On, NumericUpDown Min, NumericUpDown Max, CheckBox Excl) inputs, bool allowExclude)
        {
            Controls.Add(new Label { Text = label, Location = new Point(24, y + 6), AutoSize = true, ForeColor = fore });
            var on = new CheckBox { Text = Loc.T("ACTIVE", "AKTİF"), AutoSize = true, Location = new Point(280, y + 4), ForeColor = fore, Checked = s.Enabled };
            var num = new NumericUpDown { Location = new Point(370, y), Width = 80, Minimum = 0, Maximum = 200, Value = Math.Clamp(s.Min, 0, 200), BackColor = inputBack, ForeColor = fore, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            var max = new NumericUpDown { Location = new Point(458, y), Width = 80, Minimum = 0, Maximum = 199, Value = Math.Clamp(s.Max, 0, 199), BackColor = inputBack, ForeColor = fore, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            var excl = new CheckBox { Text = Loc.T("EXCL (!)", "HARİÇ (!)"), AutoSize = true, Location = new Point(550, y + 4), ForeColor = fore, Checked = s.Exclude, Visible = allowExclude };
            excl.CheckedChanged += (_, _) => { num.Enabled = !excl.Checked; max.Enabled = !excl.Checked; };
            num.Enabled = !s.Exclude;
            max.Enabled = !s.Exclude;
            Controls.Add(on);
            Controls.Add(num);
            Controls.Add(max);
            Controls.Add(excl);
            inputs = (on, num, max, excl);
            return y + 40;
        }

        private static Button MakeButton(string text, Color back, Color fore, Point loc) => new()
        {
            Text = text,
            Size = new Size(130, 40),
            Location = loc,
            FlatStyle = FlatStyle.Flat,
            BackColor = back,
            ForeColor = fore,
        };

        private void ApplyToSaved()
        {
            _saved.Name = string.IsNullOrWhiteSpace(_nameBox.Text) ? _saved.Name : _nameBox.Text.Trim();
            _saved.Stats = _statInputs
                .Select(i => new StatSetting { Enabled = i.On.Checked, Min = (int)i.Min.Value, Max = (int)i.Max.Value, Exclude = i.Excl.Checked && i.Excl.Visible })
                .ToList();
            _saved.Effectiveness = new StatSetting { Enabled = _effInputs.On.Checked, Min = (int)_effInputs.Min.Value, Max = (int)_effInputs.Max.Value, Exclude = _effInputs.Excl.Checked };
            _saved.OnlyMods = _modInputs.Where(m => m.Only.Checked).Select(m => m.Pattern).ToList();
            _saved.HiddenMods = _modInputs.Where(m => m.Hide.Checked).Select(m => m.Pattern).ToList();
            _saved.CorruptMode = _corruptCombo.SelectedIndex;
            _saved.Tags = _tags;
            _saved.RebuildQuery();
        }

        private string TagsText()
            => _tags.Count == 0 ? Loc.T("(none)", "(yok)") : string.Join("  ", _tags.Select(t => "[" + t + "]"));
    }
}
