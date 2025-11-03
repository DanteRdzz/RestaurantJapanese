using Microsoft.UI.Xaml.Data;
using System;

namespace RestaurantJapanese.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("C");
            }
            if (value is double doubleValue)
            {
                return doubleValue.ToString("C");
            }
            if (value is int intValue)
            {
                return intValue.ToString("C");
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue && decimal.TryParse(stringValue.Replace("$", "").Replace(",", ""), out decimal result))
            {
                return result;
            }
            return 0m;
        }
    }
}