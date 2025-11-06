namespace RestaurantJapanese.Models
{
    public class PosMenuItemModel
    {
        public int IdMenuItem { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
