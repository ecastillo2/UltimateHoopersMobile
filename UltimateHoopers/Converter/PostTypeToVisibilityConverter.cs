using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace UltimateHoopers.Converters
{
    public class PostTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string postType && parameter is string parameterType)
            {
                return postType.Equals(parameterType, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LikedPostToHeartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool liked)
            {
                return liked ? "❤️" : "🤍";
            }

            return "🤍";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SavedPostToBookmarkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool saved)
            {
                return saved ? "🔖" : "🏷️";
            }

            return "🏷️";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check for non-nullable int
            if (value is int count)
            {
                return count > 0;
            }

            // Check for nullable int properly
            if (value is int?)
            {
                int? nullableCount = (int?)value;
                return nullableCount.HasValue && nullableCount.Value > 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}