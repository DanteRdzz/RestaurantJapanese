using System;

namespace RestaurantJapanese.Models
{
    public class SalesReportRowModel
    {
        public int IdTicket { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Subtotal { get; set; } // Cambiado de int a decimal
        public decimal Tax { get; set; }      // Cambiado de int a decimal
        public decimal Total { get; set; }    // Cambiado de int a decimal

        // Propiedades calculadas para la UI
        public string FormattedDate => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    }
}
