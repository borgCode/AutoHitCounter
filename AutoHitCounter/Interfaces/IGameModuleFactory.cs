//

using System.Collections.Generic;
using AutoHitCounter.Enums;
using AutoHitCounter.Models;

namespace AutoHitCounter.Interfaces;

public interface IGameModuleFactory
{
    List<Game> GetRegisteredGames();
    Dictionary<uint, string> GetEventsForGame(GameTitle title);
    IGameModule CreateModule(Game game, Dictionary<uint, (string Name, int Required, int Hit)> events, IHitRulesProvider rules);
}
