using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoHitCounter.Converters;

public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string str) return Colors.Transparent;

        str = str.Trim();

        // hex
        if (str.StartsWith("#"))
        {
            // fixing RRGGBBAA preview
            if (str.Length == 9)
            {
                try
                {
                    var r = System.Convert.ToByte(str.Substring(1, 2), 16);
                    var g = System.Convert.ToByte(str.Substring(3, 2), 16);
                    var b = System.Convert.ToByte(str.Substring(5, 2), 16);
                    var a = System.Convert.ToByte(str.Substring(7, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                }
                catch
                {
                    return Colors.Transparent;
                }
            }

            try
            {
                var result = ColorConverter.ConvertFromString(str);
                return result != null ? (Color)result : Colors.Transparent;
            }
            catch
            {
                return Colors.Transparent;
            }
        }

        // rgba(r, g, b, a)
        if (str.StartsWith("rgba(") && str.EndsWith(")"))
        {
            var inner = str.Substring(5, str.Length - 6);
            var parts = inner.Split(',');
            if (parts.Length == 4
                && byte.TryParse(parts[0].Trim(), out var r)
                && byte.TryParse(parts[1].Trim(), out var g)
                && byte.TryParse(parts[2].Trim(), out var b)
                && double.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
            {
                return Color.FromArgb((byte)(a * 255), r, g, b);
            }
        }

        // rgb(r, g, b)
        if (str.StartsWith("rgb(") && str.EndsWith(")"))
        {
            var inner = str.Substring(4, str.Length - 5);
            var parts = inner.Split(',');
            if (parts.Length == 3
                && byte.TryParse(parts[0].Trim(), out var r)
                && byte.TryParse(parts[1].Trim(), out var g)
                && byte.TryParse(parts[2].Trim(), out var b))
            {
                return Color.FromRgb(r, g, b);
            }
        }

        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Color color) return "#000000";

        // If fully opaque return hex, otherwise return rgba
        if (color.A == 255)
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
    }
}