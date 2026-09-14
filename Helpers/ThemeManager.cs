using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NextGenPOS.Helpers
{
    /// <summary>
    /// Centralized dark-violet theme palette for all forms.
    /// </summary>
    public static class ThemeManager
    {
        // ── Core palette ──────────────────────────────────────────────────────
        public static readonly Color BgDark        = Color.FromArgb(18, 18, 32);
        public static readonly Color BgPanel       = Color.FromArgb(28, 28, 48);
        public static readonly Color BgCard        = Color.FromArgb(40, 38, 66);
        public static readonly Color BgCardHover   = Color.FromArgb(58, 48, 95);
        public static readonly Color BgHeader      = Color.FromArgb(20, 18, 40);
        public static readonly Color AccentViolet  = Color.FromArgb(124, 58, 237);
        public static readonly Color AccentHover   = Color.FromArgb(109, 40, 217);
        public static readonly Color AccentLight   = Color.FromArgb(167, 139, 250);
        public static readonly Color TextPrimary   = Color.FromArgb(236, 233, 254);
        public static readonly Color TextSecondary = Color.FromArgb(196, 181, 253);
        public static readonly Color TextMuted     = Color.FromArgb(148, 148, 180);
        public static readonly Color Success       = Color.FromArgb(34, 197, 94);
        public static readonly Color SuccessDark   = Color.FromArgb(22, 163, 74);
        public static readonly Color Danger        = Color.FromArgb(239, 68, 68);
        public static readonly Color DangerDark    = Color.FromArgb(185, 28, 28);
        public static readonly Color Warning       = Color.FromArgb(251, 191, 36);
        public static readonly Color InfoBlue      = Color.FromArgb(59, 130, 246);
        public static readonly Color Border        = Color.FromArgb(60, 55, 100);
        public static readonly Color SidebarActive = Color.FromArgb(48, 28, 100);

        // Table status colors
        public static Color TableAvailable => Success;
        public static Color TableOccupied  => Danger;
        public static Color TableReserved  => Warning;

        // ── Typography ────────────────────────────────────────────────────────
        public static readonly Font FontHero    = new Font("Segoe UI", 22F, FontStyle.Bold);
        public static readonly Font FontTitle   = new Font("Segoe UI", 17F, FontStyle.Bold);
        public static readonly Font FontLarge   = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font FontMedium  = new Font("Segoe UI", 11F, FontStyle.Regular);
        public static readonly Font FontMediumB = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font FontButton  = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontSmall   = new Font("Segoe UI",  9F, FontStyle.Regular);
        public static readonly Font FontCaption = new Font("Segoe UI",  8F, FontStyle.Regular);

        // ── Color helpers ────────────────────────────────────────────────────
        public static Color GetTableColor(string status)
        {
            switch ((status ?? "").ToLower())
            {
                case "occupied":  return TableOccupied;
                case "reserved":  return TableReserved;
                default:          return TableAvailable;
            }
        }

        public static Color WithAlpha(Color c, int alpha)
            => Color.FromArgb(alpha, c.R, c.G, c.B);

        // ── Gradient helpers ─────────────────────────────────────────────────
        public static void FillGradient(System.Drawing.Graphics g, System.Drawing.Rectangle rect, Color top, Color bottom)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            using (var br = new LinearGradientBrush(rect, top, bottom, LinearGradientMode.Vertical))
                g.FillRectangle(br, rect);
        }

        public static void FillGradientH(System.Drawing.Graphics g, System.Drawing.Rectangle rect, Color left, Color right)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            using (var br = new LinearGradientBrush(rect, left, right, LinearGradientMode.Horizontal))
                g.FillRectangle(br, rect);
        }

        // ── Rounded rect path ────────────────────────────────────────────────
        public static GraphicsPath RoundedRect(System.Drawing.Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (r.Width <= d || r.Height <= d) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ── Standard button style ─────────────────────────────────────────────
        public static void StyleButton(Button btn, Color bg, Color fg)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = bg;
            btn.ForeColor = fg;
            btn.Font = FontButton;
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.1f);
        }

        // ── Apply base dark theme to form ─────────────────────────────────────
        public static void ApplyTo(Form form)
        {
            form.BackColor = BgDark;
            form.ForeColor = TextPrimary;
            form.Font = FontMedium;
        }
    }
}
