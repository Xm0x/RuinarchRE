using Traits;

public class SaveDataZapped : SaveDataTrait
{
	public bool isPlayerSource;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Zapped zapped = trait as Zapped;
		isPlayerSource = zapped.isPlayerSource;
	}
}
