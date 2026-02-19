// 

using System.Collections.Generic;

namespace AutoHitCounter.Models;

public class OverlayState
{
    public List<OverlaySplit> Splits { get; set; } = new List<OverlaySplit>();
    public int TotalHits { get; set; }
    public int TotalPb { get; set; }
    public string InGameTime { get; set; }
    public bool IsRunComplete { get; set; }
}

public class OverlaySplit
{
    public string Name { get; set; }
    public int Hits { get; set; }
    public int Pb { get; set; }
    public int Diff { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsParent { get; set; }
}