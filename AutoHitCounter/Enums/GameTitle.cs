// 

using System.ComponentModel;

namespace AutoHitCounter.Enums;

public enum GameTitle
{
    [Description("Dark Souls Remastered")]
    DarkSoulsRemastered,
    
    [Description("Dark Souls 2")]
    DarkSouls2,

    [Description("Dark Souls 3")]
    DarkSouls3,

    [Description("Sekiro")]
    Sekiro,

    [Description("Elden Ring")]
    EldenRing,
    
    [Description("Nightreign")]
    Nightreign,

    [Description("Manual")]
    Manual,
}