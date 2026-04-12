using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoHitCounter.Converters;

public class AttemptCountColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count && count > 0)
            return GetBrush("AttemptsCounterBrush");
        
        return GetBrush("TextBrush");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    private static SolidColorBrush GetBrush(string key)
    {
        if (Application.Current.Resources[key] is SolidColorBrush brush)
            return brush;
        
        return new SolidColorBrush(Colors.White);
    }
}