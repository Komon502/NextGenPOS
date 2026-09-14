using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using NextGenPOS.Controls;
using NextGenPOS.Data;
using NextGenPOS.Helpers;
using NextGenPOS.Models;
using NextGenPOS.Services;

namespace NextGenPOS.Forms
{
    /// <summary>
    /// Order entry form: left side = category tabs + menu items, right side = current order.
    /// </summary>
    public class OrderEntryForm : Form
    {
        private readonly Table _table;
        private readonly IMenuRepository _menuRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly ITableRepository _tableRepo;
        private readonly ConfigService _config;
        private readonly OrderService _orderSvc;
        private Order _currentOrder;

        // Left panel
        private FlowLayoutPanel _categoryBar;
        private FlowLayoutPanel _menuGrid;

        // Right panel
        private Panel _orderPanel;
        private Panel _orderItemsContainer;
        private Label _lblSubtotal, _lblTax, _lblTotal;
        private Button _btnSendKitchen, _btnPay;

        public OrderEntryForm(Table table, IMenuRepository menuRepo,
            IOrderRepository orderRepo, ITableRepository tableRepo, ConfigService config)
        {
            _table = table;
            _menuRepo = menuRepo;
            _orderRepo = orderRepo;
            _tableRepo = tableRepo;
            _config = config;
            _orderSvc = new OrderService(orderRepo, tableRepo, config);

            InitializeUI();
            LoadCategories();

            _currentOrder = _orderSvc.GetOrCreateActiveOrder(table.TableId, config.Get("DefaultStaff", "Cashier"));
            RefreshOrderList();
        }

        private void InitializeUI()
        {
            Text = $"Order — {_table.TableName} ({_table.ZoneArea})";
            Size = new Size(1100, 740);
            StartPosition = FormStartPosition.CenterParent;
            ThemeManager.ApplyTo(this);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            // ── RIGHT PANEL ──────────────────────────────────────────────────
            _orderPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 380,
                BackColor = ThemeManager.BgPanel
            };

            // Order title header
            var orderHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(35, 20, 80),
                Padding = new Padding(16, 12, 12, 0)
            };
            var lblOrderTitle = new Label
            {
                Text = $"🧾  Order — {_table.TableName}",
                Font = ThemeManager.FontLarge,
                ForeColor = ThemeManager.AccentLight,
                Dock = DockStyle.Fill
            };
            orderHeader.Controls.Add(lblOrderTitle);

