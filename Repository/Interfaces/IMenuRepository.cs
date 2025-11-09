using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantJapanese.Models;

namespace RestaurantJapanese.Repository.Interfaces
{
    public interface IMenuRepository
    {
        Task<IEnumerable<MenuItemModel>> GetAllAsync(bool? onlyActive, string? search);
        Task<MenuItemModel?> GetByIdAsync(int id);
        Task<MenuItemModel> CreateAsync(MenuItemModel item);
        Task<MenuItemModel> UpdateAsync(MenuItemModel item);
        Task<MenuItemModel?> SoftDeleteAsync(int id);
    }
}
