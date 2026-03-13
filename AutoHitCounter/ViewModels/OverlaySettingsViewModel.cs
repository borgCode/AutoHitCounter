using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using AutoHitCounter.Core;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.ViewModels;

public class OverlaySettingsViewModel : BaseViewModel
{
    private readonly OverlayServerService _overlayServerService;
    private readonly Dictionary<string, object> _values = new();

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

    public void ReloadFromSettings()
    {
        LoadFromSettings();
        OnPropertyChanged(string.Empty);
        IsDirty = false;
        BroadcastCurrentConfig();
    }

    #region Layout

    public bool ShowAttempts      { get => Get<bool>();   set => Set(value); }
    public bool ShowProgress      { get => Get<bool>();   set => Set(value); }
    public bool ShowDiff          { get => Get<bool>();   set => Set(value); }
    public bool ShowPb            { get => Get<bool>();   set => Set(value); }
    public bool ShowIgt           { get => Get<bool>();   set => Set(value); }
    public bool ShowFooterTotals  { get => Get<bool>();   set => Set(value); }
    public int  PrevSplits        { get => Get<int>();    set => Set(value); }
    public int  NextSplits        { get => Get<int>();    set => Set(value); }
    public int  OverlayWidth      { get => Get<int>();    set => Set(value); }
    public int  OverlayHeight     { get => Get<int>();    set => Set(value); }
    public double BackgroundOpacity { get => Get<double>(); set => Set(value); }
    public bool TableMode         { get => Get<bool>();   set => Set(value); }

    #endregion

    #region Font

    // this only applies to splits
    public string FontFamily      { get => Get<string>(); set => Set(value); }
    public int    FontSize        { get => Get<int>();    set => Set(value); }
    public int    RowHeight       { get => Get<int>();    set => Set(value); }
    public bool   FontBold        { get => Get<bool>();   set => Set(value); }
    public bool   FontItalic      { get => Get<bool>();   set => Set(value); }
    public bool   FontUnderline   { get => Get<bool>();   set => Set(value); }
    public string IgtFontFamily   { get => Get<string>(); set => Set(value); }
    public int    IgtFontSize     { get => Get<int>();    set => Set(value); }

    #endregion

    #region Colors

    public string HeaderTextColor          { get => Get<string>(); set => Set(value); }
    public string AttemptsZeroColor        { get => Get<string>(); set => Set(value); }
    public string AttemptsActiveColor      { get => Get<string>(); set => Set(value); }
    public string SplitNameColor           { get => Get<string>(); set => Set(value); }
    public string SplitNameOnHitColor           { get => Get<string>(); set => Set(value); }
    public string SplitNameOnHitlessColor           { get => Get<string>(); set => Set(value); }
    public string GroupNameColor           { get => Get<string>(); set => Set(value); }
    public string HitsActiveColor          { get => Get<string>(); set => Set(value); }
    public string PbColor                  { get => Get<string>(); set => Set(value); }
    public string DiffPosColor             { get => Get<string>(); set => Set(value); }
    public string DiffNegColor             { get => Get<string>(); set => Set(value); }
    public string DiffZeroColor            { get => Get<string>(); set => Set(value); }
    public string IgtColor                 { get => Get<string>(); set => Set(value); }
    public string RowHitColor              { get => Get<string>(); set => Set(value); }
    public string RowClearedColor          { get => Get<string>(); set => Set(value); }
    public string CurrentSplitColor        { get => Get<string>(); set => Set(value); }
    public string CurrentSplitBorderColor  { get => Get<string>(); set => Set(value); }
    public string CurrentSplitHitColor     { get => Get<string>(); set => Set(value); }
    public string CurrentSplitHitBorderColor { get => Get<string>(); set => Set(value); }
    public string RunCompleteBannerColor   { get => Get<string>(); set => Set(value); }
    public string AlternatingRows          { get => Get<string>(); set => Set(value); }
    public string HitsCurrentColor          { get => Get<string>(); set => Set(value); }
    public string HitsClearedColor          { get => Get<string>(); set => Set(value); }
    public string HeaderFontFamily          { get => Get<string>(); set => Set(value); }
    public int HeaderFontSize          { get => Get<int>(); set => Set(value); }
    
    

