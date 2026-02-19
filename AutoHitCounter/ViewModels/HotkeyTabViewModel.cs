// 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Utilities;
using H.Hooks;
using Key = H.Hooks.Key;
using KeyboardEventArgs = H.Hooks.KeyboardEventArgs;

namespace AutoHitCounter.ViewModels;

public class HotkeyTabViewModel : BaseViewModel
{
    private readonly HotkeyManager _hotkeyManager;
    
    private string _currentSettingHotkeyId;
    private LowLevelKeyboardHook _tempHook;
    private Keys _currentKeys;
    
    private readonly Dictionary<string, HotkeyBindingViewModel> _hotkeyLookup;
    
    public ObservableCollection<HotkeyBindingViewModel> HotkeyBindings { get; }

    public HotkeyTabViewModel(HotkeyManager hotkeyManager, IStateService stateService)
    {
        _hotkeyManager = hotkeyManager;
        
        stateService.Subscribe(State.AppStart, OnAppStart);

        HotkeyBindings =
        [
            new("Next Split", HotkeyActions.NextSplit),
            new("Previous Split", HotkeyActions.PreviousSplit),
            new("Reset", HotkeyActions.Reset),
            new("Increment Hit", HotkeyActions.IncrementHit),
            new("Decrement Hit", HotkeyActions.DecrementHit),
            
            
        ];

        _hotkeyLookup = HotkeyBindings.ToDictionary(h => h.ActionId);
        
        LoadHotkeyDisplays();
    }

    #region Commands

    public ICommand ClearHotkeysCommand { get; set; }

    #endregion

    #region Properties

    private bool _isEnableHotkeysEnabled;

    public bool IsEnableHotkeysEnabled
    {
        get => _isEnableHotkeysEnabled;
        set
        {
            if (SetProperty(ref _isEnableHotkeysEnabled, value))
            {
                SettingsManager.Default.EnableHotkeys = value;
                SettingsManager.Default.Save();
                if (_isEnableHotkeysEnabled) _hotkeyManager.Start();
                else _hotkeyManager.Stop();
            }
        }
    }
    
    private bool _isGlobalHotkeysEnabled;

    public bool IsGlobalHotkeysEnabled
    {
        get => _isGlobalHotkeysEnabled;
        set
        {
            if (SetProperty(ref _isGlobalHotkeysEnabled, value))
            {
                SettingsManager.Default.GlobalHotkeys = value;
                SettingsManager.Default.Save();
                _hotkeyManager.SetGlobalHotkeys(_isGlobalHotkeysEnabled);
            }
        }
    }
    
    
    
    private bool _isBlockGameHotkeysEnabled;

    public bool IsBlockGameHotkeysEnabled
    {
        get => _isBlockGameHotkeysEnabled;
        set
        {
            if (SetProperty(ref _isBlockGameHotkeysEnabled, value))
            {
                SettingsManager.Default.BlockHotkeysFromGame = value;
                SettingsManager.Default.Save();
                _hotkeyManager.SetKeyboardHandling(_isBlockGameHotkeysEnabled);
            }
        }
    }

    #endregion

    #region Public Methods

    public void StartSettingHotkey(string actionId)
    {
        if (_currentSettingHotkeyId != null &&
            _hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var prev))
        {
            prev.HotkeyText = GetHotkeyDisplayText(_currentSettingHotkeyId);
        }

        _currentSettingHotkeyId = actionId;

        if (_hotkeyLookup.TryGetValue(actionId, out var current))
        {
            current.HotkeyText = "Press keys...";
        }

