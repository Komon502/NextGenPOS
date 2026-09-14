using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NextGenPOS.Data;
using NextGenPOS.Helpers;
using NextGenPOS.Models;
using NextGenPOS.Services;

namespace NextGenPOS.Forms
{
    /// <summary>
    /// Daily Reports: today sales summary, top items, and all paid orders.
    /// </summary>
    public class ReportsForm : UserControl
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly ITableRepository _tableRepo;
        private readonly ConfigService _config;

        private DataGridView _gridOrders;
        private Label _lblCount, _lblSubtotal, _lblTax, _lblTotal;
        private FlowLayoutPanel _topItemsPanel;
        private DateTime _reportDate;

        public ReportsForm(IOrderRepository orderRepo, IMenuRepository menuRepo,
            ITableRepository tableRepo, ConfigService config)
        {
            _orderRepo = orderRepo;
            _menuRepo = menuRepo;
            _tableRepo = tableRepo;
            _config = config;
            _reportDate = DateTime.Today;

            BackColor = ThemeManager.BgDark;
            InitializeUI();
            LoadReport();
        }

        private void InitializeUI()
        {
            // Top bar
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(20, 18, 50),
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblTitle = new Label
            {
                Text = "📊  Daily Reports",
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.AccentLight,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            var dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Width = 130,
                Dock = DockStyle.Left
            };
            dtpDate.ValueChanged += (s, e) => { _reportDate = dtpDate.Value.Date; LoadReport(); };

            var btnExport = new Button
            {
                Text = "📄 Export .txt",
                Font = ThemeManager.FontButton,
                ForeColor = Color.White,
                BackColor = ThemeManager.InfoBlue,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Width = 130,
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            var btnRefresh = new Button
            {
                Text = "🔄",
                Font = ThemeManager.FontButton,
                ForeColor = ThemeManager.TextPrimary,
                BackColor = ThemeManager.BgCard,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Width = 46,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadReport();

            topBar.Controls.AddRange(new Control[] { lblTitle, dtpDate, btnExport, btnRefresh });

            // Summary strip
            var summaryStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = ThemeManager.BgPanel,
                Padding = new Padding(16, 10, 16, 10)
            };

            _lblCount    = MakeSummaryLabel("Orders",   "0",   ThemeManager.InfoBlue);
            _lblSubtotal = MakeSummaryLabel("Subtotal", "฿0",  ThemeManager.TextSecondary);
            _lblTax      = MakeSummaryLabel("VAT",      "฿0",  ThemeManager.TextMuted);
            _lblTotal    = MakeSummaryLabel("Grand Total","฿0", ThemeManager.Warning);

            summaryStrip.Controls.AddRange(new Control[] { _lblCount, _lblSubtotal, _lblTax, _lblTotal });

            // Left panel: Top items
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = ThemeManager.BgPanel,
                Padding = new Padding(12)
            };
            var lblTopTitle = new Label
            {
                Text = "🏆 เมนูขายดี",
                Font = ThemeManager.FontLarge,
                ForeColor = ThemeManager.AccentLight,
                Dock = DockStyle.Top,
                Height = 38
            };
            _topItemsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = ThemeManager.BgPanel
            };
            leftPanel.Controls.AddRange(new Control[] { _topItemsPanel, lblTopTitle });

            var splitter = new Panel { Dock = DockStyle.Left, Width = 2, BackColor = ThemeManager.Border };

