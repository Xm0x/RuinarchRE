using System.Collections.Generic;
using Traits;

public class SaveDataFreezing : SaveDataTrait
{
	public List<string> excludedStructuresInSeekingShelter;

	public string currentShelterStructure;

	public bool isPlayerSource;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Freezing freezing = trait as Freezing;
		excludedStructuresInSeekingShelter = SaveUtilities.ConvertSavableListToIDs(freezing.excludedStructuresInSeekingShelter);
		if (freezing.currentShelterStructure != null)
		{
			currentShelterStructure = freezing.currentShelterStructure.persistentID;
		}
		isPlayerSource = freezing.isPlayerSource;
	}
}
