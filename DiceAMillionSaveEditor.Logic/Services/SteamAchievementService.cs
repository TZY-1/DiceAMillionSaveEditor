using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.Logic.Services;

public class SteamAchievementService : ISteamAchievementService
{
    private const string GameAppId = "3430340";
    private readonly HttpClient _httpClient;

    public SteamAchievementService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<IEnumerable<SteamAchievement>> FetchAchievementsAsync(string profileUrl)
    {
        var statsUrl = BuildStatsXmlUrl(profileUrl);
        var results = new List<SteamAchievement>();
        var response = await _httpClient.GetStringAsync(statsUrl);
        
        var doc = XDocument.Parse(response);

        var errorElement = doc.Root?.Element("error");
        if (errorElement != null)
        {
            throw new InvalidOperationException($"Steam API error: {errorElement.Value}");
        }

        var achievements = doc.Descendants("achievement");

        foreach (var ach in achievements)
        {
            var isClosed = ach.Attribute("closed")?.Value == "1";
            var name = ach.Element("name")?.Value ?? string.Empty;
            var apiname = ach.Element("apiname")?.Value ?? string.Empty;
            var desc = ach.Element("description")?.Value ?? string.Empty;

            results.Add(new SteamAchievement
            {
                Name = name,
                ApiName = apiname,
                Description = desc,
                Unlocked = isClosed
            });
        }

        return results;
    }

    /// <summary>
    /// Builds the full Steam stats XML URL from a user's profile URL.
    /// Accepts formats like:
    ///   https://steamcommunity.com/id/Lilpoint/
    ///   https://steamcommunity.com/profiles/76561198056746886/
    ///   https://steamcommunity.com/id/Lilpoint
    /// And also the full stats URL (passed through unchanged).
    /// </summary>
    private static string BuildStatsXmlUrl(string profileUrl)
    {
        var url = profileUrl.Trim();

        // If it already contains /stats/, assume it's a full stats URL
        if (url.Contains("/stats/", StringComparison.OrdinalIgnoreCase))
        {
            // Ensure ?xml=1 is appended
            if (!url.Contains("xml=1", StringComparison.OrdinalIgnoreCase))
            {
                url += url.Contains('?') ? "&xml=1" : "?xml=1";
            }
            return url;
        }

        // Strip trailing slash for consistent building
        url = url.TrimEnd('/');

        // Build: {profileUrl}/stats/{appId}/?xml=1
        return $"{url}/stats/{GameAppId}/?xml=1";
    }
}
