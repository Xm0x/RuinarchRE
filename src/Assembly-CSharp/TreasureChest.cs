using System;
using System.Linq;
using Inner_Maps;

public class TreasureChest : TileObject
{
	private Character[] _users;

	private WeightedDictionary<string> _possibleItems;

	private WeightedDictionary<string> _openDropWeights;

	private const string Scroll_Lightning_Tower = "STRUCTURE_SCROLL_LIGHTNING_TOWER";

	private const string Scroll_Arrow_Tower = "STRUCTURE_SCROLL_ARROW_TOWER";

	private const string Scroll_Wyvern_Coop = "STRUCTURE_SCROLL_WYVERN_COOP";

	private const string Scroll_Beast_Pen = "STRUCTURE_SCROLL_BEAST_PEN";

	public IPointOfInterest objectInside { get; private set; }

	public override Character[] users => _users;

	public override Type serializedData => typeof(SaveDataTreasureChest);

	public TreasureChest()
	{
		Initialize(TILE_OBJECT_TYPE.TREASURE_CHEST, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.OPEN);
		ConstructPossibleItems();
		_openDropWeights = new WeightedDictionary<string>();
	}

	public TreasureChest(SaveDataTreasureChest data)
		: base(data)
	{
		ConstructPossibleItems();
		_openDropWeights = new WeightedDictionary<string>();
	}

