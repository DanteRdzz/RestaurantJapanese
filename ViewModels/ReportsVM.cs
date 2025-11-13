using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using RestaurantJapanese.Helpers; // BaseViewModel, RelayCommand
using RestaurantJapanese.Models;
using RestaurantJapanese.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics; // Para logging de diagnóstico

namespace RestaurantJapanese.ViewModels
{
    public class ReportsVM : BaseViewModel
    {
        public Window? OwnWindow { get; set; }

        private readonly IReportsService _svc;

        public ReportsVM(IReportsService svc)
        {
            _svc = svc;

            // Rango por defecto: todo el mes actual
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            FromDateOffset = new DateTimeOffset(firstDayOfMonth);
            ToDateOffset = new DateTimeOffset(lastDayOfMonth);

            // Carga automática
            _ = LoadAsync();
        }

        // DatePicker en WinUI usa DateTimeOffset:
        private DateTimeOffset _fromOffset;
        public DateTimeOffset FromDateOffset
        {
            get => _fromOffset;
            set => Set(ref _fromOffset, value);
        }

        private DateTimeOffset _toOffset;
        public DateTimeOffset ToDateOffset
        {
            get => _toOffset;
            set => Set(ref _toOffset, value);
        }

        // Datos
        public ObservableCollection<SalesReportRowModel> Items { get; } = new();

        // Totales
        private int _totalTickets;
        public int TotalTickets { get => _totalTickets; set => Set(ref _totalTickets, value); }

        private decimal _sumSubtotal;
        public decimal SumSubtotal { get => _sumSubtotal; set => Set(ref _sumSubtotal, value); }

        private decimal _sumTax;
        public decimal SumTax { get => _sumTax; set => Set(ref _sumTax, value); }

        private decimal _sumTotal;
        public decimal SumTotal { get => _sumTotal; set => Set(ref _sumTotal, value); }

        private string? _error;
        public string? Error { get => _error; set => Set(ref _error, value); }

        // Comandos
        public ICommand LoadCommand => new RelayCommand(async _ => await LoadAsync());
        
        public ICommand QuickTodayCommand => new RelayCommand(async _ =>
        {
            FromDateOffset = new DateTimeOffset(DateTime.Today);
            ToDateOffset = new DateTimeOffset(DateTime.Today);
            await LoadAsync();
        });
        
        public ICommand QuickAllCommand => new RelayCommand(async _ =>
        {
            // Mostrar todos los registros (un rango muy amplio)
            FromDateOffset = new DateTimeOffset(new DateTime(2020, 1, 1));
            ToDateOffset = new DateTimeOffset(DateTime.Today.AddDays(1));
            await LoadAsync();
        });

        public async Task LoadAsync()
        {
            Error = null;
            try
            {
                Items.Clear();

                // Convertimos los DateTimeOffset del picker a DateTime (local)
                var from = FromDateOffset.DateTime;
                var to = ToDateOffset.DateTime;

                Debug.WriteLine($"ReportsVM.LoadAsync: Cargando datos desde {from:yyyy-MM-dd} hasta {to:yyyy-MM-dd}");

                var data = await _svc.GetSalesAsync(from, to);
                
                Debug.WriteLine($"ReportsVM.LoadAsync: Servicio devolvió {data.Count} registros");
                
                foreach (var r in data) 
                {
                    Items.Add(r);
                }

                Debug.WriteLine($"ReportsVM.LoadAsync: Items.Count final = {Items.Count}");

                // Totales
                TotalTickets = Items.Count;
                SumSubtotal = Items.Sum(x => x.Subtotal);
                SumTax = Items.Sum(x => x.Tax);
                SumTotal = Items.Sum(x => x.Total);
                
                Debug.WriteLine($"ReportsVM.LoadAsync: Totales - Tickets:{TotalTickets}, Total:{SumTotal:C}");
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (Exception ex) { Error = ex.Message; }
        }
    }
}
