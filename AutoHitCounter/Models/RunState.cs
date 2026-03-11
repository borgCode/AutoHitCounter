// 

namespace AutoHitCounter.Models;

public class RunState
{
    public int CurrentSplitIndex { get; set; }
    public int[] HitCounts { get; set; }
    public bool IsRunComplete { get; set; }
    public long IgtMilliseconds { get; set; }
}