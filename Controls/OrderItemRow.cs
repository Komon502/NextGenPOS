using System;
using System.Drawing;
using System.Windows.Forms;
using NextGenPOS.Helpers;

namespace NextGenPOS.Controls
{
    /// <summary>
    /// Row control for displaying an item in the current order list.
    /// </summary>
    public class OrderItemRow : Panel
    {
        public Models.OrderItem OrderItem { get; private set; }
        public event EventHandler RemoveClicked;
        public event EventHandler QtyChanged;

        private Label _lblName, _lblPrice;
        private NumericUpDown _nudQty;
        private Button _btnRemove;

        public OrderItemRow(Models.OrderItem item)
        {
            OrderItem = item;
            Height = 48;
            Dock = DockStyle.Top;
            BackColor = ThemeManager.BgCard;
            Margin = new Padding(0, 0, 0, 2);

            // Bottom border as spacer
            Padding = new Padding(8, 4, 8, 4);

            _lblName = new Label
            {
                Text = item.ItemName,
                ForeColor = ThemeManager.TextPrimary,
                Font = ThemeManager.FontMediumB,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 145,
                Dock = DockStyle.Left
            };

            // Qty spinner (custom styled)
            _nudQty = new NumericUpDown
            {
                Minimum = 1, Maximum = 99,
                Value = item.Qty,
                Width = 58,
                BackColor = ThemeManager.BgPanel,
                ForeColor = ThemeManager.AccentLight,
                Font = ThemeManager.FontMediumB,
                TextAlign = HorizontalAlignment.Center,
                Dock = DockStyle.Left,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(4, 0, 4, 0)
            };
            _nudQty.ValueChanged += (s, e) =>
            {
                OrderItem.Qty = (int)_nudQty.Value;
                UpdatePrice();
                QtyChanged?.Invoke(this, e);
            };

            _lblPrice = new Label
            {
                Text = $"฿{item.UnitPrice * item.Qty:N0}",
                ForeColor = ThemeManager.Warning,
                Font = ThemeManager.FontMediumB,
                AutoSize = false,
                Width = 76,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Left
            };

            _btnRemove = new Button
            {
                Text = "✕",
                ForeColor = ThemeManager.Danger,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.FontButton,
                Size = new Size(34, 34),
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            _btnRemove.FlatAppearance.BorderSize = 0;
            _btnRemove.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, ThemeManager.Danger);
            _btnRemove.Click += (s, e) => RemoveClicked?.Invoke(this, e);

            Controls.Add(_btnRemove);
            Controls.Add(_lblPrice);
            Controls.Add(_nudQty);
            Controls.Add(_lblName);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            // Bottom separator line
            using (var pen = new System.Drawing.Pen(ThemeManager.Border, 1))
                e.Graphics.DrawLine(pen, 8, Height - 1, Width - 8, Height - 1);
        }

        private void UpdatePrice()
        {
            _lblPrice.Text = $"฿{OrderItem.UnitPrice * OrderItem.Qty:N0}";
        }
    }
}

