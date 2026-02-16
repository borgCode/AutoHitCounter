// 

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using AutoHitCounter.Utilities;

namespace TarnishedTool.Views.Windows;

public partial class CustomMessageBox : Window
{
    public bool Result { get; private set; }

    public CustomMessageBox(string message, bool showCancel, string title = "Message")
    {
        InitializeComponent();
        MessageText.Text = message;
        TitleText.Text = title;
            
        if (showCancel)
        {
            CancelButton.Visibility = Visibility.Visible;
        }
        
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
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}