using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NextGenPOS.Data;
using NextGenPOS.Helpers;
using NextGenPOS.Services;

namespace NextGenPOS.Forms
{
    /// <summary>
    /// Main navigation shell with gradient header and polished sidebar.
    /// </summary>
    public class MainForm : Form
    {
        private readonly ConfigService _config;
        private readonly ITableRepository _tableRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentRepository _paymentRepo;

        private Panel _sidebar;
        private Panel _contentArea;
        private GradientHeaderPanel _header;
        private Label _lblTitle, _lblClock, _lblDate;
        private Timer _clockTimer;
        private Button _activeNavBtn;

        public MainForm(ConfigService config, ITableRepository tableRepo,
            IMenuRepository menuRepo, IOrderRepository orderRepo, IPaymentRepository paymentRepo)
        {
            _config = config;
            _tableRepo = tableRepo;
            _menuRepo = menuRepo;
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;

            InitializeUI();
            ShowPage("Tables");
        }

        private void InitializeUI()
        {
            Text = _config.Get("RestaurantName", "NextGen POS");
            Size = new Size(1366, 860);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            ThemeManager.ApplyTo(this);

            // ── Header ──
            _header = new GradientHeaderPanel
            {
                Dock = DockStyle.Top,
                Height = 64,
                Padding = new Padding(16, 0, 20, 0)
            };

            // Logo icon
            var lblIcon = new Label
            {
                Text = "🍽",
                Font = new Font("Segoe UI Emoji", 22F),
                ForeColor = ThemeManager.AccentLight,
                AutoSize = true,
                Location = new Point(16, 12)
            };

            _lblTitle = new Label
            {
                Text = _config.Get("RestaurantName", "NextGen POS"),
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.TextPrimary,
                AutoSize = true,
                Location = new Point(56, 19)
            };

            bool testMode = _config.GetBool("TestMode");
            var lblMode = new Label
            {
                Text = testMode ? "⚡ TEST MODE" : "● LIVE",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = testMode ? ThemeManager.Warning : ThemeManager.Success,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblMode.Location = new Point(Width - 260, 23);

            _lblDate = new Label
            {
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.TextMuted,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight
            };
            _lblDate.Location = new Point(Width - 155, 14);

            _lblClock = new Label
            {
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeManager.AccentLight,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight
            };
            _lblClock.Location = new Point(Width - 155, 30);

            _header.Controls.AddRange(new Control[] { lblIcon, _lblTitle, lblMode, _lblDate, _lblClock });
            UpdateClock();

            _clockTimer = new Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) => UpdateClock();
            _clockTimer.Start();

            // ── Sidebar ──
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = ThemeManager.BgPanel,
                Padding = new Padding(0, 8, 0, 8)
            };

            // Sidebar top branding strip
            var sidebarTop = new Panel
            {
                Height = 56,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(35, 20, 80)
            };
            var sidebarLbl = new Label
            {
                Text = "NAVIGATION",
                Font = ThemeManager.FontCaption,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(16, 0, 0, 8)
            };
            sidebarTop.Controls.Add(sidebarLbl);
            _sidebar.Controls.Add(sidebarTop);

            // Nav items
            var navItems = new[] { "🪑  Tables", "📋  Orders", "🍳  Kitchen", "💳  Payment", "📊  Reports", "⚙  Settings" };
            var navKeys  = new[] { "Tables", "Orders", "Kitchen", "Payment", "Reports", "Settings" };
            int yPos = 64;
            for (int i = 0; i < navItems.Length; i++)
            {
                var btn = CreateNavButton(navItems[i], navKeys[i], yPos);
                _sidebar.Controls.Add(btn);
                yPos += 54;
            }

            // Sidebar separator
            var sep = new Panel
            {
                Height = 1,
                BackColor = ThemeManager.Border,
                Location = new Point(12, yPos + 6),
                Width = 176
            };
            _sidebar.Controls.Add(sep);

            // ── Content area ──
            _contentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.BgDark,
                Padding = new Padding(0)
            };

            Controls.Add(_contentArea);
            Controls.Add(_sidebar);
            Controls.Add(_header);
        }

        private Button CreateNavButton(string text, string key, int y)
        {
            var btn = new Button
            {
                Text = text,
                Tag = key,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.FontButton,
                ForeColor = ThemeManager.TextMuted,
                BackColor = Color.Transparent,
                Size = new Size(196, 48),
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, ThemeManager.AccentLight);
            btn.Click += NavButton_Click;
            return btn;
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            ShowPage(btn.Tag.ToString());
        }

        private void ShowPage(string page)
        {
            _contentArea.Controls.Clear();

            Control child = null;
            switch (page)
            {
                case "Tables":
                    child = new TableLayoutForm(_tableRepo, _menuRepo, _orderRepo, _paymentRepo, _config);
                    break;
                case "Orders":
                    child = new OrdersForm(_orderRepo, _menuRepo, _tableRepo, _config);
                    break;
                case "Kitchen":
                    child = new KitchenForm(_orderRepo, _tableRepo, _config);
                    break;
                case "Payment":
                    child = MakePlaceholder("💳 Payment — กรุณาเลือกโต๊ะก่อนนะครับ", "ไปที่ Tables แล้วคลิกโต๊ะที่ต้องการ");
                    break;
                case "Reports":
                    child = new ReportsForm(_orderRepo, _menuRepo, _tableRepo, _config);
                    break;
                case "Settings":
                    child = new SettingsForm(_config);
                    break;
            }

            if (child != null)
            {
                child.Dock = DockStyle.Fill;
                _contentArea.Controls.Add(child);
            }

            // Highlight active nav
            foreach (Control c in _sidebar.Controls)
            {
                if (c is Button b && b.Tag != null)
                {
                    bool active = b.Tag.ToString() == page;
                    b.BackColor = active ? ThemeManager.SidebarActive : Color.Transparent;
                    b.ForeColor = active ? ThemeManager.TextPrimary : ThemeManager.TextMuted;
                    b.Font = active ? ThemeManager.FontMediumB : ThemeManager.FontButton;
                    if (active) _activeNavBtn = b;
                }
            }
        }

        private Panel MakePlaceholder(string title, string sub = "")
        {
            var p = new Panel { BackColor = ThemeManager.BgDark };
            var wrapper = new Panel
            {
                Width = 400, Height = 200,
                BackColor = ThemeManager.BgCard,
                Anchor = AnchorStyles.None
            };
            using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, 400, 200), 16)) { }
            var lbl = new Label
            {
                Text = title,
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            var sub2 = new Label
            {
                Text = sub,
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.TopCenter
            };
            wrapper.Controls.AddRange(new Control[] { lbl, sub2 });
            p.Controls.Add(new Label
            {
                Text = title + (sub.Length > 0 ? "\n\n" + sub : ""),
                Font = ThemeManager.FontLarge,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            return p;
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            _lblClock.Text = now.ToString("HH:mm:ss");
            _lblDate.Text = now.ToString("ddd, dd MMM yyyy");
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _clockTimer?.Stop();
            base.OnFormClosed(e);
        }
    }

    // ── Custom gradient header panel ─────────────────────────────────────
    internal class GradientHeaderPanel : Panel
    {
        public GradientHeaderPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var rect = new Rectangle(0, 0, Width, Height);
            ThemeManager.FillGradientH(e.Graphics, rect,
                ThemeManager.BgHeader,
                Color.FromArgb(30, 15, 70));
            // Bottom border
            using (var pen = new Pen(ThemeManager.AccentViolet, 2))
                e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }
    }
}

