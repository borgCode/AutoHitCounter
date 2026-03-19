using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using static AutoHitCounter.Converters.CssColorSpaces;

namespace AutoHitCounter.Converters;


internal static class CssColorParser
{


    internal static Color? TryParseColor(string str)
    {
        str = str.Trim();
        if (string.IsNullOrEmpty(str)) return null;

        // light-dark()
        if (str.StartsWith("light-dark(", StringComparison.OrdinalIgnoreCase))
            return TryParseLightDark(str);

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
            var result = ColorConverter.ConvertFromString(str);
            if (result is Color c) return c;
        }
        catch (FormatException) { }

        if (str.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseRgbDirect(str.Substring(4, str.Length - 5).Trim(), hasAlpha: false);

        if (str.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseRgbDirect(str.Substring(5, str.Length - 6).Trim(), hasAlpha: true);

        if (str.StartsWith("hsl(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseHslDirect(str.Substring(4, str.Length - 5).Trim(), hasAlpha: false);

        if (str.StartsWith("hsla(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseHslDirect(str.Substring(5, str.Length - 6).Trim(), hasAlpha: true);

        if (str.StartsWith("hwb(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseHwbDirect(str.Substring(4, str.Length - 5).Trim());

        if (str.StartsWith("lab(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseLabDirect(str.Substring(4, str.Length - 5).Trim());

        if (str.StartsWith("lch(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseLchDirect(str.Substring(4, str.Length - 5).Trim());

        if (str.StartsWith("oklab(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseOklabDirect(str.Substring(6, str.Length - 7).Trim());

        if (str.StartsWith("oklch(", StringComparison.OrdinalIgnoreCase) && str.EndsWith(")"))
            return TryParseOklchDirect(str.Substring(6, str.Length - 7).Trim());

        return null;
    }

    internal static bool IsDarkMode { get; set; } = true;
    
    // light-dark()
    private static Color? TryParseLightDark(string str)
    {
        var trimmed = str.TrimEnd();
        if (!trimmed.EndsWith(")")) return null;
        var inner = trimmed.Substring(11, trimmed.Length - 12).Trim();

        var commaIdx = FindTopLevelComma(inner);
        if (commaIdx < 0) return null;

        var lightStr = inner.Substring(0, commaIdx).Trim();
        var darkStr = inner.Substring(commaIdx + 1).Trim();

        return IsDarkMode ? TryParseColor(darkStr) : TryParseColor(lightStr);
    }
    
    // rgb
    private static Color? TryParseRgbDirect(string inner, bool hasAlpha)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = SplitChannels(inner);
        if (parts.Count < 3) return null;

        if (!TryParseNoneOrNumber(parts[0], out var r)) return null;
        if (!TryParseNoneOrNumber(parts[1], out var g)) return null;
        if (!TryParseNoneOrNumber(parts[2], out var b)) return null;
        
        if (hasAlpha && parts.Count == 4 && alpha.Value >= 1.0)
        {
            if (!TryParseNoneOrNumber(parts[3], out var a)) return null;
            alpha = Clamp01(a);
        }

        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r), ClampToByte(g), ClampToByte(b));
    }

    // hsl
    private static Color? TryParseHslDirect(string inner, bool hasAlpha)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = SplitChannels(inner);
        if (parts.Count < 3) return null;

        if (!TryParseHue(parts[0], out var h)) return null;
        if (!TryParsePercentOrNone(parts[1], out var s)) return null;
        if (!TryParsePercentOrNone(parts[2], out var l)) return null;

        if (hasAlpha && parts.Count == 4 && alpha.Value >= 1.0)
        {
            if (!TryParseNoneOrNumber(parts[3], out var a)) return null;
            alpha = Clamp01(a);
        }

        var (r, g, b) = HslToRgb(h, s / 100.0, l / 100.0);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
    }

    // hwb(h w% b% [/ a])
    private static Color? TryParseHwbDirect(string inner)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = SplitChannels(inner);
        if (parts.Count != 3) return null;

        if (!TryParseHue(parts[0], out var h)) return null;
        if (!TryParsePercentOrNone(parts[1], out var w)) return null;
        if (!TryParsePercentOrNone(parts[2], out var b)) return null;

        var (r, g, bv) = HwbToRgb(h, w / 100.0, b / 100.0);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(bv * 255));
    }

    // lab(l a b [/ alpha])
    private static Color? TryParseLabDirect(string inner)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = SplitChannels(inner);
        if (parts.Count != 3) return null;

        if (!TryParsePercentOrNone(parts[0], out var l)) return null;
        if (!TryParseNoneOrNumber(parts[1], out var a)) return null;
        if (!TryParseNoneOrNumber(parts[2], out var b)) return null;

        var (r, g, bv) = LabToRgb(l, a, b);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(bv * 255));
    }

    // lch(l c h [/ alpha])
    private static Color? TryParseLchDirect(string inner)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = SplitChannels(inner);
        if (parts.Count != 3) return null;

        if (!TryParsePercentOrNone(parts[0], out var l)) return null;
        if (!TryParsePercentOrNone(parts[1], out var c)) return null;
        if (!TryParseHue(parts[2], out var h)) return null;

        var (r, g, b) = LchToRgb(l, c, h);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
    }

    // oklab(l a b [/ alpha])
    private static Color? TryParseOklabDirect(string inner)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = SplitChannels(inner);
        if (parts.Count != 3) return null;

        if (!TryParsePercentOrNone(parts[0], out var l)) return null;
        if (!TryParseNoneOrNumber(parts[1], out var a)) return null;
        if (!TryParseNoneOrNumber(parts[2], out var b)) return null;

        if (parts[0].Contains("%")) l /= 100.0;

        var (r, g, bv) = OklabToRgb(l, a, b);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
            ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(bv * 255));
    }

    // oklch(l c h [/ alpha])
    private static Color? TryParseOklchDirect(string inner)
    {
        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = SplitChannels(inner);
        if (parts.Count != 3) return null;

        if (!TryParsePercentOrNone(parts[0], out var l)) return null;
        if (!TryParsePercentOrNone(parts[1], out var c)) return null;
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

        var alpha = SplitAlpha(ref inner);
        if (!alpha.HasValue) return null;

        var parts = inner.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4) return null;

        var space = parts[0].ToLowerInvariant();
        if (!TryParseNoneOrNumber(parts[1], out var c1)) return null;
        if (!TryParseNoneOrNumber(parts[2], out var c2)) return null;
        if (!TryParseNoneOrNumber(parts[3], out var c3)) return null;

        double[] xyz;
        switch (space)
        {
            case "srgb":
                return Color.FromArgb(ClampToByte(alpha.Value * 255),
                    ClampToByte(c1 * 255), ClampToByte(c2 * 255), ClampToByte(c3 * 255));
            case "display-p3":   xyz = DisplayP3ToXyzD65(c1, c2, c3);   break;
            case "a98-rgb":      xyz = A98RgbToXyzD65(c1, c2, c3);      break;
            case "prophoto-rgb": xyz = ProphotoRgbToXyzD65(c1, c2, c3); break;
            case "rec2020":      xyz = Rec2020ToXyzD65(c1, c2, c3);     break;
            case "xyz":
            case "xyz-d65":      xyz = new[] { c1, c2, c3 };            break;
            case "xyz-d50":      xyz = XyzD50ToXyzD65(c1, c2, c3);      break;
            default: return null;
        }

        var (r, g, b) = XyzD65ToSrgb(xyz[0], xyz[1], xyz[2]);
        return Color.FromArgb(ClampToByte(alpha.Value * 255),
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

        var channels1 = ExtractChannelStrings(colorStr1);
        var channels2 = ExtractChannelStrings(colorStr2);

        var c1 = TryParseColor(colorStr1);
        var c2 = TryParseColor(colorStr2);
        if (c1 == null || c2 == null) return null;

        return MixColors(c1.Value, c2.Value, percent1.Value, percent2.Value, colorSpace,
            colorStr1, colorStr2, channels1, channels2);
    }
    
    private static List<string> ExtractChannelStrings(string colorStr)
    {
        var parenIdx = colorStr.IndexOf('(');
        if (parenIdx < 0) return null;
        var closeIdx = colorStr.LastIndexOf(')');
        if (closeIdx < 0) return null;

        var inner = colorStr.Substring(parenIdx + 1, closeIdx - parenIdx - 1).Trim();

        var slashIdx = FindTopLevelSlash(inner);
        if (slashIdx >= 0) inner = inner.Substring(0, slashIdx).Trim();

        return SplitChannels(inner);
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

    private static Color MixColors(Color c1, Color c2, double p1, double p2, string colorSpace,
        string colorStr1 = null, string colorStr2 = null,
        List<string> channels1 = null, List<string> channels2 = null)
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
                var (h1, s1, l1) = GetRawChannels(colorStr1 ?? "", channels1,
                    () => RgbToHsl(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0));
                var (h2, s2, l2) = GetRawChannels(colorStr2 ?? "", channels2,
                    () => RgbToHsl(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0));
                ApplyNone(channels1, channels2, ref h1, ref s1, ref l1, ref h2, ref s2, ref l2);
                var (r, g, b) = HslToRgb(InterpolateHue(h1, h2, w1, w2), s1 * w1 + s2 * w2, l1 * w1 + l2 * w2);
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "hwb":
            {
                var (h1, w1v, b1) = GetRawChannels(colorStr1 ?? "", channels1,
                    () => RgbToHwb(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0));
                var (h2, w2v, b2) = GetRawChannels(colorStr2 ?? "", channels2,
                    () => RgbToHwb(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0));
                ApplyNone(channels1, channels2, ref h1, ref w1v, ref b1, ref h2, ref w2v, ref b2);
                var (r, g, b) = HwbToRgb(InterpolateHue(h1, h2, w1, w2), w1v * w1 + w2v * w2, b1 * w1 + b2 * w2);
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "lab":
            {
                var (l1, a1, b1) = GetRawChannels(colorStr1 ?? "", channels1,
                    () => RgbToLab(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0));
                var (l2, a2, b2) = GetRawChannels(colorStr2 ?? "", channels2,
                    () => RgbToLab(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0));
                ApplyNone(channels1, channels2, ref l1, ref a1, ref b1, ref l2, ref a2, ref b2);
                var (r, g, b) = LabToRgb(l1 * w1 + l2 * w2, a1 * w1 + a2 * w2, b1 * w1 + b2 * w2);
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "oklab":
            {
                var (l1, a1, b1) = GetRawChannels(colorStr1 ?? "", channels1,
                    () => RgbToOklab(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0));
                var (l2, a2, b2) = GetRawChannels(colorStr2 ?? "", channels2,
                    () => RgbToOklab(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0));
                ApplyNone(channels1, channels2, ref l1, ref a1, ref b1, ref l2, ref a2, ref b2);
                var (r, g, b) = OklabToRgb(l1 * w1 + l2 * w2, a1 * w1 + a2 * w2, b1 * w1 + b2 * w2);
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "oklch":
            {
                var (l1, c1v, h1) = GetRawChannels(colorStr1 ?? "", channels1,
                    () => RgbToOklch(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0));
                var (l2, c2v, h2) = GetRawChannels(colorStr2 ?? "", channels2,
                    () => RgbToOklch(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0));
                ApplyNone(channels1, channels2, ref l1, ref c1v, ref h1, ref l2, ref c2v, ref h2);
                var (r, g, b) = OklchToRgb(l1 * w1 + l2 * w2, c1v * w1 + c2v * w2, InterpolateHue(h1, h2, w1, w2));
                return Color.FromArgb(ClampToByte(alpha * 255),
                    ClampToByte(r * 255), ClampToByte(g * 255), ClampToByte(b * 255));
            }
            case "lch":
            {
                var (l1, c1v, h1) = GetRawChannels(colorStr1 ?? "", channels1,
                    () => RgbToLch(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0));
                var (l2, c2v, h2) = GetRawChannels(colorStr2 ?? "", channels2,
                    () => RgbToLch(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0));
                ApplyNone(channels1, channels2, ref l1, ref c1v, ref h1, ref l2, ref c2v, ref h2);
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
    
    private static (double, double, double) GetRawChannels(string colorStr,
        List<string> channels, Func<(double, double, double)> fallback)
    {
        if (channels == null || channels.Count < 3) return fallback();

        if (!TryParsePercentOrNone(channels[0], out var ch0)) return fallback();
        if (!TryParsePercentOrNone(channels[1], out var ch1)) return fallback();

        double ch2;
        if (!TryParseHue(channels[2], out ch2))
            if (!TryParsePercentOrNone(channels[2], out ch2)) return fallback();
        
        if (channels[0].Contains("%"))
        {
            var fn = colorStr.Contains("oklch") || colorStr.Contains("oklab") ? "ok" : "";
            if (fn == "ok") ch0 /= 100.0;
        }

        if (channels[1].Contains("%") && (colorStr.StartsWith("oklch", StringComparison.OrdinalIgnoreCase)))
            ch1 = ch1 / 100.0 * 0.4;

        return (ch0, ch1, ch2);
    }
    
    private static void ApplyNone(List<string> ch1, List<string> ch2,
        ref double v1a, ref double v1b, ref double v1c,
        ref double v2a, ref double v2b, ref double v2c)
    {
        if (ch1 == null || ch2 == null) return;

        var orig1a = v1a; var orig1b = v1b; var orig1c = v1c;
        var orig2a = v2a; var orig2b = v2b; var orig2c = v2c;

        if (ch1.Count > 0 && IsNone(ch1[0])) v1a = orig2a;
        if (ch1.Count > 1 && IsNone(ch1[1])) v1b = orig2b;
        if (ch1.Count > 2 && IsNone(ch1[2])) v1c = orig2c;

        if (ch2.Count > 0 && IsNone(ch2[0])) v2a = orig1a;
        if (ch2.Count > 1 && IsNone(ch2[1])) v2b = orig1b;
        if (ch2.Count > 2 && IsNone(ch2[2])) v2c = orig1c;
    }

    private static bool IsNone(string s)
        => s.Equals("none", StringComparison.OrdinalIgnoreCase);
    
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

        var alphaPart = inner.Substring(slashIdx + 1).Trim();
        inner = inner.Substring(0, slashIdx).Trim();

        if (alphaPart.EndsWith("%"))
        {
            if (!double.TryParse(alphaPart.TrimEnd('%'), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var pct)) return null;
            return Clamp01(pct / 100.0);
        }

        if (!double.TryParse(alphaPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var alpha))
            return null;
        return Clamp01(alpha);
    }

    private static List<string> SplitChannels(string inner)
    {
        if (FindTopLevelComma(inner) >= 0)
        {
            var result = new List<string>();
            var depth = 0;
            var start = 0;
            for (var i = 0; i <= inner.Length; i++)
            {
                var ch = i < inner.Length ? inner[i] : ',';
                if (ch == '(') depth++;
                else if (ch == ')') depth--;
                else if (ch == ',' && depth == 0)
                {
                    result.Add(inner.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }
            return result;
        }
        return SplitTopLevelSpaces(inner);
    }

    private static bool TryParseNoneOrNumber(string s, out double result)
    {
        if (s.Equals("none", StringComparison.OrdinalIgnoreCase)) { result = 0; return true; }
        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParsePercentOrNone(string s, out double result)
    {
        if (s.Equals("none", StringComparison.OrdinalIgnoreCase)) { result = 0; return true; }
        if (s.EndsWith("%"))
            return double.TryParse(s.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }
    
    private static bool TryParseHue(string part, out double degrees)
    {
        degrees = 0;
        if (part.Equals("none", StringComparison.OrdinalIgnoreCase)) return true;

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