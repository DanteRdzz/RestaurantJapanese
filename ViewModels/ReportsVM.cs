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
        private readonly IReportsService _svc;
        public ReportsVM(IReportsService svc)
        {
            _svc = svc;

            // Defaults
            ToDate = DateTime.Today;
            FromDate = ToDate.AddDays(-30);
            GroupBy = "DAY";
        }

        public Window? OwnWindow { get; set; }

        // Filtros
        private DateTime _from;
        public DateTime FromDate { get => _from; set => Set(ref _from, value); }

        private DateTime _to;
        public DateTime ToDate { get => _to; set => Set(ref _to, value); }

        private string _groupBy = "DAY";
        public string GroupBy { get => _groupBy; set => Set(ref _groupBy, value); }

        // Datos
        public ObservableCollection<SalesReportRowModel> Items { get; } = new();

        // Totales
        private int _totalTickets;
        public int TotalTickets { get => _totalTickets; set => Set(ref _totalTickets, value); }

        private decimal _sumSubtotal;
        public decimal SumSubtotal { get => _sumSubtotal; set => Set(ref _sumSubtotal, value); }

        private decimal _sumIVA;
        public decimal SumIVA { get => _sumIVA; set => Set(ref _sumIVA, value); }

        private decimal _sumPropina;
        public decimal SumPropina { get => _sumPropina; set => Set(ref _sumPropina, value); }

        private decimal _sumTotal;
        public decimal SumTotal { get => _sumTotal; set => Set(ref _sumTotal, value); }

        private string? _error;
        public string? Error { get => _error; set => Set(ref _error, value); }

        public ICommand LoadCommand => new RelayCommand(async _ => await LoadAsync());

        public async Task LoadAsync()
        {
            Error = null;
            try
            {
                Items.Clear();
                var data = await _svc.GetSalesAsync(FromDate, ToDate, GroupBy);
                foreach (var r in data) Items.Add(r);

                TotalTickets = Items.Sum(x => x.Tickets);
                SumSubtotal = Items.Sum(x => x.Subtotal);
                SumIVA = Items.Sum(x => x.IVA);
                SumPropina = Items.Sum(x => x.Propina);
                SumTotal = Items.Sum(x => x.Total);
            }
            catch (SqlException ex) { Error = $"Error SQL ({ex.Number})."; }
            catch (Exception ex) { Error = ex.Message; }
        }
    }
}
