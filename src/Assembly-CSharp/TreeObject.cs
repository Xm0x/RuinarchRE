using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using Locations.Settlements;

public abstract class TreeObject : TileObject
{
	public enum Occupied_State
	{
		Undecided,
		Occupied,
		Unoccupied
	}

	private Ent _ent;

	private Character[] _users;

	private Occupied_State _occupiedState;

	private TreeGameObject _treeGameObject;

	public int count { get; set; }

	public override Type serializedData => typeof(SaveDataTreeObject);

	public StructureConnector structureConnector
	{
		get
		{
			if (_treeGameObject != null)
			{
				return _treeGameObject.structureConnector;
			}
			return null;
		}
	}

	public Occupied_State occupiedState => _occupiedState;

	public Ent ent => _ent;

	public override Character[] users => _users;

	protected TreeObject(TILE_OBJECT_TYPE p_treeType)
	{
		Initialize(p_treeType, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.CHOP_WOOD);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		_occupiedState = Occupied_State.Undecided;
		count = 80;
	}

	protected TreeObject(SaveDataTreeObject data)
		: base(data)
	{
		_occupiedState = data.occupiedState;
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SaveDataTreeObject saveDataTreeObject = data as SaveDataTreeObject;
		if (!string.IsNullOrEmpty(saveDataTreeObject.occupyingEntID) && gridTileLocation != null)
		{
			if (DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataTreeObject.occupyingEntID) is Ent occupyingEnt)
			{
				SetOccupyingEnt(occupyingEnt);
			}
			else
			{
				_occupiedState = Occupied_State.Undecided;
			}
		}
	}

	public override void LoadAdditionalInfo(SaveDataTileObject data)
	{
		base.LoadAdditionalInfo(data);
		if (ent != null && ent.hasMarker && gridTileLocation != null)
		{
			ent.marker.PlaceMarkerAt(gridTileLocation);
			ent.marker.SetVisualState(state: false);
			ent.marker.SetLightState(p_state: false);
		}
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		LocationGridTile locationGridTile = gridTileLocation;
		base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar, piercingPower, isPlayerSource, isTrueDamage);
		if (!CanBeDamaged() || amount >= 0)
		{
			return;
		}
		switch (occupiedState)
		{
		case Occupied_State.Occupied:
			if (ent != null && locationGridTile != null)
			{
				AwakenOccupant(locationGridTile);
			}
			break;
		case Occupied_State.Undecided:
			if (locationGridTile != null)
			{
				RollForOccupant(locationGridTile);
			}
			break;
		case Occupied_State.Unoccupied:
			break;
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (ent != null)
		{
			ent.marker.PlaceMarkerAt(gridTileLocation);
			ent.marker.SetVisualState(state: false);
			ent.marker.SetLightState(p_state: false);
		}
		if (structureConnector != null && gridTileLocation != null)
		{
			structureConnector.OnPlaceConnector(gridTileLocation.parentMap);
		}
	}

	public override void OnLoadPlacePOI()
	{
		DefaultProcessOnPlacePOI();
		if (ent != null)
		{
			ent.marker.PlaceMarkerAt(gridTileLocation);
			ent.marker.SetVisualState(state: false);
			ent.marker.SetLightState(p_state: false);
		}
		if (structureConnector != null && gridTileLocation != null)
		{
			structureConnector.LoadConnectorForTileObjects(gridTileLocation.parentMap);
		}
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_treeGameObject = mapVisual as TreeGameObject;
	}

	public override void DestroyMapVisualGameObject()
	{
		base.DestroyMapVisualGameObject();
		_treeGameObject = null;
	}

	public override string GetAdditionalTestingData()
	{
		return base.GetAdditionalTestingData() + " <b>Count:</b> " + count;
	}

	public override void DestroyPermanently()
	{
		base.DestroyPermanently();
		_ent = null;
		_users = null;
	}

	public void SetOccupyingEnt(Ent p_ent)
	{
		_occupiedState = Occupied_State.Occupied;
		_ent = p_ent;
		_users = new Character[1] { p_ent };
		if (_ent != null)
		{
			_ent.SubscribeToAwakenEntEvent(this);
		}
	}

	private void RemoveOccupyingEnt()
	{
		if (_ent != null)
		{
			_ent.UnsubscribeToAwakenEntEvent(this);
			_ent = null;
		}
	}

	private void RollForOccupant(LocationGridTile location)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Ent_Spawn))
		{
			SpawnEnt(location, FactionManager.Instance.wildMonsterFaction, null);
		}
		else
		{
			_occupiedState = Occupied_State.Unoccupied;
		}
	}

	public Ent SpawnEnt(Faction p_faction, BaseSettlement p_homeSettlement)
	{
		return SpawnEnt(gridTileLocation, p_faction, p_homeSettlement);
	}

	private Ent SpawnEnt(LocationGridTile location, Faction p_faction, BaseSettlement p_homeSettlement)
	{
		SUMMON_TYPE sUMMON_TYPE;
		if (location.corruptionComponent.isCorrupted)
		{
			sUMMON_TYPE = SUMMON_TYPE.Corrupt_Ent;
		}
		else
		{
			switch (location.mainBiomeType)
			{
			case BIOMES.DESERT:
				sUMMON_TYPE = SUMMON_TYPE.Desert_Ent;
				break;
			case BIOMES.FOREST:
				sUMMON_TYPE = SUMMON_TYPE.Forest_Ent;
				break;
			case BIOMES.GRASSLAND:
				sUMMON_TYPE = SUMMON_TYPE.Grass_Ent;
				break;
			case BIOMES.SNOW:
			case BIOMES.TUNDRA:
				sUMMON_TYPE = SUMMON_TYPE.Snow_Ent;
				break;
			default:
				sUMMON_TYPE = SUMMON_TYPE.Grass_Ent;
				break;
			}
		}
		CharacterManager instance = CharacterManager.Instance;
		SUMMON_TYPE summonType = sUMMON_TYPE;
		Region region = location.parentMap.region;
		Ent ent = instance.CreateNewSummon(summonType, p_faction, p_homeSettlement, region) as Ent;
		CharacterManager.Instance.PlaceSummonInitially(ent, location);
		if (p_homeSettlement == null)
		{
			ent.SetTerritory(location.GetNearestAreaWithinRegion());
		}
		TraitManager.Instance.CopyStatuses(this, ent);
		location.structure.RemovePOI(this);
		RemoveOccupyingEnt();
		return ent;
	}

	private void AwakenOccupant(LocationGridTile location)
	{
		if (!ent.isDead)
		{
			if (!ent.hasMarker)
			{
				ent.CreateMarker();
			}
			ent.marker.SetVisualState(state: true);
			ent.marker.SetLightState(p_state: true);
			ent.marker.PlaceMarkerAt(location);
			ent.SetIsTree(state: false);
		}
		location.structure.RemovePOI(this);
		TraitManager.Instance.CopyStatuses(this, ent);
		RemoveOccupyingEnt();
	}

	public void TryAwakenEnt(Ent p_ent)
	{
		if (ent == p_ent && p_ent.gridTileLocation != null)
		{
			AwakenOccupant(p_ent.gridTileLocation);
		}
	}

	public bool TryAwakenEnt()
	{
		if (HasEnt() && ent.gridTileLocation != null && !ent.isDead && ent.isTree)
		{
			AwakenOccupant(ent.gridTileLocation);
			return true;
		}
		return false;
	}

	public bool HasEnt()
	{
		return _ent != null;
	}
}
