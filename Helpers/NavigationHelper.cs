using Microsoft.UI.Xaml.Controls;

namespace RestaurantJapanese.Helpers
{
    public static class NavigationHelper
    {
        public static Frame? RootFrame { get; set; }

        public static void Navigate<TPage>(object? parameter = null) where TPage : class
        {
            RootFrame?.Navigate(typeof(TPage), parameter);
        }
    }
}