using System.Collections.Generic;
using System.Text.Json.Nodes;
using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.Logic.Rules;

public abstract class BaseUnlockRule : IAchievementRule
{
    public abstract bool Matches(string propertyKey);

    public virtual void Apply(SteamAchievement achievement, string propertyKey, string currentValue, JsonObject saveGameData, List<string> logs)
    {
        if (currentValue == "unseen" || currentValue == "locked" || currentValue == "new")
        {
            saveGameData[propertyKey] = "seen";
            logs.Add($"[SUCCESS] Achievement '{achievement.Name}' ({achievement.ApiName}): {propertyKey} set to 'seen'.");
        }
        else if (currentValue == "seen")
        {
            logs.Add($"[INFO] Achievement '{achievement.Name}' ({achievement.ApiName}): {propertyKey} already unlocked.");
        }
    }
}
