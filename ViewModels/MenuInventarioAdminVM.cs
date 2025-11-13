using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RestaurantJapanese.Helpers;
using RestaurantJapanese.Models;
using RestaurantJapanese.Services.Interfaces;
<<<<<<< HEAD
=======
using System;
using System.Threading;
>>>>>>> adding return buttons
using Microsoft.UI.Xaml;

namespace RestaurantJapanese.ViewModels
{
    public class MenuInventarioAdminVM : BaseViewModel
    {
        private readonly IMenuService _svc;
        public MenuInventarioAdminVM(IMenuService svc)
        {
            _svc = svc;
            _loadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
            _saveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy);
            _deleteCommand = new AsyncRelayCommand(SoftDeleteAsync, () => !IsBusy && IdMenuItem > 0);
            _newCommand = new RelayCommand(_ => { if (!IsBusy) ClearForm(); });
        }

        public Window? OwnWindow { get; set; }

        public Window? OwnWindow { get; set; }

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

<<<<<<< HEAD
        // ===== Secciones de navegación (similar a empleados) =====
        private bool _isListSelected = true;
        public bool IsListSelected
        {
            get => _isListSelected;
            set
            {
                Set(ref _isListSelected, value);
                if (value) _ = LoadAsync();
            }
        }

        private bool _isSearchSelected;
        public bool IsSearchSelected
        {
            get => _isSearchSelected;
            set => Set(ref _isSearchSelected, value);
        }

        private bool _isCreateSelected;
        public bool IsCreateSelected
        {
            get => _isCreateSelected;
            set
            {
                Set(ref _isCreateSelected, value);
                if (value) ClearForm();
            }
        }

        private bool _isUpdateSelected;
        public bool IsUpdateSelected
        {
            get => _isUpdateSelected;
            set
            {
                Set(ref _isUpdateSelected, value);
                if (value && Selected != null) LoadToForm(Selected);
            }
        }

        private bool _isDeleteSelected;
        public bool IsDeleteSelected
        {
            get => _isDeleteSelected;
            set => Set(ref _isDeleteSelected, value);
        }

