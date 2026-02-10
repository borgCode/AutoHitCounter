// 

using System.Collections.Generic;
using AutoHitCounter.Models;

namespace AutoHitCounter.Interfaces;

public interface IProfileService
{
    List<Profile> GetProfiles(string gameName);
    void SaveProfile(Profile profile);
    void DeleteProfile(string gameName, string profileName);
}