using Traits;

public class SaveDataBurning : SaveDataTrait
{
	public string burningSourceID;

	public bool isPlayerSource;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Burning burning = trait as Burning;
		burningSourceID = burning.persistentID;
		isPlayerSource = burning.isPlayerSource;
	}
}
