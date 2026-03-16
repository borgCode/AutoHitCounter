using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using AutoHitCounter.Core;
using AutoHitCounter.Models;
using AutoHitCounter.Services;
using AutoHitCounter.Utilities;
using Microsoft.Win32;

namespace AutoHitCounter.ViewModels;

public class OverlaySettingsViewModel : BaseViewModel
{
    private readonly OverlayServerService _overlayServerService;
    private readonly OverlayProfileManager _profileManager;
    private readonly Dictionary<string, object> _values = new();
    private static readonly PropertyInfo[] ConfigProps =
        typeof(OverlayConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    public IReadOnlyList<string> AvailableFonts { get; } = Fonts.SystemFontFamilies
        .Select(f => f.Source)
        .OrderBy(f => f)
        .ToList();

    public OverlaySettingsViewModel(OverlayServerService overlayServerService, OverlayProfileManager profileManager)
    {
        _overlayServerService = overlayServerService;
        _profileManager = profileManager;

        LoadFromProfile();
        RefreshProfileNames();

        SaveCommand = new DelegateCommand(Save);
        ResetToDefaultsCommand = new DelegateCommand(ResetToDefaults);
        NewProfileCommand = new DelegateCommand(NewProfile);
        RenameProfileCommand = new DelegateCommand(RenameProfile);
        DeleteProfileCommand = new DelegateCommand(DeleteProfile);
        ImportProfileCommand = new DelegateCommand(ImportProfile);
        ExportProfileCommand = new DelegateCommand(ExportProfile);
    }

    public DelegateCommand SaveCommand { get; }
    public DelegateCommand ResetToDefaultsCommand { get; }
    public DelegateCommand NewProfileCommand { get; }
    public DelegateCommand RenameProfileCommand { get; }
    public DelegateCommand DeleteProfileCommand { get; }
    public DelegateCommand ImportProfileCommand { get; }
    public DelegateCommand ExportProfileCommand { get; }

    private bool _isDirty;

    public bool IsDirty
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    public ObservableCollection<string> ProfileNames { get; } = new();

    private string _selectedProfileName;

    public string SelectedProfileName
    {
        get => _selectedProfileName;
        set
        {
            if (_selectedProfileName == value) return;

            if (_isDirty && _selectedProfileName != null)
            {
                var result = MsgBox.ShowYesNoCancel(
                    "You have unsaved changes. Would you like to save before switching profiles?",
                    "Unsaved Changes");

                if (result == null)
                {
                    OnPropertyChanged(nameof(SelectedProfileName));
                    return;
                }
                if (result == true) Save();
                // false = discard
            }

            SetProperty(ref _selectedProfileName, value);
            if (value != null)
            {
                _profileManager.SetActiveProfile(value);
                LoadFromProfile();
                OnPropertyChanged(string.Empty);
                IsDirty = false;
                BroadcastCurrentConfig();
            }
        }
    }

    public void ReloadFromProfile()
    {
        LoadFromProfile();
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
    public bool ShowRunComplete          { get => Get<bool>(); set => Set(value); }
    public string RunCompleteText          { get => Get<string>(); set => Set(value); }
    public string CustomCss { get => Get<string>(); set => Set(value); }

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
    public string FooterFontFamily   { get => Get<string>(); set => Set(value); }
    public int  FooterFontSize     { get => Get<int>();    set => Set(value); }

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
    public bool PbMatchesHit { get => Get<bool>(); set => Set(value); }
    public string FooterHitFontColor          { get => Get<string>(); set => Set(value); }
    public string FooterHitsCurrentColor          { get => Get<string>(); set => Set(value); }
    public string FooterPbFontColor          { get => Get<string>(); set => Set(value); }
    public bool ShowDpbHighlight         { get => Get<bool>();   set => Set(value); }
    public string DpbHighlightColor          { get => Get<string>(); set => Set(value); }


    #endregion

    #region Title

    public bool   ShowTitle       { get => Get<bool>();   set => Set(value); }
    public string TitleText       { get => Get<string>(); set => Set(value); }
    public string TitleColor      { get => Get<string>(); set => Set(value); }
    public string TitleFontFamily { get => Get<string>(); set => Set(value); }
    public int    TitleFontSize   { get => Get<int>();    set => Set(value); }

    #endregion

    #region Private Methods

    private void LoadFromProfile()
    {
        var config = _profileManager.LoadActiveProfile();
        _selectedProfileName = _profileManager.ActiveProfileName;

        foreach (var prop in ConfigProps)
        {
            var value = prop.GetValue(config);
            if (value != null)
                _values[prop.Name] = value;
        }
    }

    private OverlayConfig BuildCurrentConfig()
    {
        var config = new OverlayConfig();
        foreach (var prop in ConfigProps)
        {
            if (_values.TryGetValue(prop.Name, out var value))
            {
                try { prop.SetValue(config, Convert.ChangeType(value, prop.PropertyType)); }
                catch { /* skip incompatible */ }
            }
        }
        return config;
    }

    private void Save()
    {
        var config = BuildCurrentConfig();
        _profileManager.SaveActiveProfile(config);
        BroadcastCurrentConfig();
        IsDirty = false;
    }

    private void BroadcastCurrentConfig()
    {
        var config = BuildCurrentConfig();
        _overlayServerService.BroadcastConfig(config);
    }

    private void ResetToDefaults()
    {
        var confirmed = MsgBox.ShowOkCancel(
            "This will clear all overlay modifications. Are you sure?",
            "Overlay Reset");
        if (!confirmed) return;

        var defaults = OverlayProfileManager.CreateDefaultConfig();
        foreach (var prop in ConfigProps)
        {
            var value = prop.GetValue(defaults);
            if (value != null)
                _values[prop.Name] = value;
        }

        OnPropertyChanged(string.Empty);
        IsDirty = true;
        BroadcastCurrentConfig();
    }

    private void NewProfile()
    {
        var name = MsgBox.ShowInput("Profile name:", "", "New Profile");
        if (string.IsNullOrWhiteSpace(name)) return;

        if (ProfileNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            MsgBox.Show("A profile with that name already exists.", "Error");
            return;
        }

        if (IsDirty) Save();

        var config = BuildCurrentConfig();
        _profileManager.CreateProfile(name, config);
        _profileManager.SetActiveProfile(name);
        RefreshProfileNames();
        _selectedProfileName = name;
        OnPropertyChanged(nameof(SelectedProfileName));
        IsDirty = false;
    }

    private void RenameProfile()
    {
        var current = _selectedProfileName;
        var name = MsgBox.ShowInput("New name:", current, "Rename Profile");
        if (string.IsNullOrWhiteSpace(name) || name == current) return;

        if (ProfileNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            MsgBox.Show("A profile with that name already exists.", "Error");
            return;
        }

        _profileManager.RenameProfile(current, name);
        RefreshProfileNames();
        _selectedProfileName = name;
        OnPropertyChanged(nameof(SelectedProfileName));
    }

    private void DeleteProfile()
    {
        if (ProfileNames.Count <= 1)
        {
            MsgBox.Show("Cannot delete the last remaining profile.", "Error");
            return;
        }

        var confirmed = MsgBox.ShowOkCancel(
            $"Delete profile \"{_selectedProfileName}\"?",
            "Delete Profile");
        if (!confirmed) return;

        _profileManager.DeleteProfile(_selectedProfileName);
        RefreshProfileNames();
        _selectedProfileName = _profileManager.ActiveProfileName;
        LoadFromProfile();
        OnPropertyChanged(string.Empty);
        IsDirty = false;
        BroadcastCurrentConfig();
    }

    private void ImportProfile()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Import Overlay Profile"
        };

        if (dlg.ShowDialog() != true) return;

        var importedName = _profileManager.ImportProfile(dlg.FileName);
        RefreshProfileNames();
        _profileManager.SetActiveProfile(importedName);
        _selectedProfileName = importedName;
        LoadFromProfile();
        OnPropertyChanged(string.Empty);
        IsDirty = false;
        BroadcastCurrentConfig();
    }

    private void ExportProfile()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            FileName = _selectedProfileName + ".json",
            Title = "Export Overlay Profile"
        };

        if (dlg.ShowDialog() != true) return;

        if (IsDirty) Save();
        _profileManager.ExportProfile(_selectedProfileName, dlg.FileName);
    }

    private void RefreshProfileNames()
    {
        ProfileNames.Clear();
        foreach (var name in _profileManager.GetProfileNames())
            ProfileNames.Add(name);
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
