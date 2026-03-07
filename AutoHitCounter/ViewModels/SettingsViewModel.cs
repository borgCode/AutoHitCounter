// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly OverlayServerService _overlayServerService;

    public event Action OnGameSettingChanged;

    public IReadOnlyList<GameTitle> GameTitles { get; } = EnumExtensions.GetValues<GameTitle>()
        .Where(title => title != GameTitle.DarkSoulsRemastered).ToList();

    private GameTitle _selectedSettingsGame;

    public GameTitle SelectedSettingsGame
    {
        get => _selectedSettingsGame;
        set => SetProperty(ref _selectedSettingsGame, value);
    }

    public SettingsViewModel(IStateService stateService, OverlayServerService overlayServerService)
    {
        _overlayServerService = overlayServerService;
        SelectedSettingsGame = GameTitle.DarkSouls2;
        stateService.Subscribe(State.AppStart, OnAppStart);
    }

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

    private bool _isShowNotesEnabled;

    public bool IsShowNotesEnabled
    {
        get => _isShowNotesEnabled;
        set
        {
            if (!SetProperty(ref _isShowNotesEnabled, value)) return;
            SettingsManager.Default.ShowNotesSection = value;
            SettingsManager.Default.Save();
        }
    }

    private bool _allowManualSplitOnAutoSplits;

    public bool AllowManualSplitOnAutoSplits
    {
        get => _allowManualSplitOnAutoSplits;
        set
        {
            if (!SetProperty(ref _allowManualSplitOnAutoSplits, value)) return;
            SettingsManager.Default.AllowManualSplitOnAutoSplits = value;
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

    private int _maxSplits;

    public int MaxSplits
    {
        get => _maxSplits;
        set
        {
            if (!SetProperty(ref _maxSplits, value)) return;
            SettingsManager.Default.MaxSplits = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private int _prevSplits;

    public int PrevSplits
    {
        get => _prevSplits;
        set
        {
            if (!SetProperty(ref _prevSplits, value)) return;
            SettingsManager.Default.PrevSplits = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private int _nextSplits;

    public int NextSplits
    {
        get => _nextSplits;
        set
        {
            if (!SetProperty(ref _nextSplits, value)) return;
            SettingsManager.Default.NextSplits = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private bool _showDiff;

    public bool ShowDiff
    {
        get => _showDiff;
        set
        {
            if (!SetProperty(ref _showDiff, value)) return;
            SettingsManager.Default.ShowDiff = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private bool _showPb;

    public bool ShowPb
    {
        get => _showPb;
        set
        {
            if (!SetProperty(ref _showPb, value)) return;
            SettingsManager.Default.ShowPb = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private bool _showIgt;

    public bool ShowIgt
    {
        get => _showIgt;
        set
        {
            if (!SetProperty(ref _showIgt, value)) return;
            SettingsManager.Default.ShowIgt = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
        }
    }

    private int _overlayWidth;

    public int OverlayWidth
    {
        get => _overlayWidth;
        set
        {
            if (!SetProperty(ref _overlayWidth, value)) return;
            SettingsManager.Default.OverlayWidth = value;
            SettingsManager.Default.Save();
            BroadcastConfigChanged();
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

        _isShowNotesEnabled = SettingsManager.Default.ShowNotesSection;
        OnPropertyChanged(nameof(IsShowNotesEnabled));

        _allowManualSplitOnAutoSplits = SettingsManager.Default.AllowManualSplitOnAutoSplits;
        OnPropertyChanged(nameof(AllowManualSplitOnAutoSplits));

        _isPracticeMode = SettingsManager.Default.PracticeMode;
        OnPropertyChanged(nameof(IsPracticeMode));

        LoadSplitConfig();
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

    private void LoadSplitConfig()
    {
        _maxSplits = SettingsManager.Default.MaxSplits;
        OnPropertyChanged(nameof(MaxSplits));

        _prevSplits = SettingsManager.Default.PrevSplits;
        OnPropertyChanged(nameof(PrevSplits));

        _nextSplits = SettingsManager.Default.NextSplits;
        OnPropertyChanged(nameof(NextSplits));

        _showDiff = SettingsManager.Default.ShowDiff;
        OnPropertyChanged(nameof(ShowDiff));

        _showPb = SettingsManager.Default.ShowPb;
        OnPropertyChanged(nameof(ShowPb));

        _showIgt = SettingsManager.Default.ShowIgt;
        OnPropertyChanged(nameof(ShowIgt));

        _overlayWidth = SettingsManager.Default.OverlayWidth;
        OnPropertyChanged(nameof(OverlayWidth));

        BroadcastConfigChanged();
    }

    private void BroadcastConfigChanged()
    {
        var config = new OverlayConfig(MaxSplits, PrevSplits, NextSplits, ShowDiff, ShowPb, ShowIgt, OverlayWidth);
        _overlayServerService.BroadcastConfig(config);
    }

    #endregion
}