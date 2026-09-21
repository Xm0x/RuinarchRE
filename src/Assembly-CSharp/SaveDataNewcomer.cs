using Traits;

public class SaveDataNewcomer : SaveDataTrait
{
	public string sharedOpinionID;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Newcomer newcomer = trait as Newcomer;
		sharedOpinionID = newcomer.opinionModifier.persistentID;
	}
}
