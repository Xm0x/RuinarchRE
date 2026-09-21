using System.Collections.Generic;
using Traits;

public class SaveDataCriminal : SaveDataTrait
{
	public List<string> alreadyWorriedCharacterIDs;

	public bool isImprisoned;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Criminal criminal = trait as Criminal;
		alreadyWorriedCharacterIDs = SaveUtilities.ConvertSavableListToIDs(criminal.charactersThatAreAlreadyWorried);
		isImprisoned = criminal.isImprisoned;
	}
}
