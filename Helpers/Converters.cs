using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ShowMyMusic.Helpers
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = value is bool b && b;
            if (Invert) val = !val;
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility vis && (vis == Visibility.Visible) ^ Invert;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNotNull = value != null;
            bool shouldInvert = Invert || (parameter is string p && p.Equals("Inverse", StringComparison.OrdinalIgnoreCase));
            if (shouldInvert) isNotNull = !isNotNull;
            return isNotNull ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class PercentToCornerRadiusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double percent && values[1] is double dimension)
            {
                double radius = (percent / 100.0) * (dimension / 2.0);
                return new CornerRadius(Math.Max(0, radius));
            }
            if (values.Length >= 1 && values[0] is double p)
            {
                double radius = (p / 100.0) * 44.0;
                return new CornerRadius(Math.Max(0, radius));
            }
            return new CornerRadius(16);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class PercentToRadiusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double percent && values[1] is double dimension)
            {
                double radius = (percent / 100.0) * (dimension / 2.0);
                return Math.Max(0, radius);
            }
            if (values.Length >= 1 && values[0] is double p)
            {
                return (p / 100.0) * 32.0;
            }
            return 8.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch { }
            }
            return new SolidColorBrush(Color.FromRgb(30, 30, 40));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ProgressWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length >= 2)
            {
                double percent = 0.0;
                double totalWidth = 0.0;

                if (values[0] is double d1) percent = d1;
                else if (values[0] is IConvertible c1) try { percent = System.Convert.ToDouble(c1, CultureInfo.InvariantCulture); } catch { }

                if (values[1] is double d2) totalWidth = d2;
                else if (values[1] is IConvertible c2) try { totalWidth = System.Convert.ToDouble(c2, CultureInfo.InvariantCulture); } catch { }

                if (totalWidth > 0)
                {
                    double clamped = Math.Clamp(percent, 0.0, 100.0);
                    return totalWidth * (clamped / 100.0);
                }
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class PlayPauseIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPlaying = value is bool b && b;
            return isPlaying
                ? "M6 19h4V5H6v14zm8-14v14h4V5h-4z"
                : "M8 5v14l11-7z";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}