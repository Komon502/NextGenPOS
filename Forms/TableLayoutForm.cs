using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Floor plan view showing all tables as color-coded buttons.
    /// Tap a table to open its order.
    /// </summary>
    public class TableLayoutForm : UserControl
    {
        private readonly ITableRepository _tableRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ConfigService _config;

        private FlowLayoutPanel _floorPanel;
        private Panel _zoneBar;
        private string _currentZone = "All";

        public TableLayoutForm(ITableRepository tableRepo, IMenuRepository menuRepo,
            IOrderRepository orderRepo, IPaymentRepository paymentRepo, ConfigService config)
        {
            _tableRepo = tableRepo;
            _menuRepo = menuRepo;
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
            _config = config;

            BackColor = ThemeManager.BgDark;
            InitializeUI();
            LoadTables();
        }

        private void InitializeUI()
        {
            // Zone filter bar
            _zoneBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = ThemeManager.BgPanel,
                Padding = new Padding(8, 6, 8, 6)
            };

            string[] zones = { "All", "Floor", "Patio", "Bar" };
            int x = 8;
            foreach (var zone in zones)
            {
                var btn = new Button
                {
                    Text = zone,
                    Tag = zone,
                    FlatStyle = FlatStyle.Flat,
                    Font = ThemeManager.FontButton,
                    ForeColor = ThemeManager.TextPrimary,
                    BackColor = zone == "All" ? ThemeManager.AccentViolet : ThemeManager.BgCard,
                    Size = new Size(90, 36),
                    Location = new Point(x, 6),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += ZoneFilter_Click;
                _zoneBar.Controls.Add(btn);
                x += 100;
            }

            // Legend
            var legend = new Label
            {
                Text = "🟢 Available    🔴 Occupied    🟡 Reserved",
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.TextMuted,
                AutoSize = true,
                Location = new Point(x + 30, 14)
            };
            _zoneBar.Controls.Add(legend);

            // Table grid
            _floorPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.BgDark,
                Padding = new Padding(16)
            };

            Controls.Add(_floorPanel);
            Controls.Add(_zoneBar);
        }

        private void ZoneFilter_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            _currentZone = btn.Tag.ToString();

            foreach (Control c in _zoneBar.Controls)
                if (c is Button b) b.BackColor = ThemeManager.BgCard;
            btn.BackColor = ThemeManager.AccentViolet;

            LoadTables();
        }

        public void LoadTables()
        {
            _floorPanel.Controls.Clear();
            var tables = _tableRepo.GetAll();

            if (_currentZone != "All")
                tables = tables.Where(t => t.ZoneArea == _currentZone).ToList();

            foreach (var table in tables)
            {
                var tb = new TableButton
                {
                    TableData = table,
                    Margin = new Padding(8)
                };
                tb.TableClicked += TableButton_Clicked;
                _floorPanel.Controls.Add(tb);
            }
        }

        private void TableButton_Clicked(object sender, EventArgs e)
        {
            var tb = (TableButton)sender;
            var table = tb.TableData;

            // Open order entry for this table
            var orderForm = new OrderEntryForm(table, _menuRepo, _orderRepo, _tableRepo, _config);
            orderForm.FormClosed += (s, ev) => LoadTables(); // Refresh after order
            orderForm.ShowDialog();
        }
    }
}
