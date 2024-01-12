namespace DynamicSQL.Parser;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class RegexExtensions
{
    public static IEnumerable<(string Value, Match? Match)> FindMatchesAndIntervals(this Regex regex, string input)
    {
        var lastIndex = 0;

        foreach (Match match in regex.Matches(input))
        {
            if (lastIndex < input.Length)
            {
                yield return (input.Substring(lastIndex, match.Index - lastIndex), null);
            }

            yield return (match.Value, match);

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < input.Length)
        {
            yield return (input.Substring(lastIndex), null);
        }
    }
}
