// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoHitCounter.Core;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using AutoHitCounter.Views.Windows;

namespace AutoHitCounter.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly OverlaySettingsViewModel _overlaySettingsViewModel;
    private readonly HotkeyManager _hotkeyManager;

    public event Action OnGameSettingChanged;

    public IReadOnlyList<GameTitle> GameTitles { get; } = EnumExtensions.GetValues<GameTitle>()
        .Where(title => title != GameTitle.DarkSoulsRemastered && title != GameTitle.Manual).ToList();

    private GameTitle _selectedSettingsGame;
    private OverlaySettingsWindow _overlaySettingsWindow;

    public GameTitle SelectedSettingsGame
    {
        get => _selectedSettingsGame;
        set => SetProperty(ref _selectedSettingsGame, value);
    }

    public SettingsViewModel(IStateService stateService, OverlaySettingsViewModel overlaySettingsViewModel,
        HotkeyManager hotkeyManager)
    {
        _overlaySettingsViewModel = overlaySettingsViewModel;
        _hotkeyManager = hotkeyManager;
        SelectedSettingsGame = GameTitle.DarkSouls2;
        stateService.Subscribe(State.AppStart, OnAppStart);
        OpenOverlaySettingsCommand = new DelegateCommand(OpenOverlaySettings);
        RegisterHotkeys();
    }

    
    #region Commands

    public DelegateCommand OpenOverlaySettingsCommand { get; }

    #endregion

    #region Properties

    private bool _isAlwaysOnTopEnabled;

    public bool IsAlwaysOnTopEnabled
    {
        get => _isAlwaysOnTopEnabled;
        set
        {
            if (!SetProperty(ref _isAlwaysOnTopEnabled, value)) return;
            SettingsManager.Default.AlwaysOnTop = value;
            SettingsManager.Default.Save();
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null) mainWindow.Topmost = _isAlwaysOnTopEnabled;
        }
    }

    public IReadOnlyList<NotesDisplayMode> NotesDisplayModes { get; } = EnumExtensions.GetValues<NotesDisplayMode>().ToList();

    private NotesDisplayMode _notesDisplayMode;

    public NotesDisplayMode NotesDisplayMode
    {
        get => _notesDisplayMode;
        set
        {
            if (!SetProperty(ref _notesDisplayMode, value)) return;
            SettingsManager.Default.NotesDisplayMode = (int)value;
            SettingsManager.Default.Save();
        }
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


    #region Elden Ring

    private bool _erNoLogo;

    public bool ErNoLogo
    {
        get => _erNoLogo;
        set
        {
            if (!SetProperty(ref _erNoLogo, value)) return;
            SettingsManager.Default.ERNoLogo = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    private bool _erStutterFix;

    public bool ERStutterFix
    {
        get => _erStutterFix;
        set
        {
            if (!SetProperty(ref _erStutterFix, value)) return;
            SettingsManager.Default.ERStutterFix = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    private bool _erDisableAchievements;

    public bool ERDisableAchievements
    {
        get => _erDisableAchievements;
        set
        {
            if (!SetProperty(ref _erDisableAchievements, value)) return;
            SettingsManager.Default.ERDisableAchievements = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    #endregion

    #region Dark Souls 3

    private bool _ds3NoLogo;

    public bool DS3NoLogo
    {
        get => _ds3NoLogo;
        set
        {
            if (!SetProperty(ref _ds3NoLogo, value)) return;
            SettingsManager.Default.DS3NoLogo = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    private bool _ds3StutterFix;

    public bool DS3StutterFix
    {
        get => _ds3StutterFix;
        set
        {
            if (!SetProperty(ref _ds3StutterFix, value)) return;
            SettingsManager.Default.DS3StutterFix = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    #endregion

    #region Sekiro

    private bool _skNoLogo;

    public bool SKNoLogo
    {
        get => _skNoLogo;
        set
        {
            if (!SetProperty(ref _skNoLogo, value)) return;
            SettingsManager.Default.SKNoLogo = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    private bool _skNoTutorials;

    public bool SKNoTutorials
    {
        get => _skNoTutorials;
        set
        {
            if (!SetProperty(ref _skNoTutorials, value)) return;
            SettingsManager.Default.SKNoTutorials = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    #endregion

    #region Dark Souls 2

    private bool _ds2NoBabyJump;

    public bool DS2NoBabyJump
    {
        get => _ds2NoBabyJump;
        set
        {
            if (!SetProperty(ref _ds2NoBabyJump, value)) return;
            SettingsManager.Default.DS2NoBabyJump = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    private bool _ds2SkipCredits;

    public bool DS2SkipCredits
    {
        get => _ds2SkipCredits;
        set
        {
            if (!SetProperty(ref _ds2SkipCredits, value)) return;
            SettingsManager.Default.DS2SkipCredits = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    private bool _ds2DisableDoubleClick;

    public bool DS2DisableDoubleClick
    {
        get => _ds2DisableDoubleClick;
        set
        {
            if (!SetProperty(ref _ds2DisableDoubleClick, value)) return;
            SettingsManager.Default.DS2DisableDoubleClick = value;
            SettingsManager.Default.Save();
            OnGameSettingChanged?.Invoke();
        }
    }

    #endregion

    #endregion

    #region Private Methods

    private void OnAppStart()
    {
        ApplyERSettings();
        ApplyDS3Settings();
        ApplySKSettings();
        ApplyDS2Settings();

        IsAlwaysOnTopEnabled = SettingsManager.Default.AlwaysOnTop;

        _notesDisplayMode = (NotesDisplayMode)SettingsManager.Default.NotesDisplayMode;
        OnPropertyChanged(nameof(NotesDisplayMode));
        
        _isPracticeMode = SettingsManager.Default.PracticeMode;
        OnPropertyChanged(nameof(IsPracticeMode));
    }
    
    
    private void RegisterHotkeys()
    {
        _hotkeyManager.RegisterAction(HotkeyActions.TogglePracticeMode, () => { IsPracticeMode = !IsPracticeMode; });
    }
    
    private void OpenOverlaySettings()
    {
        if (_overlaySettingsWindow != null)
        {
            _overlaySettingsWindow.Activate();
            return;
        }

        _overlaySettingsWindow = new OverlaySettingsWindow { DataContext = _overlaySettingsViewModel };
        _overlaySettingsWindow.Closed += (s, e) => _overlaySettingsWindow = null;
        _overlaySettingsWindow.Show();
    }

    private void ApplyERSettings()
    {
        _erNoLogo = SettingsManager.Default.ERNoLogo;
        OnPropertyChanged(nameof(ErNoLogo));

        _erStutterFix = SettingsManager.Default.ERStutterFix;
        OnPropertyChanged(nameof(ERStutterFix));

        _erDisableAchievements = SettingsManager.Default.ERDisableAchievements;
        OnPropertyChanged(nameof(ERDisableAchievements));
    }

    private void ApplyDS3Settings()
    {
        _ds3NoLogo = SettingsManager.Default.DS3NoLogo;
        OnPropertyChanged(nameof(DS3NoLogo));

        _ds3StutterFix = SettingsManager.Default.DS3StutterFix;
        OnPropertyChanged(nameof(DS3StutterFix));
    }

    private void ApplySKSettings()
    {
        _skNoLogo = SettingsManager.Default.SKNoLogo;
        OnPropertyChanged(nameof(SKNoLogo));

        _skNoTutorials = SettingsManager.Default.SKNoTutorials;
        OnPropertyChanged(nameof(SKNoTutorials));
    }

    private void ApplyDS2Settings()
    {
        _ds2NoBabyJump = SettingsManager.Default.DS2NoBabyJump;
        OnPropertyChanged(nameof(DS2NoBabyJump));

        _ds2SkipCredits = SettingsManager.Default.DS2SkipCredits;
        OnPropertyChanged(nameof(DS2SkipCredits));

        _ds2DisableDoubleClick = SettingsManager.Default.DS2DisableDoubleClick;
        OnPropertyChanged(nameof(DS2DisableDoubleClick));
    }

    #endregion
}