using System.Threading;
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
        private static Mutex _mutex;
        MainViewModel _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "AutoHitCounter";

            _mutex = new Mutex(true, appName, out var createdNew);

            if (!createdNew)
            {
                Current.Shutdown();
            }

            base.OnStartup(e);

            ProfileMigrator.RunIfNeeded();
            
            var savedTheme = (ThemeMode)SettingsManager.Default.ThemeMode;
            ThemeService.Apply(savedTheme);
            if (savedTheme == ThemeMode.System)
                ThemeService.StartWatchingSystem();

            IMemoryService memoryService = new MemoryService();
            IStateService stateService = new StateService();
            IProfileService profileService = new ProfileService();

            ITickService tickService = new TickService(memoryService, stateService);

            OverlayServerService overlayServerService = new OverlayServerService();
            SplitNavigationService splitNavigationService = new SplitNavigationService();
            ExternalIntegrationService externalIntegrationService = new ExternalIntegrationService();

            HookManager hookManager = new HookManager(memoryService);

            var hotkeyManager = new HotkeyManager(memoryService);

            IGameModuleFactory gameModuleFactory =
                new GameModuleFactory(memoryService, stateService, hookManager, tickService);

            OverlayProfileManager.MigrateFromSettingsIfNeeded();
            var overlayProfileManager = new OverlayProfileManager();

            var overlaySettingsViewModel = new OverlaySettingsViewModel(overlayServerService, overlayProfileManager);

            var settingsViewModel = new SettingsViewModel(stateService, overlaySettingsViewModel);

            var hotkeysViewModel = new HotkeyTabViewModel(hotkeyManager, stateService);


            var runStateService = new RunStateService(profileService);
            var customGameService = new CustomGameService(new SettingsCustomGamesStore(), profileService, runStateService);
            var orchestrator = new GameSessionOrchestrator(memoryService, hotkeyManager, gameModuleFactory, stateService);

            _mainViewModel = new MainViewModel(hotkeyManager, gameModuleFactory, profileService,
                stateService,
                settingsViewModel, hotkeysViewModel, overlayServerService, splitNavigationService, externalIntegrationService,
                orchestrator, runStateService, customGameService);
            var mainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };
            mainWindow.Show();

            

            stateService.Publish(State.AppStart);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ThemeService.StopWatchingSystem();
            _mainViewModel?.FlushRunState();
            base.OnExit(e);
        }
    }
}