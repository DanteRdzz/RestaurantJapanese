using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantJapanese.Models;

namespace RestaurantJapanese.Repository.Interfaces
{
    public interface IPosRepository
    {
        Task<IEnumerable<PosMenuItemModel>> GetMenuAsync();

        Task<PosTicket> CreateTicketAsync(int createdBy, decimal tip, decimal taxRate,
                                          IEnumerable<(int IdMenuItem, int Qty)> items);

        Task<PosTicket?> GetTicketAsync(int idTicket);
    }
}
