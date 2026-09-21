using Traits;

public class SaveDataWet : SaveDataTrait
{
	public bool isPlayerSource;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Wet wet = trait as Wet;
		isPlayerSource = wet.isPlayerSource;
	}
}
