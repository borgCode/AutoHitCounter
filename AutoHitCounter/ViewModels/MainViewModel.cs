// 

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private IGameModule _currentModule;

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

            SelectedGame = Games.FirstOrDefault();
            
            UpdateSplits();
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
                    Profiles.Clear();
                    foreach (var p in _profileService.GetProfiles(_selectedGame?.GameName))
                        Profiles.Add(p);

                    ActiveProfile = Profiles.FirstOrDefault();
                }
            }
        }

        public ObservableCollection<SplitViewModel> Splits { get; } = new();

        private SplitViewModel _selectedSplit;

        public SplitViewModel SelectedSplit
        {
            get => _selectedSplit;
            set => SetProperty(ref _selectedSplit, value);
        }

        private Profile _activeProfile;

        public Profile ActiveProfile
        {
            get => _activeProfile;
            set
            {
                if (SetProperty(ref _activeProfile, value))
                {
                    UpdateSplits();
                    SelectedSplit = Splits.FirstOrDefault();
                }
            }
        }

        public ObservableCollection<Profile> Profiles { get; } = new();

        #endregion

        private void SwapModule()
        {
            _tickService.UnregisterGameTick();

            if (_selectedGame == null) return;

            _memoryService.StartAutoAttach(_selectedGame.ProcessName);
            _currentModule = _gameModuleFactory.CreateModule(_selectedGame);
            _currentModule.OnHit += count => SelectedSplit.NumOfHits += count;
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

            Profiles.Clear();
            foreach (var p in _profileService.GetProfiles(_selectedGame.GameName))
                Profiles.Add(p);

            ActiveProfile = vm.SelectedProfile;
            // TODO: rebuild filtered dict and pass to module
        }

        private void UpdateSplits()
        {
            Splits.Clear();
            if (ActiveProfile == null) return;
            foreach (var activeProfileSplit in ActiveProfile.Splits)
            {
                Splits.Add(new SplitViewModel
                {
                    Name = activeProfileSplit.Name,
                    NumOfHits = 0,
                    PersonalBest = activeProfileSplit.PersonalBest
                });
            }
        }
    }
}