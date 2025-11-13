using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using RestaurantJapanese.Helpers;
using RestaurantJapanese.Models;
using RestaurantJapanese.Services.Interfaces;
using Microsoft.UI.Xaml;

namespace RestaurantJapanese.ViewModels
{
    public class MenuInventarioAdminVM : BaseViewModel
    {
        private readonly IMenuService _svc;
        public MenuInventarioAdminVM(IMenuService svc) => _svc = svc;

        public Window? OwnWindow { get; set; }

        // Add XamlRoot property to be set by the view
        public XamlRoot? ViewXamlRoot { get; set; }

        // Event to notify view when operation finished (success flag, message)
        public event Action<bool, string?>? OperationCompleted;

        public ObservableCollection<MenuItemModel> Items { get; } = new();

        private MenuItemModel? _selected;
        public MenuItemModel? Selected
        {
            get => _selected;
            set
            {
                Set(ref _selected, value);
                LoadToForm(value);
            }
        }

        // ===== Secciones de navegación (similar a empleados) =====
        private bool _isListSelected = true;
        public bool IsListSelected
        {
            get => _isListSelected;
            set
            {
                Set(ref _isListSelected, value);
                if (value) _ = LoadAsync();
                UpdateVisibilities();
            }
        }

        private bool _isSearchSelected;
        public bool IsSearchSelected
        {
            get => _isSearchSelected;
            set
            {
                Set(ref _isSearchSelected, value);
                UpdateVisibilities();
            }
        }

        private bool _isCreateSelected;
        public bool IsCreateSelected
        {
            get => _isCreateSelected;
            set
            {
                Set(ref _isCreateSelected, value);
                if (value) ClearForm();
                UpdateVisibilities();
            }
        }

        private bool _isUpdateSelected;
        public bool IsUpdateSelected
        {
            get => _isUpdateSelected;
            set
            {
                Set(ref _isUpdateSelected, value);
                if (value) 
                {
                    // Solo limpiar el formulario cuando se activa la pestaña
                    ClearForm();
                }
                UpdateVisibilities();
            }
        }

        private bool _isDeleteSelected;
        public bool IsDeleteSelected
        {
            get => _isDeleteSelected;
            set
            {
                Set(ref _isDeleteSelected, value);
                UpdateVisibilities();
            }
        }

        // Visibilidades para las secciones
        public Visibility ListVisibility => IsListSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SearchVisibility => IsSearchSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CreateVisibility => IsCreateSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UpdateVisibility => IsUpdateSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DeleteVisibility => IsDeleteSelected ? Visibility.Visible : Visibility.Collapsed;

        // Filtros
        private bool _onlyActive = true;
        public bool OnlyActive { get => _onlyActive; set => Set(ref _onlyActive, value); }

        private string? _search;
        public string? Search { get => _search; set => Set(ref _search, value); }

        // Búsqueda por ID para eliminación
        private string? _idLookup;
        public string? IdLookup { get => _idLookup; set => Set(ref _idLookup, value); }

        // Formulario - propiedades con backing fields
        private int _idMenuItem;
        public int IdMenuItem 
        { 
            get => _idMenuItem; 
            set => Set(ref _idMenuItem, value); 
        }

        // Propiedad para el binding del ID como texto
        public string IdMenuItemText
        {
            get => _idMenuItem.ToString();
            set
            {
                if (int.TryParse(value, out var result) && result >= 0)
                {
                    Set(ref _idMenuItem, result);
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    Set(ref _idMenuItem, 0);
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IdMenuItem));
            }
        }

        private string _name = "";
        public string Name 
        { 
            get => _name; 
            set => Set(ref _name, value); 
        }

        private string? _description;
        public string? Description 
        { 
            get => _description; 
            set => Set(ref _description, value); 
        }

        private decimal _price;
        public decimal Price 
        { 
            get => _price; 
            set => Set(ref _price, value); 
        }

        // Propiedad auxiliar para el binding del TextBox de precio
        public string PriceText
        {
            get => _price.ToString("0.##");
            set
            {
                if (decimal.TryParse(value, out var result) && result >= 0)
                {
                    Set(ref _price, result);
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    Set(ref _price, 0m);
                }
                OnPropertyChanged();
            }
        }

        private bool _isActive = true;
        public bool IsActive 
        { 
            get => _isActive; 
            set => Set(ref _isActive, value); 
        }

        private string? _error;
        public string? Error { get => _error; set => Set(ref _error, value); }

