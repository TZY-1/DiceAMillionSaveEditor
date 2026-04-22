using DiceAMillionSaveEditor.Logic.Interfaces;

namespace DiceAMillionSaveEditor.Logic.Services;

public class AchievementMapper : IAchievementMapper
{
    public string MapApiNameToJsonKey(string apiName)
    {
        if (apiName.StartsWith("dice_"))
            return "dicelist_" + apiName.Substring(5);
        if (apiName.StartsWith("ring_"))
            return "ringlist_" + apiName.Substring(5);
        if (apiName.StartsWith("card_"))
            return "cardlist_" + apiName.Substring(5);
        if (apiName.StartsWith("char_"))
            return "charlist_" + apiName.Substring(5);
        if (apiName.StartsWith("challenge_"))
            return "challengelist_" + apiName.Substring(10);
        if (apiName.StartsWith("boss_"))
            return "bosslist_" + apiName.Substring(5);

        // Phone-related achievements use staged progress in jphonestate.
        if (apiName.Equals("misc_piece1", System.StringComparison.OrdinalIgnoreCase)
            || apiName.Equals("misc_piece2", System.StringComparison.OrdinalIgnoreCase)
            || apiName.Equals("misc_hollowcall", System.StringComparison.OrdinalIgnoreCase))
            return "jphonestate";
            
        return string.Empty; // Unknown mappings
    }
}
