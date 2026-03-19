using System;
using System.Windows.Media;

namespace AutoHitCounter.Converters;


internal static class CssColorSpaces
{
    // HSL
    internal static (double h, double s, double l) RgbToHsl(double r, double g, double b)
    {
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var l = (max + min) / 2.0;
        if (max - min < 1e-10) return (0, 0, l);
        var d = max - min;
        var s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        double h;
        if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
        else if (max == g) h = (b - r) / d + 2;
        else h = (r - g) / d + 4;
        h *= 60;
        return (h, s, l);
    }

    internal static (double r, double g, double b) HslToRgb(double h, double s, double l)
    {
        if (s == 0) return (l, l, l);
        var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        var p = 2 * l - q;
        h /= 360.0;
        return (HueToRgb(p, q, h + 1.0 / 3),
                HueToRgb(p, q, h),
                HueToRgb(p, q, h - 1.0 / 3));
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2) return q;
        if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
        return p;
    }

    // HWB
    internal static (double h, double w, double b) RgbToHwb(double r, double g, double bv)
    {
        var (h, _, _) = RgbToHsl(r, g, bv);
        var w = Math.Min(r, Math.Min(g, bv));
        var b = 1 - Math.Max(r, Math.Max(g, bv));
        return (h, w, b);
    }

    internal static (double r, double g, double b) HwbToRgb(double h, double w, double b)
    {
        if (w + b >= 1) { var gray = w / (w + b); return (gray, gray, gray); }
        var (r, g, bv) = HslToRgb(h, 1, 0.5);
        r = r * (1 - w - b) + w;
        g = g * (1 - w - b) + w;
        bv = bv * (1 - w - b) + w;
        return (r, g, bv);
    }

    // Lab / LCH
    internal static (double l, double a, double b) RgbToLab(double r, double g, double b)
    {
        var (x, y, z) = SrgbToXyzD50(r, g, b);
        return XyzD50ToLab(x, y, z);
    }

    internal static (double r, double g, double b) LabToRgb(double l, double a, double b)
    {
        var (x, y, z) = LabToXyzD50(l, a, b);
        return XyzD50ToSrgb(x, y, z);
    }

    internal static (double l, double c, double h) RgbToLch(double r, double g, double b)
    {
        var (l, a, bv) = RgbToLab(r, g, b);
        var c = Math.Sqrt(a * a + bv * bv);
        var h = Math.Atan2(bv, a) * (180.0 / Math.PI);
        if (h < 0) h += 360;
        return (l, c, h);
    }

    internal static (double r, double g, double b) LchToRgb(double l, double c, double h)
    {
        var hRad = h * (Math.PI / 180.0);
        return LabToRgb(l, c * Math.Cos(hRad), c * Math.Sin(hRad));
    }

    // Oklab / Oklch

    internal static (double l, double a, double b) RgbToOklab(double r, double g, double b)
    {
        r = SrgbToLinear(r); g = SrgbToLinear(g); b = SrgbToLinear(b);

        var lms_l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b;
        var lms_m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b;
        var lms_s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b;

        lms_l = CubeRoot(lms_l); lms_m = CubeRoot(lms_m); lms_s = CubeRoot(lms_s);

        return (
            0.2104542553 * lms_l + 0.7936177850 * lms_m - 0.0040720468 * lms_s,
            1.9779984951 * lms_l - 2.4285922050 * lms_m + 0.4505937099 * lms_s,
            0.0259040371 * lms_l + 0.7827717662 * lms_m - 0.8086757660 * lms_s);
    }

    internal static (double r, double g, double b) OklabToRgb(double l, double a, double b)
    {
        var lms_l = l + 0.3963377774 * a + 0.2158037573 * b;
        var lms_m = l - 0.1055613458 * a - 0.0638541728 * b;
        var lms_s = l - 0.0894841775 * a - 1.2914855480 * b;

        lms_l = lms_l >= 0 ? lms_l * lms_l * lms_l : -((-lms_l) * (-lms_l) * (-lms_l));
        lms_m = lms_m >= 0 ? lms_m * lms_m * lms_m : -((-lms_m) * (-lms_m) * (-lms_m));
        lms_s = lms_s >= 0 ? lms_s * lms_s * lms_s : -((-lms_s) * (-lms_s) * (-lms_s));

        var r = +4.0767416621 * lms_l - 3.3077115913 * lms_m + 0.2309699292 * lms_s;
        var g = -1.2684380046 * lms_l + 2.6097574011 * lms_m - 0.3413193965 * lms_s;
        var bv = -0.0041960863 * lms_l - 0.7034186147 * lms_m + 1.7076147010 * lms_s;
        return (LinearToSrgb(r), LinearToSrgb(g), LinearToSrgb(bv));
    }

    internal static (double l, double c, double h) RgbToOklch(double r, double g, double b)
    {
        var (l, a, bv) = RgbToOklab(r, g, b);
        var c = Math.Sqrt(a * a + bv * bv);
        var h = Math.Atan2(bv, a) * (180.0 / Math.PI);
        if (h < 0) h += 360;
        return (l, c, h);
    }

    internal static (double r, double g, double b) OklchToRgb(double l, double c, double h)
    {
        var hRad = h * (Math.PI / 180.0);
        return OklabToRgb(l, c * Math.Cos(hRad), c * Math.Sin(hRad));
    }

    // XYZ / Lab interop
    private static (double x, double y, double z) SrgbToXyzD50(double r, double g, double b)
    {
        r = SrgbToLinear(r); g = SrgbToLinear(g); b = SrgbToLinear(b);
        var x65 = 0.4124564 * r + 0.3575761 * g + 0.1804375 * b;
        var y65 = 0.2126729 * r + 0.7151522 * g + 0.0721750 * b;
        var z65 = 0.0193339 * r + 0.1191920 * g + 0.9503041 * b;
        return XyzD65ToD50(x65, y65, z65);
    }

    private static (double r, double g, double b) XyzD50ToSrgb(double x, double y, double z)
    {
        var (x65, y65, z65) = XyzD50ToD65(x, y, z);
        var r =  3.2404542 * x65 - 1.5371385 * y65 - 0.4985314 * z65;
        var g = -0.9692660 * x65 + 1.8760108 * y65 + 0.0415560 * z65;
        var b =  0.0556434 * x65 - 0.2040259 * y65 + 1.0572252 * z65;
        return (LinearToSrgb(r), LinearToSrgb(g), LinearToSrgb(b));
    }

    private static (double l, double a, double b) XyzD50ToLab(double x, double y, double z)
    {
        x /= 0.3457 / 0.3585; y /= 1.0; z /= (1 - 0.3457 - 0.3585) / 0.3585;
        x = LabF(x); y = LabF(y); z = LabF(z);
        return (116 * y - 16, 500 * (x - y), 200 * (y - z));
    }

    private static (double x, double y, double z) LabToXyzD50(double l, double a, double b)
    {
        var fy = (l + 16) / 116;
        var fx = a / 500 + fy;
        var fz = fy - b / 200;
        var x = LabFInv(fx) * (0.3457 / 0.3585);
        var y = LabFInv(fy);
        var z = LabFInv(fz) * ((1 - 0.3457 - 0.3585) / 0.3585);
        return (x, y, z);
    }

    private static double LabF(double t)
    {
        const double delta = 6.0 / 29;
        return t > delta * delta * delta
            ? Math.Pow(t, 1.0 / 3)
            : t / (3 * delta * delta) + 4.0 / 29;
    }

    private static double LabFInv(double t)
    {
        const double delta = 6.0 / 29;
        return t > delta ? t * t * t : 3 * delta * delta * (t - 4.0 / 29);
    }

    // Wide gamut color spaces → XYZ D65
    internal static double[] DisplayP3ToXyzD65(double r, double g, double b)
    {
        r = SrgbToLinear(r); g = SrgbToLinear(g); b = SrgbToLinear(b);
        return new[]
        {
            0.4865709 * r + 0.2656677 * g + 0.1982173 * b,
            0.2289746 * r + 0.6917385 * g + 0.0792869 * b,
            0.0000000 * r + 0.0451134 * g + 1.0439444 * b
        };
    }

    internal static double[] A98RgbToXyzD65(double r, double g, double b)
    {
        r = Math.Pow(Math.Abs(r), 563.0 / 256) * Math.Sign(r);
        g = Math.Pow(Math.Abs(g), 563.0 / 256) * Math.Sign(g);
        b = Math.Pow(Math.Abs(b), 563.0 / 256) * Math.Sign(b);
        return new[]
        {
            0.5766690 * r + 0.1855582 * g + 0.1882286 * b,
            0.2973450 * r + 0.6273635 * g + 0.0752915 * b,
            0.0270314 * r + 0.0706869 * g + 0.9911085 * b
        };
    }

    internal static double[] ProphotoRgbToXyzD65(double r, double g, double b)
    {
        const double et2 = 16.0 / 512;
        r = r <= et2 ? r / 16 : Math.Pow(r, 1.8);
        g = g <= et2 ? g / 16 : Math.Pow(g, 1.8);
        b = b <= et2 ? b / 16 : Math.Pow(b, 1.8);
        var x50 = 0.7977604896 * r + 0.1351791870 * g + 0.0313106074 * b;
        var y50 = 0.2880711282 * r + 0.7118432178 * g + 0.0000856540 * b;
        var z50 = 0.0000000000 * r + 0.0000000000 * g + 0.8252080505 * b;
        var (x65, y65, z65) = XyzD50ToD65(x50, y50, z50);
        return new[] { x65, y65, z65 };
    }

    internal static double[] Rec2020ToXyzD65(double r, double g, double b)
    {
        const double alpha = 1.09929682680944;
        const double beta = 0.018053968510807;
        r = r < beta * 4.5 ? r / 4.5 : Math.Pow((r + alpha - 1) / alpha, 1 / 0.45);
        g = g < beta * 4.5 ? g / 4.5 : Math.Pow((g + alpha - 1) / alpha, 1 / 0.45);
        b = b < beta * 4.5 ? b / 4.5 : Math.Pow((b + alpha - 1) / alpha, 1 / 0.45);
        return new[]
        {
            0.6369580 * r + 0.1446169 * g + 0.1688810 * b,
            0.2627002 * r + 0.6779981 * g + 0.0593017 * b,
            0.0000000 * r + 0.0280727 * g + 1.0609851 * b
        };
    }

    internal static (double r, double g, double b) XyzD65ToSrgb(double x, double y, double z)
    {
        var r =  3.2404542 * x - 1.5371385 * y - 0.4985314 * z;
        var g = -0.9692660 * x + 1.8760108 * y + 0.0415560 * z;
        var b =  0.0556434 * x - 0.2040259 * y + 1.0572252 * z;
        return (LinearToSrgb(r), LinearToSrgb(g), LinearToSrgb(b));
    }

    internal static double[] XyzD50ToXyzD65(double x, double y, double z)
    {
        var (x65, y65, z65) = XyzD50ToD65(x, y, z);
        return new[] { x65, y65, z65 };
    }

    // D50 <-> D65 chromatic adaptation
    private static (double x, double y, double z) XyzD65ToD50(double x, double y, double z) => (
         1.0478112 * x + 0.0228866 * y - 0.0501270 * z,
         0.0295424 * x + 0.9904844 * y - 0.0170491 * z,
        -0.0092345 * x + 0.0150436 * y + 0.7521316 * z);

    private static (double x, double y, double z) XyzD50ToD65(double x, double y, double z) => (
         0.9554734527042182  * x - 0.02303948780868590 * y + 0.0632957294913511 * z,
        -0.0283697069632030  * x + 1.00996574890298700 * y + 0.0210413989224835 * z,
         0.0123140016883717  * x - 0.02050158987103560 * y + 1.3303659366080753 * z);

    // sRGB gamma
    private static double SrgbToLinear(double v)
        => v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);

    private static double LinearToSrgb(double v)
        => v <= 0.0031308 ? 12.92 * v : 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055;


    internal static double InterpolateHue(double h1, double h2, double w1, double w2)
    {
        var diff = h2 - h1;
        if (diff > 180) diff -= 360;
        if (diff < -180) diff += 360;
        return ((h1 + diff * w2) % 360 + 360) % 360;
    }

    internal static byte ClampToByte(double v)
        => (byte)Math.Max(0, Math.Min(255, (int)Math.Round(v)));

    internal static double Clamp01(double v)
        => Math.Max(0, Math.Min(1, v));

    private static double CubeRoot(double v)
        => v < 0 ? -Math.Pow(-v, 1.0 / 3) : Math.Pow(v, 1.0 / 3);
}