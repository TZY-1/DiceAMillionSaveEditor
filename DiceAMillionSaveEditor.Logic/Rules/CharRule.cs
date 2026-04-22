namespace DiceAMillionSaveEditor.Logic.Rules;

public class CharRule : BaseUnlockRule
{
    public override bool Matches(string propertyKey) => propertyKey.StartsWith("charlist_");
}
