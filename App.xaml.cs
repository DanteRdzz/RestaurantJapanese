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

                    // ViewModels
                    services.AddTransient<LoginVM>();
                    services.AddTransient<HomeVM>();
                    services.AddTransient<AdminMenuVM>();
                    services.AddTransient<AdminEmployeesMenuVM>();
                    services.AddTransient<PosVM>();

                    // Views 
                    services.AddTransient<LoginView>();
                    services.AddTransient<HomeView>();
                    services.AddTransient<AdminMenuView>();
                    services.AddTransient<PosView>();
                })
                .Build();

            // Exponer el contenedor al NavigationHelper (estilo ALTYS)
            NavigationHelper.Services = HostApp.Services;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Abrir la ventana de Login
            var win = NavigationHelper.OpenWindow<LoginView, LoginVM>();

            // Guardar la referencia de la Window en el VM para poder cerrarla al navegar
            if ((win.Content as Microsoft.UI.Xaml.FrameworkElement)?.DataContext is LoginVM vm)
                vm.OwnWindow = win;

            win.Activate();
        }
    }
}
