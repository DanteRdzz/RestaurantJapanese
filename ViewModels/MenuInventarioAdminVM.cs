using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RestaurantJapanese.Helpers;
using RestaurantJapanese.Models;
using RestaurantJapanese.Services.Interfaces;

namespace RestaurantJapanese.ViewModels
{
    public class MenuInventarioAdminVM : BaseViewModel
    {
        private readonly IMenuService _svc;
        public MenuInventarioAdminVM(IMenuService svc) => _svc = svc;

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

        // filtros
        private bool _onlyActive = true;
        public bool OnlyActive { get => _onlyActive; set => Set(ref _onlyActive, value); }

        private string? _search;
        public string? Search { get => _search; set => Set(ref _search, value); }

        // formulario - converted to proper properties with backing fields
        private int _idMenuItem;
        public int IdMenuItem 
        { 
            get => _idMenuItem; 
            set => Set(ref _idMenuItem, value); 
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

        private bool _isActive = true;
        public bool IsActive 
        { 
            get => _isActive; 
            set => Set(ref _isActive, value); 
        }

        private string? _error;
        public string? Error { get => _error; set => Set(ref _error, value); }

        // comandos
        public ICommand LoadCommand => new RelayCommand(async _ => await LoadAsync());
        public ICommand NewCommand => new RelayCommand(_ => ClearForm());
        public ICommand SaveCommand => new RelayCommand(async _ => await SaveAsync());
        public ICommand DeleteCommand => new RelayCommand(async _ => await SoftDeleteAsync());

        public async Task LoadAsync()
        {
            Error = null;
            Items.Clear();
            var list = await _svc.GetAllAsync(OnlyActive, string.IsNullOrWhiteSpace(Search) ? null : Search!.Trim());
            foreach (var it in list) Items.Add(it);
        }

        private void LoadToForm(MenuItemModel? m)
        {
            if (m is null) { ClearForm(); return; }
            IdMenuItem = m.IdMenuItem;
            Name = m.Name;
            Description = m.Description;
            Price = m.Price;
            IsActive = m.IsActive;
        }

        private void ClearForm()
        {
            IdMenuItem = 0;
            Name = "";
            Description = null;
            Price = 0;
            IsActive = true;
            // Fix: Set backing field directly to avoid recursive loop
            _selected = null;
            OnPropertyChanged(nameof(Selected));
        }

        private async Task SaveAsync()
        {
            Error = null;
            if (string.IsNullOrWhiteSpace(Name)) { Error = "Nombre es requerido."; return; }
            if (Price < 0) { Error = "Precio inválido."; return; }

            var dto = new MenuItemModel
            {
                IdMenuItem = IdMenuItem,
                Name = Name.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description!.Trim(),
                Price = Price,
                IsActive = IsActive
            };

            if (dto.IdMenuItem == 0)
            {
                var created = await _svc.CreateAsync(dto);
                Items.Add(created);
                Selected = created;
            }
            else
            {
                var updated = await _svc.UpdateAsync(dto);
                var row = Items.FirstOrDefault(x => x.IdMenuItem == updated.IdMenuItem);
                if (row != null)
                {
                    var idx = Items.IndexOf(row);
                    Items[idx] = updated;
                }
                Selected = updated;
            }
        }

        private async Task SoftDeleteAsync()
        {
            if (IdMenuItem <= 0) { Error = "Selecciona un producto."; return; }
            var updated = await _svc.SoftDeleteAsync(IdMenuItem);
            if (updated != null)
            {
                var row = Items.FirstOrDefault(x => x.IdMenuItem == updated.IdMenuItem);
                if (row != null)
                {
                    var idx = Items.IndexOf(row);
                    Items[idx] = updated; // ahora IsActive=false
                }
                Selected = updated;
            }
        }
    }
}
