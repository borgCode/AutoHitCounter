using System.Windows;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Memory;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
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
            
            var hotkeyManager = new HotkeyManager(memoryService);

            GameModuleFactory gameModuleFactory =
                new GameModuleFactory(memoryService, stateService, hookManager, tickService);

            var settingsViewModel = new SettingsViewModel(stateService);
            var hotkeysViewModel = new HotkeyTabViewModel(hotkeyManager, stateService);

            var mainViewModel = new MainViewModel(memoryService, hotkeyManager, gameModuleFactory, profileService, stateService,
                settingsViewModel, hotkeysViewModel);
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
            
            stateService.Publish(State.AppStart);
        }
    }
}