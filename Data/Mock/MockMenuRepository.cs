using System.Collections.Generic;
using System.Linq;
using NextGenPOS.Data;
using NextGenPOS.Models;

namespace NextGenPOS.Data.Mock
{
    public class MockMenuRepository : IMenuRepository
    {
        private readonly List<Category> _cats;
        private readonly List<MenuItem> _items;
        private int _nextItemId;

        public MockMenuRepository()
        {
            _cats = new List<Category>
            {
                new Category { CategoryId=1, Name="Appetizers", SortOrder=1, IsActive=true },
                new Category { CategoryId=2, Name="Main Course", SortOrder=2, IsActive=true },
                new Category { CategoryId=3, Name="Drinks",      SortOrder=3, IsActive=true },
                new Category { CategoryId=4, Name="Desserts",    SortOrder=4, IsActive=true },
            };
            _items = new List<MenuItem>
            {
                // Appetizers
                new MenuItem { ItemId=1,  CategoryId=1, Name="Spring Rolls",    Price=89m,  IsActive=true, ImagePath=@"Images\spring_rolls.jpg" },
                new MenuItem { ItemId=2,  CategoryId=1, Name="Chicken Wings",   Price=129m, IsActive=true },
                new MenuItem { ItemId=3,  CategoryId=1, Name="Garlic Bread",    Price=69m,  IsActive=true },
                new MenuItem { ItemId=4,  CategoryId=1, Name="Tom Yum Soup",    Price=99m,  IsActive=true },
                // Main Course
                new MenuItem { ItemId=5,  CategoryId=2, Name="Pad Thai",        Price=149m, IsActive=true },
                new MenuItem { ItemId=6,  CategoryId=2, Name="Green Curry",     Price=169m, IsActive=true },
                new MenuItem { ItemId=7,  CategoryId=2, Name="Fried Rice",      Price=129m, IsActive=true },
                new MenuItem { ItemId=8,  CategoryId=2, Name="Grilled Salmon",  Price=289m, IsActive=true },
                new MenuItem { ItemId=9,  CategoryId=2, Name="Steak",           Price=349m, IsActive=true },
                // Drinks
                new MenuItem { ItemId=10, CategoryId=3, Name="Thai Iced Tea",   Price=49m,  IsActive=true },
                new MenuItem { ItemId=11, CategoryId=3, Name="Coke",            Price=35m,  IsActive=true },
                new MenuItem { ItemId=12, CategoryId=3, Name="Fresh Juice",     Price=69m,  IsActive=true },
                new MenuItem { ItemId=13, CategoryId=3, Name="Beer Chang",      Price=89m,  IsActive=true },
                // Desserts
                new MenuItem { ItemId=14, CategoryId=4, Name="Mango Sticky Rice", Price=99m, IsActive=true },
                new MenuItem { ItemId=15, CategoryId=4, Name="Ice Cream",        Price=59m,  IsActive=true },
            };
            _nextItemId = 16;
        }

        public List<Category> GetCategories() => _cats.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToList();
        public List<MenuItem> GetByCategory(int catId) => _items.Where(i => i.CategoryId == catId && i.IsActive).ToList();
        public List<MenuItem> GetAll() => _items.Where(i => i.IsActive).ToList();
        public MenuItem GetById(int id) => _items.FirstOrDefault(i => i.ItemId == id);
        public int Insert(MenuItem i) { i.ItemId = _nextItemId++; _items.Add(i); return i.ItemId; }
        public void Update(MenuItem i) { var e = GetById(i.ItemId); if(e!=null){ e.Name=i.Name; e.Price=i.Price; e.CategoryId=i.CategoryId; e.IsActive=i.IsActive; } }
        public void Delete(int id) { var e = GetById(id); if(e!=null) e.IsActive=false; }
    }
}