        // Visibilidades para las secciones
        public Visibility ListVisibility => IsListSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SearchVisibility => IsSearchSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CreateVisibility => IsCreateSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UpdateVisibility => IsUpdateSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DeleteVisibility => IsDeleteSelected ? Visibility.Visible : Visibility.Collapsed;

=======
        // Estado
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; private set { Set(ref _isBusy, value); RaiseCanExec(); } }

        private void RaiseCanExec()
        {
            (_loadCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (_saveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (_deleteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

>>>>>>> adding return buttons
        // filtros
        private bool _onlyActive = true;
        public bool OnlyActive { get => _onlyActive; set { Set(ref _onlyActive, value); _ = DebouncedReload(); } }

        private string? _search;
        public string? Search { get => _search; set { Set(ref _search, value); _ = DebouncedReload(); } }

        private CancellationTokenSource? _debounceCts;
        private async Task DebouncedReload()
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;
            try
            {
                await Task.Delay(400, token);
                if (!token.IsCancellationRequested)
                    await LoadAsync();
            }
            catch (TaskCanceledException) { }
        }

        // Búsqueda por ID para eliminación
        private string? _idLookup;
        public string? IdLookup { get => _idLookup; set => Set(ref _idLookup, value); }

        // formulario - converted to proper properties with backing fields
        private int _idMenuItem;
        public int IdMenuItem { get => _idMenuItem; set { Set(ref _idMenuItem, value); RaiseCanExec(); } }

        private string _name = "";
        public string Name { get => _name; set => Set(ref _name, value); }

        private string? _description;
        public string? Description { get => _description; set => Set(ref _description, value); }

        private decimal _price;
        public decimal Price { get => _price; set => Set(ref _price, value); }

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
        public bool IsActive { get => _isActive; set => Set(ref _isActive, value); }

        private string? _error;
        public string? Error { get => _error; set => Set(ref _error, value); }

<<<<<<< HEAD
        // ===== Comandos =====
        public ICommand LoadAllCommand => new RelayCommand(async _ => await LoadAsync());
        public ICommand SearchCommand => new RelayCommand(async _ => await SearchAsync());
        public ICommand CreateCommand => new RelayCommand(async _ => await CreateAsync());
        public ICommand SaveCommand => new RelayCommand(async _ => await SaveAsync());
        public ICommand SoftDeleteCommand => new RelayCommand(async _ => await SoftDeleteAsync());
        public ICommand LoadByIdCommand => new RelayCommand(async _ => await LoadByIdAsync());
        public ICommand ClearCreateCommand => new RelayCommand(_ => ClearForm());
=======
        // comandos
        private readonly ICommand _loadCommand; public ICommand LoadCommand => _loadCommand;
        private readonly ICommand _newCommand; public ICommand NewCommand => _newCommand;
        private readonly ICommand _saveCommand; public ICommand SaveCommand => _saveCommand;
        private readonly ICommand _deleteCommand; public ICommand DeleteCommand => _deleteCommand;
>>>>>>> adding return buttons

        // ===== Métodos =====
        public async Task LoadAsync()
        {
            if (IsBusy) return;
            Error = null;
            IsBusy = true;
            try
            {
                Items.Clear();
                var search = string.IsNullOrWhiteSpace(Search) ? null : Search!.Trim();
                var list = await _svc.GetAllAsync(OnlyActive, search);
                foreach (var it in list) Items.Add(it);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL al cargar datos ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error al cargar datos: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task SearchAsync()
        {
<<<<<<< HEAD
            if (string.IsNullOrWhiteSpace(Search)) 
            {
                Error = "Ingresa un término de búsqueda.";
                return;
            }
            await LoadAsync();
=======
            if (m is null) { ClearForm(); return; }
            IdMenuItem = m.IdMenuItem;
            Name = m.Name;
            Description = m.Description;
            Price = m.Price;
            IsActive = m.IsActive;
            OnPropertyChanged(nameof(PriceText));
>>>>>>> adding return buttons
        }

        private async Task CreateAsync()
        {
<<<<<<< HEAD
            Error = null;
            if (string.IsNullOrWhiteSpace(Name)) { Error = "Nombre es requerido."; return; }
            if (Price < 0) { Error = "Precio inválido."; return; }

            try
            {
                var dto = new MenuItemModel
                {
                    IdMenuItem = 0, // Nuevo producto
                    Name = Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description!.Trim(),
                    Price = Price,
                    IsActive = IsActive
                };

                var created = await _svc.CreateAsync(dto);
                if (created != null)
                {
                    Items.Add(created);
                    Selected = created;
                    ClearForm();
                    Error = null;
                    
                    // Cambiar a la sección de lista para ver el nuevo elemento
                    IsListSelected = true;
                    UpdateVisibilities();
                }
                else
                {
                    Error = "No se pudo crear el elemento del menú.";
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error: {ex.Message}";
            }
=======
            IdMenuItem = 0;
            Name = "";
            Description = null;
            Price = 0;
            IsActive = true;
            _selected = null;
            OnPropertyChanged(nameof(Selected));
            OnPropertyChanged(nameof(PriceText));
            Error = null;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) { Error = "Nombre es requerido."; return false; }
            if (Name.Length > 120) { Error = "Nombre demasiado largo (máx120)."; return false; }
            if (Price < 0) { Error = "Precio inválido."; return false; }
            if (Price > 10000) { Error = "Precio fuera de rango."; return false; }
            return true;
>>>>>>> adding return buttons
        }

        private async Task SaveAsync()
        {
            if (IsBusy) return;
            Error = null;
<<<<<<< HEAD
            if (IdMenuItem <= 0) { Error = "Selecciona un producto para editar."; return; }
            if (string.IsNullOrWhiteSpace(Name)) { Error = "Nombre es requerido."; return; }
            if (Price < 0) { Error = "Precio inválido."; return; }
=======
            if (!Validate()) return;
>>>>>>> adding return buttons

            IsBusy = true;
            try
            {
                var dto = new MenuItemModel
                {
                    IdMenuItem = IdMenuItem,
                    Name = Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description!.Trim(),
                    Price = Price,
                    IsActive = IsActive
                };

                var updated = await _svc.UpdateAsync(dto);
                if (updated != null)
                {
<<<<<<< HEAD
                    var row = Items.FirstOrDefault(x => x.IdMenuItem == updated.IdMenuItem);
                    if (row != null)
                    {
                        var idx = Items.IndexOf(row);
                        Items[idx] = updated;
                    }
                    Selected = updated;
                    Error = null;
                }
                else
                {
                    Error = "No se pudo actualizar el elemento del menú.";
=======
                    var created = await _svc.CreateAsync(dto);
                    if (created != null)
                    {
                        Items.Add(created);
                        Selected = created;
                    }
                    else Error = "No se pudo crear el elemento del menú.";
                }
                else
                {
                    var updated = await _svc.UpdateAsync(dto);
                    if (updated != null)
                    {
                        var row = Items.FirstOrDefault(x => x.IdMenuItem == updated.IdMenuItem);
                        if (row != null)
                        {
                            var idx = Items.IndexOf(row);
                            Items[idx] = updated;
                        }
                        Selected = updated;
                    }
                    else Error = "No se pudo actualizar el elemento del menú.";
>>>>>>> adding return buttons
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task SoftDeleteAsync()
        {
<<<<<<< HEAD
            if (Selected == null || Selected.IdMenuItem <= 0) 
            { 
                Error = "Selecciona un producto para dar de baja."; 
                return; 
            }
            
=======
            if (IsBusy) return;
            if (IdMenuItem <= 0) { Error = "Selecciona un producto."; return; }

            IsBusy = true;
>>>>>>> adding return buttons
            Error = null;
            try
            {
                var updated = await _svc.SoftDeleteAsync(Selected.IdMenuItem);
                if (updated != null)
                {
                    var row = Items.FirstOrDefault(x => x.IdMenuItem == updated.IdMenuItem);
                    if (row != null)
                    {
                        var idx = Items.IndexOf(row);
                        Items[idx] = updated;
                    }
                    Selected = updated;
<<<<<<< HEAD
                    Error = null;
                }
                else
                {
                    Error = "No se pudo dar de baja el elemento del menú.";
=======
>>>>>>> adding return buttons
                }
                else Error = "No se pudo dar de baja el elemento del menú.";
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Error = $"Error SQL al dar de baja ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error al dar de baja: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task LoadByIdAsync()
        {
            Error = null;
            if (string.IsNullOrWhiteSpace(IdLookup) || !int.TryParse(IdLookup, out var id) || id <= 0)
            {
                Error = "Ingresa un ID válido.";
                return;
            }

            try
            {
                var allItems = await _svc.GetAllAsync(false, null); // Incluir inactivos para búsqueda por ID
                var found = allItems.FirstOrDefault(x => x.IdMenuItem == id);
                
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
                Error = $"Error SQL al buscar ({ex.Number}): {ex.Message}";
            }
            catch (System.Exception ex)
            {
                Error = $"Error al buscar: {ex.Message}";
            }
        }

        private void LoadToForm(MenuItemModel? m)
        {
            if (m is null) { ClearForm(); return; }
            IdMenuItem = m.IdMenuItem;
            Name = m.Name;
            Description = m.Description;
            Price = m.Price;
            IsActive = m.IsActive;
            OnPropertyChanged(nameof(PriceText));
        }

        private void ClearForm()
        {
            IdMenuItem = 0;
            Name = "";
            Description = null;
            Price = 0;
            IsActive = true;
            Error = null;
            OnPropertyChanged(nameof(PriceText));
        }

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
