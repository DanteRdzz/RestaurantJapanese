using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RestaurantJapanese.Models;
using RestaurantJapanese.ViewModels;
using System;

namespace RestaurantJapanese.Views
{
    public sealed partial class PosPage : Page
    {
        public PosPage(int userId, string display)
        {
            this.InitializeComponent();
            var vm = (DataContext as PosVM)!;
            vm.UserId = userId;
            vm.UserDisplay = $"Bienvenido, {display}";
            vm.LoadMenu();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DataContext as PosVM)!;
            var item = (sender as Button)!.DataContext as MenuItem;
            vm.AddToCart(item!);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DataContext as PosVM)!;
            var ci = (sender as Button)!.DataContext as CartItem;
            vm.RemoveOne(ci!);
        }

        private async void Checkout_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DataContext as PosVM)!;
            var id = vm.Checkout();
            if (id > 0)
            {
                var dlg = new ContentDialog
                {
                    Title = "Venta registrada",
                    Content = $"Ticket #{id} guardado.",
                    PrimaryButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dlg.ShowAsync();
            }
        }
    }
}
