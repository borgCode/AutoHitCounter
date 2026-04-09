//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AutoHitCounter.Core;
using AutoHitCounter.Enums;
using AutoHitCounter.Games.Manual;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Mappers;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using AutoHitCounter.Views.Windows;

namespace AutoHitCounter.ViewModels
{
    public class MainViewModel : BaseViewModel, IReorderHandler, IHitRulesProvider
    {
        private readonly IMemoryService _memoryService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly GameModuleFactory _gameModuleFactory;
        private readonly IProfileService _profileService;
        private readonly SplitNavigationService _splitNav;
        private readonly OverlayServerService _overlayServerService;
        private readonly ExternalIntegrationService _externalIntegrationService;
        private IGameModule _currentModule;
        private string _lastIgt;
        private readonly DispatcherTimer _saveDebounce;

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
            HotkeyTabViewModel hotkeyTabViewModel, OverlayServerService overlayServerService,
            SplitNavigationService splitNavigationService, ExternalIntegrationService externalIntegrationService)
        {
            Settings = settings;
            Hotkeys = hotkeyTabViewModel;
            _memoryService = memoryService;
            _hotkeyManager = hotkeyManager;
            _gameModuleFactory = gameModuleFactory;
            _profileService = profileService;
            _overlayServerService = overlayServerService;
            _externalIntegrationService = externalIntegrationService;
            _overlayServerService.Start();


            stateService.Subscribe(State.AppStart, OnAppStart);
            stateService.Subscribe(State.Attached, OnAttached);
            stateService.Subscribe(State.NotAttached, OnNotAttached);

            Settings.OnGameSettingChanged += () => _currentModule?.ApplySettings();

            _splitNav = splitNavigationService;
            _splitNav.Load(Splits);
            _splitNav.StateChanged += OnSplitStateChanged;

            RegisterHotkeys();

            _saveDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _saveDebounce.Tick += (_, _) =>
            {
                _saveDebounce.Stop();
                if (_activeProfile?.SavedRun != null)
                    Task.Run(() => _profileService.SaveProfile(_activeProfile));
            };


            _isUnlocked = SettingsManager.Default.IsUnlocked;

            ThemeService.ThemeChanged += OnThemeChanged;
            InitialiseCommands();


            foreach (var game in _gameModuleFactory.GetRegisteredGames())
                Games.Add(game);

            LoadCustomGames();

            SelectedGame = Games.FirstOrDefault(game => game.GameName == SettingsManager.Default.LastSelectedGame);
            if (_selectedGame != null)
                StartTrackingGame();
        }

        #region Commands

        public DelegateCommand CheckUpdateCommand { get; set; }
        public DelegateCommand OpenProfileEditorCommand { get; set; }
        public DelegateCommand OpenEventLogCommand { get; set; }

        public DelegateCommand TrackGameCommand { get; set; }
        public DelegateCommand CreateCustomGameCommand { get; set; }
        public DelegateCommand DeleteCustomGameCommand { get; set; }
        public DelegateCommand RenameCustomGameCommand { get; set; }

        public DelegateCommand ManualSplitCommand { get; set; }
        public DelegateCommand AdvanceSplitCommand { get; set; }
        public DelegateCommand PrevSplitCommand { get; set; }

        public DelegateCommand IncrementHitCommand { get; set; }
        public DelegateCommand DecrementHitCommand { get; set; }

        public DelegateCommand ResetCommand { get; set; }

        public DelegateCommand SetPbCommand { get; set; }

        public DelegateCommand SaveNotesCommand { get; set; }

        public DelegateCommand ClearAllNotesCommand { get; set; }

        public DelegateCommand ToggleLockCommand { get; set; }

        public DelegateCommand ResetSelectedSplitHitsCommand { get; set; }

        public DelegateCommand RenameSelectedSplitCommand { get; set; }

        public DelegateCommand EditAttemptsCommand { get; set; }

        public DelegateCommand ClearTotalPbCommand { get; set; }

        public DelegateCommand EditSplitPbCommand { get; set; }

        public DelegateCommand MoveSplitUpCommand { get; set; }
        public DelegateCommand MoveSplitDownCommand { get; set; }

        public DelegateCommand SetDistancePbCommand { get; set; }

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

                if (_selectedGame?.IsManual == true)
                {
                    _isPracticeMode = false;
                    OnPropertyChanged(nameof(IsPracticeMode));
                    StartTrackingGame();
                }
                else
                {
                    _isPracticeMode = SettingsManager.Default.PracticeMode;
                    OnPropertyChanged(nameof(IsPracticeMode));
                }


