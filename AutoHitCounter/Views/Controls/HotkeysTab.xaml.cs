// 

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Views.Controls;

public partial class HotkeysTab : UserControl
{
    public HotkeysTab()
    {
        InitializeComponent();
    }
    
    
    private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && DataContext is HotkeyTabViewModel vm)
        {
            string actionId = textBox.Tag.ToString();
            vm.StartSettingHotkey(actionId);
        }
    }

    private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is HotkeyTabViewModel vm)
        {
            vm.ConfirmHotkey();
        }
    }

    private void HotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not HotkeyTabViewModel vm) return;

        switch (e.Key)
        {
            case Key.Enter:
            {
                vm.ConfirmHotkey();
                if (sender is TextBox textBox)
                {
                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
                e.Handled = true;
                break;
            }
            case Key.Escape:
            {
                vm.CancelSettingHotkey();
                if (sender is TextBox textBox)
                {
                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
                e.Handled = true;
                break;
            }
        }
    }
}