using System.Collections.Generic;

namespace RestaurantJapanese.Models
{
    public class PosTicketHeader
    {
        public int IdTicket { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Tip { get; set; }
        public decimal Total { get; set; }
        public int CreatedBy { get; set; }
        public string? UserName { get; set; } // se llena en sp_Pos_GetTicket
    }

    public class PosTicketItemRow
    {
        public int IdTicketItem { get; set; }
        public int IdTicket { get; set; }
        public int IdMenuItem { get; set; }
        public string Name { get; set; } = "";
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class PosTicket
    {
        public PosTicketHeader Header { get; set; } = new();
        public List<PosTicketItemRow> Items { get; set; } = new();
    }
}
