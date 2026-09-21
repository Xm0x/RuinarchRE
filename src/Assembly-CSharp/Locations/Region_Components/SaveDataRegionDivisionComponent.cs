using System;
using System.Collections.Generic;
using UtilityScripts;

namespace Locations.Region_Components;

[Serializable]
public class SaveDataRegionDivisionComponent : SaveData<BiomeDivisionComponent>
{
	public List<SaveDataRegionDivision> divisions { get; private set; }

	public override void Save(BiomeDivisionComponent data)
	{
		divisions = RuinarchListPool<SaveDataRegionDivision>.Claim();
		for (int i = 0; i < data.divisions.Count; i++)
		{
			SaveDataRegionDivision saveDataRegionDivision = new SaveDataRegionDivision();
			saveDataRegionDivision.Save(data.divisions[i]);
			divisions.Add(saveDataRegionDivision);
		}
	}

	public override BiomeDivisionComponent Load()
	{
		return new BiomeDivisionComponent(this);
	}

	public override void CleanUp()
	{
		if (divisions != null)
		{
			for (int i = 0; i < divisions.Count; i++)
			{
				divisions[i].CleanUp();
			}
			RuinarchListPool<SaveDataRegionDivision>.Release(divisions);
			divisions = null;
		}
	}
}
