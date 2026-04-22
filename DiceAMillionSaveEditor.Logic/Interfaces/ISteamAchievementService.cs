using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiceAMillionSaveEditor.Logic.Interfaces;

public class SteamAchievement
{
    public string Name { get; set; } = string.Empty;
    public string ApiName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Unlocked { get; set; }
}

public interface ISteamAchievementService
{
    Task<IEnumerable<SteamAchievement>> FetchAchievementsAsync(string profileUrl);
}
