using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantJapanese.Models;

namespace RestaurantJapanese.Repository.Interfaces
{
    public interface IReportsRepository
    {
        Task<IEnumerable<SalesReportRowModel>> GetSalesAsync(DateTime? from, DateTime? to, string groupBy);
    }
}
