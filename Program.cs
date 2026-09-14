using System;
using System.Windows.Forms;
using NextGenPOS.Services;
using NextGenPOS.Data.Mock;
using NextGenPOS.Forms;

namespace NextGenPOS
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var config = new ConfigService("settings.txt");
            bool testMode = config.GetBool("TestMode", true);

            // Repository wiring
            Data.ITableRepository tableRepo;
            Data.IMenuRepository menuRepo;
            Data.IOrderRepository orderRepo;
            Data.IPaymentRepository paymentRepo;

            if (testMode)
            {
                tableRepo = new MockTableRepository();
                menuRepo = new MockMenuRepository();
                orderRepo = new MockOrderRepository();
                paymentRepo = new MockPaymentRepository();
            }
            else
            {
                string dbPath = config.Get("DbPath", @"Database\NextGenPOS.accdb");
                var db = new Data.AccessDb.DbContext(dbPath);
                tableRepo = new Data.AccessDb.TableRepository(db);
                menuRepo = new Data.AccessDb.MenuRepository(db);
                orderRepo = new Data.AccessDb.OrderRepository(db);
                paymentRepo = new Data.AccessDb.PaymentRepository(db);
            }

            Application.Run(new MainForm(config, tableRepo, menuRepo, orderRepo, paymentRepo));
        }
    }
}
