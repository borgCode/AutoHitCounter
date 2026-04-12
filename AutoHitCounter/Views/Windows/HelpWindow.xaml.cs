using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Views.Windows;

public partial class HelpWindow
{
    private readonly StackPanel[] _pages;
    private readonly Button[] _tabs;
    private int _currentPage;

    public HelpWindow()
    {
        InitializeComponent();
        _pages =
        [
            (StackPanel)FindName("PageOverview"),
            (StackPanel)FindName("PageSplits"),
            (StackPanel)FindName("PageProfileEditor"),
            (StackPanel)FindName("PageHotkeys"),
            (StackPanel)FindName("PageSettingsAndOverlay")
        ];
        _tabs =
        [
            (Button)FindName("TabOverview"),
            (Button)FindName("TabSplits"),
            (Button)FindName("TabProfileEditor"),
            (Button)FindName("TabHotkeys"),
            (Button)FindName("TabSettingsAndOverlay")
        ];
        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        _currentPage = index;
        for (int i = 0; i < _pages.Length; i++)
            _pages[i].Visibility = i == index ? Visibility.Visible : Visibility.Collapsed;

        PageIndicator.Text = $"{index + 1} / {_pages.Length}";
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int index))
            ShowPage(index);
    }

    private void Prev_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 0) ShowPage(_currentPage - 1);
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _pages.Length - 1) ShowPage(_currentPage + 1);
    }

    private void FontIncrease_Click(object sender, RoutedEventArgs e) => AdjustFontSize(1);
    private void FontDecrease_Click(object sender, RoutedEventArgs e) => AdjustFontSize(-1);

    private void AdjustFontSize(double delta)
    {
        foreach (var page in _pages)
            SetFontSizeOnChildren(page, delta);
    }

    private static void SetFontSizeOnChildren(DependencyObject parent, double delta)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBlock tb)
            {
                var newSize = tb.FontSize + delta;
                if (newSize >= 9 && newSize <= 24)
                    tb.FontSize = newSize;
            }

            SetFontSizeOnChildren(child, delta);
        }
    }

    private void TrackExample_Click(object sender, RoutedEventArgs e)
    {
        var selected = (ExampleGame.SelectedItem as ComboBoxItem)?.Content?.ToString();
        TrackingText.Text = $"Currently Tracking: {selected}";
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}