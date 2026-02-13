// 

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AutoHitCounter.Enums;

namespace AutoHitCounter.Converters;

public class ParentChildMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isParent = value switch
        {
            bool b => b,
            SplitType type => type == SplitType.Parent,
            _ => false
        };

        return isParent ? new Thickness(4, 0, 0, 0) : new Thickness(20, 0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}