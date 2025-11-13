using RestaurantJapanese.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RestaurantJapanese.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected void Set<T>(ref T backing, T value, [CallerMemberName] string? propName = null)
        {
      if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(backing, value))
            {
             backing = value;
       OnPropertyChanged(propName);
   }
        }

        // Comando global para volver atrás
        public ICommand GoBackCommand => new RelayCommand(_ =>
        {
    var viewModelName = GetType().Name;
   var navStatus = NavigationHelper.GetNavigationStatus();
   System.Diagnostics.Debug.WriteLine($"[{viewModelName}] GoBackCommand ejecutado. Estado: {navStatus}");
  
      if (NavigationHelper.CanGoBack)
            {
   NavigationHelper.GoBack();
          System.Diagnostics.Debug.WriteLine($"[{viewModelName}] Navegación hacia atrás ejecutada");
   }
  else
            {
         System.Diagnostics.Debug.WriteLine($"[{viewModelName}] No se puede navegar hacia atrás - no hay historial disponible");
   }
        });
    }
}