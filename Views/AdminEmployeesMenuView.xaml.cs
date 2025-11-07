using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using RestaurantJapanese.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestaurantJapanese.Views
{
    public sealed partial class AdminEmployeesMenuView : UserControl
    {
        public AdminEmployeesMenuView()
        {
            InitializeComponent();
            this.Loaded += AdminEmployeesMenuView_Loaded;
            this.Unloaded += AdminEmployeesMenuView_Unloaded;
        }

        private void AdminEmployeesMenuView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AdminEmployeesMenuVM vm)
            {
                vm.CreateCompleted += Vm_CreateCompleted;
                vm.OwnWindow = (Window?)Window.Current; // best effort; NavigationHelper usually sets it
            }
        }

        private void AdminEmployeesMenuView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AdminEmployeesMenuVM vm)
            {
                vm.CreateCompleted -= Vm_CreateCompleted;
            }
        }

        private async void Vm_CreateCompleted(bool success, string? message)
        {
            var root = (this.XamlRoot);
            var dlg = new ContentDialog
            {
                Title = success ? "Operación exitosa" : "Error",
                Content = message ?? (success ? "Operación completada." : "Ocurrió un error."),
                CloseButtonText = "OK",
                XamlRoot = root
            };

            await dlg.ShowAsync();

            // Si se creó correctamente, limpiar PasswordBox (VM ya limpió CreateRequest)
            if (success && CreatePasswordBox != null)
            {
                CreatePasswordBox.Password = string.Empty;
            }
        }

        private void CreatePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AdminEmployeesMenuVM vm && sender is PasswordBox pb)
            {
                vm.CreatePassword = pb.Password;
            }
        }
    }
}
