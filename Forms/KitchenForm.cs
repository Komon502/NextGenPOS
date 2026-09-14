using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using NextGenPOS.Data;
using NextGenPOS.Helpers;
using NextGenPOS.Models;
using NextGenPOS.Services;

namespace NextGenPOS.Forms
{
    /// <summary>
    /// Kitchen Display System: shows all "Preparing" orders as cards.
    /// Auto-refreshes every 10 seconds. Staff can mark orders as Ready.
    /// </summary>
    public class KitchenForm : UserControl
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ITableRepository _tableRepo;
        private readonly ConfigService _config;
        private Timer _refreshTimer;
        private FlowLayoutPanel _cardsPanel;
        private Label _lblStatus;

        public KitchenForm(IOrderRepository orderRepo, ITableRepository tableRepo, ConfigService config)
        {
            _orderRepo = orderRepo;
            _tableRepo = tableRepo;
            _config = config;

            BackColor = ThemeManager.BgDark;
            InitializeUI();
            LoadKitchenOrders();
        }

        private void InitializeUI()
        {
            // Top bar
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(22, 16, 50),
                Padding = new Padding(20, 0, 20, 0)
            };

            var lblTitle = new Label
            {
                Text = "🍳  Kitchen Display",
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.AccentLight,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblStatus = new Label
            {
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            var btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Font = ThemeManager.FontButton,
                ForeColor = ThemeManager.TextPrimary,
                BackColor = ThemeManager.BgCard,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Width = 110,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadKitchenOrders();

            topBar.Controls.AddRange(new Control[] { lblTitle, btnRefresh, _lblStatus });

            // Cards panel
            _cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.BgDark,
                Padding = new Padding(16)
            };

            Controls.Add(_cardsPanel);
            Controls.Add(topBar);

            // Auto-refresh every 10 seconds
            _refreshTimer = new Timer { Interval = 10000 };
            _refreshTimer.Tick += (s, e) => LoadKitchenOrders();
            _refreshTimer.Start();
        }

        private void LoadKitchenOrders()
        {
            _cardsPanel.Controls.Clear();
            var tables = _tableRepo.GetAll();
            var activeOrders = new List<Order>();

            foreach (var table in tables)
            {
                var orders = _orderRepo.GetByTable(table.TableId)
                    .Where(o => o.Status == "Preparing" || o.Status == "New")
                    .ToList();
                activeOrders.AddRange(orders);
            }

            _lblStatus.Text = $"Active orders: {activeOrders.Count}  |  Last update: {DateTime.Now:HH:mm:ss}";

            if (activeOrders.Count == 0)
            {
                _cardsPanel.Controls.Add(new Label
                {
                    Text = "✅  ไม่มีออเดอร์รอทำ\nครัวว่างอยู่",
                    Font = ThemeManager.FontTitle,
                    ForeColor = ThemeManager.Success,
                    Size = new Size(400, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(40)
                });
                return;
            }

            foreach (var order in activeOrders.OrderBy(o => o.OrderTime))
            {
                var table = tables.FirstOrDefault(t => t.TableId == order.TableId);
                var card = BuildKitchenCard(order, table);
                _cardsPanel.Controls.Add(card);
            }
        }

        private Panel BuildKitchenCard(Order order, Table table)
        {
            var card = new Panel
            {
                Size = new Size(300, 0), // height auto
                BackColor = ThemeManager.BgCard,
                Margin = new Padding(10),
                Padding = new Padding(0)
            };

            // Round corners via Paint
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var path = ThemeManager.RoundedRect(r, 12))
                {
                    Color statusColor = order.Status == "Preparing" ? ThemeManager.Warning : ThemeManager.InfoBlue;
                    using (var pen = new Pen(statusColor, 2))
                        e.Graphics.DrawPath(pen, path);
                }
            };

            // Card header
            var cardHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = order.Status == "Preparing"
                    ? Color.FromArgb(60, 45, 10)
                    : Color.FromArgb(15, 35, 70),
                Padding = new Padding(12, 8, 12, 8)
            };

            var lblTable = new Label
            {
                Text = table?.TableName ?? $"Table #{order.TableId}",
                Font = ThemeManager.FontLarge,
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            var lblTime = new Label
            {
                Text = order.OrderTime.ToString("HH:mm"),
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Right,
                AutoSize = true
            };

            var lblStatusBadge = new Label
            {
                Text = order.Status == "Preparing" ? "🔥 Preparing" : "🆕 New",
                Font = ThemeManager.FontSmall,
                ForeColor = order.Status == "Preparing" ? ThemeManager.Warning : ThemeManager.InfoBlue,
                Dock = DockStyle.Right,
                AutoSize = true,
                Margin = new Padding(0, 0, 8, 0)
            };

            cardHeader.Controls.AddRange(new Control[] { lblTable, lblStatusBadge, lblTime });

            // Items list
            var itemsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = ThemeManager.BgCard,
                Padding = new Padding(10, 8, 10, 4)
            };

            var items = _orderRepo.GetOrderItems(order.OrderId);
            foreach (var item in items)
            {
                var itemLbl = new Label
                {
                    Text = $"  ×{item.Qty}  {item.ItemName}",
                    Font = ThemeManager.FontMedium,
                    ForeColor = ThemeManager.TextPrimary,
                    AutoSize = true,
                    Margin = new Padding(0, 2, 0, 2)
                };
                itemsPanel.Controls.Add(itemLbl);

                if (!string.IsNullOrWhiteSpace(item.Notes))
                {
                    var noteLbl = new Label
                    {
                        Text = $"     📝 {item.Notes}",
                        Font = ThemeManager.FontSmall,
                        ForeColor = ThemeManager.TextMuted,
                        AutoSize = true,
                        Margin = new Padding(0, 0, 0, 4)
                    };
                    itemsPanel.Controls.Add(noteLbl);
                }
            }

            // Mark Ready button
            var btnReady = new Button
            {
                Text = "✅  Mark as Ready",
                Font = ThemeManager.FontButton,
                ForeColor = Color.White,
                BackColor = ThemeManager.Success,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Bottom,
                Height = 44,
                Cursor = Cursors.Hand
            };
            btnReady.FlatAppearance.BorderSize = 0;
            btnReady.FlatAppearance.MouseOverBackColor = ThemeManager.SuccessDark;
            btnReady.Click += (s, e) =>
            {
                order.Status = "Served";
                _orderRepo.Update(order);
                LoadKitchenOrders();
            };

            card.Controls.Add(btnReady);
            card.Controls.Add(itemsPanel);
            card.Controls.Add(cardHeader);

            // Auto-size height
            card.Height = 52 + itemsPanel.PreferredSize.Height + 52 + 10;
            if (card.Height < 160) card.Height = 160;

            return card;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _refreshTimer?.Stop();
            base.Dispose(disposing);
        }
    }
}
