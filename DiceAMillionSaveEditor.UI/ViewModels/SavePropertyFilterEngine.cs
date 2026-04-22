using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiceAMillionSaveEditor.UI.ViewModels;

public sealed class SavePropertyFilterEngine
{
    public bool Matches(SavePropertyItem item, string searchFilter, IEnumerable<FilterRuleItem> rules)
    {
        return MatchesSearchFilter(item, searchFilter) && MatchesRuleFilters(item, rules);
    }

    private static bool MatchesSearchFilter(SavePropertyItem item, string searchFilter)
    {
        if (string.IsNullOrWhiteSpace(searchFilter))
        {
            return true;
        }

        return MatchesPattern(item.Key, searchFilter) || MatchesPattern(item.Value, searchFilter);
    }

    private static bool MatchesRuleFilters(SavePropertyItem item, IEnumerable<FilterRuleItem> rules)
    {
        var activeRules = rules.Where(r => !string.IsNullOrWhiteSpace(r.Pattern)).ToList();
        if (activeRules.Count == 0)
        {
            return true;
        }

        return activeRules.All(rule => MatchesRule(item, rule));
    }

    private static bool MatchesRule(SavePropertyItem item, FilterRuleItem rule)
    {
        var candidate = rule.Field.Equals("Value", StringComparison.OrdinalIgnoreCase)
            ? item.Value
            : item.Key;

        var pattern = rule.Pattern.Trim();
        if (pattern.Length == 0)
        {
            return true;
        }

        var matches = MatchesPattern(candidate, pattern);
        return rule.Operator == "!=" ? !matches : matches;
    }

    private static bool MatchesPattern(string input, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return true;
        }

        var trimmed = pattern.Trim();

        if (TryBuildRegexPattern(trimmed, out var regexPattern))
        {
            try
            {
                return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                return input.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
            }
        }

        return input.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryBuildRegexPattern(string pattern, out string regexPattern)
    {
        regexPattern = string.Empty;

        if (pattern.StartsWith("re:", StringComparison.OrdinalIgnoreCase))
        {
            var raw = pattern[3..].Trim();
            if (raw.Length == 0)
            {
                return false;
            }

            regexPattern = raw;
            return true;
        }

        if (pattern.Length >= 2 && pattern.StartsWith('/') && pattern.EndsWith('/'))
        {
            var raw = pattern[1..^1].Trim();
            if (raw.Length == 0)
            {
                return false;
            }

            regexPattern = raw;
            return true;
        }

        if (!pattern.Contains('*') && !pattern.Contains('?'))
        {
            return false;
        }

        var escaped = Regex.Escape(pattern);
        escaped = escaped.Replace("\\*", ".*").Replace("\\?", ".");
        regexPattern = $"^{escaped}$";
        return true;
    }
}
