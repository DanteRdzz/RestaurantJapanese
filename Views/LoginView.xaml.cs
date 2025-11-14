using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RestaurantJapanese.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestaurantJapanese.Views
{
    public sealed partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            this.Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            // Enfocar el campo de usuario al cargar
            var userNameTextBox = this.FindName("UserNameTextBox") as TextBox;
            userNameTextBox?.Focus(FocusState.Programmatic);
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Si presiona Enter, ejecutar el comando de login
            if (e.Key == VirtualKey.Enter)
            {
                if (this.DataContext is LoginVM viewModel)
                {
                    var passwordBox = this.FindName("Pwd") as PasswordBox;
                    if (passwordBox != null)
                    {
                        // Pasar la contraseña del PasswordBox al comando
                        viewModel.SignInCommand.Execute(passwordBox.Password);
                    }
                }
                e.Handled = true;
            }
        }

        private void UserNameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                // Enfocar el PasswordBox cuando presione Enter en el campo de usuario
                var passwordBox = this.FindName("Pwd") as PasswordBox;
                passwordBox?.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                // Ejecutar login cuando presione Enter en el campo de contraseña
                if (this.DataContext is LoginVM viewModel)
                {
                    var passwordBox = sender as PasswordBox;
                    if (passwordBox != null)
                    {
                        viewModel.SignInCommand.Execute(passwordBox.Password);
                    }
                }
                e.Handled = true;
            }
        }
    }
}
