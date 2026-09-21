using System.Collections.Generic;
using Traits;

public class SaveDataPyrophobic : SaveDataTrait
{
	public List<string> seenBurningSources;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Pyrophobic pyrophobic = trait as Pyrophobic;
		seenBurningSources = new List<string>();
		for (int i = 0; i < pyrophobic.seenBurningSources.Count; i++)
		{
			BurningSource burningSource = pyrophobic.seenBurningSources[i];
			if (burningSource != null)
			{
				seenBurningSources.Add(burningSource.persistentID);
			}
		}
	}
}
