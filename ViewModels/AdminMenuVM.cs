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

        // Propaga el Id del usuario (para POS)
        private int _currentUserId;
        public int CurrentUserId
        {
            get => _currentUserId;
            set => Set(ref _currentUserId, value);
        }

        // ===== Empleados =====
        public ICommand OpenEmployeesCommand => new RelayCommand(_ =>
        {
            if (OwnWindow is not null)
            {
                NavigationHelper.ReplaceWindow<Views.AdminEmployeesMenuView, AdminEmployeesMenuVM>(
                    OwnWindow, parameter: null, init: (vm, w) => { vm.OwnWindow = w; });
            }
            else
            {
                var win = NavigationHelper.OpenWindow<Views.AdminEmployeesMenuView, AdminEmployeesMenuVM>(
                    parameter: null, init: (vm, w) => { vm.OwnWindow = w; });
                win.Activate();
            }
        });

        // ===== POS =====
        public ICommand OpenPosCommand => new RelayCommand(_ =>
        {
            var win = NavigationHelper.OpenWindow<Views.PosView, PosVM>(
                parameter: null,
                init: (vm, w) =>
                {
                    vm.OwnWindow = w;
                    vm.CurrentUserId = CurrentUserId > 0 ? CurrentUserId : 1;
                    _ = vm.LoadMenuAsync();
                });
            win.Activate();
        });

        // ===== Reportes =====
        public ICommand OpenSalesReportCommand => new RelayCommand(_ =>
        {
            if (OwnWindow is not null)
            {
                NavigationHelper.ReplaceWindow<Views.ReportsView, ReportsVM>(
                    OwnWindow, parameter: null, init: (vm, w) => { vm.OwnWindow = w; _ = vm.LoadAsync(); });
            }
            else
            {
                var win = NavigationHelper.OpenWindow<Views.ReportsView, ReportsVM>(
                    parameter: null, init: (vm, w) => { vm.OwnWindow = w; _ = vm.LoadAsync(); });
                win.Activate();
            }
        });

        // ===== Placeholders (productos, etc.) =====
        public ICommand OpenInventoryCommand => new RelayCommand(_ =>
        {
            var win = NavigationHelper.OpenWindow<Views.MenuInventarioAdminView, MenuInventarioAdminVM>();
            win.Activate();
        });
        private async Task PendingAsync(string module)
        {
            var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
            if (root is null) return;

            var dlg = new ContentDialog
            {
                Title = module,
                Content = "Módulo pendiente. En esta demo solo está implementado Empleados, POS y Reportes.",
                CloseButtonText = "OK",
                XamlRoot = root
            };
            await dlg.ShowAsync();
        }
    }
}
