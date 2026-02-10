// 

using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoHitCounter.Core;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using AutoHitCounter.Views.Windows;

namespace AutoHitCounter.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IMemoryService _memoryService;
        private readonly GameModuleFactory _gameModuleFactory;
        private readonly ITickService _tickService;
        private readonly IProfileService _profileService;
        private readonly Dictionary<uint, string> _eldenRingEvents;
        private IGameModule? _currentModule;
        private Profile? _activeProfile;
        
        
        
        public MainViewModel(IMemoryService memoryService, GameModuleFactory gameModuleFactory,
            ITickService tickService, IProfileService profileService)
        {
            _memoryService = memoryService;
            _gameModuleFactory = gameModuleFactory;
            _tickService = tickService;
            _profileService = profileService;
            _eldenRingEvents = EventLoader.GetEvents("EldenRingEvents");

            OpenProfileEditorCommand = new DelegateCommand(OpenProfileEditor);
            
            
            Games.Add(new Game { GameName = "Dark Souls Remastered", ProcessName = "darksoulsremastered" });
            Games.Add(new Game { GameName = "Dark Souls 2 Vanilla", ProcessName = "darksoulsii" });
            Games.Add(new Game { GameName = "Dark Souls 2 Scholar", ProcessName = "darksoulsii" });
            Games.Add(new Game { GameName = "Dark Souls 3", ProcessName = "darksoulsiii" });
            Games.Add(new Game { GameName = "Sekiro", ProcessName = "sekiro" });
            Games.Add(new Game { GameName = "Elden Ring", ProcessName = "eldenring" });
        }

        #region Commands

        public DelegateCommand OpenProfileEditorCommand { get; }

        #endregion

        #region Properties

        public ObservableCollection<Game> Games { get; } = new();

        private Game _selectedGame;
        public Game SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (SetProperty(ref _selectedGame, value))
                {
                    SwapModule();
                }
            }
        }
        
        private int _hitCount;
        public int HitCount
        {
            get => _hitCount;
            set => SetProperty(ref _hitCount, value);
        }
        
        #endregion
        
        
        private void SwapModule()
        {
            _tickService.UnregisterGameTick();

            if (_selectedGame == null) return;

            _memoryService.StartAutoAttach(_selectedGame.ProcessName);
            _currentModule = _gameModuleFactory.CreateModule(_selectedGame);
            _currentModule.OnHit += count => HitCount += count;
            // _currentModule.OnBossKilled += () => AdvanceSplit();
            // _currentModule.StartGameTick();
        }
        
        private void OpenProfileEditor()
        {
            if (_selectedGame == null) return;

            var vm = new ProfileEditorViewModel(
                _eldenRingEvents,
                _profileService,
                _selectedGame.GameName,
                _activeProfile);

            var window = new ProfileEditorWindow { DataContext = vm };
            window.ShowDialog();

            _activeProfile = vm.SelectedProfile;
            // TODO: rebuild filtered dict and pass to module
        }

    }
    
    
}