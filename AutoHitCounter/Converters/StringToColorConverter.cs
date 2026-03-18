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
        if (string.IsNullOrEmpty(str)) return Colors.Transparent;

        return CssColorParser.TryParseColor(str) ?? Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Color color) return "#000000";
        return color.A == 255
            ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
    }
}