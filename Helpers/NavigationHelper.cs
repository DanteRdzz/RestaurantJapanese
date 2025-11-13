using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace RestaurantJapanese.Helpers
{
    public static class NavigationHelper
    {
        public static IServiceProvider? Services { get; set; }

  private static readonly Stack<Window> _navigationStack = new();
   
        // Mantenemos referencia a ventanas reemplazadas para el historial
   private static readonly Stack<(FrameworkElement View, object ViewModel)> _contentStack = new();

        private static T GetRequired<T>() where T : notnull
        {
if (Services is null)
          throw new InvalidOperationException("NavigationHelper.Services no está inicializado.");

     return Services.GetRequiredService<T>();
}

  private static void MakeFullScreen(Window win)
      {
     try
     {
        var appWindow = win.AppWindow; // API WinUI3
      appWindow?.SetPresenter(AppWindowPresenterKind.FullScreen);
          }
          catch { }
    }

        public static bool CanGoBack => _navigationStack.Count > 1 || _contentStack.Count > 0;
        
        public static void GoBack()
     {
   System.Diagnostics.Debug.WriteLine($"GoBack llamado. ContentStack: {_contentStack.Count}, WindowStack: {_navigationStack.Count}");
       
        // Si hay contenido en el stack de la misma ventana, navegar hacia atrás dentro de la ventana
    if (_contentStack.Count > 0)
            {
     var currentWindow = _navigationStack.Peek();
              var (previousView, previousViewModel) = _contentStack.Pop();
       
     previousView.DataContext = previousViewModel;
    currentWindow.Content = previousView;
     MakeFullScreen(currentWindow);
 
   System.Diagnostics.Debug.WriteLine("Navegación hacia atrás completada (contenido)");
     return;
   }
     
         // Si no hay contenido en el stack pero hay múltiples ventanas, cerrar ventana actual
     if (_navigationStack.Count > 1)
          {
      var current = _navigationStack.Pop();
    current.Close();

       var previous = _navigationStack.Peek();
      previous.Activate();
      System.Diagnostics.Debug.WriteLine("Navegación hacia atrás completada (ventana)");
       }
   else
 {
  System.Diagnostics.Debug.WriteLine("No se puede navegar hacia atrás - no hay historial");
      }
        }

        /// <summary>
<<<<<<< HEAD
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
=======
   /// Abre una nueva Window con la View/VM del contenedor.
>>>>>>> adding return buttons
        /// </summary>
        public static Window OpenWindow<TView, TViewModel>()
   where TView : FrameworkElement
         where TViewModel : class
        {
     var view = GetRequired<TView>();
      var vm = GetRequired<TViewModel>();

    view.DataContext = vm;

            var win = new Window { Content = view };
<<<<<<< HEAD
            
            // Configurar ventana (título y maximización)
            ConfigureWindow<TView>(win);
            
            _openWindows.Add(win);
            win.Closed += (_, __) => _openWindows.Remove(win);
=======
        MakeFullScreen(win);
            _navigationStack.Push(win);
      
  // Limpiar el stack de contenido al abrir nueva ventana
      _contentStack.Clear();
  
   System.Diagnostics.Debug.WriteLine($"Nueva ventana abierta. WindowStack: {_navigationStack.Count}");
            
  win.Closed += (_, __) => 
            { 
 if (_navigationStack.Contains(win)) 
          { 
      var temp = new Stack<Window>(); 
  while (_navigationStack.Count > 0) 
{ 
var w = _navigationStack.Pop(); 
   if (w != win) temp.Push(w); 
   } 
  while (temp.Count > 0) 
        _navigationStack.Push(temp.Pop()); 
       
      // También limpiar stack de contenido si se cierra la ventana
       if (_navigationStack.Count <= 1)
    _contentStack.Clear();
 
       System.Diagnostics.Debug.WriteLine("Ventana cerrada y stacks actualizados");
} 
       }; 
>>>>>>> adding return buttons

          return win;
        }

<<<<<<< HEAD
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
=======
   /// <summary>
>>>>>>> adding return buttons
        /// Reemplaza el contenido de una Window existente con otra View/VM del contenedor.
        /// Mantiene historial para navegación con botón Volver.
     /// </summary>
      public static Window ReplaceWindow<TView, TViewModel>(
 Window owner,
        object? parameter = null,
  Action<TViewModel, Window>? init = null)
          where TView : FrameworkElement
       where TViewModel : class
     {
            if (owner is null) throw new ArgumentNullException(nameof(owner));

            // Guardar el contenido actual para el historial ANTES de reemplazar
 if (owner.Content is FrameworkElement currentView && currentView.DataContext != null)
   {
         _contentStack.Push((currentView, currentView.DataContext));
  System.Diagnostics.Debug.WriteLine($"Contenido guardado en historial. ContentStack: {_contentStack.Count}");
}

     var view = GetRequired<TView>();
         var vm = GetRequired<TViewModel>();

       view.DataContext = vm;
            owner.Content = view;

<<<<<<< HEAD
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
=======
 MakeFullScreen(owner);
         init?.Invoke(vm, owner);

  System.Diagnostics.Debug.WriteLine($"Contenido reemplazado. Nuevo tipo: {typeof(TView).Name}");

            return owner;
        }
        
 /// <summary>
/// Limpia todos los stacks de navegación (usar con precaución).
   /// </summary>
        public static void ClearNavigationHistory()
   {
   _contentStack.Clear();
      System.Diagnostics.Debug.WriteLine("Historial de navegación limpiado");
        }
        
/// <summary>
   /// Información de debug del estado actual de navegación.
 /// </summary>
        public static string GetNavigationStatus()
       {
   return $"Windows: {_navigationStack.Count}, Content: {_contentStack.Count}, CanGoBack: {CanGoBack}";
>>>>>>> adding return buttons
        }
    }
}
