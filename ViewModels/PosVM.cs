using RestaurantJapanese.Data;
using RestaurantJapanese.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace RestaurantJapanese.ViewModels
{
    public class PosVM : BaseVM
    {
        public int UserId { get; set; }
        public string UserDisplay { get; set; } = "";

        public ObservableCollection<MenuItem> Menu { get; } = new();
        public ObservableCollection<CartItem> Cart { get; } = new();

        private decimal _tip;
        public decimal Tip
        {
            get => _tip;
            set { _tip = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(Tax)); OnPropertyChanged(nameof(Total)); }
        }

        public void LoadMenu()
        {
            Menu.Clear();
            foreach (var m in DB.GetMenu()) Menu.Add(m);
        }

        public void AddToCart(MenuItem item)
        {
            var found = Cart.FirstOrDefault(x => x.IdMenuItem == item.IdMenuItem);
            if (found == null)
                Cart.Add(new CartItem { IdMenuItem = item.IdMenuItem, Name = item.Name, UnitPrice = item.Price, Qty = 1 });
            else
            {
                found.Qty++;
                OnPropertyChanged(nameof(Cart));
            }
            Recalc();
        }

        public void RemoveOne(CartItem ci)
        {
            if (ci == null) return;
            ci.Qty--;
            if (ci.Qty <= 0) Cart.Remove(ci);
            Recalc();
        }

        private void Recalc()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
        }

        public decimal Subtotal => Cart.Sum(x => x.UnitPrice * x.Qty);
        public decimal Tax => System.Math.Round(Subtotal * 0.16m, 2);
        public decimal Total => Subtotal + Tax + Tip;

        public int Checkout()
        {
            if (Cart.Count == 0) return 0;
            var items = Cart.ToList();
            var id = DB.CreateTicket(UserId, Tip, items, out var _, out var _, out var _);
            Cart.Clear(); Tip = 0;
            Recalc();
            return id;
        }
    }
}
