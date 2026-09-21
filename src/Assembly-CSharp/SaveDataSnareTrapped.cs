using System.Collections.Generic;
using Traits;

public class SaveDataSnareTrapped : SaveDataTrait
{
	public List<string> awareCharacterIDs;

	public GameDate dateAdded;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		SnareTrapped snareTrapped = trait as SnareTrapped;
		awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(snareTrapped.awareCharacters);
		dateAdded = snareTrapped.dateAdded;
	}
}
