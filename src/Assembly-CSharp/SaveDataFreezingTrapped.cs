using System.Collections.Generic;
using Traits;

public class SaveDataFreezingTrapped : SaveDataTrait
{
	public List<string> awareCharacterIDs;

	public GameDate dateAdded;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		FreezingTrapped freezingTrapped = trait as FreezingTrapped;
		awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(freezingTrapped.awareCharacters);
		dateAdded = freezingTrapped.dateAdded;
	}
}
