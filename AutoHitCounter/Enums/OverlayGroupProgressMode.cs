//

using System.ComponentModel;

namespace AutoHitCounter.Enums;

public enum OverlayGroupProgressMode
{
    [Description("Off")]
    Hidden = 0,

    [Description("Active group")]
    CurrentGroupOnly = 1,

    [Description("All groups")]
    AllGroups = 2,
}
