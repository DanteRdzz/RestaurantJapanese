using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RestaurantJapanese.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantJapanese.ViewModels
{
    public class AdminMenuVM : BaseViewModel
    {
        public Window? OwnWindow { get; set; }

        // Propaga el Id del usuario (así el POS puede cobrar)
        private int _currentUserId;
        public int CurrentUserId
        {
            get => _currentUserId;
            set => Set(ref _currentUserId, value);
        }

        // Empleados
        public ICommand OpenEmployeesCommand => new RelayCommand(_ =>
        {
            if (OwnWindow is not null)
                NavigationHelper.ReplaceWindow<
                    RestaurantJapanese.Views.AdminEmployeesMenuView,
                    RestaurantJapanese.ViewModels.AdminEmployeesMenuVM>(OwnWindow);
            else
                NavigationHelper.OpenWindow<
                    RestaurantJapanese.Views.AdminEmployeesMenuView,
                    RestaurantJapanese.ViewModels.AdminEmployeesMenuVM>();
        });

        // Abre el POS desde el panel de administración
        public ICommand OpenPosCommand => new RelayCommand(_ =>
        {
            NavigationHelper.OpenWindow<Views.PosView, PosVM>(
                parameter: null,
                init: (vm, _) =>
                {
                    vm.OwnWindow = null; // Será asignada después
                    vm.CurrentUserId = 1; // ID temporal para demo, idealmente sería el usuario actual
                });
        });

        // Productos (pendiente)
        public ICommand OpenProductsCommand => new RelayCommand(async _ => await PendingAsync("Gestión de Productos"));

        // Reporte de ventas (pendiente)
        public ICommand OpenSalesReportCommand => new RelayCommand(async _ => await PendingAsync("Reporte de Ventas"));

        private async Task PendingAsync(string module)
        {
            var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
            if (root is null) return;

            var dlg = new ContentDialog
            {
                Title = module,
                Content = "Módulo pendiente. En esta demo solo está implementado Empleados y POS.",
                CloseButtonText = "OK",
                XamlRoot = root
            };
            await dlg.ShowAsync();
        }
    }
}
