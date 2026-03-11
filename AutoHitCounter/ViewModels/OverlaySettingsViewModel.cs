using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using AutoHitCounter.Core;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.ViewModels;

public class OverlaySettingsViewModel : BaseViewModel
{
    private readonly OverlayServerService _overlayServerService;

    public IReadOnlyList<string> AvailableFonts { get; } = Fonts.SystemFontFamilies
        .Select(f => f.Source)
        .OrderBy(f => f)
        .ToList();

    public OverlaySettingsViewModel(OverlayServerService overlayServerService)
    {
        _overlayServerService = overlayServerService;
        LoadFromSettings();

        SaveCommand = new DelegateCommand(Save);
        ResetToDefaultsCommand = new DelegateCommand(ResetToDefaults);
    }

    public DelegateCommand SaveCommand { get; }
    public DelegateCommand ResetToDefaultsCommand { get; }

    private bool _isDirty;

    public bool IsDirty
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    #region Layout

    private bool _showAttempts;

    public bool ShowAttempts
    {
        get => _showAttempts;
        set
        {
            if (SetProperty(ref _showAttempts, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private bool _showProgress;

    public bool ShowProgress
    {
        get => _showProgress;
        set
        {
            if (SetProperty(ref _showProgress, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private bool _showDiff;

    public bool ShowDiff
    {
        get => _showDiff;
        set
        {
            if (SetProperty(ref _showDiff, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private bool _showPb;

    public bool ShowPb
    {
        get => _showPb;
        set
        {
            if (SetProperty(ref _showPb, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private bool _showIgt;

    public bool ShowIgt
    {
        get => _showIgt;
        set
        {
            if (SetProperty(ref _showIgt, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private bool _showFooterTotals;

    public bool ShowFooterTotals
    {
        get => _showFooterTotals;
        set
        {
            if (SetProperty(ref _showFooterTotals, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private int _prevSplits;

    public int PrevSplits
    {
        get => _prevSplits;
        set
        {
            if (SetProperty(ref _prevSplits, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private int _nextSplits;

    public int NextSplits
    {
        get => _nextSplits;
        set
        {
            if (SetProperty(ref _nextSplits, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private int _overlayWidth;

    public int OverlayWidth
    {
        get => _overlayWidth;
        set
        {
            if (SetProperty(ref _overlayWidth, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private int _overlayHeight;

    public int OverlayHeight
    {
        get => _overlayHeight;
        set
        {
            if (SetProperty(ref _overlayHeight, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private double _backgroundOpacity;

    public double BackgroundOpacity
    {
        get => _backgroundOpacity;
        set
        {
            if (SetProperty(ref _backgroundOpacity, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private bool _tableMode;

    public bool TableMode
    {
        get => _tableMode;
        set
        {
            if (SetProperty(ref _tableMode, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    #endregion

    #region Font

    // this only applies to splits
    private string _fontFamily;

    public string FontFamily
    {
        get => _fontFamily;
        set
        {
            if (SetProperty(ref _fontFamily, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _igtFontFamily;

    public string IgtFontFamily
    {
        get => _igtFontFamily;
        set
        {
            if (SetProperty(ref _igtFontFamily, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }
    
    private int _igtFontSize;

    public int IgtFontSize
    {
        get => _igtFontSize;
        set
        {
            if (SetProperty(ref _igtFontSize, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }
    
    private string _alternatingRows;

    public string AlternatingRows
    {
        get => _alternatingRows;
        set
        {
            if (SetProperty(ref _alternatingRows, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private int _fontSize;

    public int FontSize
    {
        get => _fontSize;
        set
        {
            if (SetProperty(ref _fontSize, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private int _rowHeight;

    public int RowHeight
    {
        get => _rowHeight;
        set
        {
            if (SetProperty(ref _rowHeight, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }


    private bool _fontBold;

    public bool FontBold
    {
        get => _fontBold;
        set
        {
            if (SetProperty(ref _fontBold, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private bool _fontItalic;

    public bool FontItalic
    {
        get => _fontItalic;
        set
        {
            if (SetProperty(ref _fontItalic, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private bool _fontUnderline;

    public bool FontUnderline
    {
        get => _fontUnderline;
        set
        {
            if (SetProperty(ref _fontUnderline, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    #endregion

    #region Colors

    private string _headerTextColor;

    public string HeaderTextColor
    {
        get => _headerTextColor;
        set
        {
            if (SetProperty(ref _headerTextColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _attemptsZeroColor;

    public string AttemptsZeroColor
    {
        get => _attemptsZeroColor;
        set
        {
            if (SetProperty(ref _attemptsZeroColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _attemptsActiveColor;

    public string AttemptsActiveColor
    {
        get => _attemptsActiveColor;
        set
        {
            if (SetProperty(ref _attemptsActiveColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _splitNameColor;

    public string SplitNameColor
    {
        get => _splitNameColor;
        set
        {
            if (SetProperty(ref _splitNameColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _groupNameColor;

    public string GroupNameColor
    {
        get => _groupNameColor;
        set
        {
            if (SetProperty(ref _groupNameColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _hitsZeroColor;

    public string HitsZeroColor
    {
        get => _hitsZeroColor;
        set
        {
            if (SetProperty(ref _hitsZeroColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _hitsActiveColor;

    public string HitsActiveColor
    {
        get => _hitsActiveColor;
        set
        {
            if (SetProperty(ref _hitsActiveColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _pbColor;

    public string PbColor
    {
        get => _pbColor;
        set
        {
            if (SetProperty(ref _pbColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _diffPosColor;

    public string DiffPosColor
    {
        get => _diffPosColor;
        set
        {
            if (SetProperty(ref _diffPosColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _diffNegColor;

    public string DiffNegColor
    {
        get => _diffNegColor;
        set
        {
            if (SetProperty(ref _diffNegColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _diffZeroColor;

    public string DiffZeroColor
    {
        get => _diffZeroColor;
        set
        {
            if (SetProperty(ref _diffZeroColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _igtColor;

    public string IgtColor
    {
        get => _igtColor;
        set
        {
            if (SetProperty(ref _igtColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _rowHitColor;

    public string RowHitColor
    {
        get => _rowHitColor;
        set
        {
            if (SetProperty(ref _rowHitColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _rowClearedColor;

    public string RowClearedColor
    {
        get => _rowClearedColor;
        set
        {
            if (SetProperty(ref _rowClearedColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _currentSplitColor;

    public string CurrentSplitColor
    {
        get => _currentSplitColor;
        set
        {
            if (SetProperty(ref _currentSplitColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _currentSplitBorderColor;

    public string CurrentSplitBorderColor
    {
        get => _currentSplitBorderColor;
        set
        {
            if (SetProperty(ref _currentSplitBorderColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _currentSplitHitColor;

    public string CurrentSplitHitColor
    {
        get => _currentSplitHitColor;
        set
        {
            if (SetProperty(ref _currentSplitHitColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _currentSplitHitBorderColor;

    public string CurrentSplitHitBorderColor
    {
        get => _currentSplitHitBorderColor;
        set
        {
            if (SetProperty(ref _currentSplitHitBorderColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    private string _runCompleteBannerColor;

    public string RunCompleteBannerColor
    {
        get => _runCompleteBannerColor;
        set
        {
            if (SetProperty(ref _runCompleteBannerColor, value))
            {
                IsDirty = true;
                BroadcastCurrentConfig();
            }
        }
    }

    #endregion

    #region Public Methods

    private void BroadcastCurrentConfig()
    {
        var config = new OverlayConfig(
            ShowAttempts, PrevSplits, NextSplits, ShowDiff, ShowPb, ShowIgt,
            OverlayWidth, OverlayHeight, ShowProgress, FontFamily, FontSize, RowHeight,
            BackgroundOpacity, TableMode, HitsZeroColor, HitsActiveColor, RowHitColor,
            RowClearedColor, CurrentSplitColor, CurrentSplitBorderColor, CurrentSplitHitColor,
            CurrentSplitHitBorderColor, DiffPosColor, DiffNegColor, DiffZeroColor,
            AttemptsZeroColor, AttemptsActiveColor, HeaderTextColor, IgtColor,
            RunCompleteBannerColor, FontBold, FontItalic, FontUnderline,
            SplitNameColor, GroupNameColor, PbColor, ShowFooterTotals, IgtFontFamily, IgtFontSize, AlternatingRows);
        _overlayServerService.BroadcastConfig(config);
    }

    #endregion

    #region Private Methods

    private void LoadFromSettings()
    {
        var s = SettingsManager.Default;

        _showAttempts = s.ShowAttempts;
        _showProgress = s.ShowProgress;
        _showDiff = s.ShowDiff;
        _showPb = s.ShowPb;
        _showIgt = s.ShowIgt;
        _showFooterTotals = s.ShowFooterTotals;
        _prevSplits = s.PrevSplits;
        _nextSplits = s.NextSplits;
        _overlayWidth = s.OverlayWidth;
        _overlayHeight = s.OverlayHeight;
        _backgroundOpacity = s.BackgroundOpacity;
        _tableMode = s.TableMode;
        _fontFamily = s.FontFamily;
        _fontSize = s.FontSize;
        _rowHeight = s.RowHeight;
        _fontBold = s.FontBold;
        _fontItalic = s.FontItalic;
        _fontUnderline = s.FontUnderline;
        _headerTextColor = s.HeaderTextColor;
        _attemptsZeroColor = s.AttemptsZeroColor;
        _attemptsActiveColor = s.AttemptsActiveColor;
        _splitNameColor = s.SplitNameColor;
        _groupNameColor = s.GroupNameColor;
        _hitsZeroColor = s.HitsZeroColor;
        _hitsActiveColor = s.HitsActiveColor;
        _pbColor = s.PbColor;
        _diffPosColor = s.DiffPosColor;
        _diffNegColor = s.DiffNegColor;
        _diffZeroColor = s.DiffZeroColor;
        _igtColor = s.IgtColor;
        _rowHitColor = s.RowHitColor;
        _rowClearedColor = s.RowClearedColor;
        _currentSplitColor = s.CurrentSplitColor;
        _currentSplitBorderColor = s.CurrentSplitBorderColor;
        _currentSplitHitColor = s.CurrentSplitHitColor;
        _currentSplitHitBorderColor = s.CurrentSplitHitBorderColor;
        _runCompleteBannerColor = s.RunCompleteBannerColor;
        _igtFontFamily = s.IgtFontFamily;
        _igtFontSize = s.IgtFontSize;
        _alternatingRows = s.AlternatingRows;
        
    }

    private void Save()
    {
        var s = SettingsManager.Default;

        s.ShowAttempts = ShowAttempts;
        s.ShowProgress = ShowProgress;
        s.ShowDiff = ShowDiff;
        s.ShowPb = ShowPb;
        s.ShowIgt = ShowIgt;
        s.ShowFooterTotals = ShowFooterTotals;
        s.PrevSplits = PrevSplits;
        s.NextSplits = NextSplits;
        s.OverlayWidth = OverlayWidth;
        s.OverlayHeight = OverlayHeight;
        s.BackgroundOpacity = BackgroundOpacity;
        s.TableMode = TableMode;
        s.FontFamily = FontFamily;
        s.FontSize = FontSize;
        s.RowHeight = RowHeight;
        s.FontBold = FontBold;
        s.FontItalic = FontItalic;
        s.FontUnderline = FontUnderline;
        s.HeaderTextColor = HeaderTextColor;
        s.AttemptsZeroColor = AttemptsZeroColor;
        s.AttemptsActiveColor = AttemptsActiveColor;
        s.SplitNameColor = SplitNameColor;
        s.GroupNameColor = GroupNameColor;
        s.HitsZeroColor = HitsZeroColor;
        s.HitsActiveColor = HitsActiveColor;
        s.PbColor = PbColor;
        s.DiffPosColor = DiffPosColor;
        s.DiffNegColor = DiffNegColor;
        s.DiffZeroColor = DiffZeroColor;
        s.IgtColor = IgtColor;
        s.RowHitColor = RowHitColor;
        s.RowClearedColor = RowClearedColor;
        s.CurrentSplitColor = CurrentSplitColor;
        s.CurrentSplitBorderColor = CurrentSplitBorderColor;
        s.CurrentSplitHitColor = CurrentSplitHitColor;
        s.CurrentSplitHitBorderColor = CurrentSplitHitBorderColor;
        s.RunCompleteBannerColor = RunCompleteBannerColor;
        s.IgtFontFamily = IgtFontFamily;
        s.IgtFontSize = IgtFontSize;
        s.AlternatingRows = AlternatingRows;

        s.Save();
        _overlayServerService.BroadcastConfig(BuildConfig());
        IsDirty = false;
    }

    private void ResetToDefaults()
    {
        var confirmed = MsgBox.ShowOkCancel(
            "This will clear all overlay modifications. Are you sure?",
            "Overlay Reset");
        if (!confirmed) return;

        FontFamily = "Segoe UI";
        FontSize = 15;
        RowHeight = 29;
        FontBold = false;
        FontItalic = false;
        FontUnderline = false;
        HeaderTextColor = "#bbbbbb";
        AttemptsZeroColor = "#ffffff";
        AttemptsActiveColor = "#9D61A8";
        SplitNameColor = "#e0e0e0";
        GroupNameColor = "#999999";
        HitsZeroColor = "#888888";
        HitsActiveColor = "#c8843a";
        PbColor = "#bbbbbb";
        DiffPosColor = "#ff4c4c";
        DiffNegColor = "#00cc66";
        DiffZeroColor = "#bbbbbb";
        IgtColor = "#c47fd4";
        RowHitColor = "rgba(255, 76, 76, 0.17)";
        RowClearedColor = "rgba(0, 204, 102, 0.17)";
        CurrentSplitColor = "rgba(0, 204, 102, 0.06)";
        CurrentSplitBorderColor = "#00cc66";
        CurrentSplitHitColor = "rgba(255, 76, 76, 0.06)";
        CurrentSplitHitBorderColor = "#ff4c4c";
        RunCompleteBannerColor = "#00cc66";
        IgtFontFamily = "Consolas";
        IgtFontSize = 16;
        AlternatingRows = "rgba(255,255,255,0.05)";

        {
            IsDirty = true;
            BroadcastCurrentConfig();
        }
    }

    private static OverlayConfig BuildConfig()
    {
        var s = SettingsManager.Default;
        return new OverlayConfig(
            s.ShowAttempts,
            s.PrevSplits,
            s.NextSplits,
            s.ShowDiff,
            s.ShowPb,
            s.ShowIgt,
            s.OverlayWidth,
            s.OverlayHeight,
            s.ShowProgress,
            s.FontFamily,
            s.FontSize,
            s.RowHeight,
            s.BackgroundOpacity,
            s.TableMode,
            s.HitsZeroColor,
            s.HitsActiveColor,
            s.RowHitColor,
            s.RowClearedColor,
            s.CurrentSplitColor,
            s.CurrentSplitBorderColor,
            s.CurrentSplitHitColor,
            s.CurrentSplitHitBorderColor,
            s.DiffPosColor,
            s.DiffNegColor,
            s.DiffZeroColor,
            s.AttemptsZeroColor,
            s.AttemptsActiveColor,
            s.HeaderTextColor,
            s.IgtColor,
            s.RunCompleteBannerColor,
            s.FontBold,
            s.FontItalic,
            s.FontUnderline,
            s.SplitNameColor,
            s.GroupNameColor,
            s.PbColor,
            s.ShowFooterTotals,
            s.IgtFontFamily,
            s.IgtFontSize,
            s.AlternatingRows);
    }

    #endregion
}