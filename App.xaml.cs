using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using RestaurantJapanese.DataAcces;
using RestaurantJapanese.Helpers;
using RestaurantJapanese.Repository;
using RestaurantJapanese.Repository.Interfaces;
using RestaurantJapanese.Services;
using RestaurantJapanese.Services.Interfaces;
using RestaurantJapanese.ViewModels;
using RestaurantJapanese.Views;

namespace RestaurantJapanese
{
    public partial class App : Application
    {
        public static IHost HostApp { get; private set; } = default!;

        public App()
        {
            InitializeComponent();
            HostApp = new HostBuilder()
                .ConfigureServices(services =>
                {
                    // DataAccess
                    services.AddSingleton<IConnFactory, ConnectionFactory>();
                    // Repository / Services
                    services.AddTransient<ILoginRepository, LoginRepository>();
                    services.AddTransient<ILoginService, LoginService>();
                    services.AddTransient<IEmployeeRepository, EmployeeRepository>();
                    services.AddTransient<IEmployeeService, EmployeeService>();
                    services.AddTransient<IPosRepository, PosRepository>();
                    services.AddTransient<IPosService, PosService>();
                    services.AddTransient<IReportsRepository, ReportsRepository>();
                    services.AddTransient<IReportsService, ReportsService>();
                    services.AddTransient<IMenuRepository, MenuRepository>();
                    services.AddTransient<IMenuService, MenuService>();
                    // ViewModels
                    services.AddTransient<LoginVM>();
                    services.AddTransient<AdminMenuVM>();
                    services.AddTransient<AdminEmployeesMenuVM>();
                    services.AddTransient<PosVM>();
                    services.AddTransient<ReportsVM>();
                    services.AddTransient<MenuInventarioAdminVM>();
                    // Views
                    services.AddTransient<LoginView>();
                    services.AddTransient<AdminMenuView>();
                    services.AddTransient<AdminEmployeesMenuView>();
                    services.AddTransient<PosView>();
                    services.AddTransient<ReportsPage>();
                    services.AddTransient<MenuInventarioAdminView>();
                })
                .Build();
            NavigationHelper.Services = HostApp.Services;
            this.HighContrastAdjustment = ApplicationHighContrastAdjustment.None;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
<<<<<<< HEAD
            // Abrir la ventana de Login (sin maximizar)
=======
>>>>>>> adding return buttons
            var win = NavigationHelper.OpenWindow<LoginView, LoginVM>();
            if ((win.Content as FrameworkElement)?.DataContext is LoginVM vm) vm.OwnWindow = win;
            win.Activate();
        }
    }
}
