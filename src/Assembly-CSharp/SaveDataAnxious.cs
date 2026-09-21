using System.Collections.Generic;
using Traits;

public class SaveDataAnxious : SaveDataTrait
{
	public List<string> sourceOfAnxietyIDs;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Anxious anxious = trait as Anxious;
		sourceOfAnxietyIDs = new List<string>(anxious.sourceOfAnxietyIDs);
	}
}
