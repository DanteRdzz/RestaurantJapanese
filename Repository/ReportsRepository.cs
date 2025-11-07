using Dapper;
using Microsoft.Data.SqlClient;
using RestaurantJapanese.DataAcces;
using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RestaurantJapanese.Repository
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly IConnFactory _connFactory;
        public ReportsRepository(IConnFactory connFactory) => _connFactory = connFactory;

        public async Task<IEnumerable<SalesReportRowModel>> GetSalesAsync(DateTime? from, DateTime? to, string groupBy)
        {
            using var conn = (SqlConnection)_connFactory.Create();
            var rows = await conn.QueryAsync<SalesReportRowModel>(
                "dbo.sp_RestJP_Sales_Report",
                new { From = from, To = to, GroupBy = groupBy },
                commandType: CommandType.StoredProcedure
            );
            return rows;
        }
    }
}
