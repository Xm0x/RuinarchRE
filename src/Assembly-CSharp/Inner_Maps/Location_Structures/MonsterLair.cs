using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class MonsterLair : NaturalStructure
{
	public MonsterLair(Region location)
		: base(STRUCTURE_TYPE.MONSTER_LAIR, location)
	{
		AddStructureTag(STRUCTURE_TAG.Dangerous);
		AddStructureTag(STRUCTURE_TAG.Treasure);
		AddStructureTag(STRUCTURE_TAG.Monster_Spawner);
	}

	public MonsterLair(Region location, SaveDataNaturalStructure data)
		: base(location, data)
	{
		AddStructureTag(STRUCTURE_TAG.Dangerous);
		AddStructureTag(STRUCTURE_TAG.Treasure);
		AddStructureTag(STRUCTURE_TAG.Monster_Spawner);
	}

	public FACILITY_TYPE GetMostNeededValidFacility()
	{
		return FACILITY_TYPE.NONE;
	}

	public List<LocationGridTile> GetUnoccupiedFurnitureSpotsThatCanProvide(FACILITY_TYPE type)
	{
		return null;
	}

	public bool HasFacilityDeficit()
	{
		return false;
	}

	public override void CenterOnStructure()
	{
		if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != base.region.innerMap)
		{
			InnerMapManager.Instance.HideAreaMap();
		}
		if (!base.region.innerMap.isShowing)
		{
			InnerMapManager.Instance.ShowInnerMap(base.region);
		}
		if (base.occupiedArea != null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(base.occupiedArea.gridTileComponent.centerGridTile.centeredWorldLocation);
		}
	}

	public override void ShowSelectorOnStructure()
	{
	}

	public override bool TryGetMinimapColorForTileInStructure(LocationGridTile p_tile, out Color p_color)
	{
		p_color = GameUtilities.StructureMinimapColor;
		return true;
	}
}