            _orderItemsContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.BgPanel,
                Padding = new Padding(4)
            };

            // Totals panel
            var totalsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 220,
                BackColor = ThemeManager.BgCard,
                Padding = new Padding(14, 10, 14, 10)
            };

            var divider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = ThemeManager.AccentViolet
            };

            _lblSubtotal = new Label
            {
                Text = "Subtotal: \t฿0",
                Font = ThemeManager.FontMedium,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Top,
                Height = 26,
                Padding = new Padding(0, 4, 0, 0)
            };
            _lblTax = new Label
            {
                Text = "Tax (7%): \t฿0",
                Font = ThemeManager.FontMedium,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Top,
                Height = 26
            };
            _lblTotal = new Label
            {
                Text = "TOTAL  ฿0",
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.Warning,
                Dock = DockStyle.Top,
                Height = 38
            };

            _btnPay = new Button
            {
                Text = "💳  ชำระเงิน",
                Font = ThemeManager.FontMediumB,
                ForeColor = Color.White,
                BackColor = ThemeManager.Success,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Bottom,
                Height = 46,
                Cursor = Cursors.Hand
            };
            _btnPay.FlatAppearance.BorderSize = 0;
            _btnPay.FlatAppearance.MouseOverBackColor = ThemeManager.SuccessDark;
            _btnPay.Click += BtnPay_Click;

            _btnSendKitchen = new Button
            {
                Text = "🍳  ส่งครัว",
                Font = ThemeManager.FontMediumB,
                ForeColor = Color.White,
                BackColor = ThemeManager.AccentViolet,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Bottom,
                Height = 46,
                Cursor = Cursors.Hand
            };
            _btnSendKitchen.FlatAppearance.BorderSize = 0;
            _btnSendKitchen.FlatAppearance.MouseOverBackColor = ThemeManager.AccentHover;
            _btnSendKitchen.Click += BtnSendKitchen_Click;

            totalsPanel.Controls.AddRange(new Control[] { _btnPay, _btnSendKitchen, _lblTotal, _lblTax, _lblSubtotal, divider });

            _orderPanel.Controls.Add(_orderItemsContainer);
            _orderPanel.Controls.Add(totalsPanel);
            _orderPanel.Controls.Add(orderHeader);

            // ── LEFT PANEL ───────────────────────────────────────────────────
            var splitter = new Panel
            {
                Dock = DockStyle.Right,
                Width = 2,
                BackColor = ThemeManager.Border
            };

            _categoryBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = ThemeManager.BgPanel,
                Padding = new Padding(12, 10, 8, 6),
                AutoScroll = false,
                WrapContents = false
            };

            _menuGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.BgDark,
                Padding = new Padding(14)
            };

            Controls.Add(_menuGrid);
            Controls.Add(_categoryBar);
            Controls.Add(splitter);
            Controls.Add(_orderPanel);
        }

        private void LoadCategories()
        {
            _categoryBar.Controls.Clear();
            var cats = _menuRepo.GetCategories();
            bool first = true;
            foreach (var cat in cats)
            {
                var btn = new Button
                {
                    Text = cat.Name,
                    Tag = cat.CategoryId,
                    FlatStyle = FlatStyle.Flat,
                    Font = ThemeManager.FontButton,
                    ForeColor = first ? Color.White : ThemeManager.TextMuted,
                    BackColor = first ? ThemeManager.AccentViolet : ThemeManager.BgCard,
                    Size = new Size(130, 36),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 8, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = ThemeManager.AccentHover;
                btn.Click += CategoryBtn_Click;
                _categoryBar.Controls.Add(btn);

                if (first) { LoadMenuItems(cat.CategoryId); first = false; }
            }
        }

        private void CategoryBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            foreach (Control c in _categoryBar.Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = ThemeManager.BgCard;
                    b.ForeColor = ThemeManager.TextMuted;
                }
            }
            btn.BackColor = ThemeManager.AccentViolet;
            btn.ForeColor = Color.White;
            LoadMenuItems((int)btn.Tag);
        }

        private void LoadMenuItems(int categoryId)
        {
            _menuGrid.Controls.Clear();
            var items = _menuRepo.GetByCategory(categoryId);
            foreach (var item in items)
            {
                var card = new MenuItemCard
                {
                    Item = item,
                    Margin = new Padding(8)
                };
                card.ItemClicked += (s, ev) => AddItemToOrder(item);
                _menuGrid.Controls.Add(card);
            }
        }

        private void AddItemToOrder(Models.MenuItem menuItem)
        {
            var existing = _orderRepo.GetOrderItems(_currentOrder.OrderId)
                .FirstOrDefault(i => i.ItemId == menuItem.ItemId);

            if (existing != null)
            {
                existing.Qty++;
                _orderRepo.UpdateOrderItem(existing);
            }
            else
            {
                _orderSvc.AddItem(_currentOrder.OrderId, menuItem, 1, "");
            }

            RefreshOrderList();
        }

        private void RefreshOrderList()
        {
            _orderItemsContainer.Controls.Clear();
            var items = _orderRepo.GetOrderItems(_currentOrder.OrderId);

            foreach (var item in items.AsEnumerable().Reverse())
            {
                var row = new OrderItemRow(item) { Margin = new Padding(0, 0, 0, 2) };
                row.RemoveClicked += (s, e) =>
                {
                    _orderRepo.RemoveOrderItem(item.OrderItemId);
                    RefreshOrderList();
                };
                row.QtyChanged += (s, e) =>
                {
                    _orderRepo.UpdateOrderItem(item);
                    RefreshOrderList();
                };
                _orderItemsContainer.Controls.Add(row);
            }

            if (items.Count == 0)
            {
                _orderItemsContainer.Controls.Add(new Label
                {
                    Text = "ยังไม่มีรายการ\nเลือกเมนูด้านซ้าย",
                    Font = ThemeManager.FontSmall,
                    ForeColor = ThemeManager.TextMuted,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                });
            }

            _orderSvc.RecalcTotals(_currentOrder);
            _lblSubtotal.Text = $"Subtotal: \t฿{_currentOrder.SubTotal:N0}";
            _lblTax.Text = $"Tax ({_config.GetInt("TaxRate", 7)}%): \t฿{_currentOrder.TaxAmt:N0}";
            _lblTotal.Text = $"TOTAL  ฿{_currentOrder.GrandTotal:N0}";
        }

        private void BtnSendKitchen_Click(object sender, EventArgs e)
        {
            if (_orderRepo.GetOrderItems(_currentOrder.OrderId).Count == 0)
            {
                MessageBox.Show("ยังไม่มีรายการในออเดอร์", "ไม่มีรายการ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _orderSvc.SendToKitchen(_currentOrder);
            MessageBox.Show("ส่งออเดอร์ไปยังครัวแล้ว! 🍳", "ส่งสำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (_currentOrder.GrandTotal <= 0)
            {
                MessageBox.Show("ยังไม่มีรายการที่จะชำระ", "ไม่มีรายการ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var payForm = new PaymentForm(_currentOrder, _orderRepo, _tableRepo,
                new PaymentService(_orderRepo, _tableRepo, null, _config), _config);
            payForm.ShowDialog();
            if (_currentOrder.Status == "Paid") Close();
        }
    }
}

