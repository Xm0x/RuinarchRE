using System.Collections.Generic;
using Traits;

public class SaveDataPyromaniac : SaveDataTrait
{
	public List<string> seenBurningSources;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Pyromaniac pyromaniac = trait as Pyromaniac;
		seenBurningSources = new List<string>();
		for (int i = 0; i < pyromaniac.seenBurningSources.Count; i++)
		{
			BurningSource burningSource = pyromaniac.seenBurningSources[i];
			if (burningSource != null)
			{
				seenBurningSources.Add(burningSource.persistentID);
			}
		}
	}
}