    #endregion
    
    #region Private Methods

    private void LoadFromSettings()
    {
        var s = SettingsManager.Default;

        _values[nameof(ShowAttempts)]      = s.ShowAttempts;
        _values[nameof(ShowProgress)]      = s.ShowProgress;
        _values[nameof(ShowDiff)]          = s.ShowDiff;
        _values[nameof(ShowPb)]            = s.ShowPb;
        _values[nameof(ShowIgt)]           = s.ShowIgt;
        _values[nameof(ShowFooterTotals)]  = s.ShowFooterTotals;
        _values[nameof(PrevSplits)]        = s.PrevSplits;
        _values[nameof(NextSplits)]        = s.NextSplits;
        _values[nameof(OverlayWidth)]      = s.OverlayWidth;
        _values[nameof(OverlayHeight)]     = s.OverlayHeight;
        _values[nameof(BackgroundOpacity)] = s.BackgroundOpacity;
        _values[nameof(TableMode)]         = s.TableMode;
        _values[nameof(FontFamily)]        = s.FontFamily;
        _values[nameof(FontSize)]          = s.FontSize;
        _values[nameof(RowHeight)]         = s.RowHeight;
        _values[nameof(FontBold)]          = s.FontBold;
        _values[nameof(FontItalic)]        = s.FontItalic;
        _values[nameof(FontUnderline)]     = s.FontUnderline;
        _values[nameof(IgtFontFamily)]     = s.IgtFontFamily;
        _values[nameof(IgtFontSize)]       = s.IgtFontSize;
        _values[nameof(HeaderTextColor)]   = s.HeaderTextColor;
        _values[nameof(AttemptsZeroColor)] = s.AttemptsZeroColor;
        _values[nameof(AttemptsActiveColor)] = s.AttemptsActiveColor;
        _values[nameof(SplitNameColor)]    = s.SplitNameColor;
        _values[nameof(SplitNameOnHitColor)]    = s.SplitNameOnHitColor;
        _values[nameof(SplitNameOnHitlessColor)]    = s.SplitNameOnHitlessColor;
        _values[nameof(GroupNameColor)]    = s.GroupNameColor;
        _values[nameof(HitsActiveColor)]   = s.HitsActiveColor;
        _values[nameof(PbColor)]           = s.PbColor;
        _values[nameof(DiffPosColor)]      = s.DiffPosColor;
        _values[nameof(DiffNegColor)]      = s.DiffNegColor;
        _values[nameof(DiffZeroColor)]     = s.DiffZeroColor;
        _values[nameof(IgtColor)]          = s.IgtColor;
        _values[nameof(RowHitColor)]       = s.RowHitColor;
        _values[nameof(RowClearedColor)]   = s.RowClearedColor;
        _values[nameof(CurrentSplitColor)] = s.CurrentSplitColor;
        _values[nameof(CurrentSplitBorderColor)] = s.CurrentSplitBorderColor;
        _values[nameof(CurrentSplitHitColor)] = s.CurrentSplitHitColor;
        _values[nameof(CurrentSplitHitBorderColor)] = s.CurrentSplitHitBorderColor;
        _values[nameof(RunCompleteBannerColor)] = s.RunCompleteBannerColor;
        _values[nameof(AlternatingRows)]   = s.AlternatingRows;
        _values[nameof(HitsCurrentColor)]   = s.HitsCurrentColor;
        _values[nameof(HitsClearedColor)]   = s.HitsClearedColor;
        _values[nameof(HeaderFontFamily)]   = s.HeaderFontFamily;
        _values[nameof(HeaderFontSize)]     = s.HeaderFontSize;
    }
    
