// 

using System.Collections.Generic;

namespace AutoHitCounter.Models;

public class Profile
{
    public string Name { get; set; }
    public string GameName { get; set; }
    public List<SplitEntry> Splits { get; set; }
}