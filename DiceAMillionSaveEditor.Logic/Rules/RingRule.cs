namespace DiceAMillionSaveEditor.Logic.Rules;

public class RingRule : BaseUnlockRule
{
    public override bool Matches(string propertyKey) => propertyKey.StartsWith("ringlist_");
}
