// 

using System.Collections.ObjectModel;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;

namespace AutoHitCounter.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IMemoryService _memoryService;

        public MainViewModel(IMemoryService memoryService)
        {
            _memoryService = memoryService;
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
                    _memoryService.StartAutoAttach(_selectedGame.ProcessName);
                }
            }
        }

        #endregion
    }
    
    
}