    private void Save()
    {
        System.Diagnostics.Debug.WriteLine("Save called from: " + Environment.StackTrace);
        var s = SettingsManager.Default;

        s.ShowAttempts      = ShowAttempts;
        s.ShowProgress      = ShowProgress;
        s.ShowDiff          = ShowDiff;
        s.ShowPb            = ShowPb;
        s.ShowIgt           = ShowIgt;
        s.ShowFooterTotals  = ShowFooterTotals;
        s.PrevSplits        = PrevSplits;
        s.NextSplits        = NextSplits;
        s.OverlayWidth      = OverlayWidth;
        s.OverlayHeight     = OverlayHeight;
        s.BackgroundOpacity = BackgroundOpacity;
        s.TableMode         = TableMode;
        s.FontFamily        = FontFamily;
        s.FontSize          = FontSize;
        s.RowHeight         = RowHeight;
        s.FontBold          = FontBold;
        s.FontItalic        = FontItalic;
        s.FontUnderline     = FontUnderline;
        s.IgtFontFamily     = IgtFontFamily;
        s.IgtFontSize       = IgtFontSize;
        s.HeaderTextColor   = HeaderTextColor;
        s.AttemptsZeroColor = AttemptsZeroColor;
        s.AttemptsActiveColor = AttemptsActiveColor;
        s.SplitNameColor    = SplitNameColor;
        s.SplitNameOnHitColor    = SplitNameOnHitColor;
        s.SplitNameOnHitlessColor    = SplitNameOnHitlessColor;
        s.GroupNameColor    = GroupNameColor;
        s.HitsActiveColor   = HitsActiveColor;
        s.PbColor           = PbColor;
        s.DiffPosColor      = DiffPosColor;
        s.DiffNegColor      = DiffNegColor;
        s.DiffZeroColor     = DiffZeroColor;
        s.IgtColor          = IgtColor;
        s.RowHitColor       = RowHitColor;
        s.RowClearedColor   = RowClearedColor;
        s.CurrentSplitColor = CurrentSplitColor;
        s.CurrentSplitBorderColor = CurrentSplitBorderColor;
        s.CurrentSplitHitColor = CurrentSplitHitColor;
        s.CurrentSplitHitBorderColor = CurrentSplitHitBorderColor;
        s.RunCompleteBannerColor = RunCompleteBannerColor;
        s.AlternatingRows   = AlternatingRows;
        s.HitsCurrentColor   = HitsCurrentColor;
        s.HitsClearedColor   = HitsClearedColor;
        s.HeaderFontFamily   = HeaderFontFamily;
        s.HeaderFontSize    = HeaderFontSize;

        s.Save();
        BroadcastCurrentConfig();
        IsDirty = false;
    }

