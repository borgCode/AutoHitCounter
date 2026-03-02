//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoHitCounter.Core;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Mappers;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using AutoHitCounter.Views.Windows;

namespace AutoHitCounter.ViewModels
{
    public class MainViewModel : BaseViewModel, IReorderHandler
    {
        private readonly IMemoryService _memoryService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly GameModuleFactory _gameModuleFactory;
        private readonly IProfileService _profileService;
        private readonly OverlayServerService _overlayServerService;
        private IGameModule _currentModule;
        private string _lastIgt;

        private class RunSnapshot(int currentSplitIndex, int[] hitCounts, bool isRunComplete, TimeSpan inGameTime)
        {
            public int CurrentSplitIndex { get; } = currentSplitIndex;
            public int[] HitCounts { get; } = hitCounts;
            public bool IsRunComplete { get; } = isRunComplete;
            public TimeSpan InGameTime { get; } = inGameTime;
        }


        private readonly Dictionary<string, RunSnapshot> _runSnapshots = new();

        public SettingsViewModel Settings { get; }
        public HotkeyTabViewModel Hotkeys { get; }

        public MainViewModel(IMemoryService memoryService, HotkeyManager hotkeyManager,
            GameModuleFactory gameModuleFactory,
            IProfileService profileService, IStateService stateService, SettingsViewModel settings,
            HotkeyTabViewModel hotkeyTabViewModel, OverlayServerService overlayServerService)
        {
            Settings = settings;
            Hotkeys = hotkeyTabViewModel;
            _memoryService = memoryService;
            _hotkeyManager = hotkeyManager;
            _gameModuleFactory = gameModuleFactory;
            _profileService = profileService;
            _overlayServerService = overlayServerService;
            _overlayServerService.Start();

            stateService.Subscribe(State.Attached, OnAttached);
            stateService.Subscribe(State.NotAttached, OnNotAttached);

            RegisterHotkeys();

            OpenProfileEditorCommand = new DelegateCommand(OpenProfileEditor);
            SaveNotesCommand = new DelegateCommand(SaveNotes);
            TrackGameCommand = new DelegateCommand(StartTrackingGame);

            ManualSplitCommand = new DelegateCommand(ManualAdvanceSplit);
            AdvanceSplitCommand = new DelegateCommand(AdvanceSplit);
            PrevSplitCommand = new DelegateCommand(PreviousSplit);

            IncrementHitCommand = new DelegateCommand(IncrementHit);
            DecrementHitCommand = new DelegateCommand(DecrementHit);

            ResetCommand = new DelegateCommand(ResetSplits);
            SetPbCommand = new DelegateCommand(SetPb);


            _isUnlocked = SettingsManager.Default.IsUnlocked;
            ToggleLockCommand = new DelegateCommand(() =>
            {
                IsUnlocked = !IsUnlocked;
                SettingsManager.Default.IsUnlocked = IsUnlocked;
                SettingsManager.Default.Save();
            });

            InitialiseCommands();


            Games.Add(new Game { GameName = "Dark Souls Remastered", ProcessName = "darksoulsremastered" });
            Games.Add(new Game { GameName = "Dark Souls 2 Vanilla", ProcessName = "darksoulsii" });
            Games.Add(new Game { GameName = "Dark Souls 2 Scholar", ProcessName = "darksoulsii" });
            Games.Add(new Game { GameName = "Dark Souls 3", ProcessName = "darksoulsiii" });
            Games.Add(new Game { GameName = "Sekiro", ProcessName = "sekiro" });
            Games.Add(new Game { GameName = "Elden Ring", ProcessName = "eldenring" });

            SelectedGame = Games.FirstOrDefault(game => game.GameName == SettingsManager.Default.LastSelectedGame);
            if (_selectedGame != null)
                StartTrackingGame();
        }

        #region Commands

        public DelegateCommand OpenProfileEditorCommand { get; }

