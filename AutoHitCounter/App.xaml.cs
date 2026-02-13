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
            IStateService stateService = new StateService();
            IProfileService profileService = new ProfileService();

            ITickService tickService = new TickService(memoryService, stateService);

            HookManager hookManager = new HookManager(memoryService);
            
            GameModuleFactory gameModuleFactory = new GameModuleFactory(memoryService, stateService, hookManager, tickService);
            
            var mainViewModel = new MainViewModel(memoryService, gameModuleFactory, profileService, stateService);
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
        }
    }
}