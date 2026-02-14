// 

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoHitCounter.Behaviors;

public static class EditableTextBoxBehavior
{
    public static readonly DependencyProperty EditTriggerProperty =
        DependencyProperty.RegisterAttached(
            "EditTrigger",
            typeof(bool),
            typeof(EditableTextBoxBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEditTriggerChanged));

    public static bool GetEditTrigger(DependencyObject obj) => (bool)obj.GetValue(EditTriggerProperty);
    public static void SetEditTrigger(DependencyObject obj, bool value) => obj.SetValue(EditTriggerProperty, value);

    private static void OnEditTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox) return;

        if ((bool)e.NewValue)
        {
            textBox.Focus();
            textBox.SelectAll();
            textBox.LostKeyboardFocus += OnLostFocus;
            textBox.PreviewKeyDown += OnKeyDown;
        }
        else
        {
            textBox.LostKeyboardFocus -= OnLostFocus;
            textBox.PreviewKeyDown -= OnKeyDown;
        }
    }

    private static void OnLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox textBox)
            SetEditTrigger(textBox, false);
    }

    private static void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && sender is TextBox textBox)
        {
            SetEditTrigger(textBox, false);
            Keyboard.ClearFocus();
        }
    }
}