                if (!Profiles.Any())
                {
                    _activeProfile = null;
                    Splits.Clear();
                    _splitNav.SetPosition(null, false);
                    OnPropertyChanged(nameof(CurrentSplit));
                    OnPropertyChanged(nameof(IsRunComplete));
                    OnPropertyChanged(nameof(ActiveProfile));
                    OnPropertyChanged(nameof(TotalHits));
                    OnPropertyChanged(nameof(TotalDiff));
                    OnPropertyChanged(nameof(TotalHitsBrush));
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
                    OnPropertyChanged(nameof(TimerLabel));
                }
            }
        }

        public string TrackingText => _activeGame != null
            ? $"Track hits for the currently selected game.\nCurrently Tracking: {_activeGame.GameName}"
            : "Not tracking";

        public string TimerLabel => _activeGame?.IsManual == true ? "RTA" : "IGT";

        public ObservableCollection<SplitViewModel> Splits { get; } = new();

        private SplitViewModel _selectedSplit;

        public SplitViewModel SelectedSplit
        {
            get => _selectedSplit;
            set
            {
                if (_selectedSplit != null && _selectedSplit.IsEditing)
                    CommitRename(_selectedSplit);

                if (SetProperty(ref _selectedSplit, value))
                {
                    MoveSplitUpCommand?.RaiseCanExecuteChanged();
                    MoveSplitDownCommand?.RaiseCanExecuteChanged();
                    SetDistancePbCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public SplitViewModel CurrentSplit
        {
            get => _splitNav.CurrentSplit;
            set
            {
                _splitNav.SetPosition(value, _splitNav.IsRunComplete);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentSplitNumber));
            }
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

                if (_activeGame == _selectedGame && _currentModule != null)
                    _currentModule.UpdateEvents(GetActiveEvents());

                OnHitRulesChanged?.Invoke();
            }
        }

        public ObservableCollection<Profile> Profiles { get; } = new();

        private TimeSpan _inGameTime;

        public TimeSpan InGameTime
        {
            get => _inGameTime;
            set => SetProperty(ref _inGameTime, value);
        }

        private bool _isPracticeMode;

        public bool IsPracticeMode
        {
            get => _isPracticeMode;
            set
            {
                if (!SetProperty(ref _isPracticeMode, value)) return;
                SettingsManager.Default.PracticeMode = value;
                SettingsManager.Default.Save();
            }
        }

        private bool _showNotes;

        public bool ShowNotes
        {
            get => _showNotes;
            set => SetProperty(ref _showNotes, value);
        }

        public bool IsRunComplete
        {
            get => _splitNav.IsRunComplete;
            set
            {
                _splitNav.SetPosition(_splitNav.CurrentSplit, value);
                OnPropertyChanged();
            }
        }

        private string _inGameTimeFormatted;

        public string InGameTimeFormatted
        {
            get => _inGameTimeFormatted;
            set => SetProperty(ref _inGameTimeFormatted, value);
        }

        public int TotalHits => Splits.Where(s => s.Type == SplitType.Child).Sum(s => s.NumOfHits);

        public Brush TotalHitsBrush
        {
            get
            {
                if (TotalPb == 0) return GetBrush("DiffNeutralBrush");
                if (TotalHits < TotalPb) return GetBrush("DiffNegativeBrush");
                if (TotalHits > TotalPb) return GetBrush("DiffPositiveBrush");
                return GetBrush("DiffNeutralBrush");
            }
        }

        public int TotalDiff => Splits.Where(s => s.Type == SplitType.Child).Sum(s => s.Diff);

        public int TotalPb => Splits.Where(s => s.Type == SplitType.Child).Sum(s => s.PersonalBest);

        public Brush TotalPbBrush
        {
            get
            {
                if (TotalDiff > 0) return GetBrush("DiffPositiveBrush");
                if (TotalDiff < 0) return GetBrush("DiffNegativeBrush");
                return GetBrush("DiffNeutralBrush");
            }
        }

        public event Action OnHitRulesChanged;

        public bool GetRule(string key) => _activeProfile != null
                                           && _activeProfile.GameSettings.TryGetValue(key, out var val)
                                           && val;

        public void CommitRename(SplitViewModel split)
        {
            split.IsEditing = false;
            if (ActiveProfile == null) return;
            var index = Splits.IndexOf(split);
            if (index >= 0 && index < ActiveProfile.Splits.Count)
            {
                ActiveProfile.Splits[index].Name = split.Name;
                if (!split.IsParent)
                    ActiveProfile.Splits[index].DisplayName = split.Name;
                _profileService.SaveProfile(ActiveProfile);
            }

            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
            NotifyProfileSplitsChanged();
        }

        public bool HasSplits => TotalSplitCount > 0;

        private bool _isSplitListScrollbarVisible;

        public bool IsSplitListScrollbarVisible
        {
            get => _isSplitListScrollbarVisible;
            set
            {
                _isSplitListScrollbarVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isEditingAttempts;

        public bool IsEditingAttempts
        {
            get => _isEditingAttempts;
            set => SetProperty(ref _isEditingAttempts, value);
        }

        private bool _isUnlocked = true;

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set => SetProperty(ref _isUnlocked, value);
        }

        public int AttemptCount => _activeProfile?.AttemptCount ?? 0;

        public int CurrentSplitNumber
        {
            get
            {
                if (CurrentSplit == null) return 0;
                var children = Splits.Where(s => s.Type == SplitType.Child).ToList();
                return children.IndexOf(CurrentSplit) + 1;
            }
        }

        public int TotalSplitCount => Splits.Count(s => s.Type == SplitType.Child);

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
            NotifyProfileSplitsChanged();
        }

        private void MoveSplitUp()
        {
            if (!CanMoveSplitUp()) return;
            var split = SelectedSplit;
            var index = Splits.IndexOf(split);
            MoveItem(split, index - 1);
            SelectedSplit = split;
            MoveSplitUpCommand.RaiseCanExecuteChanged();
            MoveSplitDownCommand.RaiseCanExecuteChanged();
            NotifyProfileSplitsChanged();
        }

        private void MoveSplitDown()
        {
            if (!CanMoveSplitDown()) return;
            var split = SelectedSplit;
            var index = Splits.IndexOf(split);
            MoveItem(split, index + 2);
            SelectedSplit = split;
            MoveSplitUpCommand.RaiseCanExecuteChanged();
            MoveSplitDownCommand.RaiseCanExecuteChanged();
            NotifyProfileSplitsChanged();
        }

        private bool CanMoveSplitUp()
        {
            if (!IsUnlocked || SelectedSplit == null || SelectedSplit.IsParent) return false;
            var index = Splits.IndexOf(SelectedSplit);
            return index > 0 && !Splits[index - 1].IsParent;
        }

        private bool CanMoveSplitDown()
        {
            if (!IsUnlocked || SelectedSplit == null || SelectedSplit.IsParent) return false;
            var index = Splits.IndexOf(SelectedSplit);
            return index < Splits.Count - 1 && !Splits[index + 1].IsParent;
        }

        public void CommitAttemptsEdit(string value)
        {
            if (int.TryParse(value, out var count) && count >= 0)
            {
                _activeProfile.AttemptCount = count;
                _profileService.SaveProfile(_activeProfile);
                OnPropertyChanged(nameof(AttemptCount));
                _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
            }

            IsEditingAttempts = false;
        }

        public void JumpToSplit(SplitViewModel target) => _splitNav.JumpTo(target);

        private void OnThemeChanged()
        {
            OnPropertyChanged(nameof(TotalHitsBrush));
        }

        public override void Dispose()
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
        }

        #endregion

        #region Private Methods

        private void OnAppStart()
        {
            AppVer = VersionChecker.GetVersionText();
            if (SettingsManager.Default.EnableUpdateChecks)
                VersionChecker.CheckForUpdates(Application.Current.MainWindow);
            _isPracticeMode = _selectedGame?.IsManual != true && SettingsManager.Default.PracticeMode;
            OnPropertyChanged(nameof(IsPracticeMode));
        }

        private void CheckUpdate() =>
            VersionChecker.CheckForUpdates(Application.Current.MainWindow, true);

        private void InitialiseCommands()
        {
            CheckUpdateCommand = new DelegateCommand(CheckUpdate);
            TrackGameCommand = new DelegateCommand(StartTrackingGame);
            CreateCustomGameCommand = new DelegateCommand(CreateCustomGame);
            DeleteCustomGameCommand = new DelegateCommand(DeleteCustomGame);
            RenameCustomGameCommand = new DelegateCommand(RenameCustomGame);
            OpenProfileEditorCommand = new DelegateCommand(OpenProfileEditor);
            OpenEventLogCommand = new DelegateCommand(OpenEventLog);
            ManualSplitCommand = new DelegateCommand(ManualAdvanceSplit);
            AdvanceSplitCommand = new DelegateCommand(() => _splitNav.Advance());
            PrevSplitCommand = new DelegateCommand(() => _splitNav.Previous());
            IncrementHitCommand = new DelegateCommand(IncrementHit);
            DecrementHitCommand = new DelegateCommand(DecrementHit);
            ResetCommand = new DelegateCommand(ResetSplits);
            SetPbCommand = new DelegateCommand(SetPb);
            SetDistancePbCommand = new DelegateCommand(SetDistancePb, CanSetDistancePb);

            ClearAllNotesCommand = new DelegateCommand(() =>
            {
                var confirmed = MsgBox.ShowOkCancel("This will clear all notes. Are you sure?", "Clear Notes");
                if (!confirmed) return;

                foreach (var split in Splits)
                    split.Notes = string.Empty;

                SaveNotes();
            });

            EditAttemptsCommand = new DelegateCommand(() => IsEditingAttempts = true);
            SaveNotesCommand = new DelegateCommand(SaveNotes);

            RenameSelectedSplitCommand = new DelegateCommand(() =>
            {
                if (SelectedSplit == null) return;
                SelectedSplit.IsEditing = true;
            });

            ResetSelectedSplitHitsCommand = new DelegateCommand(() =>
            {
                if (SelectedSplit == null || SelectedSplit.IsParent) return;
                SelectedSplit.NumOfHits = 0;
                _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
            });

            ClearTotalPbCommand = new DelegateCommand(() =>
            {
                var confirmed = MsgBox.ShowOkCancel("This will clear all personal bests. Are you sure?", "Clear PBs");
                if (!confirmed) return;

                foreach (var split in Splits.Where(s => s.Type == SplitType.Child))
                {
                    split.PersonalBest = 0;
                    var index = Splits.IndexOf(split);
                    if (index >= 0 && index < _activeProfile.Splits.Count)
                        _activeProfile.Splits[index].PersonalBest = 0;
                }

                if (_activeProfile != null)
                    _activeProfile.DistancePb = -1;

                _profileService.SaveProfile(_activeProfile);
                RefreshSplitValues();
                _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
            });

            EditSplitPbCommand = new DelegateCommand(() =>
            {
                if (SelectedSplit != null)
                    SelectedSplit.IsEditingPb = true;
            });

            ToggleLockCommand = new DelegateCommand(() =>
            {
                IsUnlocked = !IsUnlocked;
                SettingsManager.Default.IsUnlocked = IsUnlocked;
                SettingsManager.Default.Save();
                if (!IsUnlocked) SelectedSplit = null;
                MoveSplitUpCommand.RaiseCanExecuteChanged();
                MoveSplitDownCommand.RaiseCanExecuteChanged();
            });

            MoveSplitUpCommand = new DelegateCommand(MoveSplitUp, () => CanMoveSplitUp());
            MoveSplitDownCommand = new DelegateCommand(MoveSplitDown, () => CanMoveSplitDown());
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.NextSplit, ManualAdvanceSplit);
            _hotkeyManager.RegisterAction(HotkeyActions.PreviousSplit, () => _splitNav.Previous());

            _hotkeyManager.RegisterAction(HotkeyActions.Reset, ResetSplits);
            _hotkeyManager.RegisterAction(HotkeyActions.IncrementHit, IncrementHit);
            _hotkeyManager.RegisterAction(HotkeyActions.DecrementHit, DecrementHit);
            _hotkeyManager.RegisterAction(HotkeyActions.StartTimer, () =>
            {
                if (_currentModule is ManualGameModule manual) manual.StartTimer();
            });
            _hotkeyManager.RegisterAction(HotkeyActions.PauseTimer, () =>
            {
                if (_currentModule is ManualGameModule manual) manual.StopTimer();
            });
            _hotkeyManager.RegisterAction(HotkeyActions.TogglePracticeMode,
                () =>
                {
                    if (_activeGame?.IsManual != true) IsPracticeMode = !IsPracticeMode;
                });
        }

        private void OnSplitStateChanged()
        {
            UpdateDistancePb();
            OnPropertyChanged(nameof(CurrentSplit));
            OnPropertyChanged(nameof(CurrentSplitNumber));
            OnPropertyChanged(nameof(IsRunComplete));
            OnPropertyChanged(nameof(TotalHits));
            OnPropertyChanged(nameof(TotalDiff));
            OnPropertyChanged(nameof(TotalPb));
            SaveRunState();
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private void UpdateAttachedText()
        {
            var moduleGame = _activeGame;
            if (_currentModule is IVersionedGameModule versioned)
                versioned.OnVersionDetected += () =>
                {
                    var version = versioned.GameVersion;
                    AttachedText = string.IsNullOrEmpty(version)
                        ? $"Attached to {moduleGame.GameName}"
                        : $"Attached to {moduleGame.GameName} ({version})";
                };
        }

        private void OnAttached()
        {
            IsAttached = true;
            UpdateAttachedText();
        }

        private void OnNotAttached()
        {
            if (_activeGame?.IsManual == true) return;
            IsAttached = false;
            AttachedText = _activeGame != null ? $"Waiting for {_activeGame.GameName}..." : "Not attached";
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

            _currentModule = _gameModuleFactory.CreateModule(_activeGame, GetActiveEvents(), this);
            _hotkeyManager.SetManualGameActive(_activeGame.IsManual);

            if (_activeGame.IsManual)
            {
                IsAttached = true;
                AttachedText = $"Custom Game: {_activeGame.GameName}";

                if (_currentModule is ManualGameModule m && InGameTime.TotalMilliseconds > 0)
                    m.SetElapsed((long)InGameTime.TotalMilliseconds);
            }
            else
            {
                if (_currentModule is IVersionedGameModule versioned)
                    versioned.OnVersionDetected += UpdateAttachedText;

                _memoryService.StartAutoAttach(_activeGame.ProcessName);
            }

            _currentModule.OnHit += async count =>
            {
                if (IsRunComplete || CurrentSplit == null || IsPracticeMode) return;
                if (_selectedGame != _activeGame) return;
                CurrentSplit.NumOfHits += count;
                SaveRunState();
                _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));

                var payload = new HitPayload(_activeGame, ActiveProfile, CurrentSplit, TotalHits, TotalPb, InGameTime);
                await _externalIntegrationService.SendHitAsync(payload);

            };
            _currentModule.OnRunStart += HandleRunStart;
            _currentModule.OnEventSet += AutoAdvanceSplit;
            _currentModule.OnEventLogEntriesReceived += entries => _eventLogViewModel?.RefreshEventLogs(entries);
            _currentModule.OnTimeChanged += UpdateInGameTime;

            if (_eventLogWindow != null)
                _currentModule.SetEventLogEnabled(true);

            SettingsManager.Default.LastSelectedGame = _activeGame.GameName;
            SettingsManager.Default.Save();
        }

        private void AutoAdvanceSplit()
        {
            if (_selectedGame != _activeGame) return;
            if (IsPracticeMode) return;
            if (CurrentSplit == null) return;
            _splitNav.Advance();
        }

        private void HandleRunStart()
        {
            if (_activeGame?.IsManual == true) return;
            if (_selectedGame != _activeGame) return;
            if (IsPracticeMode) return;
            if (!SettingsManager.Default.AutoResetOnNewGameStart) return;
            if (!HasRunProgress()) return;

            ResetRun();
        }

        private bool HasRunProgress() => CurrentSplitNumber > 1 || TotalHits > 0 || InGameTime > TimeSpan.Zero;

        private void ManualAdvanceSplit()
        {
            if (CurrentSplit == null) return;
            _splitNav.Advance();
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

        private ProfileEditorWindow _profileEditorWindow;
        private EventLogWindow _eventLogWindow;
        private EventLogViewModel _eventLogViewModel;

        private void OpenEventLog()
        {
            if (_eventLogWindow != null)
            {
                _eventLogWindow.Activate();
                return;
            }

            _eventLogViewModel = new EventLogViewModel();
            _eventLogWindow = new EventLogWindow { DataContext = _eventLogViewModel };

            _currentModule?.SetEventLogEnabled(true);

            _eventLogWindow.Closed += (s, e) =>
            {
                _currentModule?.SetEventLogEnabled(false);
                _eventLogWindow = null;
                _eventLogViewModel = null;
            };

            _eventLogWindow.Show();
        }

        private event Action ActiveProfileSplitsChanged;

        private void NotifyProfileSplitsChanged()
            => ActiveProfileSplitsChanged?.Invoke();

        private void OpenProfileEditor()
        {
            if (_selectedGame == null) return;

            if (_profileEditorWindow != null)
            {
                _profileEditorWindow.Activate();
                return;
            }

            var events = _selectedGame.IsManual
                ? new Dictionary<uint, string>()
                : GetAllEventsForGame(_selectedGame.Title);
            var vm = new ProfileEditorViewModel(
                events,
                _profileService,
                _selectedGame.GameName,
                _selectedGame.Title,
                _activeProfile,
                _selectedGame.IsManual);

            _profileEditorWindow = new ProfileEditorWindow { DataContext = vm };

            ActiveProfileSplitsChanged += vm.RefreshSplits;

            Action onSaved = () =>
            {
                var updatedProfiles = _profileService.GetProfiles(_selectedGame.GameName);
                Profiles.Clear();
                foreach (var p in updatedProfiles)
                    Profiles.Add(p);

                ActiveProfile = Profiles.FirstOrDefault(p => p.Name == vm.SelectedProfile?.Name);
                _currentModule.UpdateEvents(GetActiveEvents());
            };
            vm.OnSaved += onSaved;

            _profileEditorWindow.Closed += (s, e) =>
            {
                _profileEditorWindow = null;

                if (_activeProfile != null)
                {
                    var key = $"{_selectedGame.GameName}|{_activeProfile.Name}";
                    _runSnapshots.Remove(key);
                }

                var validKeys = new HashSet<string>(
                    _profileService.GetProfiles(_selectedGame.GameName)
                        .Select(p => $"{_selectedGame.GameName}|{p.Name}"));
                var staleKeys = _runSnapshots.Keys
                    .Where(k => k.StartsWith($"{_selectedGame.GameName}|") && !validKeys.Contains(k))
                    .ToList();
                foreach (var stale in staleKeys)
                    _runSnapshots.Remove(stale);
            };

            _profileEditorWindow.Show();
        }

        private void UpdateSplits()
        {
            foreach (var split in Splits)
                ((IDisposable)split).Dispose();

            Splits.Clear();
            if (ActiveProfile == null) return;
            if (ActiveProfile.Splits.Count == 0) return;

            foreach (var split in ActiveProfile.Splits)
            {
                var vm = new SplitViewModel
                {
                    Name = split.Label,
                    IsAuto = split.IsAuto,
                    Type = split.Type,
                    NumOfHits = 0,
                    PersonalBest = split.PersonalBest,
                    Notes = split.Notes
                };
                vm.PropertyChanged += (_, _) =>
                {
                    OnPropertyChanged(nameof(TotalSplitCount));
                    OnPropertyChanged(nameof(AttemptCount));
                    OnPropertyChanged(nameof(TotalHits));
                    OnPropertyChanged(nameof(TotalDiff));
                    OnPropertyChanged(nameof(TotalHitsBrush));
                    OnPropertyChanged(nameof(TotalPb));
                };
                Splits.Add(vm);
            }
        }

        private static Brush GetBrush(string key)
        {
            if (Application.Current.Resources[key] is SolidColorBrush brush)
                return brush;
            return new SolidColorBrush(Colors.White);
        }

        private void UpdateDistancePb()
        {
            if (_activeProfile == null || CurrentSplit == null) return;
            if (TotalHits == 0)
                TryAdvanceDistancePb();
        }

        private void SetDistancePb()
        {
            if (_activeProfile == null || SelectedSplit == null || SelectedSplit.IsParent) return;

            var index = Splits.IndexOf(SelectedSplit);
            if (index < 0) return;

            _activeProfile.DistancePb = index;
            _profileService.SaveProfile(_activeProfile);
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private bool CanSetDistancePb() =>
            _activeProfile != null && SelectedSplit != null && !SelectedSplit.IsParent;

        private void TryAdvanceDistancePb()
        {
            var currentIdx = Splits.IndexOf(CurrentSplit);
            if (currentIdx > _activeProfile.DistancePb)
            {
                _activeProfile.DistancePb = currentIdx;
                _profileService.SaveProfile(_activeProfile);
            }
        }

        private void RefreshSplitValues()
        {
            var hits = Splits.Select(s => s.NumOfHits).ToArray();
            var currentIndex = CurrentSplit != null ? Splits.IndexOf(CurrentSplit) : -1;

            UpdateSplits();

            for (int i = 0; i < Splits.Count && i < hits.Length; i++)
                Splits[i].NumOfHits = hits[i];

            if (currentIndex >= 0 && currentIndex < Splits.Count)
            {
                CurrentSplit = Splits[currentIndex];
                CurrentSplit.IsCurrent = true;
            }
        }

        private Dictionary<uint, (string Name, int Required, int Hit)> GetActiveEvents()
        {
            if (ActiveProfile == null) return new();

            return ActiveProfile.Splits
                .Where(s => s.EventId.HasValue)
                .GroupBy(s => s.EventId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => (Name: g.First().Label, Required: g.Count(), Hit: 0));
        }

        private Dictionary<uint, string> GetAllEventsForGame(GameTitle title) =>
            _gameModuleFactory.GetEventsForGame(title);

        private void LoadCustomGames()
        {
            var raw = SettingsManager.Default.CustomGames;
            if (string.IsNullOrWhiteSpace(raw)) return;

            foreach (var name in raw.Split(','))
            {
                var trimmed = name.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                Games.Add(new Game
                {
                    Title = GameTitle.Manual,
                    GameName = trimmed,
                    ProcessName = null,
                    IsManual = true
                });
            }
        }

        private void CreateCustomGame()
        {
            var name = MsgBox.ShowInput(
                "Create a game to add profiles and splits to.\nAuto hit counting and auto splitting are not supported,\nbut you can use a timer and track hits manually.",
                "", "New Custom Game");
            if (string.IsNullOrWhiteSpace(name)) return;

            if (Games.Any(g => g.GameName == name))
            {
                MsgBox.Show("A game with that name already exists.", "New Custom Game");
                return;
            }

            var game = new Game
            {
                Title = GameTitle.Manual,
                GameName = name,
                ProcessName = null,
                IsManual = true
            };
            Games.Add(game);

            var existing = SettingsManager.Default.CustomGames;
            SettingsManager.Default.CustomGames = string.IsNullOrEmpty(existing) ? name : $"{existing},{name}";
            SettingsManager.Default.Save();

            SelectedGame = game;
            StartTrackingGame();
        }

        private void DeleteCustomGame()
        {
            if (_selectedGame == null || !_selectedGame.IsManual) return;

            var name = _selectedGame.GameName;
            var profiles = _profileService.GetProfiles(name);
            var count = profiles.Count;
            var profileMsg = count > 0
                ? $"\n\nThis will delete {count} profile{(count == 1 ? "" : "s")} and all splits associated with this game."
                : "";

            if (!MsgBox.ShowYesNo(
                    $"Are you sure you want to delete \"{name}\"?{profileMsg}",
                    "Delete Custom Game"))
                return;

            foreach (var profile in profiles.ToList())
                _profileService.DeleteProfile(name, profile.Name);

            (_currentModule as IDisposable)?.Dispose();
            _currentModule = null;
            ActiveGame = null;
            IsAttached = false;
            AttachedText = "Not attached";

            Games.Remove(_selectedGame);
            SelectedGame = Games.FirstOrDefault();

            var names = (SettingsManager.Default.CustomGames ?? "")
                .Split(',')
                .Select(n => n.Trim())
                .Where(n => n != name);
            SettingsManager.Default.CustomGames = string.Join(",", names);
            SettingsManager.Default.Save();
        }

        private void RenameCustomGame()
        {
            if (_selectedGame == null || !_selectedGame.IsManual) return;

            var oldName = _selectedGame.GameName;
            var newName = MsgBox.ShowInput("Rename Game", oldName, "Rename Custom Game");
            if (string.IsNullOrWhiteSpace(newName) || newName == oldName) return;

            if (Games.Any(g => g.GameName == newName))
            {
                MsgBox.Show("A game with that name already exists.", "Rename Custom Game");
                return;
            }

            _profileService.RenameGame(oldName, newName);

            var game = _selectedGame;
            game.GameName = newName;

            // Refresh the combo box display by re-inserting the item
            var index = Games.IndexOf(game);
            Games.RemoveAt(index);
            Games.Insert(index, game);
            _selectedGame = null;
            SelectedGame = game;

            var customNames = (SettingsManager.Default.CustomGames ?? "")
                .Split(',')
                .Select(n => n.Trim())
                .Select(n => n == oldName ? newName : n);
            SettingsManager.Default.CustomGames = string.Join(",", customNames);
            SettingsManager.Default.LastSelectedGame = newName;
            SettingsManager.Default.Save();

            if (_activeGame == game)
                AttachedText = $"Custom Game: {newName}";

            // Re-key any cached run snapshots
            var staleKeys = _runSnapshots.Keys.Where(k => k.StartsWith($"{oldName}|")).ToList();
            foreach (var key in staleKeys)
            {
                var snapshot = _runSnapshots[key];
                _runSnapshots.Remove(key);
                _runSnapshots[key.Replace($"{oldName}|", $"{newName}|")] = snapshot;
            }
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
            _saveDebounce.Stop();
            IsRunComplete = false;
            UpdateSplits();
            var key = $"{_selectedGame?.GameName}|{profile?.Name}";
            if (profile != null && _runSnapshots.TryGetValue(key, out var snapshot))
                RestoreSnapshot(snapshot);
            else if (profile?.SavedRun != null)
                RestoreFromSavedRun(profile.SavedRun);
            else
                _splitNav.InitFresh();

            OnPropertyChanged(nameof(CurrentSplit));
            OnPropertyChanged(nameof(CurrentSplitNumber));
            OnPropertyChanged(nameof(IsRunComplete));
            OnPropertyChanged(nameof(TotalHits));
            OnPropertyChanged(nameof(TotalPb));
            OnPropertyChanged(nameof(TotalDiff));
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private void RestoreSnapshot(RunSnapshot snapshot)
        {
            var children = Splits.Where(s => s.Type == SplitType.Child).ToList();
            for (int i = 0; i < children.Count && i < snapshot.HitCounts.Length; i++)
                children[i].NumOfHits = snapshot.HitCounts[i];

            var toRestore = snapshot.CurrentSplitIndex >= 0 && snapshot.CurrentSplitIndex < Splits.Count
                ? Splits[snapshot.CurrentSplitIndex]
                : Splits.FirstOrDefault(s => s.Type == SplitType.Child);

            _splitNav.SetPosition(toRestore, snapshot.IsRunComplete);
            InGameTime = snapshot.InGameTime;
        }

        private void RestoreFromSavedRun(RunState state)
        {
            var children = Splits.Where(s => s.Type == SplitType.Child).ToList();
            for (int i = 0; i < children.Count && i < state.HitCounts.Length; i++)
                children[i].NumOfHits = state.HitCounts[i];

            var toRestore = state.CurrentSplitIndex >= 0 && state.CurrentSplitIndex < Splits.Count
                ? Splits[state.CurrentSplitIndex]
                : Splits.FirstOrDefault(s => s.Type == SplitType.Child);

            _splitNav.SetPosition(toRestore, state.IsRunComplete);
            InGameTime = TimeSpan.FromMilliseconds(state.IgtMilliseconds);
        }

        private void ResetSplits()
        {
            ResetRun();
        }

        private void ResetRun()
        {
            _saveDebounce.Stop();
            UpdateDistancePb();

            if (_activeProfile != null)
            {
                _activeProfile.AttemptCount++;
                _activeProfile.SavedRun = null;
                _profileService.SaveProfile(_activeProfile);
                OnPropertyChanged(nameof(AttemptCount));
            }

            var key = $"{_selectedGame?.GameName}|{_activeProfile?.Name}";
            _runSnapshots.Remove(key);
            UpdateSplits();
            _splitNav.InitFresh();

            if (_currentModule is ManualGameModule manualModule)
                manualModule.ResetTimer();

            InGameTime = TimeSpan.Zero;
            InGameTimeFormatted = "0:00:00";

            if (_activeGame == _selectedGame && _currentModule != null)
                _currentModule.UpdateEvents(GetActiveEvents());

            OnPropertyChanged(nameof(IsRunComplete));
            OnPropertyChanged(nameof(CurrentSplit));
            OnPropertyChanged(nameof(CurrentSplitNumber));
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
            _overlayServerService.BroadcastIgt(InGameTimeFormatted);
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

        public void CommitPbEdit(SplitViewModel split, string value)
        {
            if (int.TryParse(value, out int val) && val >= 0)
            {
                split.PersonalBest = val;
                var index = Splits.IndexOf(split);
                if (index >= 0 && index < _activeProfile.Splits.Count)
                {
                    _activeProfile.Splits[index].PersonalBest = val;
                    _profileService.SaveProfile(_activeProfile);
                }
            }

            split.IsEditingPb = false;
            RefreshSplitValues();
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private void IncrementHit()
        {
            if (IsRunComplete || CurrentSplit == null || IsPracticeMode) return;
            CurrentSplit.NumOfHits++;
            SaveRunState();
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        private void DecrementHit()
        {
            if (IsRunComplete || CurrentSplit == null || CurrentSplit.NumOfHits <= 0) return;
            CurrentSplit.NumOfHits--;
            SaveRunState();
            _overlayServerService.BroadcastState(OverlayMapper.MapFrom(this));
        }

        public void SaveRunState()
        {
            if (_activeProfile == null) return;

            var children = Splits.Where(s => s.Type == SplitType.Child).ToList();
            _activeProfile.SavedRun = new RunState
            {
                CurrentSplitIndex = CurrentSplit != null ? Splits.IndexOf(CurrentSplit) : -1,
                HitCounts = children.Select(s => s.NumOfHits).ToArray(),
                IsRunComplete = IsRunComplete,
                IgtMilliseconds = (long)InGameTime.TotalMilliseconds
            };

            _saveDebounce.Stop();
            _saveDebounce.Start();
        }

        public void FlushRunState()
        {
            _saveDebounce.Stop();
            if (_activeProfile?.SavedRun != null)
                _profileService.SaveProfile(_activeProfile);
        }

        #endregion
    }
}
