//

using AutoHitCounter.Interfaces;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Services;

public class SettingsCustomGamesStore : ICustomGamesStore
{
    public string Raw
    {
        get => SettingsManager.Default.CustomGames;
        set => SettingsManager.Default.CustomGames = value;
    }

    public void Save() => SettingsManager.Default.Save();
}
