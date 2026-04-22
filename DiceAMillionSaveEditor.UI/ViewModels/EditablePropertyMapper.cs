using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DiceAMillionSaveEditor.UI.ViewModels;

public sealed class EditablePropertyMapper
{
    private static readonly string[] StatusValueOptions = { "new", "unseen", "seen", "locked" };

    public IEnumerable<SavePropertyItem> CreateEditableItems(Dictionary<string, object> dictionary)
    {
        foreach (var kvp in dictionary.OrderBy(x => x.Key))
        {
            var value = kvp.Value.ToString() ?? "";
            yield return new SavePropertyItem
            {
                Key = kvp.Key,
                Value = value,
                ValueOptions = GetValueOptions(value)
            };
        }
    }

    public Dictionary<string, object> BuildTypedDictionary(IEnumerable<SavePropertyItem> editableProperties)
    {
        return editableProperties.ToDictionary(
            x => x.Key,
            x => ParseEditableValue(x.Value));
    }

    private static IList<string> GetValueOptions(string value)
    {
        if (StatusValueOptions.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            return StatusValueOptions;
        }

        return Array.Empty<string>();
    }

    private static object ParseEditableValue(string value)
    {
        var trimmed = value.Trim();

        if (bool.TryParse(trimmed, out var boolValue))
        {
            return boolValue;
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue))
        {
            return numericValue;
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out numericValue))
        {
            return numericValue;
        }

        return value;
    }
}
