// 

using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoHitCounter.Converters;

public class ExpanderArrowConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? "▼" : "▶";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}