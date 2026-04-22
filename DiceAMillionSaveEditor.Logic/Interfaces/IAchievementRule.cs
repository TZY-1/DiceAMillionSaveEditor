using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace DiceAMillionSaveEditor.Logic.Interfaces;

public interface IAchievementRule
{
    bool Matches(string propertyKey);
    void Apply(SteamAchievement achievement, string propertyKey, string currentValue, JsonObject saveGameData, List<string> logs);
}
