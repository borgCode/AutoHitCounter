// 

namespace AutoHitCounter.Models;

public class SplitEntry
{
    public uint? EventId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public int PersonalBest { get; set; }

    public string Label => DisplayName ?? Name;
    public bool IsAuto => EventId.HasValue;
}