using Traits;

public class SaveDataPrisoner : SaveDataTrait
{
	public string prisonerOfFaction;

	public string prisonerOfCharacter;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Prisoner prisoner = trait as Prisoner;
		if (prisoner.isFactionPrisoner)
		{
			prisonerOfFaction = prisoner.prisonerOfFaction.persistentID;
		}
		if (prisoner.isPersonalPrisoner)
		{
			prisonerOfCharacter = prisoner.prisonerOfCharacter.persistentID;
		}
	}
}
