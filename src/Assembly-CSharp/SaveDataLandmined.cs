using System.Collections.Generic;
using Traits;

public class SaveDataLandmined : SaveDataTrait
{
	public List<string> awareCharacterIDs;

	public GameDate dateAdded;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Landmined landmined = trait as Landmined;
		awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(landmined.awareCharacters);
		dateAdded = landmined.dateAdded;
	}
}
