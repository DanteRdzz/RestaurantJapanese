using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using RestaurantJapanese.DataAcces;
using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;

namespace RestaurantJapanese.Repository
{
    public class MenuRepository : IMenuRepository
    {
        private readonly IConnFactory _conn;
        public MenuRepository(IConnFactory conn) => _conn = conn;

        public async Task<IEnumerable<MenuItemModel>> GetAllAsync(bool? onlyActive, string? search)
        {
            using var db = (SqlConnection)_conn.Create();
            return await db.QueryAsync<MenuItemModel>(
                "dbo.sp_Menu_GetAll",
                new { OnlyActive = onlyActive, Search = search },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<MenuItemModel?> GetByIdAsync(int id)
        {
            using var db = (SqlConnection)_conn.Create();
            return await db.QueryFirstOrDefaultAsync<MenuItemModel>(
                "dbo.sp_Menu_GetById",
                new { IdMenuItem = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<MenuItemModel> CreateAsync(MenuItemModel item)
        {
            using var db = (SqlConnection)_conn.Create();
            return await db.QueryFirstAsync<MenuItemModel>(
                "dbo.sp_Menu_Create",
                new
                {
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    IsActive = item.IsActive
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<MenuItemModel> UpdateAsync(MenuItemModel item)
        {
            using var db = (SqlConnection)_conn.Create();
            return await db.QueryFirstAsync<MenuItemModel>(
                "dbo.sp_Menu_Update",
                new
                {
                    IdMenuItem = item.IdMenuItem,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    IsActive = item.IsActive
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<MenuItemModel?> SoftDeleteAsync(int id)
        {
            using var db = (SqlConnection)_conn.Create();
            return await db.QueryFirstOrDefaultAsync<MenuItemModel>(
                "dbo.sp_Menu_SoftDelete",
                new { IdMenuItem = id },
                commandType: CommandType.StoredProcedure);
        }
    }
}
