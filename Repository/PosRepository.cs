using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using RestaurantJapanese.DataAcces; // IConnFactory
using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;

namespace RestaurantJapanese.Repository
{
    public class PosRepository : IPosRepository
    {
        private readonly IConnFactory _cf;
        public PosRepository(IConnFactory cf) => _cf = cf;

        public async Task<IEnumerable<PosMenuItemModel>> GetMenuAsync()
        {
            using var cn = (SqlConnection)_cf.Create();
            return await cn.QueryAsync<PosMenuItemModel>("dbo.sp_Menu_GetActive",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PosTicket> CreateTicketAsync(int createdBy, decimal tip, decimal taxRate,
                                                       IEnumerable<(int IdMenuItem, int Qty)> items)
        {
            using var cn = (SqlConnection)_cf.Create();

            var tvp = new DataTable();
            tvp.Columns.Add("IdMenuItem", typeof(int));
            tvp.Columns.Add("Qty", typeof(int));
            foreach (var it in items)
                tvp.Rows.Add(it.IdMenuItem, it.Qty);

            var p = new DynamicParameters();
            p.Add("CreatedBy", createdBy);
            p.Add("Tip", tip);
            p.Add("TaxRate", taxRate);
            p.Add("Items", tvp.AsTableValuedParameter("dbo.PosItemTVP"));

            using var multi = await cn.QueryMultipleAsync("dbo.sp_Pos_CreateTicket", p,
                commandType: CommandType.StoredProcedure);

            var header = await multi.ReadFirstAsync<PosTicketHeader>();
            var detail = (await multi.ReadAsync<PosTicketItemRow>()).ToList();

            return new PosTicket { Header = header, Items = detail };
        }

        public async Task<PosTicket?> GetTicketAsync(int idTicket)
        {
            using var cn = (SqlConnection)_cf.Create();

            var p = new DynamicParameters();
            p.Add("IdTicket", idTicket);

            // Fixed: Should use sp_Pos_GetTicket, not sp_Pos_CreateTicket
            using var multi = await cn.QueryMultipleAsync("dbo.sp_Pos_GetTicket", p,
                commandType: CommandType.StoredProcedure);

            var header = await multi.ReadFirstOrDefaultAsync<PosTicketHeader>();
            if (header is null) return null;

            var detail = (await multi.ReadAsync<PosTicketItemRow>()).ToList();
            return new PosTicket { Header = header, Items = detail };
        }
    }
}
