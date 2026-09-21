using System.Collections.Generic;
using Traits;

public class SaveDataInteresting : SaveDataTrait
{
	public List<string> charactersThatSaw;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Interesting interesting = trait as Interesting;
		charactersThatSaw = SaveUtilities.ConvertSavableListToIDs(interesting.charactersThatSaw);
	}
}
