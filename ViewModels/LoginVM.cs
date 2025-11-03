using RestaurantJapanese.Data;

namespace RestaurantJapanese.ViewModels
{
    public class LoginVM : BaseVM
    {
        private string _user = "";
        public string UserName { get => _user; set { _user = value; OnPropertyChanged(); } }

        private string _pass = "";
        public string Password { get => _pass; set { _pass = value; OnPropertyChanged(); } }

        private string? _error;
        public string? Error { get => _error; set { _error = value; OnPropertyChanged(); } }

        public int? LoggedUserId { get; private set; }
        public string? DisplayName { get; private set; }

        public bool SignIn()
        {
            Error = null;
            var r = DB.ValidateUser(UserName, Password);
            if (r == null) { Error = "Usuario o contraseña inválidos."; return false; }
            LoggedUserId = r.Value.id;
            DisplayName = r.Value.display;
            return true;
        }
    }
}
