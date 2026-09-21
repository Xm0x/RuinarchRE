using System.Collections.Generic;
using Traits;

public class SaveDataVampire : SaveDataTrait
{
	public bool dislikedBeingVampire;

	public int numOfConvertedVillagers;

	public List<string> awareCharacters;

	public bool isInVampireBatForm;

	public bool isTraversingUnwalkableAsBat;

	public bool hasAlreadyBecomeVampireLord;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Vampire vampire = trait as Vampire;
		awareCharacters = SaveUtilities.ConvertSavableListToIDs(vampire.awareCharacters);
		dislikedBeingVampire = vampire.dislikedBeingVampire;
		numOfConvertedVillagers = vampire.numOfConvertedVillagers;
		isInVampireBatForm = vampire.isInVampireBatForm;
		isTraversingUnwalkableAsBat = vampire.isTraversingUnwalkableAsBat;
		hasAlreadyBecomeVampireLord = vampire.hasAlreadyBecomeVampireLord;
	}
}
