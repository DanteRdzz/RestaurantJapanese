namespace RestaurantJapanese.Models
{
    public class CarItemModels
    {
        public int IdMenuItem { get; set; }
        public string Name { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; }
        public decimal LineTotal => UnitPrice * Qty;
    }
}
