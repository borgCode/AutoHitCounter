using System.ComponentModel;
using System.Windows;
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
                vm.ReloadFromSettings();
        }

        base.OnClosing(e);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}