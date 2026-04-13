//

namespace AutoHitCounter.Interfaces;

public interface ICustomGamesStore
{
    string Raw { get; set; }
    void Save();
}