        _tempHook = new LowLevelKeyboardHook();
        _tempHook.IsExtendedMode = true;
        _tempHook.Down += TempHook_Down;
        _tempHook.Start();
    }

    public void ConfirmHotkey()
    {
        var currentSettingHotkeyId = _currentSettingHotkeyId;
        var currentKeys = _currentKeys;
        if (currentSettingHotkeyId == null || currentKeys == null || currentKeys.IsEmpty)
        {
            CancelSettingHotkey();
            return;
        }

        HandleExistingHotkey(currentKeys);
        SetNewHotkey(currentSettingHotkeyId, currentKeys);

        StopSettingHotkey();
    }

    public void CancelSettingHotkey()
    {
        var actionId = _currentSettingHotkeyId;

        if (actionId != null && _hotkeyLookup.TryGetValue(actionId, out var binding))
        {
            binding.HotkeyText = "None";
            _hotkeyManager.SetHotkey(actionId, new Keys());
        }

        StopSettingHotkey();
    }

    #endregion


    #region Private Methods

    private void OnAppStart()
    {
        _isEnableHotkeysEnabled = SettingsManager.Default.EnableHotkeys;
        if (_isEnableHotkeysEnabled) _hotkeyManager.Start();
        else _hotkeyManager.Stop();
        OnPropertyChanged(nameof(IsEnableHotkeysEnabled));
        
        IsBlockGameHotkeysEnabled = SettingsManager.Default.BlockHotkeysFromGame;
        IsGlobalHotkeysEnabled = SettingsManager.Default.GlobalHotkeys;
    }
    
    private void LoadHotkeyDisplays()
    {
        foreach (var hotkey in _hotkeyLookup.Values)
        {
            hotkey.HotkeyText = GetHotkeyDisplayText(hotkey.ActionId);
        }
    }
    
    private string GetHotkeyDisplayText(string actionId)
    {
        Keys keys = _hotkeyManager.GetHotkey(actionId);
        return keys != null && keys.Values.ToArray().Length > 0 ? string.Join(" + ", keys) : "None";
    }
    
    private void TempHook_Down(object sender, KeyboardEventArgs e)
    {
        if (_currentSettingHotkeyId == null || e.Keys.IsEmpty)
            return;

        try
        {
            bool containsEnter = e.Keys.Values.Contains(Key.Enter) || e.Keys.Values.Contains(Key.Return);

            if (containsEnter && _currentKeys != null)
            {
                _hotkeyManager.SetHotkey(_currentSettingHotkeyId, _currentKeys);
                StopSettingHotkey();
                e.IsHandled = true;
                return;
            }

            if (e.Keys.Values.Contains(Key.Escape))
            {
                CancelSettingHotkey();
                e.IsHandled = true;
                return;
            }

            if (containsEnter)
            {
                e.IsHandled = true;
                return;
            }

            if (e.Keys.IsEmpty)
                return;

            _currentKeys = e.Keys;

            if (_hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var binding))
            {
                binding.HotkeyText = e.Keys.ToString();
            }
        }
        catch (Exception ex)
        {
            if (_hotkeyLookup.TryGetValue(_currentSettingHotkeyId, out var binding))
            {
                binding.HotkeyText = "Error: Invalid key combination";
            }
        }

        e.IsHandled = true;
    }

    private void StopSettingHotkey()
    {
        var hook = _tempHook;
        _tempHook = null;
        _currentSettingHotkeyId = null;
        _currentKeys = null;

        if (hook != null)
        {
            hook.Down -= TempHook_Down;
            try
            {
                hook.Dispose();
            }
            catch (COMException)
            {
                // Already stopped - harmless
            }
        }
    }

    private void HandleExistingHotkey(Keys currentKeys)
    {
        string existingHotkeyId = _hotkeyManager.GetActionIdByKeys(currentKeys);
        if (string.IsNullOrEmpty(existingHotkeyId)) return;

        _hotkeyManager.ClearHotkey(existingHotkeyId);
        if (_hotkeyLookup.TryGetValue(existingHotkeyId, out var binding))
        {
            binding.HotkeyText = "None";
        }
    }

    private void SetNewHotkey(string currentSettingHotkeyId, Keys currentKeys)
    {
        _hotkeyManager.SetHotkey(currentSettingHotkeyId, currentKeys);

        if (_hotkeyLookup.TryGetValue(currentSettingHotkeyId, out var binding))
        {
            binding.HotkeyText = new Keys(currentKeys.Values.ToArray()).ToString();
        }
    }

    #endregion

    
}