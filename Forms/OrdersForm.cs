using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NextGenPOS.Data;
using NextGenPOS.Helpers;
using NextGenPOS.Models;
using NextGenPOS.Services;

namespace NextGenPOS.Forms
{
    /// <summary>
    /// Active Orders view: shows all non-paid orders in a grid with status filter.
    /// </summary>
    public class OrdersForm : UserControl
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly ITableRepository _tableRepo;
        private readonly ConfigService _config;

        private DataGridView _grid;
        private ComboBox _cboStatus;
        private Label _lblCount;
        private List<Order> _allOrders;

        public OrdersForm(IOrderRepository orderRepo, IMenuRepository menuRepo,
            ITableRepository tableRepo, ConfigService config)
        {
            _orderRepo = orderRepo;
            _menuRepo = menuRepo;
            _tableRepo = tableRepo;
            _config = config;

            BackColor = ThemeManager.BgDark;
            InitializeUI();
            LoadOrders();
        }

        private void InitializeUI()
        {
            // Top bar
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(22, 18, 50),
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblTitle = new Label
            {
                Text = "📋  Active Orders",
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.AccentLight,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            // Status filter
            var lblFilter = new Label
            {
                Text = "Filter:",
                Font = ThemeManager.FontButton,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Left,
                AutoSize = true,
                Margin = new Padding(12, 0, 4, 0)
            };

            _cboStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = ThemeManager.FontButton,
                BackColor = ThemeManager.BgCard,
                ForeColor = ThemeManager.TextPrimary,
                Width = 140,
                Dock = DockStyle.Left
            };
            _cboStatus.Items.AddRange(new object[] { "All", "New", "Preparing", "Served", "Paid", "Cancelled" });
            _cboStatus.SelectedIndex = 0;
            _cboStatus.SelectedIndexChanged += (s, e) => FilterOrders();

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
            btnRefresh.Click += (s, e) => LoadOrders();

            _lblCount = new Label
            {
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.TextMuted,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            topBar.Controls.AddRange(new Control[] { lblTitle, lblFilter, _cboStatus, _lblCount, btnRefresh });

            // DataGridView
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemeManager.BgDark,
                GridColor = ThemeManager.Border,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = ThemeManager.FontMedium,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 44 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Style
            _grid.DefaultCellStyle.BackColor = ThemeManager.BgDark;
            _grid.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            _grid.DefaultCellStyle.SelectionBackColor = ThemeManager.SidebarActive;
            _grid.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            _grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            _grid.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.BgPanel;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 20, 70);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.AccentLight;
            _grid.ColumnHeadersDefaultCellStyle.Font = ThemeManager.FontMediumB;
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            _grid.EnableHeadersVisualStyles = false;

            // Columns
            _grid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "OrderId",   HeaderText = "#Order",    Width = 80,  FillWeight = 8  },
                new DataGridViewTextBoxColumn { Name = "TableName", HeaderText = "โต๊ะ",      Width = 100, FillWeight = 12 },
                new DataGridViewTextBoxColumn { Name = "Status",    HeaderText = "สถานะ",     Width = 110, FillWeight = 14 },
                new DataGridViewTextBoxColumn { Name = "Staff",     HeaderText = "พนักงาน",   Width = 110, FillWeight = 14 },
                new DataGridViewTextBoxColumn { Name = "Items",     HeaderText = "จำนวนรายการ", Width = 80, FillWeight = 10 },
                new DataGridViewTextBoxColumn { Name = "Total",     HeaderText = "ยอดรวม",   Width = 100, FillWeight = 12 },
                new DataGridViewTextBoxColumn { Name = "OrderTime", HeaderText = "เวลา",      Width = 120, FillWeight = 15 },
            });

            _grid.CellFormatting += Grid_CellFormatting;
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            Controls.Add(_grid);
            Controls.Add(topBar);
        }

        private void LoadOrders()
        {
            var tables = _tableRepo.GetAll();
            _allOrders = new List<Order>();

            foreach (var table in tables)
            {
                var orders = _orderRepo.GetByTable(table.TableId);
                foreach (var o in orders)
                {
                    // Attach table name as Tag workaround
                    o.StaffName = o.StaffName; // keep as is
                    _allOrders.Add(o);
                }
            }
            _allOrders = _allOrders.OrderByDescending(o => o.OrderTime).ToList();
            FilterOrders();
        }

        private void FilterOrders()
        {
            _grid.Rows.Clear();
            var tables = _tableRepo.GetAll();
            string filter = _cboStatus.SelectedItem?.ToString() ?? "All";

            var filtered = filter == "All"
                ? _allOrders
                : _allOrders.Where(o => o.Status == filter).ToList();

            foreach (var o in filtered)
            {
                var table = tables.FirstOrDefault(t => t.TableId == o.TableId);
                var items = _orderRepo.GetOrderItems(o.OrderId);
                _grid.Rows.Add(
                    o.OrderId,
                    table?.TableName ?? $"Table #{o.TableId}",
                    o.Status,
                    o.StaffName,
                    items.Count,
                    $"฿{o.GrandTotal:N0}",
                    o.OrderTime.ToString("dd/MM HH:mm")
                );
            }

            _lblCount.Text = $"  {filtered.Count} orders  ";
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                switch (e.Value.ToString())
                {
                    case "New":       e.CellStyle.ForeColor = ThemeManager.InfoBlue; break;
                    case "Preparing": e.CellStyle.ForeColor = ThemeManager.Warning; break;
                    case "Served":    e.CellStyle.ForeColor = ThemeManager.AccentLight; break;
                    case "Paid":      e.CellStyle.ForeColor = ThemeManager.Success; break;
                    case "Cancelled": e.CellStyle.ForeColor = ThemeManager.Danger; break;
                }
            }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            // Could open order details here
            var orderId = (int)_grid.Rows[e.RowIndex].Cells["OrderId"].Value;
            var order = _allOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return;

            var table = _tableRepo.GetAll().FirstOrDefault(t => t.TableId == order.TableId);
            if (table == null) return;

            var oef = new OrderEntryForm(table, _menuRepo, _orderRepo, _tableRepo, _config);
            oef.ShowDialog();
            LoadOrders();
        }
    }
}
