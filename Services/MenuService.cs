using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;
using RestaurantJapanese.Services.Interfaces;

namespace RestaurantJapanese.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _repo;
        public MenuService(IMenuRepository repo) => _repo = repo;

        public Task<IEnumerable<MenuItemModel>> GetAllAsync(bool? onlyActive, string? search)
            => _repo.GetAllAsync(onlyActive, search);

        public Task<MenuItemModel?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public Task<MenuItemModel> CreateAsync(MenuItemModel item)
            => _repo.CreateAsync(item);

        public Task<MenuItemModel> UpdateAsync(MenuItemModel item)
            => _repo.UpdateAsync(item);

        public Task<MenuItemModel?> SoftDeleteAsync(int id)
            => _repo.SoftDeleteAsync(id);
    }
}
