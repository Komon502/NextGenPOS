using NextGenPOS.Models;

namespace NextGenPOS.Data
{
    public interface ITableRepository : IRepository<Table>
    {
        void UpdateStatus(int tableId, string status);
    }
}
