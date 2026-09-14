using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NextGenPOS.Helpers;
using NextGenPOS.Services;

namespace NextGenPOS.Forms
{
    /// <summary>
    /// Settings editor: lets user edit restaurant name, tax rate, staff, currency, and test mode.
    /// Saves back to settings.txt.
    /// </summary>
    public class SettingsForm : UserControl
    {
        private readonly ConfigService _config;

        private TextBox _txtRestaurantName, _txtTaxRate, _txtDefaultStaff, _txtCurrency, _txtDbPath;
        private CheckBox _chkTestMode;
        private Label _lblStatus;

        public SettingsForm(ConfigService config)
        {
            _config = config;
            BackColor = ThemeManager.BgDark;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Top bar
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(20, 18, 50),
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblTitle = new Label
            {
                Text = "⚙  Settings",
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.AccentLight,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            var btnSave = new Button
            {
                Text = "💾  Save Settings",
                Font = ThemeManager.FontMediumB,
                ForeColor = Color.White,
                BackColor = ThemeManager.Success,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Width = 160,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = ThemeManager.SuccessDark;
            btnSave.Click += BtnSave_Click;

            topBar.Controls.AddRange(new Control[] { lblTitle, btnSave });

            // Scrollable body
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.BgDark,
                AutoScroll = true,
                Padding = new Padding(40, 24, 40, 24)
            };

            int y = 24;
            const int LabelW = 200;
            const int FieldW = 360;
            const int RowH = 56;

            // Section: Restaurant
            y = AddSectionHeader(body, "🏪 ข้อมูลร้าน", y);
            _txtRestaurantName = AddField(body, "ชื่อร้าน", "RestaurantName", LabelW, FieldW, ref y, RowH);

            // Section: Financials
            y = AddSectionHeader(body, "💰 การเงิน", y);
            _txtTaxRate = AddField(body, "อัตรา VAT (%)", "TaxRate", LabelW, FieldW, ref y, RowH, "7");
            _txtCurrency = AddField(body, "สกุลเงิน", "Currency", LabelW, FieldW, ref y, RowH, "THB");

            // Section: Staff
            y = AddSectionHeader(body, "👤 พนักงาน", y);
            _txtDefaultStaff = AddField(body, "ชื่อพนักงาน Default", "DefaultStaff", LabelW, FieldW, ref y, RowH, "Cashier");

            // Section: System
            y = AddSectionHeader(body, "🖥 ระบบ", y);

            // TestMode toggle
            var lblTestMode = new Label
            {
                Text = "Test Mode (ใช้ข้อมูล Mock)",
                Font = ThemeManager.FontMedium,
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(0, y + 8),
                AutoSize = true
            };
            _chkTestMode = new CheckBox
            {
                Text = "",
                Checked = _config.GetBool("TestMode", true),
                Location = new Point(LabelW + 8, y + 8),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Size = new Size(24, 24)
            };
            body.Controls.AddRange(new Control[] { lblTestMode, _chkTestMode });
            y += RowH;

            _txtDbPath = AddField(body, "DB Path (Access)", "DbPath", LabelW, FieldW, ref y, RowH, @"Database\NextGenPOS.accdb");
            _txtDbPath.ForeColor = ThemeManager.TextMuted;

            // Status label
            _lblStatus = new Label
            {
                Text = "",
                Font = ThemeManager.FontMedium,
                ForeColor = ThemeManager.Success,
                Location = new Point(0, y + 10),
                AutoSize = true
            };
            body.Controls.Add(_lblStatus);

            // Note
            var note = new Label
            {
                Text = "⚠ การเปลี่ยนแปลงจะมีผลหลังจากเปิดโปรแกรมใหม่",
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.Warning,
                Location = new Point(0, y + 36),
                AutoSize = true
            };
            body.Controls.Add(note);

            Controls.Add(body);
            Controls.Add(topBar);
        }

        private int AddSectionHeader(Panel parent, string text, int y)
        {
            var sep = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(700, 1),
                BackColor = ThemeManager.Border
            };
            var lbl = new Label
            {
                Text = text,
                Font = ThemeManager.FontMediumB,
                ForeColor = ThemeManager.AccentLight,
                Location = new Point(0, y + 8),
                AutoSize = true
            };
            parent.Controls.AddRange(new System.Windows.Forms.Control[] { sep, lbl });
            return y + 40;
        }

        private TextBox AddField(Panel parent, string labelText, string configKey, int labelW, int fieldW, ref int y, int rowH, string defaultVal = "")
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = ThemeManager.FontMedium,
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(0, y + 8),
                AutoSize = true
            };

            var txt = new TextBox
            {
                Text = _config.Get(configKey, defaultVal),
                Font = ThemeManager.FontMedium,
                BackColor = ThemeManager.BgCard,
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(labelW + 8, y + 4),
                Size = new Size(fieldW, 34),
                BorderStyle = BorderStyle.FixedSingle
            };

            parent.Controls.AddRange(new System.Windows.Forms.Control[] { lbl, txt });
            y += rowH;
            return txt;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Build settings content
                var lines = new System.Text.StringBuilder();
                lines.AppendLine($"RestaurantName={_txtRestaurantName.Text.Trim()}");
                lines.AppendLine($"TaxRate={_txtTaxRate.Text.Trim()}");
                lines.AppendLine($"Currency={_txtCurrency.Text.Trim()}");
                lines.AppendLine($"DefaultStaff={_txtDefaultStaff.Text.Trim()}");
                lines.AppendLine($"TestMode={(_chkTestMode.Checked ? "true" : "false")}");
                lines.AppendLine($"DbPath={_txtDbPath.Text.Trim()}");

                string settingsPath = "settings.txt";
                // Try to find settings.txt relative to exe
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(exeDir, settingsPath);

                File.WriteAllText(fullPath, lines.ToString(), System.Text.Encoding.UTF8);

                _lblStatus.Text = $"✅ บันทึกแล้ว {DateTime.Now:HH:mm:ss}";
                _lblStatus.ForeColor = ThemeManager.Success;
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"❌ Error: {ex.Message}";
                _lblStatus.ForeColor = ThemeManager.Danger;
            }
        }
    }
}
