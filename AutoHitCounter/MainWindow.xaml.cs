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
    }
}