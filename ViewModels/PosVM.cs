using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using RestaurantJapanese.Helpers;              // BaseViewModel, RelayCommand
using RestaurantJapanese.Models;              // PosMenuItemModel, CarItemModels, PosTicket*
using RestaurantJapanese.Services.Interfaces; // IPosService
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantJapanese.ViewModels
{
    public class PosVM : BaseViewModel
    {
        private readonly IPosService _pos;
        public PosVM(IPosService pos) => _pos = pos;

        public Window? OwnWindow { get; set; }

        // Carga automática del menú al asignar el usuario
        private bool _menuLoaded;
        private int _currentUserId;
        public int CurrentUserId
        {
            get => _currentUserId;
            set
            {
                Set(ref _currentUserId, value);
                if (!_menuLoaded && _currentUserId > 0)
                {
                    _menuLoaded = true;
                    _ = LoadMenuAsync(); // dispara sin bloquear UI
                }
            }
        }

        // Colecciones
        public ObservableCollection<PosMenuItemModel> Menu { get; } = new();
        public ObservableCollection<CarItemModels> Cart { get; } = new();

        // Selección
        private PosMenuItemModel? _selected;
        public PosMenuItemModel? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        // Cantidad
        private int _qty = 1;
        public int Qty
        {
            get => _qty;
            set => Set(ref _qty, value < 1 ? 1 : value);
        }

        // Totales
        private decimal _tip = 0m;
        public decimal Tip
        {
            get => _tip;
            set
            {
                Set(ref _tip, value);
                Recalc();
            }
        }

        private decimal _taxRate = 0.16m; // IVA 16%
        public decimal TaxRate
        {
            get => _taxRate;
            set
            {
                Set(ref _taxRate, value);
                Recalc();
            }
        }

        private decimal _subtotal, _tax, _total;
        public decimal Subtotal { get => _subtotal; private set => Set(ref _subtotal, value); }
        public decimal Tax { get => _tax; private set => Set(ref _tax, value); }
        public decimal Total { get => _total; private set => Set(ref _total, value); }

        // Estado
        private string? _error;
        public string? Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        // Commands (sin async directo en RelayCommand)
        public ICommand LoadMenuCommand => new RelayCommand(_ => _ = LoadMenuAsync());
        public ICommand AddCommand => new RelayCommand(_ => AddSelected());
        public ICommand RemoveCommand => new RelayCommand(p => RemoveItem(p as CarItemModels));
        public ICommand ClearCommand => new RelayCommand(_ => { Cart.Clear(); Recalc(); });
        public ICommand CheckoutCommand => new RelayCommand(_ => _ = CheckoutAsync());

        // ===== Lógica =====
        public async Task LoadMenuAsync()
        {
            Error = null;
            try
            {
                Menu.Clear();
                var rows = await _pos.GetMenuAsync(); // IEnumerable<PosMenuItemModel>
                foreach (var r in rows) Menu.Add(r);
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (System.Exception ex) { Error = ex.Message; }
        }

        private void AddSelected()
        {
            if (Selected is null || Qty <= 0) return;

            var existing = Cart.FirstOrDefault(x => x.IdMenuItem == Selected.IdMenuItem);
            if (existing is null)
            {
                Cart.Add(new CarItemModels
                {
                    IdMenuItem = Selected.IdMenuItem,
                    Name = Selected.Name,
                    UnitPrice = Selected.Price,
                    Qty = Qty
                });
            }
            else
            {
                existing.Qty += Qty;
                OnPropertyChanged(nameof(Cart)); // refresca UI del item
            }

            Recalc();
        }

        private void RemoveItem(CarItemModels? item)
        {
            if (item is null) return;
            Cart.Remove(item);
            Recalc();
        }

        private void Recalc()
        {
            Subtotal = Cart.Sum(x => x.LineTotal);
            Tax = System.Math.Round(Subtotal * TaxRate, 2);
            Total = Subtotal + Tax + Tip;

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
        }

        private async Task CheckoutAsync()
        {
            Error = null;

            if (Cart.Count == 0) { Error = "Carrito vacío."; return; }
            if (CurrentUserId <= 0) { Error = "Usuario inválido."; return; }

            try
            {
                var items = Cart.Select(c => (c.IdMenuItem, c.Qty));
                var ticket = await _pos.CreateTicketAsync(CurrentUserId, Tip, TaxRate, items);

                Cart.Clear();
                Recalc();

                var msg =
                    $"Ticket #{ticket.Header.IdTicket}\n" +
                    $"Subtotal: {ticket.Header.Subtotal:C}\n" +
                    $"IVA:      {ticket.Header.Tax:C}\n" +
                    $"Propina:  {ticket.Header.Tip:C}\n" +
                    $"Total:    {ticket.Header.Total:C}";

                var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
                if (root is not null)
                {
                    var dlg = new Microsoft.UI.Xaml.Controls.ContentDialog
                    {
                        Title = "Venta registrada",
                        Content = msg,
                        CloseButtonText = "OK",
                        XamlRoot = root
                    };
                    await dlg.ShowAsync();
                }
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (System.Exception ex) { Error = ex.Message; }
        }
    }
}
