namespace DiceAMillionSaveEditor.Logic.Rules;

public class CardRule : BaseUnlockRule
{
    public override bool Matches(string propertyKey) => propertyKey.StartsWith("cardlist_");
}
