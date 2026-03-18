using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using static AutoHitCounter.Converters.CssColorSpaces;

namespace AutoHitCounter.Converters;

internal static class CssColorParser
{
    private static readonly CssParser AngleSharp = new CssParser();


    internal static Color? TryParseColor(string str)
    {
        str = str.Trim();
        if (string.IsNullOrEmpty(str)) return null;

        // color-mix()
        if (str.StartsWith("color-mix(", StringComparison.OrdinalIgnoreCase))
            return TryParseColorMix(str);

        // color()
        if (str.StartsWith("color(", StringComparison.OrdinalIgnoreCase))
            return TryParseColorFunction(str);

        // Relative color syntax
        if (ContainsFromKeyword(str))
            return TryParseRelativeColor(str);

        // #RRGGBBAA 
        if (str.StartsWith("#") && str.Length == 9)
        {
            if (byte.TryParse(str.Substring(1, 2), NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(str.Substring(3, 2), NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(str.Substring(5, 2), NumberStyles.HexNumber, null, out var b) &&
                byte.TryParse(str.Substring(7, 2), NumberStyles.HexNumber, null, out var a))
                return Color.FromArgb(a, r, g, b);
            return null;
        }

        // WPF ColorConverter
        try
        {
            var result = System.Windows.Media.ColorConverter.ConvertFromString(str);
            if (result is Color c) return c;
        }
        catch (FormatException) { }

        // hwb/oklab/oklch 
        if (str.StartsWith("hwb(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
        {
            var result = TryParseHwbDirect(str.Substring(4, str.Length - 5).Trim());
            if (result.HasValue) return result;
        }

        if (str.StartsWith("oklab(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
        {
            var result = TryParseOklabDirect(str.Substring(6, str.Length - 7).Trim());
            if (result.HasValue) return result;
        }

        if (str.StartsWith("oklch(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
        {
            var result = TryParseOklchDirect(str.Substring(6, str.Length - 7).Trim());
            if (result.HasValue) return result;
        }
        
        return TryParseWithAngleSharp(str);
    }

    private static Color? TryParseWithAngleSharp(string colorStr)
    {
        try
        {
            var sheet = AngleSharp.ParseStyleSheet($"*{{color:{colorStr}}}");
            if (sheet.Rules.Length == 0) return null;
            if (sheet.Rules[0] is not ICssStyleRule rule) return null;
            var value = rule.Style.GetPropertyValue("color");
            if (string.IsNullOrEmpty(value)) return null;
            return TryParseNormalizedRgb(value);
        }
        catch (Exception) { return null; }
    }

    // normalize formats to rgb() or rgba() on output
    private static Color? TryParseNormalizedRgb(string value)
    {
        value = value.Trim();

        if (value.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase) && value.EndsWith(")"))
        {
            var inner = value.Substring(5, value.Length - 6);
            var parts = inner.Split(',');
            if (parts.Length == 4
                && byte.TryParse(parts[0].Trim(), out var r)
                && byte.TryParse(parts[1].Trim(), out var g)
                && byte.TryParse(parts[2].Trim(), out var b)
                && double.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
                return Color.FromArgb(ClampToByte(a * 255), r, g, b);
        }

        if (value.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && value.EndsWith(")"))
        {
            var inner = value.Substring(4, value.Length - 5);
            var parts = inner.Split(',');
            if (parts.Length == 3
                && byte.TryParse(parts[0].Trim(), out var r)
                && byte.TryParse(parts[1].Trim(), out var g)
                && byte.TryParse(parts[2].Trim(), out var b))
                return Color.FromRgb(r, g, b);
        }

        return null;
    }
    
    // hwb(h w% b% [/ a])
    private static Color? TryParseHwbDirect(string inner)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = inner.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return null;

        if (!TryParseHue(parts[0], out var h)) return null;
        if (!TryParsePercentOrNumber(parts[1], out var w)) return null;
        if (!TryParsePercentOrNumber(parts[2], out var b)) return null;

        w = parts[1].Contains("%") ? w / 100.0 : w;
        b = parts[2].Contains("%") ? b / 100.0 : b;

        var (r, g, bv) = HwbToRgb(h, w, b);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(bv * 255));
    }

    // oklab(l a b [/ alpha])
    private static Color? TryParseOklabDirect(string inner)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = inner.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return null;

        if (!TryParsePercentOrNumber(parts[0], out var l)) return null;
        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var a)) return null;
        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var bv)) return null;

        if (parts[0].Contains("%")) l /= 100.0;

        var (r, g, b) = OklabToRgb(l, a, bv);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
    }

    // oklch(l c h [/ alpha])
    private static Color? TryParseOklchDirect(string inner)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = inner.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return null;

        if (!TryParsePercentOrNumber(parts[0], out var l)) return null;
        if (!TryParsePercentOrNumber(parts[1], out var c)) return null;
        if (!TryParseHue(parts[2], out var h)) return null;

        if (parts[0].Contains("%")) l /= 100.0;
        if (parts[1].Contains("%")) c = c / 100.0 * 0.4;

        var (r, g, b) = OklchToRgb(l, c, h);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
    }
    
