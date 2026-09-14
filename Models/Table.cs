namespace NextGenPOS.Models
{
    public class Table
    {
        public int TableId { get; set; }
        public string TableName { get; set; }
        public int Seats { get; set; }
        public string Status { get; set; } // Available, Occupied, Reserved
        public string ZoneArea { get; set; }
    }
}
