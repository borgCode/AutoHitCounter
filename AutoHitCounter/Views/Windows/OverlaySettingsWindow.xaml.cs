using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoHitCounter.Models;
using AutoHitCounter.Utilities;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Views.Windows;

public partial class OverlaySettingsWindow : Window
{
    public OverlaySettingsWindow()
    {
        InitializeComponent();
        if (Application.Current.MainWindow != null)
        {
            Application.Current.MainWindow.Closing += (sender, args) => { Close(); };
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is OverlaySettingsViewModel { IsDirty: true } vm)
        {
            var result = MsgBox.ShowYesNoCancel(
                "You have unsaved changes. Would you like to save before closing?",
                "Unsaved Changes");


            if (result == null)
            {
                e.Cancel = true;
                return;
            }

            if (result == true)
            {
                vm.SaveCommand.Execute(null);
            }
            else
                vm.ReloadFromProfile();
        }

        base.OnClosing(e);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResetToDefault_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;
        if (menuItem.Parent is not ContextMenu contextMenu) return;

        var target = contextMenu.PlacementTarget as FrameworkElement;
        while (target != null && target.Tag is not string { Length: > 0 })
            target = VisualTreeHelper.GetParent(target) as FrameworkElement;

        if (target?.Tag is not string propName) return;

        var defaults = OverlayProfileManager.CreateDefaultConfig();
        var configProp = typeof(OverlayConfig).GetProperty(propName);
        if (configProp == null) return;

        var defaultValue = configProp.GetValue(defaults);
        if (defaultValue == null) return;

        var vmProp = DataContext?.GetType().GetProperty(propName);
        vmProp?.SetValue(DataContext, defaultValue);
    }
}