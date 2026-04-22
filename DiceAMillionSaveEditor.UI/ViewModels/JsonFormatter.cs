using System.Text.Json;

namespace DiceAMillionSaveEditor.UI.ViewModels;

public sealed class JsonFormatter
{
    public string PrettyPrint(string json)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
