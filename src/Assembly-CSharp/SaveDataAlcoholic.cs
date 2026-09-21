using Traits;

public class SaveDataAlcoholic : SaveDataTrait
{
	public bool hasDrankWithinTheDay;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Alcoholic alcoholic = trait as Alcoholic;
		hasDrankWithinTheDay = alcoholic.hasDrankWithinTheDay;
	}
}
