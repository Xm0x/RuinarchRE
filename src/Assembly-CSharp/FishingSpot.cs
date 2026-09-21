using System;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using UtilityScripts;

public class FishingSpot : TileObject
{
	private FishingSpotGameObject _fishingSpotGameObject;

	public StructureConnector structureConnector
	{
		get
		{
			if (_fishingSpotGameObject != null)
			{
				return _fishingSpotGameObject.structureConnector;
			}
			return null;
		}
	}

	public Fishery connectedFishingShack { get; private set; }

	public override Type serializedData => typeof(SaveDataFishingSpot);

	public FishingSpot()
	{
		Initialize(TILE_OBJECT_TYPE.FISHING_SPOT);
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.traitContainer.AddTrait(this, "Immovable");
		base.traitContainer.AddTrait(this, "Frozen Immune");
		for (int i = 0; i < 10; i++)
		{
			base.traitContainer.AddTrait(this, "Wet", null, bypassElementalChance: false, 0, 0f, ELEMENTAL_TYPE.Water);
		}
	}

	public FishingSpot(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SaveDataFishingSpot saveDataFishingSpot = data as SaveDataFishingSpot;
		if (!string.IsNullOrEmpty(saveDataFishingSpot.connectedFishingShackID))
		{
			connectedFishingShack = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataFishingSpot.connectedFishingShackID) as Fishery;
		}
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_fishingSpotGameObject = mapVisual as FishingSpotGameObject;
	}

	public override void DestroyMapVisualGameObject()
	{
		base.DestroyMapVisualGameObject();
		_fishingSpotGameObject = null;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		base.traitContainer.AddTrait(this, "Indestructible");
		base.name = "a Lake";
		AddAdvertisedAction(INTERACTION_TYPE.FIND_FISH);
		Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
		if (structureConnector != null && gridTileLocation != null)
		{
			structureConnector.OnPlaceConnector(gridTileLocation.parentMap);
		}
	}

	public override void OnLoadPlacePOI()
	{
		DefaultProcessOnPlacePOI();
		Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
		if (structureConnector != null && gridTileLocation != null)
		{
			structureConnector.LoadConnectorForTileObjects(gridTileLocation.parentMap);
		}
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener(Signals.HOUR_STARTED, HourStarted);
	}

	public override bool CanBeAffectedByElementalStatus(string traitName)
	{
		if (traitName == "Wet")
		{
			return true;
		}
		return base.structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
	}

	public override bool CanBeDamaged()
	{
		return base.structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
	}

	public override bool CanBeSelected()
	{
		if (base.structureLocation != null)
		{
			return base.structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
		}
		return false;
	}

	public override string ToString()
	{
		return "Fishing Spot " + base.id;
	}

	private void HourStarted()
	{
		if (base.traitContainer.stacks.ContainsKey("Wet") && base.traitContainer.stacks["Wet"] < 10)
		{
			base.traitContainer.AddTrait(this, "Wet", null, bypassElementalChance: false, 0, 0f, ELEMENTAL_TYPE.Water);
		}
	}

	public void SetConnectedFishingShack(Fishery p_fishingShack)
	{
		connectedFishingShack = p_fishingShack;
	}

	public override void GeneralReactionToTileObject(Character actor, ref string debugLog)
	{
		base.GeneralReactionToTileObject(actor, ref debugLog);
		if (gridTileLocation != null && actor.race != RACE.TRITON && GameUtilities.RollChance(0.05f) && actor.canBeTargetedByLandActions && !actor.traitContainer.HasTrait("Sturdy", "Hibernating") && !actor.HasJobTargetingThis(JOB_TYPE.TRITON_KIDNAP))
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Triton, FactionManager.Instance.wildMonsterFaction, null, base.currentRegion, null, "", bypassIdeologyChecking: true);
			CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
			(summon as Triton).TriggerTritonKidnap(actor);
		}
	}
}
