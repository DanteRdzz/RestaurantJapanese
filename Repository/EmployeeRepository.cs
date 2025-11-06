using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using RestaurantJapanese.DataAcces;            // <- tu namespace "DataAcces"
using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;

namespace RestaurantJapanese.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IConnFactory _cf;
        public EmployeeRepository(IConnFactory cf) => _cf = cf;

        public async Task<IEnumerable<EmployeeModel>> GetAllAsync(bool? onlyActive, string? search)
        {
            using var cn = (SqlConnection)_cf.Create();
            return await cn.QueryAsync<EmployeeModel>(
                "dbo.sp_Employees_GetAll",
                new { OnlyActive = onlyActive, Search = search },   // @Search es opcional en el SP
                commandType: CommandType.StoredProcedure);
        }

        public async Task<EmployeeModel?> GetByIdAsync(int id)
        {
            using var cn = (SqlConnection)_cf.Create();
            return await cn.QueryFirstOrDefaultAsync<EmployeeModel>(
                "dbo.sp_Employees_GetById",
                new { IdEmployee = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<EmployeeModel?> CreateWithUserAsync(EmployeeCreateRequest req)
        {
            using var cn = (SqlConnection)_cf.Create();
            return await cn.QueryFirstOrDefaultAsync<EmployeeModel>(
                "dbo.sp_Employees_CreateWithUser",
                new
                {
                    FullName = req.FullName,
                    Email = req.Email,
                    Phone = req.Phone,
                    Role = req.Role,
                    UserName = req.UserName,
                    PasswordText = req.PasswordText,
                    IsActive = req.IsActive,
                    DisplayName = req.DisplayName   // el SP lo infiere como FullName si viene null/empty
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<EmployeeModel?> UpdateAsync(EmployeeModel item)
        {
            using var cn = (SqlConnection)_cf.Create();
            return await cn.QueryFirstOrDefaultAsync<EmployeeModel>(
                "dbo.sp_Employees_Update",
                new
                {
                    IdEmployee = item.IdEmployee,
                    FullName = item.FullName,
                    Email = item.Email,
                    Phone = item.Phone,
                    Role = item.Role,
                    IsActive = item.IsActive
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            using var cn = (SqlConnection)_cf.Create();
            // si el SP devuelve un row, podrías usar QueryFirstOrDefault; con Execute basta
            await cn.ExecuteAsync(
                "dbo.sp_Employees_SoftDelete",
                new { IdEmployee = id },
                commandType: CommandType.StoredProcedure);
            return true;
        }
    }
}
