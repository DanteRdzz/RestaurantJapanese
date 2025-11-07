using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace RestaurantJapanese.Helpers
{
    public sealed class CurrencyConverterHelper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal d) return d.ToString("C", CultureInfo.CurrentCulture);
            if (value is double db) return db.ToString("C", CultureInfo.CurrentCulture);
            if (value is float f) return f.ToString("C", CultureInfo.CurrentCulture);
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
