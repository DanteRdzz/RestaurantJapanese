using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RestaurantJapanese.Helpers
{
    public static class NavigationHelper
    {
        public static IServiceProvider? Services { get; set; }

        private static readonly List<Window> _openWindows = new();

        public static Window OpenWindow<TView, TViewModel>()
            where TView : UserControl, new()
            where TViewModel : class
        {
            if (Services is null) throw new InvalidOperationException("NavigationHelper.Services no inicializado.");

            var view = new TView { DataContext = Services.GetRequiredService<TViewModel>() };
            var win = new Window { Content = view };
            _openWindows.Add(win);
            win.Closed += (_, __) => _openWindows.Remove(win);
            win.Activate();
            return win;
        }

        public static Window ReplaceWindow<TView, TViewModel>(Window windowToClose)
            where TView : UserControl, new()
            where TViewModel : class
        {
            var newWin = OpenWindow<TView, TViewModel>();
            windowToClose?.Close();
            return newWin;
        }

        public static Window OpenWindow<TView, TViewModel>(object? parameter, Action<TViewModel, object?>? init)
            where TView : UserControl, new()
            where TViewModel : class
        {
            if (Services is null) throw new InvalidOperationException("NavigationHelper.Services no inicializado.");

            var vm = Services.GetRequiredService<TViewModel>();
            init?.Invoke(vm, parameter);

            var view = new TView { DataContext = vm };
            var win = new Window { Content = view };
            _openWindows.Add(win);
            win.Closed += (_, __) => _openWindows.Remove(win);
            win.Activate();
            return win;
        }
    }
}
