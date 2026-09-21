using System.Collections.Generic;
using Traits;

public class SaveDataLycanphobic : SaveDataTrait
{
	public List<string> knownLycans;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Lycanphobic lycanphobic = trait as Lycanphobic;
		knownLycans = SaveUtilities.ConvertSavableListToIDs(lycanphobic.knownLycans);
	}
}
