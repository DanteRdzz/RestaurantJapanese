using RestaurantJapanese.Models;
using System.Threading.Tasks;

namespace RestaurantJapanese.Services.Interfaces
{
    public interface ILoginService
    {
        Task<UserModel?> LoginAsync(string user, string password);
    }
}
