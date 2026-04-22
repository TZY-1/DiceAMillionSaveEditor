using System.Collections.Generic;
using System.Text.Json.Nodes;
using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.Logic.Rules;

public class ChallengeRule : IAchievementRule
{
    public bool Matches(string propertyKey) => propertyKey.StartsWith("challengelist_");

    public void Apply(SteamAchievement achievement, string propertyKey, string currentValue, JsonObject saveGameData, List<string> logs)
    {
        if (currentValue == "0")
        {
            saveGameData[propertyKey] = 1.0;
            logs.Add($"[SUCCESS] Achievement '{achievement.Name}' ({achievement.ApiName}): {propertyKey} set to 1.0.");
        }
        else
        {
            logs.Add($"[INFO] Achievement '{achievement.Name}' ({achievement.ApiName}): {propertyKey} already completed.");
        }
    }
}
