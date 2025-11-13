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

        // Información del usuario logueado
        private string _currentUserName = "Administrador";
        public string CurrentUserName
        {
            get => _currentUserName;
            set => Set(ref _currentUserName, value);
        }

        private string _currentUserRole = "Admin";
        public string CurrentUserRole
        {
            get => _currentUserRole;
            set => Set(ref _currentUserRole, value);
        }

        // ===== Empleados =====
        public ICommand OpenEmployeesCommand => new RelayCommand(_ =>
        {
<<<<<<< HEAD
            // Siempre abrir nueva ventana para empleados
            var win = NavigationHelper.OpenWindow<Views.AdminEmployeesMenuView, AdminEmployeesMenuVM>(
                parameter: null, init: (vm, w) => { vm.OwnWindow = w; });
            win.Activate();
=======
            System.Diagnostics.Debug.WriteLine("Abriendo módulo de empleados");

            if (OwnWindow is not null)
            {
                NavigationHelper.ReplaceWindow<Views.AdminEmployeesMenuView, AdminEmployeesMenuVM>(
                    OwnWindow, init: (vm, w) => {
                        vm.OwnWindow = w;
                        System.Diagnostics.Debug.WriteLine("EmployeesMenu cargado en ventana existente");
                    });
            }
            else
            {
                var win = NavigationHelper.OpenWindow<Views.AdminEmployeesMenuView, AdminEmployeesMenuVM>();
                if ((win.Content as FrameworkElement)?.DataContext is AdminEmployeesMenuVM vm)
                    vm.OwnWindow = win;
                win.Activate();
                System.Diagnostics.Debug.WriteLine("EmployeesMenu cargado en nueva ventana");
            }
>>>>>>> adding return buttons
        });

        // ===== POS =====
        public ICommand OpenPosCommand => new RelayCommand(_ =>
        {
            System.Diagnostics.Debug.WriteLine("Abriendo POS");

            if (OwnWindow is not null)
            {
                NavigationHelper.ReplaceWindow<Views.PosView, PosVM>(
                    OwnWindow, init: (vm, w) => {
                        vm.OwnWindow = w;
                        vm.CurrentUserId = CurrentUserId > 0 ? CurrentUserId : 1;
                        _ = vm.LoadMenuAsync();
                        System.Diagnostics.Debug.WriteLine($"POS cargado en ventana existente con userId: {vm.CurrentUserId}");
                    });
            }
            else
            {
                var win = NavigationHelper.OpenWindow<Views.PosView, PosVM>();
                if ((win.Content as FrameworkElement)?.DataContext is PosVM vm)
                {
                    vm.OwnWindow = win;
                    vm.CurrentUserId = CurrentUserId > 0 ? CurrentUserId : 1;
                    vm.CurrentUserName = CurrentUserName; // Pasar información del usuario
                    _ = vm.LoadMenuAsync();
                }
                win.Activate();
                System.Diagnostics.Debug.WriteLine("POS cargado en nueva ventana");
            }
        });

        // ===== Reportes =====
        public ICommand OpenSalesReportCommand => new RelayCommand(_ =>
        {
<<<<<<< HEAD
            var win = NavigationHelper.OpenWindow<Views.ReportsPage, ReportsVM>(
                parameter: null,
                init: (vm, w) =>
                {
                    vm.OwnWindow = w;
                });
            win.Activate();
        });


        // ===== Placeholders (productos, etc.) =====
=======
            System.Diagnostics.Debug.WriteLine("Abriendo reportes");

            if (OwnWindow is not null)
            {
                NavigationHelper.ReplaceWindow<Views.ReportsView, ReportsVM>(
                    OwnWindow, init: (vm, w) => {
                        vm.OwnWindow = w;
                        _ = vm.LoadAsync();
                        System.Diagnostics.Debug.WriteLine("Reportes cargado en ventana existente");
                    });
            }
            else
            {
                var win = NavigationHelper.OpenWindow<Views.ReportsView, ReportsVM>();
                if ((win.Content as FrameworkElement)?.DataContext is ReportsVM vm)
                {
                    vm.OwnWindow = win;
                    _ = vm.LoadAsync();
                }
                win.Activate();
                System.Diagnostics.Debug.WriteLine("Reportes cargado en nueva ventana");
            }
        });

        // ===== Inventario =====
>>>>>>> adding return buttons
        public ICommand OpenInventoryCommand => new RelayCommand(_ =>
        {
            System.Diagnostics.Debug.WriteLine("Abriendo inventario");

            if (OwnWindow is not null)
            {
                NavigationHelper.ReplaceWindow<Views.MenuInventarioAdminView, MenuInventarioAdminVM>(
                    OwnWindow, init: (vm, w) => {
                        vm.OwnWindow = w;
                        System.Diagnostics.Debug.WriteLine("Inventario cargado en ventana existente");
                    });
            }
            else
            {
                var win = NavigationHelper.OpenWindow<Views.MenuInventarioAdminView, MenuInventarioAdminVM>();
                if ((win.Content as FrameworkElement)?.DataContext is MenuInventarioAdminVM vm)
                    vm.OwnWindow = win;
                win.Activate();
                System.Diagnostics.Debug.WriteLine("Inventario cargado en nueva ventana");
            }
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
