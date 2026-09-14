using System.Text;
using NextGenPOS.Models;
using NextGenPOS.Services;
using System.Collections.Generic;

namespace NextGenPOS.Helpers
{
    /// <summary>
    /// Formats receipt text for thermal printer output (monospaced, fixed-width).
    /// </summary>
    public class ReceiptFormatter
    {
        private readonly int _width;
        private readonly ConfigService _config;

        public ReceiptFormatter(ConfigService config)
        {
            _config = config;
            _width = config.GetInt("ReceiptWidth", 48);
        }

        public string Format(Order order, List<OrderItem> items, Payment payment)
        {
            var sb = new StringBuilder();
            string line = new string('=', _width);
            string dash = new string('-', _width);

            sb.AppendLine(Center(_config.Get("RestaurantName", "NextGen Restaurant")));
            sb.AppendLine(line);
            sb.AppendLine($"Receipt: {payment.ReceiptNo}");
            sb.AppendLine($"Date: {payment.PaidAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Table: {order.TableId}  Staff: {order.StaffName}");
            sb.AppendLine(dash);

            // Items
            sb.AppendLine(Row("Item", "Qty", "Amount"));
            sb.AppendLine(dash);
            foreach (var item in items)
            {
                string amt = $"{(item.UnitPrice * item.Qty):N0}";
                sb.AppendLine(Row(item.ItemName, item.Qty.ToString(), amt));
            }
            sb.AppendLine(dash);

            sb.AppendLine(RightAlign("Subtotal:", $"{order.SubTotal:N0}"));
            if (order.DiscountPct > 0)
                sb.AppendLine(RightAlign($"Discount ({order.DiscountPct}%):", $"-{(order.SubTotal * (decimal)(order.DiscountPct / 100)):N0}"));
            sb.AppendLine(RightAlign($"Tax ({_config.GetInt("TaxRate", 7)}%):", $"{order.TaxAmt:N0}"));
            sb.AppendLine(line);
            sb.AppendLine(RightAlign("TOTAL:", $"{order.GrandTotal:N0}"));
            sb.AppendLine(RightAlign("Paid:", $"{payment.Amount:N0} ({payment.Method})"));
            sb.AppendLine(line);
            sb.AppendLine(Center("Thank you!"));

            return sb.ToString();
        }

        private string Center(string text)
        {
            if (text.Length >= _width) return text;
            int pad = (_width - text.Length) / 2;
            return text.PadLeft(pad + text.Length).PadRight(_width);
        }

        private string Row(string col1, string col2, string col3)
        {
            int c3 = 10, c2w = 5;
            int c1w = _width - c3 - c2w;
            return col1.PadRight(c1w).Substring(0, c1w) + col2.PadLeft(c2w) + col3.PadLeft(c3);
        }

        private string RightAlign(string label, string value)
        {
            int valW = 14;
            return label.PadRight(_width - valW) + value.PadLeft(valW);
        }
    }
}
