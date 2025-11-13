using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RestaurantJapanese.Helpers;          // BaseViewModel, RelayCommand, NavigationHelper
using RestaurantJapanese.Services.Interfaces; // ILoginService
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantJapanese.ViewModels
{
    public class LoginVM : BaseViewModel
    {
        private readonly ILoginService _auth;
        public LoginVM(ILoginService auth) => _auth = auth;

        public Window? OwnWindow { get; set; }   // La ventana actual para ReplaceWindow

        private string _user = "";
        public string UserName
        {
            get => _user;
            set => Set(ref _user, value);
        }

        private string _pass = "";
        public string Password
        {
            get => _pass;
            set => Set(ref _pass, value);
        }

        private string? _error;
        public string? Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public ICommand SignInCommand => new RelayCommand(p =>
        {
            // Si viene la contraseña desde PasswordBox.Password como CommandParameter
            if (p is string pwd && !string.IsNullOrEmpty(pwd))
                Password = pwd;

            _ = SignInAsync(); // lanzar sin bloquear
        });

        private async Task SignInAsync()
        {
            Error = null;

            var u = UserName?.Trim();
            var pw = Password?.Trim();

            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(pw))
            { Error = "Usuario y Contraseña requeridos."; return; }

            try
            {
                var user = await _auth.LoginAsync(u!, pw!);
                if (user is null)
                { Error = "Credenciales inválidas."; return; }

                var role = (user.Role ?? "").Trim().ToLowerInvariant();
                var displayName = user.DisplayName ?? user.UserName ?? "Usuario";

                if (role == "admin")
                {
                    // Navegar al panel de administración
                    NavigationHelper.ReplaceWindow<RestaurantJapanese.Views.AdminMenuView,
                                                  RestaurantJapanese.ViewModels.AdminMenuVM>(OwnWindow!);

                    if ((OwnWindow?.Content as FrameworkElement)?.DataContext is RestaurantJapanese.ViewModels.AdminMenuVM vmAdmin)
                    {
                        vmAdmin.OwnWindow = OwnWindow;
                        vmAdmin.CurrentUserId = user.IdUser;
                        vmAdmin.CurrentUserName = displayName;
                        vmAdmin.CurrentUserRole = user.Role ?? "Admin";
                    }
                }
                else if (role == "empleado" || role == "cajero")
                {
                    // Navegar a POS; el VM cargará el menú automáticamente al asignar CurrentUserId
                    NavigationHelper.ReplaceWindow<RestaurantJapanese.Views.PosView,
                                                  RestaurantJapanese.ViewModels.PosVM>(OwnWindow!);

                    if ((OwnWindow?.Content as FrameworkElement)?.DataContext is RestaurantJapanese.ViewModels.PosVM vmPos)
                    {
                        vmPos.OwnWindow = OwnWindow;
                        vmPos.CurrentUserId = user.IdUser; // dispara LoadMenuAsync dentro del setter
                        vmPos.CurrentUserName = displayName;
                    }
                }
                else
                {
                    // Fallback → POS
                    NavigationHelper.ReplaceWindow<RestaurantJapanese.Views.PosView,
                                                  RestaurantJapanese.ViewModels.PosVM>(OwnWindow!);

                    if ((OwnWindow?.Content as FrameworkElement)?.DataContext is RestaurantJapanese.ViewModels.PosVM vmPos)
                    {
                        vmPos.OwnWindow = OwnWindow;
                        vmPos.CurrentUserId = user.IdUser;
                        vmPos.CurrentUserName = displayName;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Error = ex.Message;

                // (Opcional) Mensaje UI sencillo
                var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
                if (root is not null)
                {
                    var dlg = new ContentDialog
                    {
                        Title = "Error de inicio de sesión",
                        Content = Error,
                        CloseButtonText = "OK",
                        XamlRoot = root
                    };
                    await dlg.ShowAsync();
                }
            }
        }
    }
}
