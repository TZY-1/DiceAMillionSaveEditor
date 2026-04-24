using DiceAMillionSaveEditor.Logic.Interfaces;
using System.Text.Json.Nodes;

namespace DiceAMillionSaveEditor.Logic.Rules;

public class PhoneStateRule : IAchievementRule
{
    public bool Matches(string propertyKey) => propertyKey == "jphonestate";

    public void Apply(SteamAchievement achievement, string propertyKey, string currentValue, JsonObject saveGameData, List<string> logs)
    {
        if (!double.TryParse(currentValue, out var value))
        {
            value = 0.0;
        }

        var targetState = GetTargetState(achievement.ApiName);
        if (targetState <= 0)
        {
            logs.Add($"[INFO] Achievement '{achievement.Name}' ({achievement.ApiName}): no phone-state mapping.");
            return;
        }

        if (value < targetState)
        {
            saveGameData[propertyKey] = (double)targetState;
            logs.Add($"[SUCCESS] Achievement '{achievement.Name}' ({achievement.ApiName}): {propertyKey} set to {targetState}.0.");
        }
        else
        {
            logs.Add($"[INFO] Achievement '{achievement.Name}' ({achievement.ApiName}): {propertyKey} already progressed.");
        }
    }

    private static int GetTargetState(string apiName)
    {
        return apiName.ToLowerInvariant() switch
        {
            "misc_piece1" => 2,
            "misc_piece2" => 3,
            _ => 0
        };
    }
}
