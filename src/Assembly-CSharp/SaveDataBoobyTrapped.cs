using System.Collections.Generic;
using Traits;

public class SaveDataBoobyTrapped : SaveDataTrait
{
	public List<string> awareCharacterIDs;

	public ELEMENTAL_TYPE elementalType;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		BoobyTrapped boobyTrapped = trait as BoobyTrapped;
		awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(boobyTrapped.awareCharacters);
		elementalType = boobyTrapped.elementalType;
	}
}
