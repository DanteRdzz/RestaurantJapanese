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

        // Abre el submenú de Empleados (reemplaza la ventana actual si la tienes)
        public ICommand OpenEmployeesCommand => new RelayCommand(_ =>
        {
            if (OwnWindow is not null)
                NavigationHelper.ReplaceWindow<
                    Views.AdminEmployeesMenuView,
                    AdminEmployeesMenuVM>(OwnWindow);
            else
                NavigationHelper.OpenWindow<
                    Views.AdminEmployeesMenuView,
                    AdminEmployeesMenuVM>();
        });

        // Por ahora, los demás módulos muestran “pendiente”
        public ICommand OpenProductsCommand => new RelayCommand(async _ => await PendingAsync("Gestión de Productos"));
        public ICommand OpenSalesReportCommand => new RelayCommand(async _ => await PendingAsync("Reporte de Ventas"));
        public ICommand OpenInventoryCommand => new RelayCommand(async _ => await PendingAsync("Inventario & Proveedores"));

        private async Task PendingAsync(string module)
        {
            var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
            var dlg = new ContentDialog
            {
                Title = module,
                Content = "En esta demo solo está implementado el módulo de Empleados. Próximamente agregaremos este módulo.",
                CloseButtonText = "OK",
                XamlRoot = root
            };
            await dlg.ShowAsync();
        }
    }
}
