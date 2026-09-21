using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

public class Area : IPlayerActionTarget, IPartyTargetDestination, ILocation, ISavable
{
	private int _blueprintsOnTile;

	public AreaData areaData { get; private set; }

	public Region region { get; private set; }

	public List<BaseSettlement> settlementsOnArea { get; private set; }

	public AreaItem areaItem { get; private set; }

	public int freezingTraps { get; private set; }

	public int snareTraps { get; private set; }

	public AreaFeatureComponent featureComponent { get; private set; }

	public LocationAwareness locationAwareness { get; private set; }

	public LocationCharacterTracker locationCharacterTracker { get; private set; }

	public AreaGridTileComponent gridTileComponent { get; private set; }

	public AreaNeighbourComponent neighbourComponent { get; private set; }

	public AreaTileObjectComponent tileObjectComponent { get; private set; }

	public AreaBiomeComponent biomeComponent { get; private set; }

	public AreaStructureComponent structureComponent { get; private set; }

	public AreaElevationComponent elevationComponent { get; }

	public string name => locationName;

	public string locationName => "Area " + areaData.xCoordinate + ", " + areaData.yCoordinate;

	public string persistentID => areaData.persistentID;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Area;

	public Type serializedData => typeof(SaveDataArea);

	public int id => areaData.id;

	public BIOMES biomeType => biomeComponent.biomeType;

	public ELEVATION elevationType => elevationComponent.elevationType;

	public bool hasBeenDestroyed => false;

	public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Area;

	public LocationStructure primaryStructureInArea => structureComponent.GetMostImportantStructureOnTile();

	public Vector3 worldPosition => (Vector2)areaItem.transform.position;

	public List<PLAYER_SKILL_TYPE> actions { get; private set; }

	public Area(int id, int x, int y)
	{
		areaData = new AreaData
		{
			persistentID = Guid.NewGuid().ToString(),
			id = id,
			xCoordinate = x,
			yCoordinate = y
		};
		settlementsOnArea = new List<BaseSettlement>(4);
		locationCharacterTracker = new LocationCharacterTracker();
		locationAwareness = new LocationAwareness();
		featureComponent = new AreaFeatureComponent();
		gridTileComponent = new AreaGridTileComponent();
		gridTileComponent.SetOwner(this);
		neighbourComponent = new AreaNeighbourComponent();
		neighbourComponent.SetOwner(this);
		tileObjectComponent = new AreaTileObjectComponent();
		tileObjectComponent.SetOwner(this);
		biomeComponent = new AreaBiomeComponent();
		biomeComponent.SetOwner(this);
		structureComponent = new AreaStructureComponent();
		structureComponent.SetOwner(this);
		elevationComponent = new AreaElevationComponent();
		elevationComponent.SetOwner(this);
	}

	public Area(SaveDataArea data)
	{
		areaData = data.areaData;
		settlementsOnArea = new List<BaseSettlement>(4);
		gridTileComponent = new AreaGridTileComponent();
		gridTileComponent.SetOwner(this);
		neighbourComponent = new AreaNeighbourComponent();
		neighbourComponent.SetOwner(this);
		tileObjectComponent = new AreaTileObjectComponent();
		tileObjectComponent.SetOwner(this);
		biomeComponent = new AreaBiomeComponent();
		biomeComponent.SetOwner(this);
		structureComponent = new AreaStructureComponent();
		structureComponent.SetOwner(this);
		elevationComponent = new AreaElevationComponent();
		elevationComponent.SetOwner(this);
		locationCharacterTracker = new LocationCharacterTracker();
		locationAwareness = new LocationAwareness();
		featureComponent = new AreaFeatureComponent();
	}

	public override string ToString()
	{
		return locationName + " - " + elevationType.ToStringEnum() + " - " + (region?.name ?? "No Region");
	}

	public void SetAreaItem(AreaItem p_areaItem)
	{
		areaItem = p_areaItem;
		gridTileComponent.PopulateBorderTiles(this);
	}

	public bool IsNextToOrPartOfVillage()
	{
		if (!IsPartOfVillage())
		{
			return neighbourComponent.IsNextToVillage();
		}
		return true;
	}

