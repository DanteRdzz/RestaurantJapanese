using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RestaurantJapanese.ViewModels;
using System.Linq;

namespace RestaurantJapanese.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void Pwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            (DataContext as LoginVM)!.Password = (sender as PasswordBox)!.Password;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DataContext as LoginVM)!;
            if (!vm.SignIn()) return;

            // Abre ventana con POS
            var win = new Window();
            var pos = new PosPage(vm.LoggedUserId!.Value, vm.DisplayName!);
            win.Content = pos;
            win.Activate();

            // (Opcional) Cerrar ventana actual
            App.MainWindow?.Close();
        }
    }
}
