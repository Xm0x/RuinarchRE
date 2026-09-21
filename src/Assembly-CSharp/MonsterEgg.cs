using System;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public abstract class MonsterEgg : TileObject
{
	public SUMMON_TYPE summonType { get; protected set; }

	public Faction faction { get; protected set; }

	public BaseSettlement settlement { get; protected set; }

	public LocationStructure structure { get; protected set; }

	public GameDate hatchDate { get; private set; }

	public bool isSupposedToHatch { get; private set; }

	public bool hasHatched { get; private set; }

	public bool hasInitiated { get; private set; }

	public override Type serializedData => typeof(SaveDataMonsterEgg);

	protected MonsterEgg(TILE_OBJECT_TYPE tileObjectType, SUMMON_TYPE summonType, int hatchTime)
	{
		Initialize(tileObjectType, shouldAddCommonAdvertisements: false);
		this.summonType = summonType;
		hatchDate = GameManager.Instance.Today().AddTicks(hatchTime);
	}

	public MonsterEgg(SaveDataMonsterEgg data)
		: base(data)
	{
		summonType = data.summonType;
		if (summonType == SUMMON_TYPE.None)
		{
			if (data is SaveDataSpiderEgg)
			{
				summonType = SUMMON_TYPE.Giant_Spider;
			}
			else if (data is SaveDataHarpyEgg)
			{
				summonType = SUMMON_TYPE.Harpy;
			}
		}
		hatchDate = data.hatchDate;
		hasHatched = data.hasHatched;
		isSupposedToHatch = data.isSupposedToHatch;
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SaveDataMonsterEgg saveDataMonsterEgg = data as SaveDataMonsterEgg;
		if (!string.IsNullOrEmpty(saveDataMonsterEgg.faction))
		{
			faction = DatabaseManager.Instance.factionDatabase.GetFactionByPersistentID(saveDataMonsterEgg.faction);
		}
		if (!string.IsNullOrEmpty(saveDataMonsterEgg.settlement))
		{
			settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(saveDataMonsterEgg.settlement);
		}
		if (!string.IsNullOrEmpty(saveDataMonsterEgg.structure))
		{
			structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataMonsterEgg.structure);
		}
	}

	public void SetCharacterThatLay(Character character)
	{
		faction = character.faction;
		settlement = character.homeSettlement;
		structure = character.homeStructure;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (!hasHatched && isSupposedToHatch)
		{
			GameDate gameDate = GameManager.Instance.Today().AddTicks(1);
			SchedulingManager.Instance.AddEntry(gameDate, HatchProcess, this);
		}
		else if (!hasInitiated)
		{
			hasInitiated = true;
			SchedulingManager.Instance.AddEntry(hatchDate, HatchProcess, this);
		}
	}

	protected void HatchProcess()
	{
		if (hasHatched)
		{
			return;
		}
		isSupposedToHatch = true;
		if (!base.isBeingSeized)
		{
			if (base.isBeingCarriedBy != null)
			{
				base.isBeingCarriedBy.UncarryPOI(this);
			}
			if (gridTileLocation != null)
			{
				Hatch();
				gridTileLocation.structure.RemovePOI(this);
				hasHatched = true;
			}
		}
	}

	protected virtual void Hatch()
	{
		LocationStructure homeStructure = null;
		Area territory = null;
		BaseSettlement homeSettlement = null;
		ProcessHomeOfHatchedEggs(ref homeStructure, ref territory, ref homeSettlement);
		if (homeSettlement == null)
		{
			homeSettlement = homeStructure?.settlementLocation;
		}
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction, homeSettlement, GridMap.Instance.mainRegion, homeStructure, "", bypassIdeologyChecking: true);
		CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
		if (!summon.HasHome())
		{
			if (territory == null)
			{
				territory = gridTileLocation.area;
			}
			summon.SetTerritory(territory);
		}
	}

	protected void ProcessHomeOfHatchedEggs(ref LocationStructure homeStructure, ref Area territory, ref BaseSettlement homeSettlement)
	{
		bool flag = false;
		LocationStructure locationStructure = gridTileLocation?.structure;
		if (locationStructure != null)
		{
			if (locationStructure.structureType == STRUCTURE_TYPE.WILDERNESS)
			{
				flag = true;
			}
			else if (locationStructure.settlementLocation != null)
			{
				if (locationStructure.settlementLocation.owner == null || locationStructure.settlementLocation.owner == faction)
				{
					homeSettlement = locationStructure.settlementLocation;
					homeStructure = locationStructure;
					return;
				}
				flag = true;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		BaseSettlement baseSettlement = structure?.settlementLocation;
		if (baseSettlement == null)
		{
			baseSettlement = settlement;
		}
		if (baseSettlement == null)
		{
			return;
		}
		if (baseSettlement.owner == null || baseSettlement.owner == faction)
		{
			homeSettlement = baseSettlement;
			homeStructure = structure;
			return;
		}
		Area area = gridTileLocation?.area;
		if (locationStructure != null && locationStructure.structureType == STRUCTURE_TYPE.WILDERNESS && area != null && (!area.HasSettlementOnArea() || !area.HasSettlementWithFactionOwnerOnArea() || area.HasSettlementWithFactionOwnerOnArea(faction)))
		{
			territory = area;
		}
		else
		{
			territory = area?.neighbourComponent.GetNearestAreaWithNoSettlementAndIsInWilderness();
		}
	}

	public override string ToString()
	{
		return "Monster Egg " + base.id;
	}

	public override void DestroyPermanently()
	{
		base.DestroyPermanently();
		faction = null;
		settlement = null;
		structure = null;
	}

	protected override void DisconnectFromStructure(LocationStructure p_structure)
	{
		base.DisconnectFromStructure(p_structure);
		if (structure == p_structure)
		{
			structure = null;
		}
	}
}
