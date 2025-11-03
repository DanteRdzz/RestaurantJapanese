using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantJapanese.Models
{
    public class CartItem
    {
        public int IdMenuItem { get; set; }
        public string Name { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; } = 1;
        
        public decimal LineTotal => UnitPrice * Qty;
    }
}
