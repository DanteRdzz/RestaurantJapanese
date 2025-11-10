using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace RestaurantJapanese.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Show element if value is not null, hide if null
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}