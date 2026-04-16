//

using System.ComponentModel;

namespace AutoHitCounter.Enums;

public enum OverlayGroupCollapseMode
{
    [Description("Collapse all inactive groups")]
    AllInactive = 0,

    [Description("Collapse only completed groups")]
    CollapseCompletedShowFuture = 1,

    [Description("Don't collapse groups")]
    None = 2,
}
