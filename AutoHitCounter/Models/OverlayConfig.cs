// 

namespace AutoHitCounter.Models;

public class OverlayConfig
{
    // General Layout //
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


    // Title Section //
    public bool ShowTitle { get; set; }
    public string TitleText { get; set; }
    public string TitleColor { get; set; }
    public string TitleFontFamily { get; set; }
    public int TitleFontSize { get; set; }


    // Header Section //

    public string HeaderTextColor { get; set; }
    public string HeaderFontFamily { get; set; }
    public int HeaderFontSize { get; set; }


    // Attempts Counter //

    public string AttemptsZeroColor { get; set; }
    public string AttemptsActiveColor { get; set; }


    // Splits Section //

    // Split Fonts
    public string FontFamily { get; set; }
    public int FontSize { get; set; }
    public int RowHeight { get; set; }
    public bool FontBold { get; set; }
    public bool FontItalic { get; set; }
    public bool FontUnderline { get; set; }

    // Names

    public string GroupNameColor { get; set; }
    public string SplitNameColor { get; set; }
    public string SplitNameOnHitColor { get; set; }
    public string SplitNameOnHitlessColor { get; set; }

    // Current Row

    public string CurrentSplitColor { get; set; }
    public string CurrentSplitHitColor { get; set; }

    //  Current Row Borders (Flashing Indicators)
    public string CurrentSplitBorderColor { get; set; }
    public string CurrentSplitHitBorderColor { get; set; }

    // PB Highlight (below text)

    public bool ShowDpbHighlight { get; set; }
    public string DpbHighlightColor { get; set; }


    // Previous Rows
    public string RowClearedColor { get; set; }
    public string RowHitColor { get; set; }

    // Row Alternation

    public string AlternatingRows { get; set; }

    // Hits Column

    public string HitsCurrentColor { get; set; }
    public string HitsActiveColor { get; set; }
    public string HitsClearedColor { get; set; }

    // Diff Column

    public string DiffPosColor { get; set; }
    public string DiffNegColor { get; set; }
    public string DiffZeroColor { get; set; }

    // PB Column

    public string PbColor { get; set; }
    public bool PbMatchesHit { get; set; }


    // Footer Section //

    public string FooterHitFontColor { get; set; }
    public string FooterHitsCurrentColor { get; set; }
    public string FooterPbFontColor { get; set; }
    public string FooterFontFamily { get; set; }
    public int FooterFontSize { get; set; }
    public string IgtColor { get; set; }
    public string IgtFontFamily { get; set; }
    public int IgtFontSize { get; set; }


    // Run Completion Message // 

    public bool ShowRunComplete { get; set; }
    public string RunCompleteText { get; set; }
    public string RunCompleteBannerColor { get; set; }
}