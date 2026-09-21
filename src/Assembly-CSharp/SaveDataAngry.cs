using System.Collections.Generic;
using Traits;

public class SaveDataAngry : SaveDataTrait
{
	public List<string> characterIDs;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Angry angry = trait as Angry;
		characterIDs = SaveUtilities.ConvertSavableListToIDs(angry.responsibleCharactersStack);
	}
}