        // ===== Comandos =====
        public ICommand LoadAllCommand => new RelayCommand(async _ => await LoadAsync());
        public ICommand SearchCommand => new RelayCommand(async _ => await SearchAsync());
        public ICommand CreateCommand => new RelayCommand(async _ => await CreateAsync());
        public ICommand SaveCommand => new RelayCommand(async _ => await SaveAsync());
        public ICommand SoftDeleteCommand => new RelayCommand(async _ => await SoftDeleteAsync());
        public ICommand LoadByIdCommand => new RelayCommand(async _ => await LoadByIdAsync());
        public ICommand ClearCreateCommand => new RelayCommand(_ => ClearForm());
        public ICommand NewCommand => new RelayCommand(_ => { ClearForm(); IsUpdateSelected = false; IsCreateSelected = true; });

        // ===== Métodos =====
        
        /// <summary>
        /// Carga todos los productos (pestaña Listar)
        /// </summary>
        public async Task LoadAsync()
        {
            Error = null;
            try
            {
                Items.Clear();
                var list = await _svc.GetAllAsync(OnlyActive, string.IsNullOrWhiteSpace(Search) ? null : Search!.Trim());
                foreach (var item in list) 
                {
                    Items.Add(item);
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL al cargar productos ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error al cargar productos: {ex.Message}";
            }
        }

        /// <summary>
        /// Busca productos por término (pestaña Buscar)
        /// </summary>
        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(Search)) 
            {
                Error = "Ingresa un término de búsqueda.";
                return;
            }
            
            Error = null;
            try
            {
                Items.Clear();
                var list = await _svc.GetAllAsync(OnlyActive, Search!.Trim());
                foreach (var item in list) 
                {
                    Items.Add(item);
                }
                
                if (!Items.Any())
                {
                    Error = $"No se encontraron productos que coincidan con '{Search}'.";
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL al buscar productos ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error al buscar productos: {ex.Message}";
            }
        }

