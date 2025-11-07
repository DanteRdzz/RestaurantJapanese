using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RestaurantJapanese.Helpers;              // BaseViewModel, RelayCommand
using RestaurantJapanese.Models;               // EmployeeModel
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

        // Event to notify view when create finished (success flag, message)
        public event Action<bool, string?>? CreateCompleted;

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

        private bool _isCreateSelected;
        public bool IsCreateSelected
        {
            get => _isCreateSelected;
            set
            {
                Set(ref _isCreateSelected, value);
                if (value) SelectSection("create");
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
            CreateVisibility = key == "create" ? Visibility.Visible : Visibility.Collapsed;
            UpdateVisibility = key == "update" ? Visibility.Visible : Visibility.Collapsed;
            DeleteVisibility = key == "delete" ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility _listV = Visibility.Visible;
        public Visibility ListVisibility { get => _listV; set => Set(ref _listV, value); }

        private Visibility _searchV = Visibility.Collapsed;
        public Visibility SearchVisibility { get => _searchV; set => Set(ref _searchV, value); }

        private Visibility _createV = Visibility.Collapsed;
        public Visibility CreateVisibility { get => _createV; set => Set(ref _createV, value); }

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

        // Create request model
        private EmployeeCreateRequest _createReq = new();
        public EmployeeCreateRequest CreateRequest { get => _createReq; set => Set(ref _createReq, value); }

        // Password for Create (PasswordBox cannot bind to Password property directly)
        private string? _createPassword;
        public string? CreatePassword { get => _createPassword; set => Set(ref _createPassword, value); }

        // ====== commands ======
        public ICommand LoadAllCommand => new RelayCommand(async _ => await LoadAsync());
        public ICommand SearchCommand => new RelayCommand(async _ => await SearchAsync());
        public ICommand LoadByIdCommand => new RelayCommand(async _ => await LoadByIdAsync());
        public ICommand NewCommand => new RelayCommand(_ => Editing = new EmployeeModel());
        public ICommand SaveCommand => new RelayCommand(async _ => await SaveAsync());
        public ICommand SoftDeleteCommand => new RelayCommand(async _ => await SoftDeleteAsync());

        // Create commands
        public ICommand CreateCommand => new RelayCommand(async _ => await CreateAsync());
        public ICommand ClearCreateCommand => new RelayCommand(_ => { CreateRequest = new EmployeeCreateRequest(); CreatePassword = null; });

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

        private async Task CreateAsync()
        {
            Error = null;
            if (string.IsNullOrWhiteSpace(CreateRequest.FullName)) { Error = "Nombre requerido."; CreateCompleted?.Invoke(false, Error); return; }
            if (string.IsNullOrWhiteSpace(CreateRequest.UserName)) { Error = "Usuario requerido."; CreateCompleted?.Invoke(false, Error); return; }
            if (string.IsNullOrWhiteSpace(CreatePassword)) { Error = "Contraseña requerida."; CreateCompleted?.Invoke(false, Error); return; }

            try
            {
                // Map password
                CreateRequest.PasswordText = CreatePassword;

                var created = await _svc.CreateWithUserAsync(CreateRequest);
                if (created is null)
                {
                    Error = "No se pudo crear el empleado.";
                    CreateCompleted?.Invoke(false, Error);
                    return;
                }

                await LoadAsync();

                // Limpiar formulario en VM (View también limpiará PasswordBox en handler)
                CreateRequest = new EmployeeCreateRequest();
                CreatePassword = null;

                CreateCompleted?.Invoke(true, "Empleado y usuario creados correctamente.");
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; CreateCompleted?.Invoke(false, Error); }
            catch (Exception ex) { Error = ex.Message; CreateCompleted?.Invoke(false, Error); }
        }

        private async Task DialogAsync(string title, string msg)
        {
            var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
            var dlg = new ContentDialog { Title = title, Content = msg, CloseButtonText = "OK", XamlRoot = root };
            await dlg.ShowAsync();
        }
    }
}
