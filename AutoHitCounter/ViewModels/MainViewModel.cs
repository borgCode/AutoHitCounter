// 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoHitCounter.Core;
using AutoHitCounter.Enums;
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
        private readonly IProfileService _profileService;
        private readonly Dictionary<uint, string> _eldenRingEvents;
        private IGameModule _currentModule;
        
        private readonly List<SplitViewModel> _allSplits = new();

        public MainViewModel(IMemoryService memoryService, GameModuleFactory gameModuleFactory,
            IProfileService profileService, IStateService stateService)
        {
            _memoryService = memoryService;
            _gameModuleFactory = gameModuleFactory;
            _profileService = profileService;
            _eldenRingEvents = EventLoader.GetEvents("EldenRingEvents");
            
            stateService.Subscribe(State.Attached, OnAttached);
            stateService.Subscribe(State.NotAttached, OnNotAttached);

            OpenProfileEditorCommand = new DelegateCommand(OpenProfileEditor);


            Games.Add(new Game { GameName = "Dark Souls Remastered", ProcessName = "darksoulsremastered" });
            Games.Add(new Game { GameName = "Dark Souls 2 Vanilla", ProcessName = "darksoulsii" });
            Games.Add(new Game { GameName = "Dark Souls 2 Scholar", ProcessName = "darksoulsii" });
            Games.Add(new Game { GameName = "Dark Souls 3", ProcessName = "darksoulsiii" });
            Games.Add(new Game { GameName = "Sekiro", ProcessName = "sekiro" });
            Games.Add(new Game { GameName = "Elden Ring", ProcessName = "eldenring" });

            SelectedGame = Games.FirstOrDefault(game => game.GameName == SettingsManager.Default.LastSelectedGame);
            
            UpdateSplits();
        }

        
        #region Commands

        public DelegateCommand OpenProfileEditorCommand { get; }
        public DelegateCommand ManualSplitCommand { get; }
        
        #endregion

        #region Properties

        private string _appVer;

        public string AppVer
        {
            get => _appVer;
            set => SetProperty(ref _appVer, value);
        }
        
        private string _attachedText;

        public string AttachedText
        {
            get => _attachedText;
            set => SetProperty(ref _attachedText, value);
        }

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
        
        private SplitViewModel _currentSplit;

        public SplitViewModel CurrentSplit
        {
            get => _currentSplit;
            set => SetProperty(ref _currentSplit, value);
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
                    CurrentSplit = _allSplits.FirstOrDefault(s => s.Type == SplitType.Child);
                    CurrentSplit = Splits.FirstOrDefault();
                }
            }
        }

        public ObservableCollection<Profile> Profiles { get; } = new();
        
        private TimeSpan _inGameTime;
        public TimeSpan InGameTime
        {
            get => _inGameTime;
            set => SetProperty(ref _inGameTime, value);
        }

        #endregion

        #region Private Methods

        private void OnAttached()
        {
            AttachedText = $"Attached to {SelectedGame.ProcessName}.exe";
        }
        
        private void OnNotAttached()
        {
            AttachedText = "Not attached";
        }
        
        private void SwapModule()
        {

            (_currentModule as IDisposable)?.Dispose();

            if (_selectedGame == null) return;

            var events = GetAllEventsForGame(_selectedGame.GameName);
            
            _currentModule = _gameModuleFactory.CreateModule(_selectedGame, GetActiveEvents());
            _memoryService.StartAutoAttach(_selectedGame.ProcessName);
            _currentModule.OnHit += count => CurrentSplit.NumOfHits += count;
            _currentModule.OnEventSet += AutoAdvanceSplit;
            _currentModule.OnIgtChanged += igt => InGameTime = TimeSpan.FromMilliseconds(igt);
            
            SettingsManager.Default.LastSelectedGame = _selectedGame.GameName;
            SettingsManager.Default.Save();
        }
        
        private void AutoAdvanceSplit()
        {
            if (CurrentSplit == null || !CurrentSplit.IsAuto) return;
            AdvanceSplit();
        }
        
        private void ManualAdvanceSplit()
        {
            if (CurrentSplit == null || CurrentSplit.IsAuto) return;
            AdvanceSplit();
        }
        
        private void AdvanceSplit()
        {
            var currentIndex = Splits.IndexOf(CurrentSplit);
            if (currentIndex < 0 || currentIndex >= Splits.Count - 1) return;
            
            Splits[currentIndex].IsCurrent = false;
            Splits[currentIndex + 1].IsCurrent = true;
            CurrentSplit = Splits[currentIndex + 1];
        }

        private void OpenProfileEditor()
        {
            if (_selectedGame == null) return;

            var vm = new ProfileEditorViewModel(
                GetAllEventsForGame(_selectedGame.GameName),
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
            if (ActiveProfile.Splits.Count == 0) return;

            foreach (var split in ActiveProfile.Splits)
            {
                Splits.Add(new SplitViewModel
                {
                    Name = split.Label,
                    IsAuto = split.IsAuto,
                    NumOfHits = 0,
                    PersonalBest = split.PersonalBest
                });
            }

            Splits[0].IsCurrent = true;
        }
        
        private Dictionary<uint, string> GetActiveEvents()
        {
            if (ActiveProfile == null) return new();

            return ActiveProfile.Splits
                .Where(s => s.EventId.HasValue)
                .ToDictionary(s => s.EventId.Value, s => s.Label);
        }

        private Dictionary<uint, string> GetAllEventsForGame(string gameName)
        {
            return gameName switch
            {
                "Dark Souls 2 Scholar" => EventLoader.GetEvents("DS2ScholarEvents"),
                "Elden Ring" => EventLoader.GetEvents("EldenRingEvents"),
                _ => new()
            };
        }
        
        #endregion

        
    }
}