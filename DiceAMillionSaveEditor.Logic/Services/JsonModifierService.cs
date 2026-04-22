using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.Logic.Services;

public class JsonModifierService : IJsonModifier
{
    private readonly IAchievementMapper _mapper;
    private readonly IEnumerable<IAchievementRule> _rules;

    public JsonModifierService(IAchievementMapper mapper, IEnumerable<IAchievementRule> rules)
    {
        _mapper = mapper;
        _rules = rules;
    }

    public string ApplyAchievementsToJson(string json, IEnumerable<SteamAchievement> achievements, out List<string> logResult)
    {
        logResult = new List<string>();
        var rootNode = JsonNode.Parse(json)?.AsObject();
        
        if (rootNode == null)
        {
            logResult.Add("[ERROR] Failed to parse JSON.");
            return json;
        }

        foreach (var ach in achievements)
        {
            if (ach.Unlocked)
            {
                ProcessAchievement(ach, rootNode, logResult);
            }
        }

        return rootNode.ToJsonString();
    }

    private void ProcessAchievement(SteamAchievement achievement, JsonObject rootNode, List<string> logs)
    {
        var keyToFind = _mapper.MapApiNameToJsonKey(achievement.ApiName);
        
        if (string.IsNullOrEmpty(keyToFind))
        {
            logs.Add($"[WARNING] Achievement '{achievement.Name}' ({achievement.ApiName}): No mapping rule defined.");
            return;
        }

        if (!rootNode.ContainsKey(keyToFind))
        {
            logs.Add($"[ERROR] Achievement '{achievement.Name}' ({achievement.ApiName}): JSON key '{keyToFind}' not found.");
            return;
        }

        var valString = rootNode[keyToFind]?.ToString() ?? "";

        foreach (var rule in _rules)
        {
            if (rule.Matches(keyToFind))
            {
                rule.Apply(achievement, keyToFind, valString, rootNode, logs);
                break;
            }
        }
    }

    public Dictionary<string, object> DecodeJsonToDictionary(string json)
    {
        var result = new Dictionary<string, object>();
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Number)
                    result[prop.Name] = prop.Value.GetDouble();
                else if (prop.Value.ValueKind == JsonValueKind.String)
                    result[prop.Name] = prop.Value.GetString() ?? "";
                else if (prop.Value.ValueKind == JsonValueKind.True || prop.Value.ValueKind == JsonValueKind.False)
                    result[prop.Name] = prop.Value.GetBoolean();
                else
                    result[prop.Name] = prop.Value.GetRawText();
            }
        }
        return result;
    }

    public string EncodeDictionaryToJson(Dictionary<string, object> dictionary)
    {
        return JsonSerializer.Serialize(dictionary);
    }
}