        public DelegateCommand TrackGameCommand { get; }

        public DelegateCommand ManualSplitCommand { get; }
        public DelegateCommand AdvanceSplitCommand { get; }
        public DelegateCommand PrevSplitCommand { get; }

        public DelegateCommand IncrementHitCommand { get; }
        public DelegateCommand DecrementHitCommand { get; }

        public DelegateCommand ResetCommand { get; }

        public DelegateCommand SetPbCommand { get; }

        public DelegateCommand SaveNotesCommand { get; }

        public DelegateCommand ToggleLockCommand { get; set; }

        public DelegateCommand ResetSelectedSplitHitsCommand { get; set; }

        public DelegateCommand RenameSelectedSplitCommand { get; set; }

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

        private bool _isAttached;

        public bool IsAttached
        {
            get => _isAttached;
            set => SetProperty(ref _isAttached, value);
        }

        public ObservableCollection<Game> Games { get; } = new();

        private Game _selectedGame;

        public Game SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (_selectedGame == value) return;

                if (_activeProfile != null)
                {
                    var outKey = $"{_selectedGame?.GameName}|{_activeProfile.Name}";
                    _runSnapshots[outKey] = CaptureSnapshot();
                }

                SetProperty(ref _selectedGame, value);
                _activeProfile = null;

                Profiles.Clear();
                foreach (var p in _profileService.GetProfiles(_selectedGame?.GameName))
                    Profiles.Add(p);

