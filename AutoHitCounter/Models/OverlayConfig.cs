// 

namespace AutoHitCounter.Models;

public class OverlayConfig(bool showAttempts, int prevSplits, int nextSplits, bool showDiff, bool showPb, bool showIgt, int overlayWidth, int overlayHeight, bool  showProgress)
{
    public bool ShowAttempts { get; } = showAttempts;
    public int PrevSplits { get; } = prevSplits;
    public int NextSplits { get; } = nextSplits;
    public bool ShowDiff { get; } = showDiff;
    public bool ShowPb { get; } = showPb;
    public bool ShowIgt { get; } = showIgt;
    public int OverlayWidth { get; } = overlayWidth;
    public int OverlayHeight { get; } = overlayHeight;
    public bool ShowProgress { get; } = showProgress;
}