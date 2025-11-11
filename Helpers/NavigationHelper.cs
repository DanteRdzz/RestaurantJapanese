using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

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