	public bool IsPartOfVillage()
	{
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			if (settlementsOnArea[i].locationType == LOCATION_TYPE.VILLAGE)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNearbyTo(Area p_area)
	{
		return GetAreaDistanceTo(p_area) <= 6;
	}

	public void PopulateAreasInRange(List<Area> areas, int range, bool includeCenterTile = false)
	{
		Area[,] areaMap = region.areaMap;
		int upperBound = areaMap.GetUpperBound(0);
		int upperBound2 = areaMap.GetUpperBound(1);
		int xCoordinate = areaData.xCoordinate;
		int yCoordinate = areaData.yCoordinate;
		if (includeCenterTile)
		{
			areas.Add(this);
		}
		for (int i = xCoordinate - range; i <= xCoordinate + range; i++)
		{
			for (int j = yCoordinate - range; j <= yCoordinate + range; j++)
			{
				if (i >= 0 && i <= upperBound && j >= 0 && j <= upperBound2 && (i != xCoordinate || j != yCoordinate))
				{
					Area item = areaMap[i, j];
					areas.Add(item);
				}
			}
		}
	}

	public bool HasAliveVillagerResident()
	{
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			if (settlementsOnArea[i].HasResidentThatIsVillagerAndNotDead())
			{
				return true;
			}
		}
		return false;
	}

