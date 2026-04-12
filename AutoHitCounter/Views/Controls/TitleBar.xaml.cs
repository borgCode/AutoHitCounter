// 

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoHitCounter.Behaviors;

namespace AutoHitCounter.Views.Controls;

public partial class TitleBar : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(TitleBar), new PropertyMetadata("Window"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public TitleBar()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            window.StateChanged += (_, _) => UpdateMaximizeIcon(window);
        };
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject dep)
        {
            while (dep != null)
            {
                if (dep is Button)
                    return;

                dep = VisualTreeHelper.GetParent(dep);
            }
        }

        var window = Window.GetWindow(this);
        if (window == null) return;

        if (e.ClickCount == 2)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            return;
        }

        if (window.WindowState == WindowState.Maximized)
        {
            var mousePos = e.GetPosition(window);
            double percentX = mousePos.X / window.ActualWidth;

            var cursor = WindowResizeBehavior.GetCursorPosition();

            window.WindowState = WindowState.Normal;
            window.UpdateLayout();

            window.Left = cursor.X - (window.Width * percentX);
            window.Top = cursor.Y - mousePos.Y;
        }

        window.DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null) window.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window == null) return;
        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void UpdateMaximizeIcon(Window window)
    {
        if (MaximizeButton.Content is TextBlock tb)
            tb.Text = window.WindowState == WindowState.Maximized ? "❐" : "☐";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        Window.GetWindow(this)?.Close();
}