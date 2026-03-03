// 

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Views.Windows;

public partial class CustomMessageBox : Window
{
    public bool? Result { get; private set; }
    public CustomMessageBoxResult ResultValue { get; private set; }

    public CustomMessageBox(string message, string title, bool showYesNo, bool showCancel)
    {
        InitializeComponent();
        MessageText.Text = message;
        TitleText.Text = title;

        if (showYesNo)
        {
            OkButton.Visibility = Visibility.Collapsed;
            YesButton.Visibility = Visibility.Visible;
            NoButton.Visibility = Visibility.Visible;
        }

        if (showCancel)
            CancelButton.Visibility = Visibility.Visible;

        SetupWindow();
    }

    public CustomMessageBox(string message, string title, CustomMessageBoxResult[] buttons)
    {
        InitializeComponent();
        MessageText.Text = message;
        TitleText.Text = title;

        OkButton.Visibility = Visibility.Collapsed;

        foreach (var button in buttons)
        {
            switch (button)
            {
                case CustomMessageBoxResult.Replace: ReplaceButton.Visibility = Visibility.Visible; break;
                case CustomMessageBoxResult.Rename: RenameButton.Visibility = Visibility.Visible; break;
                case CustomMessageBoxResult.Skip: SkipButton.Visibility = Visibility.Visible; break;
                case CustomMessageBoxResult.Yes: YesButton.Visibility = Visibility.Visible; break;
                case CustomMessageBoxResult.No: NoButton.Visibility = Visibility.Visible; break;
                case CustomMessageBoxResult.Ok: OkButton.Visibility = Visibility.Visible; break;
                case CustomMessageBoxResult.All: AllButton.Visibility = Visibility.Visible; break;
                case CustomMessageBoxResult.Current: CurrentButton.Visibility = Visibility.Visible; break;
                case CustomMessageBoxResult.Cancel: CancelButton.Visibility = Visibility.Visible; break;
                
            }
        }

        SetupWindow();
    }

    private void SetupWindow()
    {
        Loaded += (s, e) =>
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            User32.SetTopmost(hwnd);

            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Closing += (sender, args) => { Close(); };
            }
        };
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        ResultValue = CustomMessageBoxResult.Ok;
        Close();
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        ResultValue = CustomMessageBoxResult.Yes;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        ResultValue = CustomMessageBoxResult.No;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = null;
        ResultValue = CustomMessageBoxResult.Cancel;
        Close();
    }

    private void ReplaceButton_Click(object sender, RoutedEventArgs e)
    {
        ResultValue = CustomMessageBoxResult.Replace;
        Close();
    }

    private void RenameButton_Click(object sender, RoutedEventArgs e)
    {
        ResultValue = CustomMessageBoxResult.Rename;
        Close();
    }

    private void SkipButton_Click(object sender, RoutedEventArgs e)
    {
        ResultValue = CustomMessageBoxResult.Skip;
        Close();
    }

    private void AllButton_Click(object sender, RoutedEventArgs e)
    {
        ResultValue = CustomMessageBoxResult.All;
        Close();
    }

    private void CurrentButton_Click(object sender, RoutedEventArgs e)
    {
        ResultValue = CustomMessageBoxResult.Current;
        Close();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}

public enum CustomMessageBoxResult
{
    Ok,
    Cancel,
    Yes,
    No,
    Replace,
    Rename,
    Skip,
    All,
    Current
}