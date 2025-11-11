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

namespace RestaurantJapanese.ViewModels
{
    public class ReportsVM : BaseViewModel
    {
        public Window? OwnWindow { get; set; }

        private readonly IReportsService _svc;

        public ReportsVM(IReportsService svc)
        {
            _svc = svc;

            // Rango por defecto
            ToDateOffset = new DateTimeOffset(DateTime.Today);
            FromDateOffset = ToDateOffset.AddDays(-30);

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

        // Totales (si los quieres)
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
        public ICommand QuickLast30Command => new RelayCommand(async _ =>
        {
            ToDateOffset = new DateTimeOffset(DateTime.Today);
            FromDateOffset = ToDateOffset.AddDays(-30);
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

                var data = await _svc.GetSalesAsync(from, to);
                foreach (var r in data) Items.Add(r);

                // Totales (si no los usas, elimina estas líneas)
                TotalTickets = Items.Count;
                SumSubtotal = Items.Sum(x => (decimal)x.Subtotal);
                SumTax = Items.Sum(x => (decimal)x.Tax);
                SumTotal = Items.Sum(x => (decimal)x.Total);
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (Exception ex) { Error = ex.Message; }
        }
    }
}
