using System.Collections.Generic;
using Traits;

public class SaveDataHemophiliac : SaveDataTrait
{
	public List<string> knownVampires;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Hemophiliac hemophiliac = trait as Hemophiliac;
		knownVampires = SaveUtilities.ConvertSavableListToIDs(hemophiliac.knownVampires);
	}
}
