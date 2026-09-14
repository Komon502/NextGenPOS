using System.Collections.Generic;
using NextGenPOS.Models;

namespace NextGenPOS.Data
{
    public interface IMenuRepository : IRepository<MenuItem>
    {
        List<Category> GetCategories();
        List<MenuItem> GetByCategory(int categoryId);
    }
}
