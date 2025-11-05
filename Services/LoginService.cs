using System.Threading.Tasks;
using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;
using RestaurantJapanese.Services.Interfaces;

namespace RestaurantJapanese.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;
        public LoginService(ILoginRepository loginRepository) => _loginRepository = loginRepository;
        public Task<UserModel?> LoginAsync(string user, string password) => _loginRepository.ValidateAsync(user, password);
    }
}
