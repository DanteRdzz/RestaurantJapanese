using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;
using RestaurantJapanese.Services.Interfaces;

namespace RestaurantJapanese.Services
{
    public class PosService : IPosService
    {
        private readonly IPosRepository _repo;
        public PosService(IPosRepository repo) => _repo = repo;

        public Task<IEnumerable<PosMenuItemModel>> GetMenuAsync()
            => _repo.GetMenuAsync();

        public Task<PosTicket> CreateTicketAsync(int createdBy, decimal tip, decimal taxRate,
                                                 IEnumerable<(int IdMenuItem, int Qty)> items)
            => _repo.CreateTicketAsync(createdBy, tip, taxRate, items);

        public Task<PosTicket?> GetTicketAsync(int idTicket)
            => _repo.GetTicketAsync(idTicket);
    }
}
