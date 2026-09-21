using Traits;

public class SaveDataInvisible : SaveDataTrait
{
	public COMBAT_MODE originalCombatMode;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Invisible invisible = trait as Invisible;
		originalCombatMode = invisible.originalCombatMode;
	}
}
