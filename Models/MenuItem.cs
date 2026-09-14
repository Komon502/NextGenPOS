namespace NextGenPOS.Models
{
    public class MenuItem
    {
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string ImagePath { get; set; }
    }
}
