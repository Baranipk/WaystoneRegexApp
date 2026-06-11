namespace WaystoneRegex
{
    /// <summary>
    /// Waystone modifier filtreleri: yalnız GÖSTER (sadece bu modlular eşleşsin) seçeneği.
    /// Bilinçli olarak standart Form: MaterialForm pencere konumunu bozuyordu.
    /// </summary>
    public class ModifiersDialog : Form
    {
        private const int RowHeight = 40;
        private const int LabelWidth = 330;
        private const int CheckColWidth = 120;

        public ModifiersDialog(List<ModifierState> mods, Action onChanged, bool dark, string fontFamily = "Segoe UI")
        {
            Text = "WAYSTONE MODIFIERS";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font(fontFamily, 12f, FontStyle.Bold);

            var theme = Themes.Current;
            Color back = theme.Back;
            Color fore = theme.Fore;
            Color accent = theme.Accent;
            BackColor = back;
            ForeColor = fore;

            int tableWidth = LabelWidth + CheckColWidth * 2;
            ClientSize = new Size(tableWidth + 48, RowHeight * (mods.Count + 1) + 90);

            var table = new TableLayoutPanel
            {
                Location = new Point(24, 14),
                Size = new Size(tableWidth, RowHeight * (mods.Count + 1)),
                ColumnCount = 3,
                RowCount = mods.Count + 1,
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, CheckColWidth));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, CheckColWidth));
            for (int i = 0; i <= mods.Count; i++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));

            var headFont = new Font(fontFamily, 12f, FontStyle.Bold);
            table.Controls.Add(new Label { Text = "MODIFIER", Font = headFont, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = fore }, 0, 0);
            table.Controls.Add(new Label { Text = Loc.T("HIDE (!)", "GİZLE (!)"), Font = headFont, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = fore }, 1, 0);
            table.Controls.Add(new Label { Text = Loc.T("SHOW", "GÖSTER"), Font = headFont, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = fore }, 2, 0);

            var checks = new List<CheckBox>();
            int row = 1;
            foreach (var mod in mods)
            {
                var lbl = new Label { Text = mod.Label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = fore };
                var hide = new CheckBox { Checked = mod.Mode == ModMode.Hide, Anchor = AnchorStyles.None, AutoSize = true };
                var only = new CheckBox { Checked = mod.Mode == ModMode.Only, Anchor = AnchorStyles.None, AutoSize = true };
                hide.CheckedChanged += (_, _) =>
                {
                    if (hide.Checked) only.Checked = false;
                    mod.Mode = hide.Checked ? ModMode.Hide : (only.Checked ? ModMode.Only : ModMode.Off);
                    onChanged();
                };
                only.CheckedChanged += (_, _) =>
                {
                    if (only.Checked) hide.Checked = false;
                    mod.Mode = only.Checked ? ModMode.Only : (hide.Checked ? ModMode.Hide : ModMode.Off);
                    onChanged();
                };
                table.Controls.Add(lbl, 0, row);
                table.Controls.Add(hide, 1, row);
                table.Controls.Add(only, 2, row);
                checks.Add(hide);
                checks.Add(only);
                row++;
            }

            var resetBtn = new Button
            {
                Text = Loc.T("RESET", "SIFIRLA"),
                Size = new Size(130, 38),
                Location = new Point(24, table.Bottom + 12),
                FlatStyle = FlatStyle.Flat,
                ForeColor = fore,
                BackColor = back,
            };
            resetBtn.FlatAppearance.BorderColor = accent;
            resetBtn.Click += (_, _) => { foreach (var c in checks) c.Checked = false; };

            var okBtn = new Button
            {
                Text = Loc.T("OK", "TAMAM"),
                Size = new Size(130, 38),
                Location = new Point(ClientSize.Width - 154, table.Bottom + 12),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = accent,
                DialogResult = DialogResult.OK,
            };
            okBtn.FlatAppearance.BorderSize = 0;

            Controls.Add(table);
            Controls.Add(resetBtn);
            Controls.Add(okBtn);
            AcceptButton = okBtn;
        }
    }
}