    private static Color? TryParseColorFunction(string str)
    {
        var inner = str.Substring(6, str.Length - 7).Trim();

        var slashIdx = inner.IndexOf('/');
        double alpha = 1.0;
        if (slashIdx >= 0)
        {
            if (!double.TryParse(inner.Substring(slashIdx + 1).Trim(), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out alpha)) return null;
            alpha = Clamp01(alpha);
            inner = inner.Substring(0, slashIdx).Trim();
        }

        var parts = inner.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4) return null;

        var space = parts[0].ToLowerInvariant();
        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var c1)) return null;
        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var c2)) return null;
        if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var c3)) return null;

        double[] xyz;
        switch (space)
        {
            case "srgb":
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(c1 * 255), ClampToByte(c2 * 255), ClampToByte(c3 * 255));
            case "display-p3":  xyz = DisplayP3ToXyzD65(c1, c2, c3); break;
            case "a98-rgb":     xyz = A98RgbToXyzD65(c1, c2, c3);    break;
            case "prophoto-rgb":xyz = ProphotoRgbToXyzD65(c1, c2, c3); break;
            case "rec2020":     xyz = Rec2020ToXyzD65(c1, c2, c3);   break;
            case "xyz":
            case "xyz-d65":     xyz = new[] { c1, c2, c3 };          break;
            case "xyz-d50":     xyz = XyzD50ToXyzD65(c1, c2, c3);    break;
            default: return null;
        }

        var (r, g, b) = XyzD65ToSrgb(xyz[0], xyz[1], xyz[2]);
        return Color.FromArgb(ClampToByte(alpha * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
    }
    
    private static Color? TryParseColorMix(string str)
    {
        var trimmed = str.TrimEnd();
        if (!trimmed.EndsWith(")")) return null;
        var inner = trimmed.Substring(10, trimmed.Length - 11).Trim();

        var firstComma = inner.IndexOf(',');
        if (firstComma < 0) return null;

        var spacePart = inner.Substring(0, firstComma).Trim();
        if (!spacePart.StartsWith("in ", StringComparison.OrdinalIgnoreCase)) return null;
        var colorSpace = spacePart.Substring(3).Trim().ToLowerInvariant();

        var rest = inner.Substring(firstComma + 1).Trim();
        
        var separatorIdx = FindTopLevelComma(rest);
        if (separatorIdx < 0) return null;

        ParseColorAndPercent(rest.Substring(0, separatorIdx).Trim(), out var colorStr1, out var percent1);
        ParseColorAndPercent(rest.Substring(separatorIdx + 1).Trim(), out var colorStr2, out var percent2);

        if (!percent1.HasValue && !percent2.HasValue) { percent1 = 0.5; percent2 = 0.5; }
        else if (!percent1.HasValue) percent1 = 1.0 - percent2.Value;
        else if (!percent2.HasValue) percent2 = 1.0 - percent1.Value;

        var c1 = TryParseColor(colorStr1);
        var c2 = TryParseColor(colorStr2);
        if (c1 == null || c2 == null) return null;

        return MixColors(c1.Value, c2.Value, percent1.Value, percent2.Value, colorSpace);
    }

    private static void ParseColorAndPercent(string part, out string colorStr, out double? percent)
    {
        percent = null;
        var lastSpace = part.LastIndexOf(' ');
        if (lastSpace >= 0)
        {
            var last = part.Substring(lastSpace + 1).Trim();
            if (last.EndsWith("%") && double.TryParse(last.TrimEnd('%'), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var pct))
            {
                percent = Clamp01(pct / 100.0);
                colorStr = part.Substring(0, lastSpace).Trim();
                return;
            }
        }
        colorStr = part;
    }

    private static Color MixColors(Color c1, Color c2, double p1, double p2, string colorSpace)
    {
        var total = p1 + p2;
        if (total <= 0) return c1;
        var w1 = p1 / total;
        var w2 = p2 / total;
        var alpha = Clamp01(c1.A / 255.0 * w1 + c2.A / 255.0 * w2);

        switch (colorSpace)
        {
            case "hsl":
            {
                var (h1, s1, l1) = RgbToHsl(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0);
                var (h2, s2, l2) = RgbToHsl(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0);
                var (r, g, b) = HslToRgb(InterpolateHue(h1, h2, w1, w2), s1 * w1 + s2 * w2, l1 * w1 + l2 * w2);
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "hwb":
            {
                var (h1, w1v, b1) = RgbToHwb(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0);
                var (h2, w2v, b2) = RgbToHwb(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0);
                var (r, g, b) = HwbToRgb(InterpolateHue(h1, h2, w1, w2), w1v * w1 + w2v * w2, b1 * w1 + b2 * w2);
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "lab":
            {
                var (l1, a1, b1) = RgbToLab(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0);
                var (l2, a2, b2) = RgbToLab(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0);
                var (r, g, b) = LabToRgb(l1 * w1 + l2 * w2, a1 * w1 + a2 * w2, b1 * w1 + b2 * w2);
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "oklab":
            {
                var (l1, a1, b1) = RgbToOklab(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0);
                var (l2, a2, b2) = RgbToOklab(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0);
                var (r, g, b) = OklabToRgb(l1 * w1 + l2 * w2, a1 * w1 + a2 * w2, b1 * w1 + b2 * w2);
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "oklch":
            {
                var (l1, c1v, h1) = RgbToOklch(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0);
                var (l2, c2v, h2) = RgbToOklch(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0);
                var (r, g, b) = OklchToRgb(l1 * w1 + l2 * w2, c1v * w1 + c2v * w2, InterpolateHue(h1, h2, w1, w2));
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "lch":
            {
                var (l1, c1v, h1) = RgbToLch(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0);
                var (l2, c2v, h2) = RgbToLch(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0);
                var (r, g, b) = LchToRgb(l1 * w1 + l2 * w2, c1v * w1 + c2v * w2, InterpolateHue(h1, h2, w1, w2));
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            default: 
                return Color.FromArgb(
                    ClampToByte(alpha * 255),
                    ClampToByte((c1.R / 255.0 * w1 + c2.R / 255.0 * w2) * 255),
                    ClampToByte((c1.G / 255.0 * w1 + c2.G / 255.0 * w2) * 255),
                    ClampToByte((c1.B / 255.0 * w1 + c2.B / 255.0 * w2) * 255));
        }
    }
    
    private static bool ContainsFromKeyword(string str)
    {
        var parenIdx = str.IndexOf('(');
        if (parenIdx < 0) return false;
        var parts = str.Substring(parenIdx + 1).TrimStart()
            .Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 1 && parts[0].Equals("from", StringComparison.OrdinalIgnoreCase);
    }

    private static Color? TryParseRelativeColor(string str)
    {
        var parenIdx = str.IndexOf('(');
        if (parenIdx < 0) return null;

        var fn = str.Substring(0, parenIdx).ToLowerInvariant().Trim();
        
        var depth = 0;
        var closeIdx = -1;
        for (var i = parenIdx; i < str.Length; i++)
        {
            if (str[i] == '(') depth++;
            else if (str[i] == ')') { depth--; if (depth == 0) { closeIdx = i; break; } }
        }
        if (closeIdx < 0) return null;

        var inner = str.Substring(parenIdx + 1, closeIdx - parenIdx - 1).Trim();
        if (!inner.StartsWith("from ", StringComparison.OrdinalIgnoreCase)) return null;
        inner = inner.Substring(5).Trim();

        var originStr = ExtractOriginColor(inner, out var remainder);
        if (originStr == null) return null;

        var origin = TryParseColor(originStr);
        if (origin == null) return null;

        var vars = GetChannelVariables(fn, origin.Value);
        if (vars == null) return null;

        var slashIdx = FindTopLevelSlash(remainder.Trim());
        string channelsPart, alphaPart;
        if (slashIdx >= 0)
        {
            channelsPart = remainder.Substring(0, slashIdx).Trim();
            alphaPart = remainder.Substring(slashIdx + 1).Trim();
        }
        else
        {
            channelsPart = remainder.Trim();
            alphaPart = null;
        }

        var channelTokens = SplitTopLevelSpaces(channelsPart);
        if (channelTokens.Count < 3) return null;

        var ch1 = CssCalcEvaluator.EvaluateChannel(channelTokens[0], vars);
        var ch2 = CssCalcEvaluator.EvaluateChannel(channelTokens[1], vars);
        var ch3 = CssCalcEvaluator.EvaluateChannel(channelTokens[2], vars);
        if (!ch1.HasValue || !ch2.HasValue || !ch3.HasValue) return null;

        double alpha;
        if (alphaPart != null)
        {
            var av = CssCalcEvaluator.EvaluateChannel(alphaPart, vars);
            if (!av.HasValue) return null;
            alpha = Clamp01(av.Value);
        }
        else
        {
            alpha = origin.Value.A / 255.0;
        }

        return ResolveRelativeColor(fn, ch1.Value, ch2.Value, ch3.Value, alpha);
    }

    private static string ExtractOriginColor(string str, out string remainder)
    {
        remainder = string.Empty;
        var parenIdx = str.IndexOf('(');
        var firstSpace = str.IndexOf(' ');

        if (parenIdx >= 0 && (firstSpace < 0 || parenIdx < firstSpace))
        {
            var depth = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == '(') depth++;
                else if (str[i] == ')') { depth--; if (depth == 0) { remainder = str.Substring(i + 1).Trim(); return str.Substring(0, i + 1); } }
            }
            return null;
        }

        if (firstSpace < 0) return null;
        remainder = str.Substring(firstSpace + 1).Trim();
        return str.Substring(0, firstSpace);
    }
    
    private static Dictionary<string, double> GetChannelVariables(string fn, Color origin)
    {
        var r = origin.R / 255.0;
        var g = origin.G / 255.0;
        var b = origin.B / 255.0;
        var a = origin.A / 255.0;

        switch (fn)
        {
            case "rgb": case "rgba":
                return new Dictionary<string, double> { ["r"] = origin.R, ["g"] = origin.G, ["b"] = origin.B, ["alpha"] = a };
            case "hsl": case "hsla":
            {
                var (h, s, l) = RgbToHsl(r, g, b);
                return new Dictionary<string, double> { ["h"] = h, ["s"] = s * 100, ["l"] = l * 100, ["alpha"] = a };
            }
            case "hwb":
            {
                var (h, w, bv) = RgbToHwb(r, g, b);
                return new Dictionary<string, double> { ["h"] = h, ["w"] = w * 100, ["b"] = bv * 100, ["alpha"] = a };
            }
            case "lab":
            {
                var (l, la, lb) = RgbToLab(r, g, b);
                return new Dictionary<string, double> { ["l"] = l, ["a"] = la, ["b"] = lb, ["alpha"] = a };
            }
            case "lch":
            {
                var (l, c, h) = RgbToLch(r, g, b);
                return new Dictionary<string, double> { ["l"] = l, ["c"] = c, ["h"] = h, ["alpha"] = a };
            }
            case "oklab":
            {
                var (l, la, lb) = RgbToOklab(r, g, b);
                return new Dictionary<string, double> { ["l"] = l, ["a"] = la, ["b"] = lb, ["alpha"] = a };
            }
            case "oklch":
            {
                var (l, c, h) = RgbToOklch(r, g, b);
                return new Dictionary<string, double> { ["l"] = l, ["c"] = c, ["h"] = h, ["alpha"] = a };
            }
            default: return null;
        }
    }

    private static Color? ResolveRelativeColor(string fn, double c1, double c2, double c3, double alpha)
    {
        var a = ClampToByte(alpha * 255);
        switch (fn)
        {
            case "rgb": case "rgba":
                return Color.FromArgb(a, ClampToByte(c1), ClampToByte(c2), ClampToByte(c3));
            case "hsl": case "hsla":
            {
                var (r, g, b) = HslToRgb(c1, c2 / 100.0, c3 / 100.0);
                return Color.FromArgb(a, ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "hwb":
            {
                var (r, g, b) = HwbToRgb(c1, c2 / 100.0, c3 / 100.0);
                return Color.FromArgb(a, ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "lab":
            {
                var (r, g, b) = LabToRgb(c1, c2, c3);
                return Color.FromArgb(a, ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "lch":
            {
                var (r, g, b) = LchToRgb(c1, c2, c3);
                return Color.FromArgb(a, ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "oklab":
            {
                var (r, g, b) = OklabToRgb(c1, c2, c3);
                return Color.FromArgb(a, ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "oklch":
            {
                var (r, g, b) = OklchToRgb(c1, c2, c3);
                return Color.FromArgb(a, ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            default: return null;
        }
    }

    private static double? SplitAlpha(ref string inner)
    {
        var slashIdx = FindTopLevelSlash(inner);
        if (slashIdx < 0) return 1.0;
        if (!double.TryParse(inner.Substring(slashIdx + 1).Trim(), NumberStyles.Float,
                CultureInfo.InvariantCulture, out var alpha)) return null;
        inner = inner.Substring(0, slashIdx).Trim();
        return Clamp01(alpha);
    }

    private static bool TryParsePercentOrNumber(string s, out double result)
    {
        if (s.EndsWith("%"))
            return double.TryParse(s.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseHue(string part, out double degrees)
    {
        degrees = 0;
        if (part.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
        {
            if (!double.TryParse(part.Substring(0, part.Length - 3), NumberStyles.Float, CultureInfo.InvariantCulture, out degrees)) return false;
        }
        else if (part.EndsWith("rad", StringComparison.OrdinalIgnoreCase))
        {
            if (!double.TryParse(part.Substring(0, part.Length - 3), NumberStyles.Float, CultureInfo.InvariantCulture, out var rad)) return false;
            degrees = rad * (180.0 / Math.PI);
        }
        else if (part.EndsWith("turn", StringComparison.OrdinalIgnoreCase))
        {
            if (!double.TryParse(part.Substring(0, part.Length - 4), NumberStyles.Float, CultureInfo.InvariantCulture, out var turn)) return false;
            degrees = turn * 360.0;
        }
        else if (part.EndsWith("grad", StringComparison.OrdinalIgnoreCase))
        {
            if (!double.TryParse(part.Substring(0, part.Length - 4), NumberStyles.Float, CultureInfo.InvariantCulture, out var grad)) return false;
            degrees = grad * 0.9;
        }
        else
        {
            if (!double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out degrees)) return false;
        }
        degrees = ((degrees % 360) + 360) % 360;
        return true;
    }

    private static int FindTopLevelComma(string str)
    {
        var depth = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] == '(') depth++;
            else if (str[i] == ')') depth--;
            else if (str[i] == ',' && depth == 0) return i;
        }
        return -1;
    }

    private static int FindTopLevelSlash(string str)
    {
        var depth = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] == '(') depth++;
            else if (str[i] == ')') depth--;
            else if (str[i] == '/' && depth == 0) return i;
        }
        return -1;
    }

    private static List<string> SplitTopLevelSpaces(string str)
    {
        var result = new List<string>();
        var depth = 0;
        var start = 0;
        for (var i = 0; i <= str.Length; i++)
        {
            var ch = i < str.Length ? str[i] : ' ';
            if (ch == '(') depth++;
            else if (ch == ')') depth--;
            else if (char.IsWhiteSpace(ch) && depth == 0)
            {
                if (i > start) result.Add(str.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        return result;
    }
}