using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

public class AreaNeighbourComponent : AreaComponent
{
	public List<Area> neighbours { get; private set; }

	public List<Area> cardinalNeighbours { get; private set; }

	public AreaNeighbourComponent()
	{
		neighbours = new List<Area>();
		cardinalNeighbours = new List<Area>();
	}

	public void FindNeighbours(Area p_area, Area[,] gameBoard)
	{
		foreach (KeyValuePair<GridNeighbourDirection, Point> gridPossibleNeighbour in Utilities.gridPossibleNeighbours)
		{
			int num = p_area.areaData.xCoordinate + gridPossibleNeighbour.Value.X;
			int num2 = p_area.areaData.yCoordinate + gridPossibleNeighbour.Value.Y;
			if (num < 0 || num >= gameBoard.GetLength(0) || num2 < 0 || num2 >= gameBoard.GetLength(1))
			{
				continue;
			}
			Area area = gameBoard[num, num2];
			if (area != null)
			{
				neighbours.Add(area);
				if (gridPossibleNeighbour.Key.IsCardinalDirection())
				{
					cardinalNeighbours.Add(area);
				}
			}
		}
	}

	public bool IsAtEdgeOfMap()
	{
		return cardinalNeighbours.Count < 4;
	}

	public void AddNeighbour(Area p_area)
	{
		neighbours.Add(p_area);
	}

	public bool HasNeighbour(Area p_area)
	{
		return neighbours.Contains(p_area);
	}

	public bool HasNeighbourWithElevation(ELEVATION elevation)
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			if (neighbours[i].elevationType == elevation)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCardinalNeighbourWithElevation(ELEVATION elevation)
	{
		for (int i = 0; i < cardinalNeighbours.Count; i++)
		{
			if (cardinalNeighbours[i].elevationType == elevation)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCardinalNeighbourWithElevationThatIsNotReservedByOtherVillage(ELEVATION elevation, List<VillageSpot> p_villageSpots)
	{
		for (int i = 0; i < cardinalNeighbours.Count; i++)
		{
			Area area = cardinalNeighbours[i];
			if (!area.elevationComponent.HasElevation(elevation))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < p_villageSpots.Count; j++)
			{
				if (p_villageSpots[j].reservedAreas.Contains(area))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNeighbourWithFeature(string feature)
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			if (neighbours[i].featureComponent.HasFeature(feature))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOwnedSettlementNeighbour()
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			if (neighbours[i].HasSettlementWithFactionOwnerOnArea())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNeighbourSpecialStructure()
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			Area area = neighbours[i];
			if (area.primaryStructureInArea != null && area.primaryStructureInArea.structureType.IsSpecialStructure())
			{
				return true;
			}
		}
		return false;
	}

	private bool HasSettlementNeighbour()
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			if (neighbours[i].HasSettlementLocationType(LOCATION_TYPE.VILLAGE))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNextToVillage()
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			Area area = neighbours[i];
			if (area.region == base.owner.region && area.IsPartOfVillage())
			{
				return true;
			}
		}
		return false;
	}

	public Area GetRandomAdjacentHextileWithinRegion(bool includeSelf = false)
	{
		if (includeSelf && GameUtilities.RollChance(15))
		{
			return base.owner;
		}
		List<Area> list = RuinarchListPool<Area>.Claim(10);
		PopulatePlainNeighboursWithinRegion(list);
		Area result = null;
		if (list.Count > 0)
		{
			result = list[Random.Range(0, list.Count)];
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public Area GetRandomWildernessNeighbour(bool includeSelf = false)
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		for (int i = 0; i < neighbours.Count; i++)
		{
			Area area = neighbours[i];
			if (area.primaryStructureInArea.structureType == STRUCTURE_TYPE.WILDERNESS)
			{
				list.Add(area);
			}
		}
		Area result = null;
		if (list.Count > 0)
		{
			result = list[Random.Range(0, list.Count)];
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public Area GetRandomAdjacentNoSettlementHextileWithinRegion(bool includeSelf = false)
	{
		if (includeSelf && GameUtilities.RollChance(15))
		{
			return base.owner;
		}
		List<Area> list = RuinarchListPool<Area>.Claim(10);
		PopulatePlainNoSettlementNeighboursWithinRegion(list);
		Area result = null;
		if (list.Count > 0)
		{
			result = list[Random.Range(0, list.Count)];
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public Area GetNearestPlainAreaWithNoResident()
	{
		if (base.owner.elevationType != ELEVATION.WATER && base.owner.elevationType != ELEVATION.MOUNTAIN && !base.owner.HasAliveVillagerResident())
		{
			return base.owner;
		}
		for (int i = 0; i < neighbours.Count; i++)
		{
			Area area = neighbours[i];
			if (area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN && !area.HasAliveVillagerResident())
			{
				return area;
			}
		}
		return null;
	}

	public Area GetNearestAreaWithNoSettlementAndIsInWilderness()
	{
		if (!base.owner.HasSettlementOnArea() && base.owner.primaryStructureInArea.structureType == STRUCTURE_TYPE.WILDERNESS)
		{
			return base.owner;
		}
		Area area = null;
		float num = 0f;
		for (int i = 0; i < base.owner.region.areas.Count; i++)
		{
			Area area2 = base.owner.region.areas[i];
			if (!area2.HasSettlementOnArea() && area2.primaryStructureInArea.structureType == STRUCTURE_TYPE.WILDERNESS)
			{
				float num2 = base.owner.GetAreaDistanceTo(area2);
				if (area == null || num2 < num)
				{
					area = area2;
					num = num2;
				}
			}
		}
		return area;
	}

	private void PopulatePlainNeighboursWithinRegion(List<Area> areas)
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			Area area = neighbours[i];
			if (base.owner.region == area.region && area.elevationType == ELEVATION.PLAIN)
			{
				areas.Add(area);
			}
		}
	}

	private void PopulatePlainNoSettlementNeighboursWithinRegion(List<Area> areas)
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			Area area = neighbours[i];
			if (base.owner.region == area.region && !area.HasSettlementOnArea() && area.elevationType == ELEVATION.PLAIN)
			{
				areas.Add(area);
			}
		}
	}

	public void PopulateNeighboursNotInSettlementAndWithPathTo(List<Area> areas, BaseSettlement p_settlementException, Character p_character)
	{
		for (int i = 0; i < neighbours.Count; i++)
		{
			Area area = neighbours[i];
			if ((!area.HasSettlementOnArea() || !area.HasSettlementOnArea(p_settlementException)) && p_character.movementComponent.HasPathTo(area))
			{
				areas.Add(area);
			}
		}
	}

	public LocationGridTile GetRandomNearbyPassableWildernessGridTileWithPathTo(Character p_character, int p_searchLimit = 5)
	{
		LocationGridTile locationGridTile = null;
		List<Area> list = RuinarchListPool<Area>.Claim();
		base.owner.PopulateAreasInRange(list, 6, includeCenterTile: true);
		if (list.Count > 0)
		{
			int num = 0;
			while (locationGridTile == null && list.Count > 0 && num < p_searchLimit)
			{
				int index = GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1);
				locationGridTile = list[index].gridTileComponent.GetRandomTileThatIsPassableAndIsInWildernessAndHasPathTo(p_character);
				list.RemoveAt(index);
				num++;
			}
		}
		RuinarchListPool<Area>.Release(list);
		return locationGridTile;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