                ActiveProfile = Profiles.FirstOrDefault(p => p.Name == SettingsManager.Default.LastSelectedProfile)
                                ?? Profiles.FirstOrDefault();
                if (!Profiles.Any())
                {
                    _activeProfile = null;
                    Splits.Clear();
                    CurrentSplit = null;
                    IsRunComplete = false;
                    OnPropertyChanged(nameof(ActiveProfile));
                    OnPropertyChanged(nameof(TotalHits));
                    OnPropertyChanged(nameof(TotalPb));
                    _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
                }
            }
        }

        private Game _activeGame;

        public Game ActiveGame
        {
            get => _activeGame;
            private set
            {
                if (SetProperty(ref _activeGame, value))
                {
                    SwapModule();
                    OnPropertyChanged(nameof(TrackingText));
                }
            }
        }

        public string TrackingText => _activeGame != null ? $"Tracking: {_activeGame.GameName}" : "Not tracking";

        public ObservableCollection<SplitViewModel> Splits { get; } = new();

        private SplitViewModel _selectedSplit;

        public SplitViewModel SelectedSplit
        {
            get => _selectedSplit;
            set
            {
                if (_selectedSplit != null && _selectedSplit.IsEditing)
                    CommitRename(_selectedSplit);

                SetProperty(ref _selectedSplit, value);
            }
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
                if (_activeProfile == value) return;

                if (_activeProfile != null)
                {
                    var outKey = $"{_selectedGame?.GameName}|{_activeProfile.Name}";
                    _runSnapshots[outKey] = CaptureSnapshot();
                }

                SetProperty(ref _activeProfile, value);

                if (value != null)
                {
                    SettingsManager.Default.LastSelectedProfile = value.Name;
                    SettingsManager.Default.Save();
                }

                LoadProfile(value);
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

        private string _inGameTimeFormatted;

        public string InGameTimeFormatted
        {
            get => _inGameTimeFormatted;
            set => SetProperty(ref _inGameTimeFormatted, value);
        }

        public int TotalHits => Splits.Where(s => s.Type == SplitType.Child).Sum(s => s.NumOfHits);
        public int TotalPb => Splits.Where(s => s.Type == SplitType.Child).Sum(s => s.PersonalBest);

        public void CommitRename(SplitViewModel split)
        {
            split.IsEditing = false;
            if (ActiveProfile == null) return;
            var index = Splits.IndexOf(split);
            if (index >= 0 && index < ActiveProfile.Splits.Count)
            {
                ActiveProfile.Splits[index].DisplayName = split.Name;
                _profileService.SaveProfile(ActiveProfile);
            }

            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        #endregion

        #region Public Methods

        public void MoveItem(object draggedItem, int dropIndex)
        {
            if (draggedItem is not SplitViewModel entry) return;
            if (entry.IsParent) return;
            if (dropIndex < 0) return;

            var oldIndex = Splits.IndexOf(entry);
            if (oldIndex < 0 || oldIndex == dropIndex) return;

            var groupStart = oldIndex;
            for (int i = oldIndex - 1; i >= 0; i--)
            {
                if (Splits[i].IsParent)
                {
                    groupStart = i + 1;
                    break;
                }

                if (i == 0) groupStart = 0;
            }

            var groupEnd = Splits.Count - 1;
            for (int i = oldIndex + 1; i < Splits.Count; i++)
            {
                if (Splits[i].IsParent)
                {
                    groupEnd = i - 1;
                    break;
                }
            }

            if (dropIndex < groupStart) dropIndex = groupStart;
            if (dropIndex > groupEnd + 1) dropIndex = groupEnd + 1;

            if (oldIndex == dropIndex) return;

            Splits.RemoveAt(oldIndex);

            if (dropIndex > oldIndex)
                dropIndex--;

            Splits.Insert(dropIndex, entry);

            if (ActiveProfile?.Splits != null && oldIndex < ActiveProfile.Splits.Count)
            {
                var profileEntry = ActiveProfile.Splits[oldIndex];
                ActiveProfile.Splits.RemoveAt(oldIndex);
                ActiveProfile.Splits.Insert(dropIndex, profileEntry);
                _profileService.SaveProfile(ActiveProfile);
            }

            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private bool _isUnlocked = true;

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set => SetProperty(ref _isUnlocked, value);
        }

        #endregion

        #region Private Methods

        private void InitialiseCommands()
        {
            RenameSelectedSplitCommand = new DelegateCommand(() =>
            {
                if (SelectedSplit == null || SelectedSplit.IsParent) return;
                SelectedSplit.IsEditing = true;
            });

            ResetSelectedSplitHitsCommand = new DelegateCommand(() =>
            {
                if (SelectedSplit == null || SelectedSplit.IsParent) return;
                SelectedSplit.NumOfHits = 0;
                _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
            });
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.NextSplit, ManualAdvanceSplit);
            _hotkeyManager.RegisterAction(HotkeyActions.PreviousSplit, PreviousSplit);
            _hotkeyManager.RegisterAction(HotkeyActions.Reset, ResetSplits);
            _hotkeyManager.RegisterAction(HotkeyActions.IncrementHit, IncrementHit);
            _hotkeyManager.RegisterAction(HotkeyActions.DecrementHit, DecrementHit);
        }

        private void UpdateAttachedText()
        {
            var version = (_currentModule as IVersionedGameModule)?.GameVersion;
            AttachedText = string.IsNullOrEmpty(version)
                ? $"Attached to {SelectedGame.GameName}"
                : $"Attached to {SelectedGame.GameName} ({version})";
        }

        private void OnAttached()
        {
            IsAttached = true;
            UpdateAttachedText();
        }

        private void OnNotAttached()
        {
            IsAttached = false;
            AttachedText = "Not attached";
            OnPropertyChanged(nameof(TrackingText));
        }

        private void StartTrackingGame()
        {
            if (_selectedGame == null) return;
            ActiveGame = _selectedGame;
        }

        private void SwapModule()
        {
            (_currentModule as IDisposable)?.Dispose();

            if (_activeGame == null) return;

            _currentModule = _gameModuleFactory.CreateModule(_activeGame, GetActiveEvents());

            if (_currentModule is IVersionedGameModule versioned)
                versioned.OnVersionDetected += UpdateAttachedText;

            _memoryService.StartAutoAttach(_activeGame.ProcessName);
            _currentModule.OnHit += count =>
            {
                if (IsRunComplete || CurrentSplit == null) return;
                if (_selectedGame != _activeGame) return;
                CurrentSplit.NumOfHits += count;
                _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
            };
            _currentModule.OnEventSet += AutoAdvanceSplit;
            _currentModule.OnIgtChanged += UpdateInGameTime;

            SettingsManager.Default.LastSelectedGame = _activeGame.GameName;
            SettingsManager.Default.Save();
        }

        private void AutoAdvanceSplit()
        {
            if (_selectedGame != _activeGame) return;
            if (Settings.IsPracticeMode) return;
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
            }
            else
            {
                CurrentSplit.IsCurrent = false;
                next.IsCurrent = true;
                CurrentSplit = next;
            }

            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private void UpdateInGameTime(long igt)
        {
            InGameTime = TimeSpan.FromMilliseconds(igt);
            var formatted = $"{(int)InGameTime.TotalHours}:{InGameTime.Minutes:D2}:{InGameTime.Seconds:D2}";
            if (formatted != _lastIgt)
            {
                _lastIgt = formatted;
                InGameTimeFormatted = formatted;
                _overlayServerService.BroadcastIgt(formatted);
            }
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
                "Dark Souls 3" => EventLoader.GetEvents("DS3Events"),
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

        private RunSnapshot CaptureSnapshot()
        {
            var children = Splits.Where(s => s.Type == SplitType.Child).ToList();
            var hits = children.Select(s => s.NumOfHits).ToArray();
            var index = CurrentSplit != null ? Splits.IndexOf(CurrentSplit) : -1;
            return new RunSnapshot(index, hits, IsRunComplete, InGameTime);
        }

        private void LoadProfile(Profile profile)
        {
            IsRunComplete = false;
            UpdateSplits();
            var key = $"{_selectedGame?.GameName}|{profile?.Name}";
            if (profile != null && _runSnapshots.TryGetValue(key, out var snapshot))
                RestoreSnapshot(snapshot);
            else
                InitFreshRun();

            OnPropertyChanged(nameof(TotalHits));
            OnPropertyChanged(nameof(TotalPb));
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private void RestoreSnapshot(RunSnapshot snapshot)
        {
            IsRunComplete = snapshot.IsRunComplete;
            InGameTime = snapshot.InGameTime;
            var children = Splits.Where(s => s.Type == SplitType.Child).ToList();
            for (int i = 0; i < children.Count && i < snapshot.HitCounts.Length; i++)
                children[i].NumOfHits = snapshot.HitCounts[i];

            if (!IsRunComplete)
            {
                var toRestore = snapshot.CurrentSplitIndex >= 0 && snapshot.CurrentSplitIndex < Splits.Count
                    ? Splits[snapshot.CurrentSplitIndex]
                    : Splits.FirstOrDefault(s => s.Type == SplitType.Child);
                CurrentSplit = toRestore;
                if (CurrentSplit != null) CurrentSplit.IsCurrent = true;
            }
        }

        private void InitFreshRun()
        {
            CurrentSplit = Splits.FirstOrDefault(s => s.Type == SplitType.Child);
            if (CurrentSplit != null) CurrentSplit.IsCurrent = true;
        }

        private void ResetSplits()
        {
            var key = $"{_selectedGame?.GameName}|{_activeProfile?.Name}";
            _runSnapshots.Remove(key);
            IsRunComplete = false;
            UpdateSplits();
            InitFreshRun();
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
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
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
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
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private void IncrementHit()
        {
            if (IsRunComplete || CurrentSplit == null) return;
            CurrentSplit.NumOfHits++;
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private void DecrementHit()
        {
            if (IsRunComplete || CurrentSplit == null || CurrentSplit.NumOfHits <= 0) return;
            CurrentSplit.NumOfHits--;
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        #endregion
    }
}