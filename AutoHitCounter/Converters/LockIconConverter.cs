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
            };
            icon.SetResourceReference(TextBlock.ForegroundProperty, isUnlocked
                ? "UnlockedToolTipBrush"
                : "LockedToolTipBrush"
            );

            var label = new TextBlock
            {
                Text = isUnlocked ? " Unlocked Splits" : " Locked Splits",
                FontWeight = FontWeights.SemiBold,
            };
            label.SetResourceReference(TextBlock.ForegroundProperty, isUnlocked
                ? "UnlockedToolTipBrush"
                : "LockedToolTipBrush"
            );

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            header.Children.Add(icon);
            header.Children.Add(label);


            var body = new TextBlock
            {
                Text = isUnlocked
                    ? "Reorder splits by dragging them up or down.\nRename Splits by double clicking them."
                    : "Splits are locked and can't be reordered.\nDouble click now moves through splits.",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 220,
                FontSize = 11
            };
            body.SetResourceReference(TextBlock.ForegroundProperty, "UnlockedToolTipTextBrush");

            var panel = new StackPanel();
            panel.Children.Add(header);
            panel.Children.Add(body);

            return new ToolTip { Content = panel };
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
}