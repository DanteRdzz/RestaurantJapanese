using RestaurantJapanese.Models;
using RestaurantJapanese.Repository;
using RestaurantJapanese.Repository.Interfaces;
using RestaurantJapanese.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantJapanese.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        public EmployeeService(IEmployeeRepository repo) => _repo = repo;

        public Task<IEnumerable<EmployeeModel>> GetAllAsync(bool? onlyActive, string? search)
            => _repo.GetAllAsync(onlyActive, search);

        public Task<EmployeeModel?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public Task<EmployeeModel?> CreateWithUserAsync(EmployeeCreateRequest req)
            => _repo.CreateWithUserAsync(req);

        // En este esquema, Save solo hace Update cuando hay Id; (para alta con usuario usa CreateWithUserAsync)
        public async Task<EmployeeModel?> SaveAsync(EmployeeModel item)
        {
            if (item.IdEmployee <= 0)
            {
                // Opción A: lanzar (para forzar CreateWithUserAsync)
                // throw new InvalidOperationException("Para crear usa CreateWithUserAsync (empleado + usuario).");

                // Opción B: si quieres permitir alta sin usuario, crea un SP sp_Employees_Insert
                // y llama aquí. Por ahora devolvemos null para indicar que no aplica.
                return null;
            }
            return await _repo.UpdateAsync(item);
        }

        public Task<bool> SoftDeleteAsync(int id)
            => _repo.SoftDeleteAsync(id);
    }
}