	public override void LoadAdditionalInfo(SaveDataTileObject data)
	{
		base.LoadAdditionalInfo(data);
		SaveDataTreasureChest saveDataTreasureChest = data as SaveDataTreasureChest;
		if (string.IsNullOrEmpty(saveDataTreasureChest.objectInsideID) || gridTileLocation == null)
		{
			return;
		}
		if (saveDataTreasureChest.objectInsideType == OBJECT_TYPE.Character)
		{
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataTreasureChest.objectInsideID);
			if (characterByPersistentID is Mimic)
			{
				SetObjectInside(characterByPersistentID);
				if (characterByPersistentID.hasMarker)
				{
					characterByPersistentID.marker.PlaceMarkerAt(gridTileLocation);
					characterByPersistentID.marker.SetVisualState(state: false);
					characterByPersistentID.marker.SetLightState(p_state: false);
				}
			}
		}
		else if (saveDataTreasureChest.objectInsideType == OBJECT_TYPE.Tile_Object)
		{
			TileObject tileObjectByPersistentID = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataTreasureChest.objectInsideID);
			SetObjectInside(tileObjectByPersistentID);
		}
	}

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		if (action.goapType == INTERACTION_TYPE.OPEN)
		{
			RollForItemOnOpen(gridTileLocation, action.actor);
		}
	}

	private void RollForItemOnOpen(LocationGridTile p_gridTileLocation, Character p_actor)
	{
		CharacterClass characterClass = p_actor.characterClass;
		_openDropWeights.Clear();
		if (characterClass != null)
		{
			for (int i = 0; i < characterClass.craftableWeapons.Count; i++)
			{
				TILE_OBJECT_TYPE p_type = characterClass.craftableWeapons[i];
				EquipmentData equipmentDataBaseOnName = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(p_type.ToStringEnumWithSpace());
				if (!(equipmentDataBaseOnName != null) || !equipmentDataBaseOnName.isLegendary)
				{
					_openDropWeights.AddElement(p_type.ToStringEnum(), 10);
				}
			}
		}
		_openDropWeights.AddElement(TILE_OBJECT_TYPE.HEALING_POTION.ToStringEnum(), 20);
		_openDropWeights.AddElement(TILE_OBJECT_TYPE.ANTIDOTE.ToStringEnum(), 5);
		_openDropWeights.AddElement(TILE_OBJECT_TYPE.WOOD_PILE.ToStringEnum(), 20);
		_openDropWeights.AddElement(TILE_OBJECT_TYPE.STONE_PILE.ToStringEnum(), 20);
		_openDropWeights.AddElement(TILE_OBJECT_TYPE.ANIMAL_MEAT.ToStringEnum(), 30);
		_openDropWeights.AddElement("STRUCTURE_SCROLL_LIGHTNING_TOWER", 8);
		_openDropWeights.AddElement("STRUCTURE_SCROLL_ARROW_TOWER", 8);
		_openDropWeights.AddElement("STRUCTURE_SCROLL_WYVERN_COOP", 3);
		_openDropWeights.AddElement("STRUCTURE_SCROLL_BEAST_PEN", 6);
		for (int j = 0; j < StructureScroll.possibleStructures.Length; j++)
		{
			STRUCTURE_TYPE sTRUCTURE_TYPE = StructureScroll.possibleStructures[j];
			if (!GridMap.Instance.mainRegion.tileObjectsComponent.possibleStructureScrollChoices.Contains(sTRUCTURE_TYPE))
			{
				switch (sTRUCTURE_TYPE)
				{
				case STRUCTURE_TYPE.LIGHTNING_TOWER:
					_openDropWeights.RemoveElement("STRUCTURE_SCROLL_LIGHTNING_TOWER");
					break;
				case STRUCTURE_TYPE.ARROW_TOWER:
					_openDropWeights.RemoveElement("STRUCTURE_SCROLL_ARROW_TOWER");
					break;
				case STRUCTURE_TYPE.WYVERN_COOP:
					_openDropWeights.RemoveElement("STRUCTURE_SCROLL_WYVERN_COOP");
					break;
				case STRUCTURE_TYPE.BEAST_PEN:
					_openDropWeights.RemoveElement("STRUCTURE_SCROLL_BEAST_PEN");
					break;
				}
			}
		}
		string text = _openDropWeights.PickRandomElementGivenWeights();
		TileObject tileObject;
		if (text.Equals("STRUCTURE_SCROLL_LIGHTNING_TOWER"))
		{
			StructureScroll structureScroll = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
			structureScroll.SetStructureTypeToLearn(STRUCTURE_TYPE.LIGHTNING_TOWER);
			tileObject = structureScroll;
		}
		else
		{
			switch (text)
			{
			case "STRUCTURE_SCROLL_ARROW_TOWER":
			{
				StructureScroll structureScroll3 = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
				structureScroll3.SetStructureTypeToLearn(STRUCTURE_TYPE.ARROW_TOWER);
				tileObject = structureScroll3;
				break;
			}
			case "STRUCTURE_SCROLL_WYVERN_COOP":
			{
				StructureScroll structureScroll2 = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
				structureScroll2.SetStructureTypeToLearn(STRUCTURE_TYPE.WYVERN_COOP);
				tileObject = structureScroll2;
				break;
			}
			case "STRUCTURE_SCROLL_BEAST_PEN":
			{
				StructureScroll structureScroll4 = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
				structureScroll4.SetStructureTypeToLearn(STRUCTURE_TYPE.BEAST_PEN);
				tileObject = structureScroll4;
				break;
			}
			default:
			{
				if (Enum.TryParse<TILE_OBJECT_TYPE>(text, out var result))
				{
					tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(result);
					break;
				}
				throw new Exception("Cannot spawn item for treasure chest! " + text);
			}
			}
		}
		SetObjectInside(tileObject);
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		LocationGridTile locationGridTile = gridTileLocation;
		base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar, piercingPower, isPlayerSource, isTrueDamage);
		if (!CanBeDamaged() || amount >= 0)
		{
			return;
		}
		if (objectInside == null)
		{
			RollForItem(locationGridTile);
			if (objectInside is Mimic mimic)
			{
				SpawnInitialMimic(locationGridTile, mimic);
				locationGridTile.structure.RemovePOI(this);
				mimic.UnsubscribeToAwakenMimicEvent(this);
			}
		}
		else if (objectInside is Mimic p_mimic)
		{
			AwakenOccupant(p_mimic, locationGridTile);
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (objectInside is Character character)
		{
			character.marker.PlaceMarkerAt(gridTileLocation);
			character.marker.SetVisualState(state: false);
			character.marker.SetLightState(p_state: false);
		}
	}

	public override void DestroyPermanently()
	{
		base.DestroyPermanently();
		ClearUsers();
	}

	private void ConstructPossibleItems()
	{
		_possibleItems = new WeightedDictionary<string>();
		_possibleItems.AddElement(TILE_OBJECT_TYPE.HEALING_POTION.ToStringEnum(), 40);
		_possibleItems.AddElement(TILE_OBJECT_TYPE.ANTIDOTE.ToStringEnum(), 30);
		_possibleItems.AddElement(TILE_OBJECT_TYPE.WOOD_PILE.ToStringEnum(), 20);
		_possibleItems.AddElement(TILE_OBJECT_TYPE.STONE_PILE.ToStringEnum(), 20);
		_possibleItems.AddElement(TILE_OBJECT_TYPE.ANIMAL_MEAT.ToStringEnum(), 30);
		_possibleItems.AddElement("STRUCTURE_SCROLL_LIGHTNING_TOWER", 8);
		_possibleItems.AddElement("STRUCTURE_SCROLL_ARROW_TOWER", 8);
		_possibleItems.AddElement("STRUCTURE_SCROLL_WYVERN_COOP", 3);
		_possibleItems.AddElement("STRUCTURE_SCROLL_BEAST_PEN", 6);
	}

	private void AwakenOccupant(Mimic p_mimic, LocationGridTile p_location)
	{
		if (!p_mimic.isDead)
		{
			p_mimic.SetIsTreasureChest(state: false);
			p_mimic.marker.PlaceMarkerAt(p_location);
			p_mimic.marker.SetVisualState(state: true);
			p_mimic.marker.SetLightState(p_state: true);
		}
		p_location.structure.RemovePOI(this);
		TraitManager.Instance.CopyStatuses(this, p_mimic);
		p_mimic.UnsubscribeToAwakenMimicEvent(this);
		ClearUsers();
	}

	private void RollForItem(LocationGridTile locationGridTile)
	{
		if (objectInside != null)
		{
			return;
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Mimic_Spawn))
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Mimic, FactionManager.Instance.wildMonsterFaction, null, locationGridTile.parentMap.region);
			SetObjectInside(summon);
			return;
		}
		for (int i = 0; i < StructureScroll.possibleStructures.Length; i++)
		{
			STRUCTURE_TYPE sTRUCTURE_TYPE = StructureScroll.possibleStructures[i];
			if (!GridMap.Instance.mainRegion.tileObjectsComponent.possibleStructureScrollChoices.Contains(sTRUCTURE_TYPE))
			{
				switch (sTRUCTURE_TYPE)
				{
				case STRUCTURE_TYPE.LIGHTNING_TOWER:
					_possibleItems.RemoveElement("STRUCTURE_SCROLL_LIGHTNING_TOWER");
					break;
				case STRUCTURE_TYPE.ARROW_TOWER:
					_possibleItems.RemoveElement("STRUCTURE_SCROLL_ARROW_TOWER");
					break;
				case STRUCTURE_TYPE.WYVERN_COOP:
					_possibleItems.RemoveElement("STRUCTURE_SCROLL_WYVERN_COOP");
					break;
				case STRUCTURE_TYPE.BEAST_PEN:
					_possibleItems.RemoveElement("STRUCTURE_SCROLL_BEAST_PEN");
					break;
				}
			}
		}
		string text = _possibleItems.PickRandomElementGivenWeights();
		TileObject tileObject;
		switch (text)
		{
		case "STRUCTURE_SCROLL_LIGHTNING_TOWER":
		{
			StructureScroll structureScroll2 = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
			structureScroll2.SetStructureTypeToLearn(STRUCTURE_TYPE.LIGHTNING_TOWER);
			tileObject = structureScroll2;
			break;
		}
		case "STRUCTURE_SCROLL_ARROW_TOWER":
		{
			StructureScroll structureScroll = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
			structureScroll.SetStructureTypeToLearn(STRUCTURE_TYPE.ARROW_TOWER);
			tileObject = structureScroll;
			break;
		}
		case "STRUCTURE_SCROLL_WYVERN_COOP":
		{
			StructureScroll structureScroll4 = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
			structureScroll4.SetStructureTypeToLearn(STRUCTURE_TYPE.WYVERN_COOP);
			tileObject = structureScroll4;
			break;
		}
		case "STRUCTURE_SCROLL_BEAST_PEN":
		{
			StructureScroll structureScroll3 = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
			structureScroll3.SetStructureTypeToLearn(STRUCTURE_TYPE.BEAST_PEN);
			tileObject = structureScroll3;
			break;
		}
		default:
		{
			if (Enum.TryParse<TILE_OBJECT_TYPE>(text, out var result))
			{
				tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(result);
				break;
			}
			throw new Exception("Cannot spawn item for treasure chest! " + text);
		}
		}
		SetObjectInside(tileObject);
	}

	public void SetObjectInside(IPointOfInterest pointOfInterest)
	{
		objectInside = pointOfInterest;
		if (objectInside is Mimic mimic)
		{
			_users = new Character[1] { mimic };
			mimic.SubscribeToAwakenMimicEvent(this);
		}
	}

	public void SpawnInitialMimic(LocationGridTile p_tile, Mimic p_mimic)
	{
		CharacterManager.Instance.PlaceSummonInitially(p_mimic, p_tile);
		p_mimic.SetTerritory(p_tile.GetNearestAreaWithinRegion());
		TraitManager.Instance.CopyStatuses(this, p_mimic);
		ClearUsers();
	}

	public void TryAwakenMimic(Mimic p_mimic)
	{
		if (_users != null && _users.Contains(p_mimic) && p_mimic.gridTileLocation != null)
		{
			AwakenOccupant(p_mimic, p_mimic.gridTileLocation);
		}
	}

	private void ClearUsers()
	{
		_users = null;
		objectInside = null;
	}
}
