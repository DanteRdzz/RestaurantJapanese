using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;
using RestaurantJapanese.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantJapanese.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository _repo;
        public ReportsService(IReportsRepository repo) => _repo = repo;

        public async Task<IEnumerable<SalesReportRowModel>> GetSalesAsync(DateTime? from, DateTime? to, string groupBy)
        {
            // Defaults si vienen nulos (últimos 30 días)
            var toUse = to?.Date ?? DateTime.Today;
            var fromUse = from?.Date ?? toUse.AddDays(-30);

            var gb = (groupBy ?? "DAY").Trim().ToUpperInvariant();
            if (gb != "DAY" && gb != "WEEK" && gb != "MONTH") gb = "DAY";

            return await _repo.GetSalesAsync(fromUse, toUse, gb);
        }
    }
}
