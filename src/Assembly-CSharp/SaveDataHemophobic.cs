using System.Collections.Generic;
using Traits;

public class SaveDataHemophobic : SaveDataTrait
{
	public List<string> knownVampires;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Hemophobic hemophobic = trait as Hemophobic;
		knownVampires = SaveUtilities.ConvertSavableListToIDs(hemophobic.knownVampires);
	}
}
