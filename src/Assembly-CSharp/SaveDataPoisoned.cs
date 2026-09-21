using System.Collections.Generic;
using Traits;

public class SaveDataPoisoned : SaveDataTrait
{
	public List<string> awareCharacterIDs;

	public bool isVenomous;

	public bool isPlayerSource;

	public GameDate destroyDate;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Poisoned poisoned = trait as Poisoned;
		awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(poisoned.awareCharacters);
		isVenomous = poisoned.isVenomous;
		isPlayerSource = poisoned.isPlayerSource;
		destroyDate = poisoned.destroyDate;
	}
}
