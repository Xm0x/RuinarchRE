using Traits;

public class SaveDataMalnourished : SaveDataTrait
{
	public int currentDeathDuration;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Malnourished malnourished = trait as Malnourished;
		currentDeathDuration = malnourished.currentDeathDuration;
	}
}
