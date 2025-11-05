using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RestaurantJapanese.Helpers;
using RestaurantJapanese.Services.Interfaces;

namespace RestaurantJapanese.ViewModels
{
    public class LoginVM : BaseViewModel
    {
        private readonly ILoginService _auth;
        public LoginVM(ILoginService auth) => _auth = auth;

        // La Window que te asignamos en App.OnLaunched para poder mostrar diálogos o cerrar
        public Window? OwnWindow { get; set; }

        private string _user = "";
        public string UserName { get => _user; set => Set(ref _user, value); }

        // Nota: Password puede llegar por CommandParameter; aun así la guardamos para validación
        private string _pass = "";
        public string Password { get => _pass; set => Set(ref _pass, value); }

        private string? _error;
        public string? Error { get => _error; set => Set(ref _error, value); }

        public ICommand SignInCommand => new RelayCommand(async param =>
        {
            // 1) Si viene la contraseña desde XAML, úsala
            if (param is string pwdFromXaml && !string.IsNullOrEmpty(pwdFromXaml))
                Password = pwdFromXaml;

            await SignInAsync();
        });

        private async Task SignInAsync()
        {
            Error = null;

            // 2) Validación local
            if (string.IsNullOrWhiteSpace(UserName?.Trim()) || string.IsNullOrWhiteSpace(Password?.Trim()))
            {
                Error = "Usuario y Contraseña Requeridos";
                return;
            }

            try
            {
                // 3) Autenticación
                var user = await _auth.LoginAsync(UserName, Password);
                if (user is null)
                {
                    Error = "Credenciales inválidas.";
                    return;
                }

                // 4) Mensaje por rol
                var role = (user.Role ?? "").Trim().ToLowerInvariant();
                var content = role == "admin"
                    ? "Rol: ADMIN — A futuro se cargará la vista de administración."
                    : "Rol: EMPLEADO — A futuro se cargará la vista de empleado.";

                await ShowDialogAsync("Login correcto", content);

                // 5) (Opcional) Navegar
                // if (OwnWindow is not null)
                //     NavigationHelper.ReplaceWindow<RestaurantJapanese.Views.HomeView, RestaurantJapanese.ViewModels.HomeVM>(OwnWindow);
            }
            catch (SqlException ex)
            {
                // Errores típicos de conexión
                if (ex.Number == 40 || ex.Number == 26)
                    Error = "No se pudo conectar a SQL Server. Revisa servidor/instancia y TCP/IP.";
                else if (ex.Number == -1)
                    Error = "Servidor no accesible. Verifica el nombre del servidor o firewall.";
                else if (ex.Number == -2) // timeout
                    Error = "Tiempo de espera agotado (timeout).";
                else
                    Error = $"Error SQL ({ex.Number}).";
            }
            catch (TimeoutException)
            {
                Error = "La operación tardó demasiado (timeout).";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Login Error] {ex}");
                Error = "Ocurrió un error inesperado.";
            }
            finally
            {
                // (Opcional) limpiar password en memoria
                // Password = string.Empty;
                // OnPropertyChanged(nameof(Password));
            }
        }

        private async Task ShowDialogAsync(string title, string content)
        {
            var root = (OwnWindow?.Content as FrameworkElement)?.XamlRoot;
            if (root is null) { Error = content; return; } // fallback si no hay Window

            var dlg = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = root
            };
            await dlg.ShowAsync();
        }
    }
}
