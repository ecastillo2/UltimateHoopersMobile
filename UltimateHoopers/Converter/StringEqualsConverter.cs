using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace UltimateHoopers.Converters
{
    /// <summary>
    /// Converter to check if a string equals a specific value.
    /// Returns Colors.PrimaryColor if equal, and Colors.Transparent if not equal.
    /// </summary>
    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string stringValue = value.ToString();
            string comparisonValue = parameter.ToString();

            bool isEqual = string.Equals(stringValue, comparisonValue, StringComparison.OrdinalIgnoreCase);

            // If the target type is Color, return colors
            if (targetType == typeof(Color))
            {
                return isEqual
                    ? (Color)Application.Current.Resources["PrimaryColor"]
                    : Colors.Transparent;
            }

            // If the target type is a brush, return brushes
            if (targetType == typeof(Brush))
            {
                return isEqual
                    ? new SolidColorBrush((Color)Application.Current.Resources["PrimaryColor"])
                    : new SolidColorBrush(Colors.Transparent);
            }

            // If the target is string color, return white for selected, otherwise primary text color
            if (targetType == typeof(Color) || targetType == typeof(string))
            {
                return isEqual
                    ? Colors.White
                    : (Color)Application.Current.Resources["PrimaryTextColor"];
            }

            // Default just return true/false
            return isEqual;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}