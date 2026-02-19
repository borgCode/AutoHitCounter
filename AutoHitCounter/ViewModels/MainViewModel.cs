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
        private readonly HotkeyManager _hotkeyManager;
        private readonly GameModuleFactory _gameModuleFactory;
        private readonly IProfileService _profileService;
        private IGameModule _currentModule;

        public SettingsViewModel Settings { get; }
        public HotkeyTabViewModel Hotkeys { get; }

        public MainViewModel(IMemoryService memoryService, HotkeyManager hotkeyManager,
            GameModuleFactory gameModuleFactory,
            IProfileService profileService, IStateService stateService, SettingsViewModel settings,
            HotkeyTabViewModel hotkeyTabViewModel)
        {
            Settings = settings;
            Hotkeys = hotkeyTabViewModel;
            _memoryService = memoryService;
            _hotkeyManager = hotkeyManager;
            _gameModuleFactory = gameModuleFactory;
            _profileService = profileService;

            stateService.Subscribe(State.Attached, OnAttached);
            stateService.Subscribe(State.NotAttached, OnNotAttached);

            RegisterHotkeys();

            OpenProfileEditorCommand = new DelegateCommand(OpenProfileEditor);
            SaveNotesCommand = new DelegateCommand(SaveNotes);

            ManualSplitCommand = new DelegateCommand(ManualAdvanceSplit);
            PrevSplitCommand = new DelegateCommand(PreviousSplit);

            IncrementHitCommand = new DelegateCommand(IncrementHit);
            DecrementHitCommand = new DelegateCommand(DecrementHit);

            ResetCommand = new DelegateCommand(ResetSplits);
            SetPbCommand = new DelegateCommand(SetPb);


            Games.Add(new Game { GameName = "Dark Souls Remastered", ProcessName = "darksoulsremastered" });
            Games.Add(new Game { GameName = "Dark Souls 2 Vanilla", ProcessName = "darksoulsii" });
            Games.Add(new Game { GameName = "Dark Souls 2 Scholar", ProcessName = "darksoulsii" });
            Games.Add(new Game { GameName = "Dark Souls 3", ProcessName = "darksoulsiii" });
            Games.Add(new Game { GameName = "Sekiro", ProcessName = "sekiro" });
            Games.Add(new Game { GameName = "Elden Ring", ProcessName = "eldenring" });

            SelectedGame = Games.FirstOrDefault(game => game.GameName == SettingsManager.Default.LastSelectedGame);
        }

        #region Commands

        public DelegateCommand OpenProfileEditorCommand { get; }

        public DelegateCommand ManualSplitCommand { get; }
        public DelegateCommand PrevSplitCommand { get; }

        public DelegateCommand IncrementHitCommand { get; }
        public DelegateCommand DecrementHitCommand { get; }

        public DelegateCommand ResetCommand { get; }

        public DelegateCommand SetPbCommand { get; }

        public DelegateCommand SaveNotesCommand { get; }

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
                    Profiles.Clear();
                    foreach (var p in _profileService.GetProfiles(_selectedGame?.GameName))
                        Profiles.Add(p);

                    ActiveProfile = Profiles.FirstOrDefault();
                    SwapModule();
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
                    ResetSplits();
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

        private bool _showNotes;

        public bool ShowNotes
        {
            get => _showNotes;
            set => SetProperty(ref _showNotes, value);
        }

        private bool _isRunComplete;

        public bool IsRunComplete
        {
            get => _isRunComplete;
            set => SetProperty(ref _isRunComplete, value);
        }

        public int TotalHits => Splits.Where(s => s.Type == SplitType.Child).Sum(s => s.NumOfHits);
        public int TotalPb => Splits.Where(s => s.Type == SplitType.Child).Sum(s => s.PersonalBest);

        #endregion

        #region Private Methods

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.NextSplit, ManualAdvanceSplit);
            _hotkeyManager.RegisterAction(HotkeyActions.PreviousSplit, PreviousSplit);
            _hotkeyManager.RegisterAction(HotkeyActions.Reset, ResetSplits);
            _hotkeyManager.RegisterAction(HotkeyActions.IncrementHit, IncrementHit);
            _hotkeyManager.RegisterAction(HotkeyActions.DecrementHit, DecrementHit);
        }

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

            _currentModule = _gameModuleFactory.CreateModule(_selectedGame, GetActiveEvents());
            _memoryService.StartAutoAttach(_selectedGame.ProcessName);
            _currentModule.OnHit += count =>
            {
                if (IsRunComplete) return;
                CurrentSplit.NumOfHits += count;
            };
            _currentModule.OnEventSet += AutoAdvanceSplit;
            _currentModule.OnIgtChanged += igt => InGameTime = TimeSpan.FromMilliseconds(igt);

            SettingsManager.Default.LastSelectedGame = _selectedGame.GameName;
            SettingsManager.Default.Save();
        }

        private void AutoAdvanceSplit()
        {
            if (CurrentSplit == null) return;
            AdvanceSplit();
        }

        private void ManualAdvanceSplit()
        {
            if (CurrentSplit == null || !Settings.AllowManualSplitOnAutoSplits) return;
            AdvanceSplit();
        }

        private void AdvanceSplit()
        {
            if (IsRunComplete) return;
            var currentIndex = Splits.IndexOf(CurrentSplit);
            if (currentIndex < 0) return;

            var next = Splits.Skip(currentIndex + 1).FirstOrDefault(s => s.Type == SplitType.Child);
            if (next == null)
            {
                CurrentSplit.IsCurrent = false;
                IsRunComplete = true;
                OnPropertyChanged(nameof(TotalHits));
                OnPropertyChanged(nameof(TotalPb));
                return;
            }

            CurrentSplit.IsCurrent = false;
            next.IsCurrent = true;
            CurrentSplit = next;
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
                    Type = split.Type,
                    NumOfHits = 0,
                    PersonalBest = split.PersonalBest,
                    Notes = split.Notes
                });
            }
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

        private void SaveNotes()
        {
            if (ActiveProfile == null) return;

            for (int i = 0; i < Splits.Count && i < ActiveProfile.Splits.Count; i++)
            {
                ActiveProfile.Splits[i].Notes = Splits[i].Notes;
            }

            _profileService.SaveProfile(ActiveProfile);
        }

        private void ResetSplits()
        {
            IsRunComplete = false;
            UpdateSplits();
            CurrentSplit = Splits.FirstOrDefault(s => s.Type == SplitType.Child);
            if (CurrentSplit != null) CurrentSplit.IsCurrent = true;
        }

        private void SetPb()
        {
            for (int i = 0; i < Splits.Count && i < ActiveProfile.Splits.Count; i++)
            {
                if (Splits[i].IsParent) continue;
                Splits[i].PersonalBest = Splits[i].NumOfHits;
                ActiveProfile.Splits[i].PersonalBest = Splits[i].NumOfHits;
            }

            _profileService.SaveProfile(ActiveProfile);
        }

        private void PreviousSplit()
        {
            var currentIndex = Splits.IndexOf(CurrentSplit);
            if (currentIndex < 0) return;
            var prev = Splits.Take(currentIndex).LastOrDefault(s => s.Type == SplitType.Child);
            if (prev == null) return;
            CurrentSplit.IsCurrent = false;
            prev.IsCurrent = true;
            CurrentSplit = prev;
        }

        private void IncrementHit()
        {
            if (IsRunComplete || CurrentSplit == null) return;
            CurrentSplit.NumOfHits++;
        }

        private void DecrementHit()
        {
            if (IsRunComplete || CurrentSplit == null || CurrentSplit.NumOfHits <= 0) return;
            CurrentSplit.NumOfHits--;
        }

        #endregion
    }
}