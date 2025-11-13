using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using RestaurantJapanese.Helpers; // BaseViewModel, RelayCommand, AsyncRelayCommand
using RestaurantJapanese.Models; // PosMenuItemModel, CarItemModels, PosTicket*
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
        public PosVM(IPosService pos)
        {
            _pos = pos;
            _loadMenuCommand = new AsyncRelayCommand(LoadMenuAsync, () => !IsBusy);
            _checkoutCommand = new AsyncRelayCommand(CheckoutAsync, () => !IsBusy && Cart.Count>0 && CurrentUserId>0);
        }

        public Window? OwnWindow { get; set; }

<<<<<<< HEAD
        // Configuración de límites de negocio
        private const int MAX_ITEMS_IN_CART = 20; // Límite máximo de diferentes items en el carrito
        private const int MAX_QUANTITY_PER_ITEM = 10; // Cantidad máxima por item individual
        private const int MAX_TOTAL_ITEMS = 50; // Límite total de items (suma de todas las cantidades)

        // Propiedades de usuario logueado
        private string _currentUserName = "Usuario";
        public string CurrentUserName
        {
            get => _currentUserName;
            set => Set(ref _currentUserName, value);
        }
=======
        // Estado de ejecución
        private bool _isBusy; public bool IsBusy { get => _isBusy; private set { Set(ref _isBusy, value); RaiseCanExec(); } }
        private void RaiseCanExec() { (_loadMenuCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged(); (_checkoutCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged(); }
>>>>>>> adding return buttons

        // Carga automática del menú al asignar el usuario
        private bool _menuLoaded;
        private int _currentUserId;
        public int CurrentUserId
        {
            get => _currentUserId;
            set
            {
                Set(ref _currentUserId, value);
                RaiseCanExec();
                if (!_menuLoaded && _currentUserId >0)
                {
                    _menuLoaded = true;
                    _ = LoadMenuAsync();
                }
            }
        }

        // Colecciones
        public ObservableCollection<PosMenuItemModel> Menu { get; } = new();
        public ObservableCollection<CarItemModels> Cart { get; } = new();

        // Selección
        private PosMenuItemModel? _selected;
        public PosMenuItemModel? Selected { get => _selected; set => Set(ref _selected, value); }

        // Cantidad
<<<<<<< HEAD
        private int _qty = 1;
        public int Qty
        {
            get => _qty;
            set => Set(ref _qty, value < 1 ? 1 : (value > MAX_QUANTITY_PER_ITEM ? MAX_QUANTITY_PER_ITEM : value));
        }
=======
        private int _qty =1;
        public int Qty { get => _qty; set => Set(ref _qty, value <1 ?1 : value); }
>>>>>>> adding return buttons

        // Propiedades para mostrar información de límites
        public int MaxItemsInCart => MAX_ITEMS_IN_CART;
        public int MaxQuantityPerItem => MAX_QUANTITY_PER_ITEM;
        public int MaxTotalItems => MAX_TOTAL_ITEMS;
        
        public int CurrentItemCount => Cart.Count;
        public int CurrentTotalQuantity => Cart.Sum(x => x.Qty);
        
        public string CartStatus => $"{CurrentItemCount}/{MaxItemsInCart} productos • {CurrentTotalQuantity}/{MaxTotalItems} items";

        // Totales
<<<<<<< HEAD
        private decimal _tip = 0m;
        public decimal Tip
        {
            get => _tip;
            set
            {
                System.Diagnostics.Debug.WriteLine($"[PosVM] Setting Tip from {_tip} to {value}");
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
=======
        private decimal _tip =0m; public decimal Tip { get => _tip; set { Set(ref _tip, value); Recalc(); } }
        private decimal _taxRate =0.16m; public decimal TaxRate { get => _taxRate; set { Set(ref _taxRate, value); Recalc(); } }
        private decimal _subtotal, _tax, _total; public decimal Subtotal { get => _subtotal; private set => Set(ref _subtotal, value); } public decimal Tax { get => _tax; private set => Set(ref _tax, value); } public decimal Total { get => _total; private set => Set(ref _total, value); }
>>>>>>> adding return buttons

        // Estado
        private string? _error; public string? Error { get => _error; set => Set(ref _error, value); }

<<<<<<< HEAD
        private string? _warning;
        public string? Warning
        {
            get => _warning;
            set => Set(ref _warning, value);
        }

        // Commands (sin async directo en RelayCommand)
        public ICommand LoadMenuCommand => new RelayCommand(_ => _ = LoadMenuAsync());
        public ICommand AddCommand => new RelayCommand(_ => AddSelected());
        public ICommand RemoveCommand => new RelayCommand(p => RemoveItem(p as CarItemModels));
        public ICommand ClearCommand => new RelayCommand(_ => { Cart.Clear(); Recalc(); UpdateCartStatus(); });
        public ICommand CheckoutCommand => new RelayCommand(_ => _ = CheckoutAsync());
=======
        // Commands
        private readonly ICommand _loadMenuCommand; public ICommand LoadMenuCommand => _loadMenuCommand;
        public ICommand AddCommand => new RelayCommand(_ => AddSelected());
        public ICommand RemoveCommand => new RelayCommand(p => RemoveItem(p as CarItemModels));
        public ICommand ClearCommand => new RelayCommand(_ => { Cart.Clear(); Recalc(); RaiseCanExec(); });
        private readonly ICommand _checkoutCommand; public ICommand CheckoutCommand => _checkoutCommand;
>>>>>>> adding return buttons

        // ===== Lógica =====
        public async Task LoadMenuAsync()
        {
            if (IsBusy) return;
            Error = null; IsBusy = true;
            try
            {
                Menu.Clear();
                var rows = await _pos.GetMenuAsync();
                foreach (var r in rows) Menu.Add(r);
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (System.Exception ex) { Error = ex.Message; }
            finally { IsBusy = false; }
        }

        private void AddSelected()
        {
<<<<<<< HEAD
            if (Selected is null || Qty <= 0) 
            {
                Error = "Selecciona un producto y cantidad válida.";
                return;
            }

            // Validación de límites de negocio
            var existingItem = Cart.FirstOrDefault(x => x.IdMenuItem == Selected.IdMenuItem);
            var newQuantity = existingItem?.Qty + Qty ?? Qty;
            var newTotalQuantity = CurrentTotalQuantity + Qty;

            // Validar límite máximo de productos diferentes
            if (existingItem == null && CurrentItemCount >= MAX_ITEMS_IN_CART)
            {
                Error = $"No puedes agregar más productos. Límite máximo: {MAX_ITEMS_IN_CART} productos diferentes.";
                Warning = "Considera eliminar algunos productos antes de agregar nuevos.";
                return;
            }

            // Validar cantidad máxima por item
            if (newQuantity > MAX_QUANTITY_PER_ITEM)
            {
                Error = $"Cantidad máxima por producto: {MAX_QUANTITY_PER_ITEM} unidades.";
                Warning = $"Intentaste agregar {Qty}, pero solo puedes tener {MAX_QUANTITY_PER_ITEM - (existingItem?.Qty ?? 0)} más de este producto.";
                return;
            }

            // Validar límite total de items
            if (newTotalQuantity > MAX_TOTAL_ITEMS)
            {
                Error = $"Límite total de items excedido. Máximo: {MAX_TOTAL_ITEMS} items en total.";
                Warning = $"Tienes {CurrentTotalQuantity} items. Solo puedes agregar {MAX_TOTAL_ITEMS - CurrentTotalQuantity} más.";
                return;
            }

            // Limpiar errores si llegamos aquí
            Error = null;
            Warning = null;

            // Agregar al carrito
            if (existingItem == null)
=======
            if (Selected is null || Qty <=0) return;
            var existing = Cart.FirstOrDefault(x => x.IdMenuItem == Selected.IdMenuItem);
            if (existing is null)
>>>>>>> adding return buttons
            {
                Cart.Add(new CarItemModels { IdMenuItem = Selected.IdMenuItem, Name = Selected.Name, UnitPrice = Selected.Price, Qty = Qty });
            }
<<<<<<< HEAD
            else
            {
                existingItem.Qty = newQuantity;
                OnPropertyChanged(nameof(Cart)); // refresca UI del item
            }

            Recalc();
            UpdateCartStatus();

            // Mostrar advertencia si se está acercando a los límites
            CheckLimitsWarning();
=======
            else { existing.Qty += Qty; OnPropertyChanged(nameof(Cart)); }
            Recalc(); RaiseCanExec();
>>>>>>> adding return buttons
        }

        private void RemoveItem(CarItemModels? item)
        {
<<<<<<< HEAD
            if (item is null) return;
            Cart.Remove(item);
            Recalc();
            UpdateCartStatus();
            
            // Limpiar errores si se quita un item
            if (!string.IsNullOrEmpty(Error))
            {
                Error = null;
                Warning = null;
            }
=======
            if (item is null) return; Cart.Remove(item); Recalc(); RaiseCanExec();
>>>>>>> adding return buttons
        }

        private void Recalc()
        {
            Subtotal = Cart.Sum(x => x.LineTotal);
            Tax = System.Math.Round(Subtotal * TaxRate,2);
            Total = Subtotal + Tax + Tip;
<<<<<<< HEAD

            System.Diagnostics.Debug.WriteLine($"[PosVM] Recalc - Subtotal: {Subtotal}, Tax: {Tax}, Tip: {Tip}, Total: {Total}");

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
=======
            OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(Tax)); OnPropertyChanged(nameof(Total));
>>>>>>> adding return buttons
        }

        private void UpdateCartStatus()
        {
            OnPropertyChanged(nameof(CurrentItemCount));
            OnPropertyChanged(nameof(CurrentTotalQuantity));
            OnPropertyChanged(nameof(CartStatus));
        }

        private void CheckLimitsWarning()
        {
            var itemPercentage = (double)CurrentItemCount / MAX_ITEMS_IN_CART;
            var quantityPercentage = (double)CurrentTotalQuantity / MAX_TOTAL_ITEMS;

            if (itemPercentage >= 0.8 || quantityPercentage >= 0.8)
            {
                Warning = $"⚠️ Te estás acercando a los límites del carrito ({CurrentItemCount}/{MAX_ITEMS_IN_CART} productos, {CurrentTotalQuantity}/{MAX_TOTAL_ITEMS} items)";
            }
            else if (itemPercentage >= 0.9 || quantityPercentage >= 0.9)
            {
                Warning = $"🚨 Casi en el límite del carrito. Considera procesar la venta pronto.";
            }
        }

        private async Task CheckoutAsync()
        {
            if (IsBusy) return;
            Error = null;
<<<<<<< HEAD
            Warning = null;

            if (Cart.Count == 0) 
            { 
                Error = "Carrito vacío."; 
                return; 
            }
            
            if (CurrentUserId <= 0) 
            { 
                Error = "Usuario inválido."; 
                return; 
            }

            try
            {
                var items = Cart.Select(c => (c.IdMenuItem, c.Qty));
                
                System.Diagnostics.Debug.WriteLine($"[PosVM] CheckoutAsync - CreatedBy: {CurrentUserId}, Tip: {Tip}, TaxRate: {TaxRate}");
                
                var ticket = await _pos.CreateTicketAsync(CurrentUserId, Tip, TaxRate, items);

                Cart.Clear();
                Tip = 0m;
                Recalc();
                UpdateCartStatus();

                var msg =
                    $"✅ Venta procesada exitosamente\n\n" +
                    $"Ticket #{ticket.Header.IdTicket}\n" +
                    $"Fecha: {ticket.Header.CreatedAt:dd/MM/yyyy HH:mm}\n\n" +
                    $"Subtotal: {ticket.Header.Subtotal:C}\n" +
                    $"IVA (16%): {ticket.Header.Tax:C}\n" +
                    $"Propina:  {ticket.Header.Tip:C}\n" +
                    $"━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                    $"TOTAL:    {ticket.Header.Total:C}";

                var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
                if (root is not null)
                {
                    var dlg = new Microsoft.UI.Xaml.Controls.ContentDialog
                    {
                        Title = "🎉 Venta Registrada",
                        Content = msg,
                        CloseButtonText = "OK",
                        XamlRoot = root
                    };
=======
            if (Cart.Count ==0) { Error = "Carrito vacío."; return; }
            if (CurrentUserId <=0) { Error = "Usuario inválido."; return; }
            IsBusy = true;
            try
            {
                var items = Cart.Select(c => (c.IdMenuItem, c.Qty));
                var ticket = await _pos.CreateTicketAsync(CurrentUserId, Tip, TaxRate, items);
                Cart.Clear(); Tip =0m; Recalc(); RaiseCanExec();
                var msg = $"Ticket #{ticket.Header.IdTicket}\nSubtotal: {ticket.Header.Subtotal:C}\nIVA: {ticket.Header.Tax:C}\nPropina: {ticket.Header.Tip:C}\nTotal: {ticket.Header.Total:C}";
                var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
                if (root is not null)
                {
                    var dlg = new Microsoft.UI.Xaml.Controls.ContentDialog { Title = "Venta registrada", Content = msg, CloseButtonText = "OK", XamlRoot = root };
>>>>>>> adding return buttons
                    await dlg.ShowAsync();
                }
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (System.Exception ex) { Error = ex.Message; }
            finally { IsBusy = false; }
        }
    }
}
