using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantJapanese.Models;

namespace RestaurantJapanese.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeModel>> GetAllAsync(bool? onlyActive, string? search);
        Task<EmployeeModel?> GetByIdAsync(int id);

        // Alta con usuario
        Task<EmployeeModel?> CreateWithUserAsync(EmployeeCreateRequest req);

        // Guardar (insert/update)
        Task<EmployeeModel?> SaveAsync(EmployeeModel item);

        // Baja lógica
        Task<bool> SoftDeleteAsync(int id);
    }
}
