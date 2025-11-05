using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantJapanese.Helpers
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T backing, T value, [CallerMemberName] string? propName = null)
        {
            if (!Equals(backing, value))
            {
                backing = value;
                OnPropertyChanged(propName);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
