using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace RestaurantJapanese.Helpers
{
    public static class NavigationHelper
    {
        public static IServiceProvider? Services { get; set; }

        private static readonly List<Window> _openWindows = new();

        private static T GetRequired<T>()
        {
            if (Services is null)
                throw new InvalidOperationException("NavigationHelper.Services no está inicializado.");

            return Services.GetRequiredService<T>();
        }

        /// <summary>
        /// Obtiene el título de ventana apropiado basado en el tipo de vista
        /// </summary>
        private static string GetWindowTitle<TView>() where TView : FrameworkElement
        {
            var viewType = typeof(TView).Name;
            
            return viewType switch
            {
                "LoginView" => "🍜 Restaurant Japanese - Inicio de Sesión",
                "AdminMenuView" => "🌸 Restaurant Japanese - Panel de Administración",
                "AdminEmployeesMenuView" => "👥 Restaurant Japanese - Gestión de Empleados",
                "PosView" => "💰 Restaurant Japanese - Punto de Venta",
                "ReportsPage" => "📊 Restaurant Japanese - Reportes de Ventas",
                "MenuInventarioAdminView" => "🍣 Restaurant Japanese - Inventario de Menú",
                "HomeView" => "🏠 Restaurant Japanese - Inicio",
                _ => "🍜 Restaurant Japanese"
            };
        }

        /// <summary>
        /// Configura una ventana para que se abra maximizada (excepto Login) y con título personalizado
        /// </summary>
        private static void ConfigureWindow<TView>(Window window) where TView : FrameworkElement
        {
            try
            {
                // Configurar título de ventana
                var title = GetWindowTitle<TView>();
                window.Title = title;

                // No maximizar la ventana de login
                var shouldMaximize = !typeof(TView).Name.Contains("Login");

                if (shouldMaximize)
                {
                    // Configurar la ventana después de que se active
                    window.Activated += (sender, args) =>
                    {
                        try
                        {
                            var hWnd = WindowNative.GetWindowHandle(window);
                            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                            var appWindow = AppWindow.GetFromWindowId(windowId);

                            if (appWindow?.Presenter is OverlappedPresenter presenter)
                            {
                                presenter.Maximize();
                                presenter.IsResizable = true;
                                presenter.IsMaximizable = true;
                                presenter.IsMinimizable = true;
                            }
                        }
                        catch
                        {
                            // Ignorar errores de configuración
                        }
                    };
                }
            }
            catch
            {
                // Si falla, no hacer nada - la ventana se abrirá con configuración normal
            }
        }

        /// <summary>
        /// Obtiene la lista de todas las ventanas abiertas.
        /// </summary>
        public static IReadOnlyList<Window> OpenWindows => _openWindows.AsReadOnly();

        /// <summary>
        /// Abre una nueva Window con la View/VM del contenedor (sin parámetros).
        /// </summary>
        public static Window OpenWindow<TView, TViewModel>()
            where TView : FrameworkElement
            where TViewModel : class
        {
            var view = GetRequired<TView>();
            var vm = GetRequired<TViewModel>();

            view.DataContext = vm;

            var win = new Window { Content = view };
            
            // Configurar ventana (título y maximización)
            ConfigureWindow<TView>(win);
            
            _openWindows.Add(win);
            win.Closed += (_, __) => _openWindows.Remove(win);

            return win;
        }

        /// <summary>
        /// Abre una nueva Window con la View/VM del contenedor (con parámetro y callback de init).
        /// </summary>
        public static Window OpenWindow<TView, TViewModel>(
            object? parameter,
            Action<TViewModel, Window>? init)
            where TView : FrameworkElement
            where TViewModel : class
        {
            var view = GetRequired<TView>();
            var vm = GetRequired<TViewModel>();

            view.DataContext = vm;

            var win = new Window { Content = view };
            
            // Configurar ventana (título y maximización)
            ConfigureWindow<TView>(win);
            
            _openWindows.Add(win);
            win.Closed += (_, __) => _openWindows.Remove(win);

            // Permite configurar VM/Window (asignar OwnWindow, IdUsuario, precargar datos, etc.)
            init?.Invoke(vm, win);

            return win;
        }

        /// <summary>
        /// Reemplaza el contenido de una Window existente con otra View/VM del contenedor.
        /// Devuelve la misma Window (útil para llamar win.Activate()).
        /// </summary>
        public static Window ReplaceWindow<TView, TViewModel>(
            Window owner,
            object? parameter = null,
            Action<TViewModel, Window>? init = null)
            where TView : FrameworkElement
            where TViewModel : class
        {
            if (owner is null) throw new ArgumentNullException(nameof(owner));

            var view = GetRequired<TView>();
            var vm = GetRequired<TViewModel>();

            view.DataContext = vm;

            owner.Content = view;

            // Configurar ventana (título y maximización si corresponde)
            ConfigureWindow<TView>(owner);

            init?.Invoke(vm, owner);

            return owner;
        }

        /// <summary>
        /// Verifica si ya existe una ventana con el tipo de View especificado.
        /// </summary>
        public static bool HasWindowOfType<TView>() where TView : FrameworkElement
        {
            return _openWindows.Any(w => w.Content is TView);
        }

        /// <summary>
        /// Obtiene la primera ventana que contiene el tipo de View especificado, o null si no existe.
        /// </summary>
        public static Window? GetWindowOfType<TView>() where TView : FrameworkElement
        {
            return _openWindows.FirstOrDefault(w => w.Content is TView);
        }

        /// <summary>
        /// Abre una nueva ventana solo si no existe ya una del mismo tipo. Si existe, la activa.
        /// </summary>
        public static Window OpenOrActivateWindow<TView, TViewModel>(
            object? parameter = null,
            Action<TViewModel, Window>? init = null)
            where TView : FrameworkElement
            where TViewModel : class
        {
            var existingWindow = GetWindowOfType<TView>();
            if (existingWindow != null)
            {
                existingWindow.Activate();
                return existingWindow;
            }

            var newWindow = OpenWindow<TView, TViewModel>(parameter, init);
            newWindow.Activate();
            return newWindow;
        }
    }
}