	public int GetAreaDistanceTo(Area p_targetArea)
	{
		LocationGridTile centerGridTile = p_targetArea.gridTileComponent.centerGridTile;
		return (int)(gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile) / (float)InnerMapManager.AreaLocationGridTileSize.x);
	}

	public void SetRegion(Region region)
	{
		this.region = region;
	}

	public void AddSettlementOnArea(BaseSettlement p_settlement)
	{
		settlementsOnArea.Add(p_settlement);
		region.AddSettlementInRegion(p_settlement);
		if (GameManager.Instance.gameHasStarted)
		{
			areaItem.UpdatePathfindingGraph();
		}
	}

	public void RemoveSettlementFromArea(BaseSettlement p_settlement)
	{
		if (settlementsOnArea.Remove(p_settlement))
		{
			if (p_settlement.areas.Count <= 0)
			{
				region.RemoveSettlementFromRegion(p_settlement);
			}
			if (GameManager.Instance.gameHasStarted)
			{
				areaItem.UpdatePathfindingGraph();
			}
		}
	}

	public void TryRemoveAreaFromSettlementIfItIsNoLongerPartOfIt()
	{
		if (settlementsOnArea.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsOnArea[i];
			for (int j = 0; j < baseSettlement.allStructures.Count; j++)
			{
				if (baseSettlement.allStructures[i].HasTileOnArea(this))
				{
					return;
				}
			}
			int count = settlementsOnArea.Count;
			if (baseSettlement.RemoveAreaFromSettlement(this) && settlementsOnArea.Count < count)
			{
				i--;
			}
		}
	}

	public bool HasSettlementOnArea()
	{
		return settlementsOnArea.Count > 0;
	}

	public bool HasSettlementOnArea(BaseSettlement p_settlement)
	{
		if (p_settlement != null)
		{
			for (int i = 0; i < settlementsOnArea.Count; i++)
			{
				if (settlementsOnArea[i] == p_settlement)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasSettlementVillageTypeWithFactionOwner(out BaseSettlement p_settlement)
	{
		p_settlement = null;
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsOnArea[i];
			if (baseSettlement.locationType != LOCATION_TYPE.DUNGEON && baseSettlement.owner != null)
			{
				p_settlement = baseSettlement;
				return true;
			}
		}
		return false;
	}

	public bool HasSettlementWithFactionOwnerOnArea()
	{
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			if (settlementsOnArea[i].owner != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSettlementWithFactionOwnerOnArea(Faction p_faction)
	{
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsOnArea[i];
			if (baseSettlement.owner != null && baseSettlement.owner == p_faction)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPlayerSettlement()
	{
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsOnArea[i];
			if (baseSettlement.owner != null && baseSettlement.owner.isPlayerFaction)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSettlementLocationType(LOCATION_TYPE p_locationType)
	{
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			if (settlementsOnArea[i].locationType == p_locationType)
			{
				return true;
			}
		}
		return false;
	}

	public NPCSettlement GetFirstNPCSettlementOnArea()
	{
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			if (settlementsOnArea[i] is NPCSettlement result)
			{
				return result;
			}
		}
		return null;
	}

	public BaseSettlement GetSettlementOnArea()
	{
		BaseSettlement baseSettlement = null;
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			BaseSettlement baseSettlement2 = settlementsOnArea[i];
			if (baseSettlement2 is NPCSettlement result)
			{
				return result;
			}
			if (baseSettlement == null)
			{
				baseSettlement = baseSettlement2;
			}
		}
		return baseSettlement;
	}

	public BaseSettlement GetCurrentSettlementOfCharacter(Character p_character)
	{
		BaseSettlement result = null;
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsOnArea[i];
			if (baseSettlement is NPCSettlement result2)
			{
				return result2;
			}
			result = baseSettlement;
		}
		return result;
	}

	public LocationStructure GetRandomStructureFromAllSettlements()
	{
		LocationStructure result = null;
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			LocationStructure randomStructure = settlementsOnArea[i].GetRandomStructure();
			if (randomStructure != null)
			{
				list.Add(randomStructure);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public string GetSettlementAreaNames()
	{
		string text = string.Empty;
		for (int i = 0; i < settlementsOnArea.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsOnArea[i];
			if (i > 0)
			{
				text += ", ";
			}
			text += baseSettlement.name;
		}
		if (string.IsNullOrEmpty(text))
		{
			return "None";
		}
		return text;
	}

	public void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		actions = new List<PLAYER_SKILL_TYPE>();
	}

	public void AddPlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (!actions.Contains(action))
		{
			actions.Add(action);
			if (broadcastSignal)
			{
				Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, (IPlayerActionTarget)this);
			}
		}
	}

	public void RemovePlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (actions.Remove(action) && broadcastSignal)
		{
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, (IPlayerActionTarget)this);
		}
	}

	public void ClearPlayerActions()
	{
		actions.Clear();
	}

	public void OnPlacePOIInHex(IPointOfInterest poi)
	{
		if (poi is TileObject item)
		{
			tileObjectComponent.AddItemInArea(item);
		}
	}

	public void OnRemovePOIInHex(IPointOfInterest poi)
	{
		if (poi is TileObject item)
		{
			tileObjectComponent.RemoveItemInArea(item);
		}
	}

	public LocationGridTile GetRandomPassableTile()
	{
		return gridTileComponent.GetRandomPassableTile();
	}

	public bool IsAtTargetDestination(Character character)
	{
		if (character.gridTileLocation != null)
		{
			return character.gridTileLocation.area == this;
		}
		return false;
	}

	public void AddFreezingTrapInArea()
	{
		freezingTraps++;
	}

	public void RemoveFreezingTrapInArea()
	{
		freezingTraps--;
	}

	public void AddSnareTrapInArea()
	{
		snareTraps++;
	}

	public void RemoveSnareTrapInArea()
	{
		snareTraps--;
	}

	public void AddBlueprint()
	{
		_blueprintsOnTile++;
	}

	public void RemoveBlueprint()
	{
		_blueprintsOnTile--;
	}

	public bool HasBlueprintOnTile()
	{
		return _blueprintsOnTile > 0;
	}

	public bool IsReservedByOtherVillage(VillageSpot p_villageSpot)
	{
		return GetOccupyingVillageSpot() != p_villageSpot;
	}

	public VillageSpot GetOccupyingVillageSpot()
	{
		for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++)
		{
			VillageSpot villageSpot = GridMap.Instance.mainRegion.villageSpots[i];
			if (villageSpot.reservedAreas.Contains(this))
			{
				return villageSpot;
			}
		}
		return null;
	}

	public bool HasNeigbouringAreaThatIsReservedByVillageSpot()
	{
		for (int i = 0; i < neighbourComponent.neighbours.Count; i++)
		{
			if (neighbourComponent.neighbours[i].GetOccupyingVillageSpot() != null)
			{
				return true;
			}
		}
		return false;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		featureComponent.CheckIfStructureIsStillReferenced(p_structure);
		locationAwareness.CheckIfStructureIsStillReferenced(p_structure);
		locationCharacterTracker.CheckIfStructureIsStillReferenced(p_structure);
		gridTileComponent.CheckIfStructureIsStillReferenced(p_structure);
		neighbourComponent.CheckIfStructureIsStillReferenced(p_structure);
		tileObjectComponent.CheckIfStructureIsStillReferenced(p_structure);
		biomeComponent.CheckIfStructureIsStillReferenced(p_structure);
		structureComponent.CheckIfStructureIsStillReferenced(p_structure);
		elevationComponent.CheckIfStructureIsStillReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		featureComponent.CheckIfCharacterIsStillReferenced(p_character);
		locationAwareness.CheckIfCharacterIsStillReferenced(p_character);
		locationCharacterTracker.CheckIfCharacterIsStillReferenced(p_character);
		gridTileComponent.CheckIfCharacterIsStillReferenced(p_character);
		neighbourComponent.CheckIfCharacterIsStillReferenced(p_character);
		tileObjectComponent.CheckIfCharacterIsStillReferenced(p_character);
		biomeComponent.CheckIfCharacterIsStillReferenced(p_character);
		structureComponent.CheckIfCharacterIsStillReferenced(p_character);
		elevationComponent.CheckIfCharacterIsStillReferenced(p_character);
	}
}
