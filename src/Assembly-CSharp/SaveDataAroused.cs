using System.Collections.Generic;
using Traits;

public class SaveDataAroused : SaveDataTrait
{
	public List<string> arousedTargetIDs;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Aroused aroused = trait as Aroused;
		arousedTargetIDs = new List<string>(aroused.arousedTargetIDs);
	}
}
