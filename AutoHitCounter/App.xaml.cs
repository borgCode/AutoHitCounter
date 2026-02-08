using System.Windows;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IMemoryService memoryService = new MemoryService();
            
            ITickService tickService = new TickService(memoryService);
            IStateService stateService = new StateService();

            HookManager hookManager = new HookManager(memoryService);
            
            GameModuleFactory gameModuleFactory = new GameModuleFactory(memoryService, stateService, hookManager);
            
            var mainViewModel = new MainViewModel(memoryService);
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
        }
    }
}