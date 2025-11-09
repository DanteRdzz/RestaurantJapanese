namespace RestaurantJapanese.Models
{
    public class SalesReportRowModel
    {
        public System.DateTime BucketStart { get; set; }
        public string Label { get; set; } = "";
        public int Tickets { get; set; }
        public decimal Subtotal { get; set; }
        public decimal IVA { get; set; }
        public decimal Propina { get; set; }
        public decimal Total { get; set; }
    }
}
