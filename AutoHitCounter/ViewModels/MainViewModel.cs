// 

using System.Collections.ObjectModel;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;
using AutoHitCounter.Services;

namespace AutoHitCounter.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IMemoryService _memoryService;
        private readonly GameModuleFactory _gameModuleFactory;
        private IGameModule? _currentModule;
        
        
        
        public MainViewModel(IMemoryService memoryService, GameModuleFactory gameModuleFactory)
        {
            _memoryService = memoryService;
            _gameModuleFactory = gameModuleFactory;
            Games.Add(new Game { GameName = "Elden Ring", ProcessName = "eldenring" });
        }

        #region Commands

        

        #endregion

        #region Properties

        public ObservableCollection<Game> Games { get; } = new();

        private Game _selectedGame;
        public Game SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (SetProperty(ref _selectedGame, value))
                {
                    SwapModule();
                }
            }
        }
        
        private int _hitCount;
        public int HitCount
        {
            get => _hitCount;
            set => SetProperty(ref _hitCount, value);
        }
        
        #endregion
        
        
        private void SwapModule()
        {
            // _currentModule?.StopGameTick();

            if (_selectedGame == null) return;

            _memoryService.StartAutoAttach(_selectedGame.ProcessName);
            _currentModule = _gameModuleFactory.CreateModule(_selectedGame);
            _currentModule.OnHit += count => HitCount = count;
            // _currentModule.OnBossKilled += () => AdvanceSplit();
            // _currentModule.StartGameTick();
        }

    }
    
    
}