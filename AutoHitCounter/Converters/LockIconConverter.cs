using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoHitCounter.Converters
{
    public class LockIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? "\uE785" : "\uE72E";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class LockTooltipConverter : IValueConverter
    {
        private static readonly FontFamily Mdl2 = new FontFamily("Segoe MDL2 Assets");
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            // Prettier tooltip
        {
            var isUnlocked = value is true;

            var icon = new TextBlock
            {
                Text = isUnlocked ? "\uE785" : "\uE72E",
                FontFamily = Mdl2,
                FontSize = 13,
                Foreground = new SolidColorBrush(isUnlocked
                    ? Color.FromRgb(0x9D, 0x61, 0xA8)
                    : Color.FromRgb(0x99, 0x99, 0x99)),
            };

            var label = new TextBlock
            {
                Text = isUnlocked ? " Unlocked Splits" : " Locked Splits",
                FontWeight = FontWeights.SemiBold,
                Foreground = icon.Foreground,
            };

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            header.Children.Add(icon);
            header.Children.Add(label);


            var body = new TextBlock
            {
                Text = isUnlocked
                    ? "Reorder splits by dragging them up or down.\nRename Splits by double clicking them."
                    : "Splits are locked and can't be reordered.\nDouble click now moves through splits.",
                Foreground = new SolidColorBrush(Color.FromRgb(0xBB, 0xBB, 0xBB)),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 220,
                FontSize = 11
            };

            var panel = new StackPanel();
            panel.Children.Add(header);
            panel.Children.Add(body);

            return new ToolTip { Content = panel };
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}