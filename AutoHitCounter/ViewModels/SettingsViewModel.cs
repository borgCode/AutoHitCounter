// 

using System.Windows;
using AutoHitCounter.Enums;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IStateService _stateService;

    public SettingsViewModel(IStateService stateService)
    {
        _stateService = stateService;
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
            _stateService.Publish(State.SettingsChanged);
        }
    }
    
    
    
    #endregion


    #region Private Methods

    private void OnAppStart()
    {
     
    }

    #endregion
    
}