        /// <summary>
        /// Crea un nuevo producto (pestaña Crear)
        /// </summary>
        private async Task CreateAsync()
        {
            Error = null;
            
            // Validaciones
            if (string.IsNullOrWhiteSpace(Name)) 
            { 
                Error = "El nombre del producto es requerido."; 
                return; 
            }
            
            if (Price < 0) 
            { 
                Error = "El precio debe ser mayor o igual a cero."; 
                return; 
            }

            try
            {
                var newItem = new MenuItemModel
                {
                    IdMenuItem = 0, // Nuevo producto
                    Name = Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description!.Trim(),
                    Price = Price,
                    IsActive = IsActive
                };

                var created = await _svc.CreateAsync(newItem);
                if (created != null)
                {
                    // Agregar a la lista si estamos en modo "solo activos" y el nuevo item está activo
                    // o si no estamos en modo "solo activos"
                    if (!OnlyActive || created.IsActive)
                    {
                        Items.Add(created);
                    }
                    
                    Selected = created;
                    ClearForm();
                    Error = null;
                    
                    // Mostrar mensaje de éxito usando el evento
                    OperationCompleted?.Invoke(true, $"🍣 Producto '{created.Name}' creado exitosamente!\n\nID: {created.IdMenuItem}\nPrecio: {created.Price:C}");
                }
                else
                {
                    Error = "No se pudo crear el producto. Intenta nuevamente.";
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL al crear producto ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error al crear producto: {ex.Message}";
            }
        }

        /// <summary>
        /// Guarda cambios en un producto existente (pestaña Editar)
        /// </summary>
        private async Task SaveAsync()
        {
            Error = null;
            
            // Validaciones
            if (IdMenuItem <= 0) 
            { 
                Error = "Ingresa un ID válido del producto para editar."; 
                return; 
            }
            
            if (string.IsNullOrWhiteSpace(Name)) 
            { 
                Error = "El nombre del producto es requerido."; 
                return; 
            }
            
            if (Price < 0) 
            { 
                Error = "El precio debe ser mayor o igual a cero."; 
                return; 
            }

            try
            {
                var updatedItem = new MenuItemModel
                {
                    IdMenuItem = IdMenuItem,
                    Name = Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description!.Trim(),
                    Price = Price,
                    IsActive = IsActive
                };

                var updated = await _svc.UpdateAsync(updatedItem);
                if (updated != null)
                {
                    // Actualizar en la lista si existe
                    var existingItem = Items.FirstOrDefault(x => x.IdMenuItem == updated.IdMenuItem);
                    if (existingItem != null)
                    {
                        var index = Items.IndexOf(existingItem);
                        Items[index] = updated;
                    }
                    
                    Selected = updated;
                    Error = null;
                    
                    // Mostrar mensaje de éxito usando el evento con más detalles
                    var statusText = updated.IsActive ? "Activo ✅" : "Inactivo ❌";
                    OperationCompleted?.Invoke(true, 
                        $"🎉 ¡Producto editado correctamente!\n\n" +
                        $"📝 Nombre: {updated.Name}\n" +
                        $"🆔 ID: {updated.IdMenuItem}\n" +
                        $"💰 Precio: {updated.Price:C}\n" +
                        $"📊 Estado: {statusText}\n\n" +
                        $"✨ Todos los cambios se guardaron exitosamente");
                }
                else
                {
                    Error = "No se pudo actualizar el producto. Verifica que el ID exista.";
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL al actualizar producto ({ex.Number}): {ex.Message}";
                OperationCompleted?.Invoke(false, $"Error de base de datos:\n{ex.Message}");
            }
            catch (System.Exception ex)
            {
                Error = $"Error al actualizar producto: {ex.Message}";
                OperationCompleted?.Invoke(false, $"Error inesperado:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Da de baja un producto usando el ID ingresado (pestaña Dar de baja)
        /// </summary>
        private async Task SoftDeleteAsync()
        {
            if (string.IsNullOrWhiteSpace(IdLookup) || !int.TryParse(IdLookup, out var id) || id <= 0)
            {
                Error = "Ingresa un ID válido para dar de baja el producto.";
                return;
            }
            
            Error = null;
            try
            {
                var updated = await _svc.SoftDeleteAsync(id);
                if (updated != null)
                {
                    // Actualizar en la lista si el item existe ahí
                    var existingItem = Items.FirstOrDefault(x => x.IdMenuItem == updated.IdMenuItem);
                    if (existingItem != null)
                    {
                        var index = Items.IndexOf(existingItem);
                        Items[index] = updated; // Ahora IsActive=false
                    }
                    
                    Selected = updated;
                    Error = null;
                    
                    // Mostrar mensaje de éxito usando el evento
                    OperationCompleted?.Invoke(true, 
                        $"🗑️ ¡Producto dado de baja exitosamente!\n\n" +
                        $"📝 Producto: {updated.Name}\n" +
                        $"🆔 ID: {updated.IdMenuItem}\n" +
                        $"💰 Precio: {updated.Price:C}\n" +
                        $"📊 Estado: Inactivo ❌\n\n" +
                        $"ℹ️ El producto ya no aparecerá en las ventas");
                    
                    // Limpiar el campo de búsqueda
                    IdLookup = "";
                }
                else
                {
                    Error = $"No se pudo dar de baja el producto con ID {id}. Verifica que el ID sea correcto.";
                    Selected = null;
                    OperationCompleted?.Invoke(false, $"No se encontró el producto con ID {id} o ya está dado de baja.");
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL al dar de baja producto ({ex.Number}): {ex.Message}";
                Selected = null;
                OperationCompleted?.Invoke(false, $"Error de base de datos:\n{ex.Message}");
            }
            catch (System.Exception ex)
            {
                Error = $"Error al dar de baja producto: {ex.Message}";
                Selected = null;
                OperationCompleted?.Invoke(false, $"Error inesperado:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Busca un producto por ID (pestaña Dar de baja)
        /// </summary>
        private async Task LoadByIdAsync()
        {
            Error = null;
            if (string.IsNullOrWhiteSpace(IdLookup) || !int.TryParse(IdLookup, out var id) || id <= 0)
            {
                Error = "Ingresa un ID válido para buscar.";
                return;
            }

            try
            {
                var found = await _svc.GetByIdAsync(id);
                if (found != null)
                {
                    Selected = found;
                    LoadToForm(found);
                    Error = null;
                }
                else
                {
                    Error = $"No se encontró un producto con ID {id}.";
                    Selected = null;
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL al buscar producto ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error al buscar producto: {ex.Message}";
            }
        }

        /// <summary>
        /// Carga los datos de un producto en el formulario
        /// </summary>
        private void LoadToForm(MenuItemModel? item)
        {
            if (item == null) 
            { 
                ClearForm(); 
                return; 
            }
            
            IdMenuItem = item.IdMenuItem;
            Name = item.Name;
            Description = item.Description;
            Price = item.Price;
            IsActive = item.IsActive;
            OnPropertyChanged(nameof(PriceText));
            OnPropertyChanged(nameof(IdMenuItemText));
        }

        /// <summary>
        /// Limpia el formulario
        /// </summary>
        private void ClearForm()
        {
            IdMenuItem = 0;
            Name = "";
            Description = null;
            Price = 0;
            IsActive = true;
            Error = null;
            OnPropertyChanged(nameof(PriceText));
            OnPropertyChanged(nameof(IdMenuItemText));
        }

        /// <summary>
        /// Actualiza las visibilidades de las secciones
        /// </summary>
        private void UpdateVisibilities()
        {
            OnPropertyChanged(nameof(ListVisibility));
            OnPropertyChanged(nameof(SearchVisibility));
            OnPropertyChanged(nameof(CreateVisibility));
            OnPropertyChanged(nameof(UpdateVisibility));
            OnPropertyChanged(nameof(DeleteVisibility));
        }
    }
}
