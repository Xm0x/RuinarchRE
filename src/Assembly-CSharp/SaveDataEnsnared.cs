using Traits;

public class SaveDataEnsnared : SaveDataTrait
{
	public bool isPlayerSource;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Ensnared ensnared = trait as Ensnared;
		isPlayerSource = ensnared.isPlayerSource;
	}
}
