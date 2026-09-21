using Traits;

public class SaveDataPlagued : SaveDataTrait
{
	public int numberOfHoursPassed;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Plagued plagued = trait as Plagued;
		numberOfHoursPassed = plagued.numberOfHoursPassed;
	}
}