            // Orders grid
            _gridOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemeManager.BgDark,
                GridColor = ThemeManager.Border,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = ThemeManager.FontMedium,
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 40 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _gridOrders.DefaultCellStyle.BackColor = ThemeManager.BgDark;
            _gridOrders.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            _gridOrders.DefaultCellStyle.SelectionBackColor = ThemeManager.SidebarActive;
            _gridOrders.DefaultCellStyle.SelectionForeColor = ThemeManager.TextPrimary;
            _gridOrders.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            _gridOrders.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.BgPanel;
            _gridOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 18, 65);
            _gridOrders.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.AccentLight;
            _gridOrders.ColumnHeadersDefaultCellStyle.Font = ThemeManager.FontMediumB;
            _gridOrders.EnableHeadersVisualStyles = false;

            _gridOrders.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "OrderId",  HeaderText = "#",       FillWeight = 8  },
                new DataGridViewTextBoxColumn { Name = "Table",    HeaderText = "โต๊ะ",    FillWeight = 14 },
                new DataGridViewTextBoxColumn { Name = "Staff",    HeaderText = "พนักงาน", FillWeight = 14 },
                new DataGridViewTextBoxColumn { Name = "Subtotal", HeaderText = "Subtotal", FillWeight = 12 },
                new DataGridViewTextBoxColumn { Name = "Tax",      HeaderText = "VAT",      FillWeight = 10 },
                new DataGridViewTextBoxColumn { Name = "Total",    HeaderText = "รวม",      FillWeight = 12 },
                new DataGridViewTextBoxColumn { Name = "Time",     HeaderText = "เวลา",     FillWeight = 14 },
            });

            Controls.Add(_gridOrders);
            Controls.Add(splitter);
            Controls.Add(leftPanel);
            Controls.Add(summaryStrip);
            Controls.Add(topBar);
        }

        private Label MakeSummaryLabel(string caption, string value, Color valueColor)
        {
            return new Label
            {
                Text = $"{caption}\n{value}",
                Font = ThemeManager.FontMediumB,
                ForeColor = valueColor,
                BackColor = ThemeManager.BgCard,
                Dock = DockStyle.Left,
                Width = 155,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 8, 0)
            };
        }

        private void LoadReport()
        {
            _gridOrders.Rows.Clear();
            _topItemsPanel.Controls.Clear();

            var tables = _tableRepo.GetAll();
            var paidOrders = new List<Order>();

            foreach (var table in tables)
            {
                var orders = _orderRepo.GetByTable(table.TableId)
                    .Where(o => o.Status == "Paid" && o.OrderTime.Date == _reportDate)
                    .ToList();
                paidOrders.AddRange(orders);
            }

            decimal subtotalSum = paidOrders.Sum(o => o.SubTotal);
            decimal taxSum      = paidOrders.Sum(o => o.TaxAmt);
            decimal grandSum    = paidOrders.Sum(o => o.GrandTotal);

            _lblCount.Text    = $"Orders\n{paidOrders.Count}";
            _lblSubtotal.Text = $"Subtotal\n฿{subtotalSum:N0}";
            _lblTax.Text      = $"VAT\n฿{taxSum:N0}";
            _lblTotal.Text    = $"Grand Total\n฿{grandSum:N0}";

            // Grid rows
            foreach (var o in paidOrders.OrderByDescending(o => o.OrderTime))
            {
                var table = tables.FirstOrDefault(t => t.TableId == o.TableId);
                _gridOrders.Rows.Add(
                    o.OrderId,
                    table?.TableName ?? $"#{o.TableId}",
                    o.StaffName,
                    $"฿{o.SubTotal:N0}",
                    $"฿{o.TaxAmt:N0}",
                    $"฿{o.GrandTotal:N0}",
                    o.OrderTime.ToString("HH:mm:ss")
                );
            }

            // Top menu items
            var itemCounts = new Dictionary<string, int>();
            foreach (var o in paidOrders)
            {
                foreach (var it in _orderRepo.GetOrderItems(o.OrderId))
                {
                    if (itemCounts.ContainsKey(it.ItemName))
                        itemCounts[it.ItemName] += it.Qty;
                    else
                        itemCounts[it.ItemName] = it.Qty;
                }
            }

            int rank = 1;
            foreach (var kv in itemCounts.OrderByDescending(x => x.Value).Take(10))
            {
                _topItemsPanel.Controls.Add(new Label
                {
                    Text = $"  {rank++}. {kv.Key}  ×{kv.Value}",
                    Font = ThemeManager.FontMedium,
                    ForeColor = rank <= 4 ? ThemeManager.Warning : ThemeManager.TextPrimary,
                    AutoSize = false,
                    Size = new Size(236, 32),
                    TextAlign = ContentAlignment.MiddleLeft
                });
            }

            if (!itemCounts.Any())
                _topItemsPanel.Controls.Add(new Label { Text = "ยังไม่มีข้อมูล", Font = ThemeManager.FontSmall, ForeColor = ThemeManager.TextMuted, AutoSize = true });
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=============================");
            sb.AppendLine($"  {_config.Get("RestaurantName", "Restaurant")}");
            sb.AppendLine($"  รายงานประจำวัน: {_reportDate:dd/MM/yyyy}");
            sb.AppendLine("=============================");

            foreach (DataGridViewRow row in _gridOrders.Rows)
            {
                sb.AppendLine($"#{row.Cells["OrderId"].Value}  {row.Cells["Table"].Value}  {row.Cells["Time"].Value}  {row.Cells["Total"].Value}");
            }

            sb.AppendLine("-----------------------------");
            sb.AppendLine($"Total: {_lblTotal.Text.Split('\n')[1]}");

            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Report_{_reportDate:yyyyMMdd}.txt");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"บันทึกรายงานที่:\n{path}", "Export สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
