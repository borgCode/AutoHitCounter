using System;
using System.Collections.Generic;
using System.Globalization;

namespace AutoHitCounter.Converters;

internal static class CssCalcEvaluator
{
    internal static double? EvaluateChannel(string token, Dictionary<string, double> vars)
    {
        token = token.Trim();
        if (string.IsNullOrEmpty(token)) return null;

        if (token.Equals("none", StringComparison.OrdinalIgnoreCase)) return 0;

        if (vars.TryGetValue(token.ToLowerInvariant(), out var varVal)) return varVal;

        if (token.EndsWith("%"))
        {
            if (double.TryParse(token.TrimEnd('%'), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var pct)) return pct;
        }

        if (TryParseHueToken(token, out var hue)) return hue;

        if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
            return num;

        if (token.StartsWith("calc(", StringComparison.OrdinalIgnoreCase) && token.EndsWith(")"))
            return EvaluateCalcExpression(token.Substring(5, token.Length - 6), vars);

        return null;
    }

    private static bool TryParseHueToken(string token, out double degrees)
    {
        degrees = 0;
        if (token.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
        {
            if (!double.TryParse(token.Substring(0, token.Length - 3), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out degrees)) return false;
            degrees = ((degrees % 360) + 360) % 360;
            return true;
        }
        if (token.EndsWith("rad", StringComparison.OrdinalIgnoreCase))
        {
            if (!double.TryParse(token.Substring(0, token.Length - 3), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var rad)) return false;
            degrees = ((rad * (180.0 / Math.PI) % 360) + 360) % 360;
            return true;
        }
        if (token.EndsWith("turn", StringComparison.OrdinalIgnoreCase))
        {
            if (!double.TryParse(token.Substring(0, token.Length - 4), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var turn)) return false;
            degrees = ((turn * 360.0 % 360) + 360) % 360;
            return true;
        }
        if (token.EndsWith("grad", StringComparison.OrdinalIgnoreCase))
        {
            if (!double.TryParse(token.Substring(0, token.Length - 4), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var grad)) return false;
            degrees = ((grad * 0.9 % 360) + 360) % 360;
            return true;
        }
        return false;
    }

    private static double? EvaluateCalcExpression(string expr, Dictionary<string, double> vars)
    {
        var tokens = Tokenize(expr, vars);
        if (tokens == null) return null;
        var pos = 0;
        var result = ParseAddSub(tokens, ref pos);
        return pos == tokens.Count ? result : null;
    }
    private static List<object> Tokenize(string expr, Dictionary<string, double> vars)
    {
        var tokens = new List<object>();
        var i = 0;
        expr = expr.Trim();

        while (i < expr.Length)
        {
            var ch = expr[i];

            if (char.IsWhiteSpace(ch)) { i++; continue; }

            if (ch == '(' || ch == ')' || ch == '*' || ch == '/')
            {
                tokens.Add(ch);
                i++;
                continue;
            }
            if ((ch == '+' || ch == '-') &&
                (tokens.Count == 0 || tokens[tokens.Count - 1] is char c && (c == '(' || c == '*' || c == '/')))
            {
            }
            else if (ch == '+' || ch == '-')
            {
                tokens.Add(ch);
                i++;
                continue;
            }

            if (char.IsLetter(ch))
            {
                var vstart = i;
                while (i < expr.Length && (char.IsLetterOrDigit(expr[i]) || expr[i] == '-')) i++;
                var name = expr.Substring(vstart, i - vstart).ToLowerInvariant();
                
                if (name == "none") { tokens.Add(0.0); continue; }

                if (vars.TryGetValue(name, out var varVal)) { tokens.Add(varVal); continue; }
                return null; 
            }

            var start = i;
            if (ch == '+' || ch == '-') i++;
            while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.' || expr[i] == 'e' || expr[i] == 'E'
                   || ((expr[i] == '+' || expr[i] == '-') && i > 0 && (expr[i - 1] == 'e' || expr[i - 1] == 'E'))))
                i++;

            var numStr = expr.Substring(start, i - start);
            while (i < expr.Length && char.IsLetter(expr[i])) i++;

            if (double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            {
                tokens.Add(val);
                continue;
            }

            return null;
        }

        return tokens;
    }

    private static double? ParseAddSub(List<object> tokens, ref int pos)
    {
        var left = ParseMulDiv(tokens, ref pos);
        if (!left.HasValue) return null;

        while (pos < tokens.Count && tokens[pos] is char op && (op == '+' || op == '-'))
        {
            pos++;
            var right = ParseMulDiv(tokens, ref pos);
            if (!right.HasValue) return null;
            left = op == '+' ? left.Value + right.Value : left.Value - right.Value;
        }
        return left;
    }

    private static double? ParseMulDiv(List<object> tokens, ref int pos)
    {
        var left = ParsePrimary(tokens, ref pos);
        if (!left.HasValue) return null;

        while (pos < tokens.Count && tokens[pos] is char op && (op == '*' || op == '/'))
        {
            pos++;
            var right = ParsePrimary(tokens, ref pos);
            if (!right.HasValue) return null;
            if (op == '/' && right.Value == 0) return null;
            left = op == '*' ? left.Value * right.Value : left.Value / right.Value;
        }
        return left;
    }

    private static double? ParsePrimary(List<object> tokens, ref int pos)
    {
        if (pos < tokens.Count && tokens[pos] is char p && p == '(')
        {
            pos++;
            var val = ParseAddSub(tokens, ref pos);
            if (!val.HasValue) return null;
            if (pos < tokens.Count && tokens[pos] is char cp && cp == ')') pos++;
            return val;
        }
        if (pos < tokens.Count && tokens[pos] is double d)
        {
            pos++;
            return d;
        }

        return null;
    }
}