    private void BroadcastCurrentConfig()
    {
        var config = new OverlayConfig
        {
            ShowAttempts = ShowAttempts,
            PrevSplits = PrevSplits,
            NextSplits = NextSplits,
            ShowDiff = ShowDiff,
            ShowPb = ShowPb,
            ShowIgt = ShowIgt,
            OverlayWidth = OverlayWidth,
            OverlayHeight = OverlayHeight,
            ShowProgress = ShowProgress,
            FontFamily = FontFamily,
            FontSize = FontSize,
            RowHeight = RowHeight,
            BackgroundOpacity = BackgroundOpacity,
            TableMode = TableMode,
            HitsActiveColor = HitsActiveColor,
            RowHitColor = RowHitColor,
            RowClearedColor = RowClearedColor,
            CurrentSplitColor = CurrentSplitColor,
            CurrentSplitBorderColor = CurrentSplitBorderColor,
            CurrentSplitHitColor = CurrentSplitHitColor,
            CurrentSplitHitBorderColor = CurrentSplitHitBorderColor,
            DiffPosColor = DiffPosColor,
            DiffNegColor = DiffNegColor,
            DiffZeroColor = DiffZeroColor,
            AttemptsZeroColor = AttemptsZeroColor,
            AttemptsActiveColor = AttemptsActiveColor,
            HeaderTextColor = HeaderTextColor,
            IgtColor = IgtColor,
            RunCompleteBannerColor = RunCompleteBannerColor,
            FontBold = FontBold,
            FontItalic = FontItalic,
            FontUnderline = FontUnderline,
            SplitNameColor = SplitNameColor,
            SplitNameOnHitColor = SplitNameOnHitColor,
            SplitNameOnHitlessColor = SplitNameOnHitlessColor,
            GroupNameColor = GroupNameColor,
            PbColor = PbColor,
            ShowFooterTotals = ShowFooterTotals,
            IgtFontFamily = IgtFontFamily,
            IgtFontSize = IgtFontSize,
            AlternatingRows = AlternatingRows,
            HitsCurrentColor = HitsCurrentColor,
            HitsClearedColor = HitsClearedColor,
            HeaderFontFamily = HeaderFontFamily,
            HeaderFontSize = HeaderFontSize,
        };
        _overlayServerService.BroadcastConfig(config);
    }

    private void ResetToDefaults()
    {
        var confirmed = MsgBox.ShowOkCancel(
            "This will clear all overlay modifications. Are you sure?",
            "Overlay Reset");
        if (!confirmed) return;

        // Font
        FontFamily    = "Segoe UI";
        FontSize      = 15;
        RowHeight     = 29;
        FontBold      = false;
        FontItalic    = false;
        FontUnderline = false;
        IgtFontFamily = "Consolas";
        IgtFontSize   = 16;
        HeaderFontFamily    = "Segoe UI";
        HeaderFontSize    = 11;
        
        
        ShowAttempts     = true;
        ShowProgress     = true;
        ShowDiff         = true;
        ShowPb           = true;
        ShowIgt          = true;
        ShowFooterTotals = true;
        PrevSplits       = 4;
        NextSplits       = 13;
        OverlayWidth     = 300;
        OverlayHeight    = 420;
        BackgroundOpacity = 0;
        TableMode        = false;

        // Colors
        HeaderTextColor          = "#bbbbbb";
        AttemptsZeroColor        = "#ffffff";
        AttemptsActiveColor      = "#9D61A8";
        SplitNameColor           = "#e0e0e0";
        SplitNameOnHitColor           = "#e0e0e0";
        SplitNameOnHitlessColor           = "#e0e0e0";
        GroupNameColor           = "#999999";
        HitsCurrentColor         = "#888888";
        HitsClearedColor         = "#00cc66";
        HitsActiveColor          = "#c8843a";
        PbColor                  = "#bbbbbb";
        DiffPosColor             = "#ff4c4c";
        DiffNegColor             = "#00cc66";
        DiffZeroColor            = "#bbbbbb";
        IgtColor                 = "#c47fd4";
        RowHitColor              = "rgba(255, 76, 76, 0.17)";
        RowClearedColor          = "rgba(0, 204, 102, 0.17)";
        CurrentSplitColor        = "rgba(0, 204, 102, 0.06)";
        CurrentSplitBorderColor  = "#00cc66";
        CurrentSplitHitColor     = "rgba(255, 76, 76, 0.06)";
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
    
    private T Get<T>(T fallback = default, [CallerMemberName] string name = null)
        => _values.TryGetValue(name, out var v) ? (T)v : fallback;

    private void Set<T>(T value, [CallerMemberName] string name = null)
    {
        if (_values.TryGetValue(name, out var old) && EqualityComparer<T>.Default.Equals((T)old, value))
            return;

        _values[name] = value;
        OnPropertyChanged(name);
        IsDirty = true;
        BroadcastCurrentConfig();
    }

    #endregion
}