using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NextGenPOS.Models;
using NextGenPOS.Helpers;

namespace NextGenPOS.Controls
{
    /// <summary>
    /// Custom touch-friendly button representing a restaurant table.
    /// Color-coded: Green=Available, Red=Occupied, Yellow=Reserved.
    /// </summary>
    public class TableButton : Control
    {
        public Table TableData { get; set; }
        public event EventHandler TableClicked;

        private bool _hovered;
        private bool _pressed;

        public TableButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(140, 120);
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color statusColor = ThemeManager.GetTableColor(TableData?.Status);
            string status = (TableData?.Status ?? "Available").ToLower();

            // Darken base for gradient bottom
            Color bgTop = _pressed ? ControlPaint.Dark(statusColor, 0.2f)
                        : _hovered ? ControlPaint.Light(statusColor, 0.25f)
                        : Color.FromArgb(180, statusColor);
            Color bgBot = ControlPaint.Dark(bgTop, 0.35f);

            var rect = new Rectangle(3, 3, Width - 6, Height - 6);

            // Drop shadow
            if (!_pressed)
            {
                var shadowRect = new Rectangle(rect.X + 3, rect.Y + 4, rect.Width, rect.Height);
                using (var sp = ThemeManager.RoundedRect(shadowRect, 14))
                using (var sb = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                    g.FillPath(sb, sp);
            }

            using (var path = ThemeManager.RoundedRect(rect, 14))
            {
                // Gradient fill
                using (var br = new LinearGradientBrush(rect, bgTop, bgBot, LinearGradientMode.Vertical))
                    g.FillPath(br, path);

                // Inner highlight at top
                var topHighlight = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height / 3);
                using (var topPath = ThemeManager.RoundedRect(topHighlight, 12))
                using (var hb = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
                    g.FillPath(hb, topPath);

                // Border
                float bw = _hovered ? 2f : 1.5f;
                Color borderC = _hovered ? Color.FromArgb(200, 255, 255, 255) : Color.FromArgb(80, 255, 255, 255);
                using (var pen = new Pen(borderC, bw))
                    g.DrawPath(pen, path);
            }

            if (TableData == null) return;

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Table icon + name
                string name = TableData.TableName ?? "??";
                g.DrawString(name, ThemeManager.FontLarge, Brushes.White,
                    new RectangleF(0, 8, Width, 38), sf);

                // Seats badge
                string seatsText = $"🪑 {TableData.Seats} seats";
                using (var sb = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                    g.DrawString(seatsText, ThemeManager.FontSmall, sb,
                        new RectangleF(0, 50, Width, 22), sf);

                // Status badge background
                string statusLabel = TableData.Status ?? "Available";
                var badgeRect = new RectangleF(Width / 2 - 45, 76, 90, 24);
                using (var badgePath = ThemeManager.RoundedRect(Rectangle.Round(badgeRect), 8))
                using (var bb = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                    g.FillPath(bb, badgePath);

                using (var sb = new SolidBrush(Color.White))
                    g.DrawString(statusLabel, ThemeManager.FontCaption, sb, badgeRect, sf);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnClick(EventArgs e) { TableClicked?.Invoke(this, e); base.OnClick(e); }
    }
}
