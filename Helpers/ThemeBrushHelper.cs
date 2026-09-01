using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ShowMyMusic.Models;

namespace ShowMyMusic.Helpers
{
    public class GlassBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var style = GlassStyle.iOSFrostedGlass;
            double opacity = 0.9;
            string customBg = "#1A1A24";
            Color accentColor = Color.FromRgb(139, 92, 246);
            bool useAdaptive = false;

            if (values.Length > 0 && values[0] is GlassStyle s) style = s;
            if (values.Length > 1 && values[1] is double op) opacity = Math.Clamp(op, 0.1, 1.0);
            if (values.Length > 2 && values[2] is string bg) customBg = bg;
            if (values.Length > 3 && values[3] is Color ac) accentColor = ac;
            if (values.Length > 4 && values[4] is bool adapt) useAdaptive = adapt;

            // BUG FIX #14: Use Math.Clamp to prevent byte overflow in alpha multiplications
            byte alpha = (byte)Math.Clamp((int)(opacity * 255), 20, 255);

            switch (style)
            {
                case GlassStyle.iOSFrostedGlass:
                    if (useAdaptive)
                    {
                        byte rTop = (byte)Math.Clamp((int)(accentColor.R * 0.25 + 24), 0, 255);
                        byte gTop = (byte)Math.Clamp((int)(accentColor.G * 0.25 + 24), 0, 255);
                        byte bTop = (byte)Math.Clamp((int)(accentColor.B * 0.25 + 32), 0, 255);
                        return new LinearGradientBrush(
                            Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.85), 0, 255), rTop, gTop, bTop),
                            Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.95), 0, 255), (byte)Math.Clamp((int)(rTop * 0.5), 0, 255), (byte)Math.Clamp((int)(gTop * 0.5), 0, 255), (byte)Math.Clamp((int)(bTop * 0.5), 0, 255)),
                            new Point(0, 0), new Point(0, 1));
                    }
                    else
                    {
                        return new LinearGradientBrush(
                            Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.82), 0, 255), 34, 34, 48),
                            Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.94), 0, 255), 16, 16, 24),
                            new Point(0, 0), new Point(0, 1));
                    }

                case GlassStyle.Win11Acrylic:
                    return new LinearGradientBrush(
                        Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.88), 0, 255), 32, 32, 44),
                        Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.96), 0, 255), 18, 18, 26),
                        new Point(0, 0), new Point(0, 1));

                case GlassStyle.DarkGlass:
                    return new LinearGradientBrush(
                        Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.90), 0, 255), 18, 14, 28),
                        Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.98), 0, 255), 8, 6, 14),
                        new Point(0, 0), new Point(0, 1));

                case GlassStyle.LightGlass:
                    return new LinearGradientBrush(
                        Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.92), 0, 255), 248, 250, 252),
                        Color.FromArgb((byte)Math.Clamp((int)(alpha * 0.98), 0, 255), 226, 232, 240),
                        new Point(0, 0), new Point(0, 1));

                case GlassStyle.Solid:
                default:
                    try
                    {
                        var col = (Color)ColorConverter.ConvertFromString(customBg);
                        return new SolidColorBrush(Color.FromArgb(255, col.R, col.G, col.B));
                    }
                    catch
                    {
                        return new SolidColorBrush(Color.FromRgb(24, 24, 34));
                    }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GlassBorderBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var style = GlassStyle.iOSFrostedGlass;
            string customBorder = "#33FFFFFF";

            if (values.Length > 0 && values[0] is GlassStyle s) style = s;
            if (values.Length > 1 && values[1] is string cb) customBorder = cb;

            switch (style)
            {
                case GlassStyle.iOSFrostedGlass:
                    return new LinearGradientBrush(
                        Color.FromArgb(140, 255, 255, 255),
                        Color.FromArgb(30, 255, 255, 255),
                        new Point(0, 0), new Point(0, 1));

                case GlassStyle.Win11Acrylic:
                    return new LinearGradientBrush(
                        Color.FromArgb(80, 255, 255, 255),
                        Color.FromArgb(20, 255, 255, 255),
                        new Point(0, 0), new Point(0, 1));

                case GlassStyle.LightGlass:
                    return new LinearGradientBrush(
                        Color.FromArgb(220, 255, 255, 255),
                        Color.FromArgb(120, 203, 213, 225),
                        new Point(0, 0), new Point(0, 1));

                case GlassStyle.DarkGlass:
                    return new LinearGradientBrush(
                        Color.FromArgb(120, 255, 255, 255),
                        Color.FromArgb(40, 255, 255, 255),
                        new Point(0, 0), new Point(0, 1));

                case GlassStyle.Solid:
                default:
                    try
                    {
                        var col = (Color)ColorConverter.ConvertFromString(customBorder);
                        return new SolidColorBrush(col);
                    }
                    catch
                    {
                        return new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                    }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GlassSpecularVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GlassStyle style)
            {
                return (style == GlassStyle.iOSFrostedGlass || style == GlassStyle.LightGlass || style == GlassStyle.Win11Acrylic)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GlassForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GlassStyle style && style == GlassStyle.LightGlass)
                return new SolidColorBrush(Color.FromRgb(15, 23, 42));
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GlassSubtextForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GlassStyle style && style == GlassStyle.LightGlass)
                return new SolidColorBrush(Color.FromRgb(71, 85, 105));
            return new SolidColorBrush(Color.FromRgb(156, 163, 175));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class GlassBadgeBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GlassStyle style && style == GlassStyle.LightGlass)
                return new SolidColorBrush(Color.FromArgb(25, 0, 0, 0));
            return new SolidColorBrush(Color.FromArgb(45, 255, 255, 255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}