using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RestaurantJapanese.Helpers;              // BaseViewModel, RelayCommand
using RestaurantJapanese.Models;               // EmployeeModel
using RestaurantJapanese.Services;
using RestaurantJapanese.Services.Interfaces;  // IEmployeesService
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantJapanese.ViewModels
{
    public class AdminEmployeesMenuVM : BaseViewModel
    {
        private readonly IEmployeeService _svc;
        public AdminEmployeesMenuVM(IEmployeeService svc) => _svc = svc;

        public Window? OwnWindow { get; set; }

        // ====== pestañas ======
        private bool _isListSelected = true;
        public bool IsListSelected
        {
            get => _isListSelected;
            set
            {
                Set(ref _isListSelected, value);
                if (value) SelectSection("list");
            }
        }

        private bool _isSearchSelected;
        public bool IsSearchSelected
        {
            get => _isSearchSelected;
            set
            {
                Set(ref _isSearchSelected, value);
                if (value) SelectSection("search");
            }
        }

        private bool _isUpdateSelected;
        public bool IsUpdateSelected
        {
            get => _isUpdateSelected;
            set
            {
                Set(ref _isUpdateSelected, value);
                if (value) SelectSection("update");
            }
        }

        private bool _isDeleteSelected;
        public bool IsDeleteSelected
        {
            get => _isDeleteSelected;
            set
            {
                Set(ref _isDeleteSelected, value);
                if (value) SelectSection("delete");
            }
        }

        private void SelectSection(string key)
        {
            ListVisibility = key == "list" ? Visibility.Visible : Visibility.Collapsed;
            SearchVisibility = key == "search" ? Visibility.Visible : Visibility.Collapsed;
            UpdateVisibility = key == "update" ? Visibility.Visible : Visibility.Collapsed;
            DeleteVisibility = key == "delete" ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility _listV = Visibility.Visible;
        public Visibility ListVisibility { get => _listV; set => Set(ref _listV, value); }

        private Visibility _searchV = Visibility.Collapsed;
        public Visibility SearchVisibility { get => _searchV; set => Set(ref _searchV, value); }

        private Visibility _updateV = Visibility.Collapsed;
        public Visibility UpdateVisibility { get => _updateV; set => Set(ref _updateV, value); }

        private Visibility _deleteV = Visibility.Collapsed;
        public Visibility DeleteVisibility { get => _deleteV; set => Set(ref _deleteV, value); }

        // ====== datos ======
        public ObservableCollection<EmployeeModel> Employees { get; } = new();

        private EmployeeModel _editing = new();
        public EmployeeModel Editing { get => _editing; set => Set(ref _editing, value); }

        private EmployeeModel? _selected;
        public EmployeeModel? Selected
        {
            get => _selected;
            set
            {
                Set(ref _selected, value);
                if (value != null)
                {
                    Editing = new EmployeeModel
                    {
                        IdEmployee = value.IdEmployee,
                        IdUser = value.IdUser,
                        FullName = value.FullName,
                        Email = value.Email,
                        Phone = value.Phone,
                        Role = value.Role,
                        IsActive = value.IsActive,
                        UserName = value.UserName,
                        DisplayName = value.DisplayName,
                        UserIsActive = value.UserIsActive
                    };
                    IdLookup = value.IdEmployee.ToString();
                }
            }
        }

        private bool _onlyActive = true;
        public bool OnlyActive { get => _onlyActive; set => Set(ref _onlyActive, value); }

        private string? _searchText;
        public string? SearchText { get => _searchText; set => Set(ref _searchText, value); }

        private string? _idLookup;
        public string? IdLookup { get => _idLookup; set => Set(ref _idLookup, value); }

        private string? _error;
        public string? Error { get => _error; set => Set(ref _error, value); }

        // ====== commands ======
        public ICommand LoadAllCommand => new RelayCommand(async _ => await LoadAsync());
        public ICommand SearchCommand => new RelayCommand(async _ => await SearchAsync());
        public ICommand LoadByIdCommand => new RelayCommand(async _ => await LoadByIdAsync());
        public ICommand NewCommand => new RelayCommand(_ => Editing = new EmployeeModel());
        public ICommand SaveCommand => new RelayCommand(async _ => await SaveAsync());
        public ICommand SoftDeleteCommand => new RelayCommand(async _ => await SoftDeleteAsync());

        // ====== operaciones ======
        public async Task LoadAsync()
        {
            Error = null;
            try
            {
                Employees.Clear();
                var data = await _svc.GetAllAsync(OnlyActive, null);
                foreach (var e in data) Employees.Add(e);
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (Exception ex) { Error = ex.Message; }
        }

        private async Task SearchAsync()
        {
            Error = null;
            try
            {
                Employees.Clear();
                var q = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
                var data = await _svc.GetAllAsync(OnlyActive, q);
                foreach (var e in data) Employees.Add(e);
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (Exception ex) { Error = ex.Message; }
        }

        private async Task LoadByIdAsync()
        {
            Error = null;
            if (!int.TryParse(IdLookup, out var id) || id <= 0)
            { Error = "Id inválido."; return; }

            try
            {
                var emp = await _svc.GetByIdAsync(id);
                if (emp is null) { Error = "Empleado no encontrado."; return; }
                Selected = emp;
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (Exception ex) { Error = ex.Message; }
        }

        private async Task SaveAsync()
        {
            Error = null;
            if (string.IsNullOrWhiteSpace(Editing.FullName))
            { Error = "Nombre requerido."; return; }

            if (string.IsNullOrWhiteSpace(Editing.Role))
                Editing.Role = "Empleado";

            try
            {
                var saved = await _svc.SaveAsync(Editing); // update
                if (saved is null) { Error = "No se pudo guardar (verifica el Id)."; return; }

                await LoadAsync();
                await DialogAsync("Guardar", "Empleado guardado correctamente.");
                Editing = new EmployeeModel();
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (Exception ex) { Error = ex.Message; }
        }

        private async Task SoftDeleteAsync()
        {
            Error = null;
            if (!int.TryParse(IdLookup, out var id) || id <= 0)
            { Error = "Id inválido."; return; }

            var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
            var confirm = new ContentDialog
            {
                Title = "Dar de baja",
                Content = "¿Confirmas dar de baja (inactivar) al empleado?",
                PrimaryButtonText = "Dar de baja",
                CloseButtonText = "Cancelar",
                XamlRoot = root
            };
            var res = await confirm.ShowAsync();
            if (res != ContentDialogResult.Primary) return;

            try
            {
                await _svc.SoftDeleteAsync(id);
                await LoadAsync();
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (Exception ex) { Error = ex.Message; }
        }

        private async Task DialogAsync(string title, string msg)
        {
            var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
            var dlg = new ContentDialog { Title = title, Content = msg, CloseButtonText = "OK", XamlRoot = root };
            await dlg.ShowAsync();
        }
    }
}
