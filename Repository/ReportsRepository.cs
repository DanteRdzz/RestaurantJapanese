using Dapper;
using Microsoft.Data.SqlClient;
using RestaurantJapanese.DataAcces;
using RestaurantJapanese.Models;
using RestaurantJapanese.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // si usas DI de logging

namespace RestaurantJapanese.Repository
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly IConnFactory _connFactory;
        private readonly ILogger<ReportsRepository>? _logger;

        public ReportsRepository(IConnFactory connFactory, ILogger<ReportsRepository>? logger = null)
        {
            _connFactory = connFactory;
            _logger = logger;
        }

        public async Task<List<SalesReportRowModel>> GetSalesAsync(DateTime? FechaInicio, DateTime? FechaFin)
        {
            using var conn = (SqlConnection)_connFactory.Create();

            conn.FireInfoMessageEventOnUserErrors = true;
            conn.InfoMessage += (s, e) => _logger?.LogInformation("SQL INFO: {Msg}", e.Message);

            await conn.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@FechaInicio", FechaInicio, DbType.DateTime);
            parameters.Add("@FechaFin", FechaFin, DbType.DateTime);

            // Log de parámetros
            _logger?.LogInformation("Ejecutando SP {SP} con params: {Params}",
                "dbo.sp_RestJP_Sales_Report",
                JsonSerializer.Serialize(new { FechaInicio, FechaFin })
            );

            var sw = Stopwatch.StartNew();
            try
            {
                var result = await conn.QueryAsync<SalesReportRowModel>(
                    "dbo.sp_RestJP_Sales_Report",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60
                );

                sw.Stop();
                var list = result.ToList();

                _logger?.LogInformation("SP {SP} completado. Filas:{Count} Tiempo:{Elapsed}ms",
                    "dbo.sp_RestJP_Sales_Report", list.Count, sw.ElapsedMilliseconds);

                return list;
            }
            catch (SqlException ex)
            {
                sw.Stop();
                _logger?.LogError(ex, "SqlException ejecutando {SP}. Tiempo:{Elapsed}ms",
                    "dbo.sp_RestJP_Sales_Report", sw.ElapsedMilliseconds);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                sw.Stop();
                _logger?.LogError(ex, "Timeout ejecutando {SP}. Tiempo:{Elapsed}ms",
                    "dbo.sp_RestJP_Sales_Report", sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
