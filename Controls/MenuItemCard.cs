using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NextGenPOS.Helpers;

namespace NextGenPOS.Controls
{
    /// <summary>
    /// Touch-friendly menu item card for order entry.
    /// Shows item name, price, and a subtle gradient card with hover glow.
    /// </summary>
    public class MenuItemCard : Control
    {
        public Models.MenuItem Item { get; set; }
        public event EventHandler ItemClicked;

        private bool _hovered;
        private bool _pressed;

        public MenuItemCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(168, 108);
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(2, 2, Width - 4, Height - 4);

            // Card background gradient
            Color bgTop = _pressed ? ThemeManager.AccentViolet
                        : _hovered ? ThemeManager.BgCardHover
                        : ThemeManager.BgCard;
            Color bgBot = _pressed ? ThemeManager.AccentHover
                        : _hovered ? Color.FromArgb(72, 55, 115)
                        : Color.FromArgb(50, 45, 80);

            using (var path = ThemeManager.RoundedRect(rect, 12))
            {
                // Shadow (only when not pressed)
                if (!_pressed)
                {
                    var shadow = new Rectangle(rect.X + 2, rect.Y + 3, rect.Width, rect.Height);
                    using (var sp = ThemeManager.RoundedRect(shadow, 12))
                    using (var sb = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                        g.FillPath(sb, sp);
                }

                // Card fill
                using (var br = new LinearGradientBrush(rect, bgTop, bgBot, LinearGradientMode.Vertical))
                    g.FillPath(br, path);

                // Border
                Color borderColor = _hovered
                    ? Color.FromArgb(180, ThemeManager.AccentLight)
                    : Color.FromArgb(60, 255, 255, 255);
                using (var pen = new Pen(borderColor, _hovered ? 1.5f : 1f))
                    g.DrawPath(pen, path);
            }

            if (Item == null) return;

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Item name — up to 2 lines
                var nameRect = new RectangleF(6, 10, Width - 12, 52);
                Color nameColor = _pressed ? Color.White : ThemeManager.TextPrimary;
                using (var br = new SolidBrush(nameColor))
                    g.DrawString(Item.Name, ThemeManager.FontMediumB, br, nameRect, sf);

                // Divider
                int divY = 66;
                using (var pen = new Pen(Color.FromArgb(40, 255, 255, 255), 1))
                    g.DrawLine(pen, 12, divY, Width - 12, divY);

                // Price
                var priceRect = new RectangleF(4, 70, Width - 8, 28);
                Color priceColor = _pressed ? Color.White : ThemeManager.Warning;
                using (var br = new SolidBrush(priceColor))
                    g.DrawString($"฿{Item.Price:N0}", ThemeManager.FontLarge, br, priceRect, sf);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnClick(EventArgs e) { ItemClicked?.Invoke(this, e); base.OnClick(e); }
    }
}

