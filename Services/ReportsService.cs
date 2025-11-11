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
            // Defaults si vienen nulos (rango amplio para mostrar todos)
            var fromUse = FechaInicio?.Date ?? new DateTime(2020, 1, 1);
            var toUse = FechaFin?.Date ?? DateTime.Today.AddDays(1);

            return await _repo.GetSalesAsync(fromUse, toUse);
        }
    }
}
