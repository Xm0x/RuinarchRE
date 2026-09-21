using System.Collections.Generic;
using UtilityScripts;

namespace Locations.Region_Components;

public class SaveDataRegionTileObjectsComponent : SaveData<RegionTileObjectsComponent>
{
	public List<STRUCTURE_TYPE> possibleStructureScrollChoices;

	public override void Save(RegionTileObjectsComponent data)
	{
		base.Save(data);
		possibleStructureScrollChoices = RuinarchListPool<STRUCTURE_TYPE>.Claim();
		possibleStructureScrollChoices.AddRange(data.possibleStructureScrollChoices);
	}

	public override RegionTileObjectsComponent Load()
	{
		return new RegionTileObjectsComponent(this);
	}

	public override void CleanUp()
	{
		RuinarchListPool<STRUCTURE_TYPE>.Release(possibleStructureScrollChoices);
		possibleStructureScrollChoices = null;
	}
}
