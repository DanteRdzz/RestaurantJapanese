using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RestaurantJapanese.ViewModels;

namespace RestaurantJapanese.Views
{
    public sealed partial class MenuInventarioAdminView : UserControl
    {
        public MenuInventarioAdminView()
        {
            InitializeComponent();
            this.Loaded += MenuInventarioAdminView_Loaded;
            this.Unloaded += MenuInventarioAdminView_Unloaded;
        }

        private void MenuInventarioAdminView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MenuInventarioAdminVM vm)
            {
                vm.OperationCompleted += Vm_OperationCompleted;
                // Pass the UserControl's XamlRoot
                vm.ViewXamlRoot = this.XamlRoot;
            }
        }

        private void MenuInventarioAdminView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MenuInventarioAdminVM vm)
            {
                vm.OperationCompleted -= Vm_OperationCompleted;
            }
        }

        private async void Vm_OperationCompleted(bool success, string? message)
        {
            var root = this.XamlRoot;
            var dlg = new ContentDialog
            {
                Title = success ? "? Operación exitosa" : "? Error",
                Content = message ?? (success ? "Operación completada correctamente." : "Ocurrió un error."),
                CloseButtonText = "OK",
                XamlRoot = root
            };

            _ = dlg.ShowAsync();
        }
    }
}
