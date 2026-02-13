using System.Windows;
using System.Windows.Controls;
using AutoHitCounter.Utilities;

namespace AutoHitCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        
        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += (s, e) =>
            {
                if (SettingsManager.Default.MainWindowLeft > 0)
                    Left = SettingsManager.Default.MainWindowLeft;
            
                if (SettingsManager.Default.MainWindowTop > 0)
                    Top = SettingsManager.Default.MainWindowTop;
            };
        }
        
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            SettingsManager.Default.MainWindowLeft = Left;
            SettingsManager.Default.MainWindowTop = Top;
            SettingsManager.Default.Save();
        
         
        }

        private void GearButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}