using System.Collections.Generic;

namespace DiceAMillionSaveEditor.Logic.Interfaces;

public interface IJsonModifier
{
    string ApplyAchievementsToJson(string json, IEnumerable<SteamAchievement> achievements, out List<string> logResult);
    
    // For manual edits via DataGrid
    Dictionary<string, object> DecodeJsonToDictionary(string json);
    string EncodeDictionaryToJson(Dictionary<string, object> dictionary);
}
