namespace DiceAMillionSaveEditor.Logic.Rules;

public class BossRule : BaseUnlockRule
{
    public override bool Matches(string propertyKey) => propertyKey.StartsWith("bosslist_");
}
