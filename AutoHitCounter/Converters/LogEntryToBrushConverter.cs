//

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AutoHitCounter.Models;

namespace AutoHitCounter.Converters;

public class LogEntryToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EventLogEntry entry)
        {
            return entry.Value 
                ? GetBrush("EventTrueBrush")
                : GetBrush("EventFalseBrush");
        }

        return Brushes.Black;
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
