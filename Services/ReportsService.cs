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

        public async Task<List<SalesReportRowModel>> GetSalesAsync(DateTime? FechaInicio, DateTime? FechaFin)
        {
            // Defaults si vienen nulos (últimos 30 días)
            var fromUse = FechaInicio?.Date ?? DateTime.Today.AddDays(-30);
            var toUse = FechaFin?.Date ?? DateTime.Today;

            return await _repo.GetSalesAsync(fromUse, toUse);
        }
    }
}
