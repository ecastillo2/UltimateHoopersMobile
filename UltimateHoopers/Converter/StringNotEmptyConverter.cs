using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace UltimateHoopers.Converters
{
    /// <summary>
    /// Converts a string URL to an ImageSource for binding in XAML
    /// </summary>
    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imageUrl && !string.IsNullOrWhiteSpace(imageUrl))
            {
                try
                {
                    return ImageSource.FromUri(new Uri(imageUrl));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error converting URL to ImageSource: {ex.Message}");
                    return null;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}