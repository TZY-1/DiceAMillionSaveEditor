namespace DiceAMillionSaveEditor.Logic.Rules;

public class DiceRule : BaseUnlockRule
{
    public override bool Matches(string propertyKey) => propertyKey.StartsWith("dicelist_");
}
