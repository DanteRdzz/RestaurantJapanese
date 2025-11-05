using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantJapanese.Models;
namespace RestaurantJapanese.Repository.Interfaces
{
    public interface ILoginRepository
    {
        Task<UserModel?> ValidateAsync(string username, string password);
    }
}
