//

using System.ComponentModel;

namespace AutoHitCounter.Enums;

public enum NotesDisplayMode
{
    [Description("Off")]
    Off = 0,

    [Description("All Splits")]
    AllSplits = 1,

    [Description("Current Split")]
    CurrentSplit = 2,
}
