// 

namespace AutoHitCounter.Models;

public class OverlayConfig
{
    // Layout
    public bool ShowAttempts { get; set; }
    public int PrevSplits { get; set; }
    public int NextSplits { get; set; }
    public bool ShowDiff { get; set; }
    public bool ShowPb { get; set; }
    public bool ShowIgt { get; set; }
    public int OverlayWidth { get; set; }
    public int OverlayHeight { get; set; }
    public bool ShowProgress { get; set; }
    public bool ShowFooterTotals { get; set; }
    public bool TableMode { get; set; }
    public double BackgroundOpacity { get; set; }

    // Font
    public string FontFamily { get; set; }
    public int FontSize { get; set; }
    public int RowHeight { get; set; }
    public bool FontBold { get; set; }
    public bool FontItalic { get; set; }
    public bool FontUnderline { get; set; }
    public string IgtFontFamily { get; set; }
    public int IgtFontSize { get; set; }

    // Colors
    public string HeaderTextColor { get; set; }
    public string AttemptsZeroColor { get; set; }
    public string AttemptsActiveColor { get; set; }
    public string SplitNameColor { get; set; }
    public string GroupNameColor { get; set; }
    
    public string HitsActiveColor { get; set; }
    public string PbColor { get; set; }
    public string DiffPosColor { get; set; }
    public string DiffNegColor { get; set; }
    public string DiffZeroColor { get; set; }
    public string RowHitColor { get; set; }
    public string RowClearedColor { get; set; }
    public string AlternatingRows { get; set; }
    public string CurrentSplitColor { get; set; }
    public string CurrentSplitBorderColor { get; set; }
    public string CurrentSplitHitColor { get; set; }
    public string CurrentSplitHitBorderColor { get; set; }
    public string IgtColor { get; set; }
    public string RunCompleteBannerColor { get; set; }
    public string SplitNameOnHitColor { get; set; }
    public string SplitNameOnHitlessColor { get; set; }
    public string HitsCurrentColor { get; set; }
    public string HitsClearedColor { get; set; }
    public string HeaderFontFamily { get; set; }
    public int HeaderFontSize { get; set; }
}