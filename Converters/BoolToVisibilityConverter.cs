using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace RestaurantJapanese.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
    public object Convert(object value, Type targetType, object parameter, string language)
   {
  if (value is bool boolValue)
       {
      // Si el parámetro es "Inverse", invertir la lógica
                bool shouldInvert = parameter?.ToString() == "Inverse";
   bool result = shouldInvert ? !boolValue : boolValue;
   
                return result ? Visibility.Visible : Visibility.Collapsed;
     }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
{
            if (value is Visibility visibility)
    {
    bool shouldInvert = parameter?.ToString() == "Inverse";
             bool result = visibility == Visibility.Visible;
        
                return shouldInvert ? !result : result;
            }
         
       return false;
        }
    }
}