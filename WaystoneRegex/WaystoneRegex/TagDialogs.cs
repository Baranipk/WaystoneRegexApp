namespace WaystoneRegex
{
    /// <summary>Etiket seçme penceresi: mevcut etiketlerden tikle, istersen yeni ekle.</summary>
    public class TagPickerDialog : Form
    {
        public List<string> Selected { get; private set; } = new();

        private readonly List<string> _available;
        private readonly Action _saveAvailable;
        private readonly FlowLayoutPanel _listPanel;
        private readonly HashSet<string> _checked;
        private readonly Color _fore, _inputBack, _accent;

        public TagPickerDialog(List<string> available, IEnumerable<string> selected, bool dark, string fontFamily, Action saveAvailable)
        {
            _available = available;
            _saveAvailable = saveAvailable;
            _checked = new HashSet<string>(selected);

            Text = "TAGS";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font(fontFamily, 11f, FontStyle.Bold);

            Color back = Themes.Current.Back;
            _inputBack = Themes.Current.InputBack;
            _fore = Themes.Current.Fore;
            _accent = Themes.Current.Accent;
            BackColor = back;
            ForeColor = _fore;
            ClientSize = new Size(360, 420);

            _listPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 16),
                Size = new Size(320, 280),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
            };
            Controls.Add(_listPanel);

            var addBox = new TextBox { Location = new Point(20, 308), Width = 200, BackColor = _inputBack, ForeColor = _fore, BorderStyle = BorderStyle.FixedSingle };
            var addBtn = new Button { Text = Loc.T("ADD", "EKLE"), Location = new Point(230, 304), Size = new Size(110, 34), FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = _fore };
            addBtn.FlatAppearance.BorderColor = _accent;
            addBtn.Click += (_, _) =>
            {
                string tag = addBox.Text.Trim().ToUpperInvariant();
                if (tag.Length == 0) return;
                if (!_available.Contains(tag)) { _available.Add(tag); _saveAvailable(); }
                _checked.Add(tag);
                addBox.Text = "";
                BuildList();
            };
            Controls.Add(addBox);
            Controls.Add(addBtn);

            var okBtn = new Button { Text = Loc.T("OK", "TAMAM"), Location = new Point(210, 356), Size = new Size(130, 40), FlatStyle = FlatStyle.Flat, BackColor = _accent, ForeColor = Color.White, DialogResult = DialogResult.OK };
            okBtn.FlatAppearance.BorderSize = 0;
            okBtn.Click += (_, _) => Selected = _checked.ToList();
            Controls.Add(okBtn);
            AcceptButton = okBtn;

            BuildList();
        }

        private void BuildList()
        {
            _listPanel.SuspendLayout();
            _listPanel.Controls.Clear();
            // Mevcut etiketler + (silinmiş olsa bile) seçili etiketler
            foreach (var tag in _available.Concat(_checked).Distinct().OrderBy(t => t))
            {
                var cb = new CheckBox { Text = tag, AutoSize = true, ForeColor = _fore, Checked = _checked.Contains(tag), Margin = new Padding(4, 4, 0, 4) };
                string captured = tag;
                cb.CheckedChanged += (_, _) => { if (cb.Checked) _checked.Add(captured); else _checked.Remove(captured); };
                _listPanel.Controls.Add(cb);
            }
            _listPanel.ResumeLayout();
        }
    }

    /// <summary>Etiket yönetimi: mevcut etiketleri sil, yenisini ekle.</summary>
    public class TagManagerDialog : Form
    {
        private readonly List<string> _available;
        private readonly Action _saveAvailable;
        private readonly FlowLayoutPanel _listPanel;
        private readonly Color _fore, _inputBack, _accent, _back;

        public TagManagerDialog(List<string> available, bool dark, string fontFamily, Action saveAvailable)
        {
            _available = available;
            _saveAvailable = saveAvailable;

            Text = "TAG SETTINGS";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font(fontFamily, 11f, FontStyle.Bold);

            _back = Themes.Current.Back;
            _inputBack = Themes.Current.InputBack;
            _fore = Themes.Current.Fore;
            _accent = Themes.Current.Accent;
            BackColor = _back;
            ForeColor = _fore;
            ClientSize = new Size(380, 440);

            _listPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 16),
                Size = new Size(340, 300),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
            };
            Controls.Add(_listPanel);

            var addBox = new TextBox { Location = new Point(20, 328), Width = 220, BackColor = _inputBack, ForeColor = _fore, BorderStyle = BorderStyle.FixedSingle };
            var addBtn = new Button { Text = Loc.T("ADD", "EKLE"), Location = new Point(250, 324), Size = new Size(110, 34), FlatStyle = FlatStyle.Flat, BackColor = _back, ForeColor = _fore };
            addBtn.FlatAppearance.BorderColor = _accent;
            addBtn.Click += (_, _) =>
            {
                string tag = addBox.Text.Trim().ToUpperInvariant();
                if (tag.Length == 0 || _available.Contains(tag)) return;
                _available.Add(tag);
                _saveAvailable();
                addBox.Text = "";
                BuildList();
            };
            Controls.Add(addBox);
            Controls.Add(addBtn);

            var okBtn = new Button { Text = Loc.T("OK", "TAMAM"), Location = new Point(230, 376), Size = new Size(130, 40), FlatStyle = FlatStyle.Flat, BackColor = _accent, ForeColor = Color.White, DialogResult = DialogResult.OK };
            okBtn.FlatAppearance.BorderSize = 0;
            Controls.Add(okBtn);
            AcceptButton = okBtn;

            BuildList();
        }

        private void BuildList()
        {
            _listPanel.SuspendLayout();
            _listPanel.Controls.Clear();
            foreach (var tag in _available.OrderBy(t => t).ToList())
            {
                var row = new Panel { Size = new Size(310, 36), Margin = new Padding(0, 2, 0, 2) };
                row.Controls.Add(new Label { Text = tag, AutoSize = true, Location = new Point(4, 8), ForeColor = _fore });
                var delBtn = new Button { Text = "✕", Size = new Size(32, 30), Location = new Point(274, 2), FlatStyle = FlatStyle.Flat, BackColor = _back, ForeColor = _fore };
                delBtn.FlatAppearance.BorderColor = _accent;
                string captured = tag;
                delBtn.Click += (_, _) =>
                {
                    _available.Remove(captured);
                    _saveAvailable();
                    BuildList();
                };
                row.Controls.Add(delBtn);
                _listPanel.Controls.Add(row);
            }
            _listPanel.ResumeLayout();
        }
    }
}
