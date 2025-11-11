using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantJapanese.Models;

namespace RestaurantJapanese.Services.Interfaces
{
    public interface IReportsService
    {
        Task<List<SalesReportRowModel>> GetSalesAsync(DateTime? FechaInicio, DateTime? FechaFin);
    }
}
