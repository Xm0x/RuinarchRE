using System.Collections.Generic;
using Traits;

public class SaveDataLycanphiliac : SaveDataTrait
{
	public List<string> knownLycans;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Lycanphiliac lycanphiliac = trait as Lycanphiliac;
		knownLycans = SaveUtilities.ConvertSavableListToIDs(lycanphiliac.knownLycans);
	}
}
