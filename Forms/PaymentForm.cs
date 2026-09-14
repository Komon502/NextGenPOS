using System;
using System.Drawing;
using System.Windows.Forms;
using NextGenPOS.Data;
using NextGenPOS.Helpers;
using NextGenPOS.Models;
using NextGenPOS.Services;

namespace NextGenPOS.Forms
{
    /// <summary>
    /// Payment dialog: shows total, payment method, cash tendered, and change.
    /// </summary>
    public class PaymentForm : Form
    {
        private readonly Order _order;
        private readonly IOrderRepository _orderRepo;
        private readonly ITableRepository _tableRepo;
        private readonly PaymentService _paySvc;
        private readonly ConfigService _config;

        private Label _lblGrandTotal, _lblChange;
        private TextBox _txtTendered;
        private Button _btnCash, _btnQR, _btnTransfer, _btnConfirm;
        private string _selectedMethod = "Cash";

        public PaymentForm(Order order, IOrderRepository orderRepo, ITableRepository tableRepo,
            PaymentService paySvc, ConfigService config)
        {
            _order = order;
            _orderRepo = orderRepo;
            _tableRepo = tableRepo;
            _paySvc = paySvc;
            _config = config;

            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "ชำระเงิน";
            Size = new Size(460, 520);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ThemeManager.ApplyTo(this);

            // ── Header ────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(30, 20, 70),
                Padding = new Padding(20, 12, 20, 0)
            };
            var lblHeader = new Label
            {
                Text = "💳  ชำระเงิน",
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.AccentLight,
                Dock = DockStyle.Fill
            };
            var lblTable = new Label
            {
                Text = $"Order #{_order.OrderId}",
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Right,
                AutoSize = true
            };
            header.Controls.AddRange(new Control[] { lblHeader, lblTable });

            // ── Body ─────────────────────────────────────────────────────
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.BgDark,
                Padding = new Padding(20, 14, 20, 14)
            };

            int y = 14;

            // Total amount display
            _lblGrandTotal = new Label
            {
                Text = $"฿ {_order.GrandTotal:N0}",
                Font = ThemeManager.FontHero,
                ForeColor = ThemeManager.Warning,
                Location = new Point(20, y),
                Size = new Size(400, 52),
                TextAlign = ContentAlignment.MiddleCenter
            };
            y += 60;

            var lblTotalCaption = new Label
            {
                Text = "ยอดรวมทั้งหมด (รวม VAT)",
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.TextMuted,
                Location = new Point(20, y),
                Size = new Size(400, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            y += 30;

            var div1 = new Panel { Location = new Point(20, y), Size = new Size(400, 1), BackColor = ThemeManager.Border };
            y += 14;

            var lblMethod = new Label
            {
                Text = "วิธีชำระเงิน",
                Font = ThemeManager.FontMediumB,
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(20, y),
                AutoSize = true
            };
            y += 30;

            _btnCash     = MakeMethodBtn("💵 เงินสด",  "Cash",     20, y, true);
            _btnQR       = MakeMethodBtn("📱 QR Pay",  "QR",      162, y, false);
            _btnTransfer = MakeMethodBtn("🏦 โอนเงิน", "Transfer", 304, y, false);
            y += 56;

            var lblTendered = new Label
            {
                Text = "รับเงินมา (บาท)",
                Font = ThemeManager.FontMediumB,
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(20, y),
                AutoSize = true
            };
            y += 28;

            _txtTendered = new TextBox
            {
                Font = ThemeManager.FontTitle,
                BackColor = ThemeManager.BgCard,
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(20, y),
                Size = new Size(400, 46),
                Text = _order.GrandTotal.ToString("N0"),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Right
            };
            _txtTendered.TextChanged += (s, e) => CalcChange();
            y += 56;

            _lblChange = new Label
            {
                Text = "เงินทอน: ฿ 0",
                Font = ThemeManager.FontLarge,
                ForeColor = ThemeManager.Success,
                Location = new Point(20, y),
                Size = new Size(400, 36),
                TextAlign = ContentAlignment.MiddleRight
            };
            y += 48;

            _btnConfirm = new Button
            {
                Text = "✅  ยืนยันชำระเงิน",
                Font = ThemeManager.FontMediumB,
                ForeColor = Color.White,
                BackColor = ThemeManager.AccentViolet,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, y),
                Size = new Size(400, 52),
                Cursor = Cursors.Hand
            };
            _btnConfirm.FlatAppearance.BorderSize = 0;
            _btnConfirm.FlatAppearance.MouseOverBackColor = ThemeManager.AccentHover;
            _btnConfirm.Click += BtnConfirm_Click;

            body.Controls.AddRange(new Control[]
            {
                _lblGrandTotal, lblTotalCaption, div1,
                lblMethod, _btnCash, _btnQR, _btnTransfer,
                lblTendered, _txtTendered, _lblChange, _btnConfirm
            });

            Controls.Add(body);
            Controls.Add(header);

            CalcChange();
        }

        private Button MakeMethodBtn(string text, string method, int x, int y, bool active)
        {
            var btn = new Button
            {
                Text = text,
                Tag = method,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.FontSmall,
                ForeColor = active ? Color.White : ThemeManager.TextMuted,
                BackColor = active ? ThemeManager.AccentViolet : ThemeManager.BgCard,
                Location = new Point(x, y),
                Size = new Size(132, 44),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                _selectedMethod = method;
                foreach (var b2 in new[] { _btnCash, _btnQR, _btnTransfer })
                {
                    if (b2 != null)
                    {
                        b2.BackColor = ThemeManager.BgCard;
                        b2.ForeColor = ThemeManager.TextMuted;
                    }
                }
                btn.BackColor = ThemeManager.AccentViolet;
                btn.ForeColor = Color.White;
                _txtTendered.Enabled = method == "Cash";
                if (method != "Cash")
                    _txtTendered.Text = _order.GrandTotal.ToString("N0");
            };
            return btn;
        }

        private void CalcChange()
        {
            if (decimal.TryParse(_txtTendered.Text.Replace(",", ""), out decimal tendered))
            {
                decimal change = tendered - _order.GrandTotal;
                _lblChange.Text = $"เงินทอน: ฿ {Math.Max(0, change):N0}";
                _lblChange.ForeColor = change >= 0 ? ThemeManager.Success : ThemeManager.Danger;
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (_selectedMethod == "Cash")
            {
                if (!decimal.TryParse(_txtTendered.Text.Replace(",", ""), out decimal tendered)
                    || tendered < _order.GrandTotal)
                {
                    MessageBox.Show("จำนวนเงินไม่พอครับ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            decimal paid = decimal.TryParse(_txtTendered.Text.Replace(",", ""), out decimal t) ? t : _order.GrandTotal;
            var payment = _paySvc.ProcessPayment(_order, _selectedMethod, paid);
            decimal change = paid - _order.GrandTotal;

            MessageBox.Show(
                $"ชำระเงินสำเร็จ! ✅\n" +
                $"เลขที่ใบเสร็จ: {payment.ReceiptNo}\n" +
                $"วิธีชำระ: {_selectedMethod}\n" +
                $"เงินทอน: ฿{Math.Max(0, change):N0}",
                "ชำระเงินสำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
