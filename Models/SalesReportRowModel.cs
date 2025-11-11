using System;

namespace RestaurantJapanese.Models
{
    public class SalesReportRowModel
    {
        public int IdTicket { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Subtotal { get; set; }
        public int Tax { get; set; }
        public int Total { get; set; }

        // Propiedades calculadas para la UI
        public string FormattedDate => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    }
}
