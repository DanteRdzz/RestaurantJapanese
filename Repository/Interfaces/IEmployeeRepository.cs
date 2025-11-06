using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantJapanese.Models;


namespace RestaurantJapanese.Repository.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<EmployeeModel>> GetAllAsync(bool? onlyActive, string? search);
        Task<EmployeeModel?> GetByIdAsync(int id);

        // Create (empleado + usuario en una sola llamada)
        Task<EmployeeModel?> CreateWithUserAsync(EmployeeCreateRequest req);
        // Update básico de empleado
        Task<EmployeeModel?> UpdateAsync(EmployeeModel item);

        // Soft delete
        Task<bool> SoftDeleteAsync(int id);
    }
}
