using System.Collections.Generic;
using Traits;

public class SaveDataOverheating : SaveDataTrait
{
	public List<string> excludedStructuresInSeekingShelter;

	public string currentShelterStructure;

	public bool isPlayerSource;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Overheating overheating = trait as Overheating;
		excludedStructuresInSeekingShelter = SaveUtilities.ConvertSavableListToIDs(overheating.excludedStructuresInSeekingShelter);
		if (overheating.currentShelterStructure != null)
		{
			currentShelterStructure = overheating.currentShelterStructure.persistentID;
		}
		isPlayerSource = overheating.isPlayerSource;
	}
}
