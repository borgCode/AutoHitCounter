//

using System.Collections.Generic;
using AutoHitCounter.Models;

namespace AutoHitCounter.Interfaces;

public interface ICustomGameService
{
    IReadOnlyList<Game> Load();
    Game Add(string name);
    void Delete(string name);
    void Rename(string oldName, string newName);
}
