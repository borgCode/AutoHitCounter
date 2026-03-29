// 

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using AutoHitCounter.Properties;

namespace AutoHitCounter.Utilities;

public static class EventLoader
{
    public static Dictionary<uint, List<string>> GetEvents(string resourceName)
    {
        var dict = new Dictionary<uint, List<string>>();
        string csvData = Resources.ResourceManager.GetString(resourceName);
        if (string.IsNullOrWhiteSpace(csvData)) return dict;

        var regex = new Regex(@"^""?([^""]*)""?\s*,\s*(\d+)$", RegexOptions.Compiled);

        using var reader = new StringReader(csvData);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            var match = regex.Match(line.Trim());
            if (match.Success && uint.TryParse(match.Groups[2].Value, out uint id))
            {
                if (!dict.TryGetValue(id, out var list))
                {
                    list = new List<string>();
                    dict[id] = list;
                }

                list.Add(match.Groups[1].Value);
            }
        }

        return dict;
    }
}