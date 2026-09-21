using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Traits;

public class Wet : Status, IElementalTrait
{
	private StatusIcon _statusIcon;

	private ITraitable _owner;

	public Character dryer { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataWet);

	public override bool shouldBeLoadedInMainThread => true;

	public Wet()
	{
		name = "Wet";
		description = "Soaked with water.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(6);
		isTangible = true;
		isStacking = true;
		moodEffect = -6;
		stackLimit = 10;
		stackModifier = 0f;
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.EXTRACT_ITEM };
		AddTraitOverrideFunctionIdentifier("Villager_Reaction");
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataWet saveDataWet = saveDataTrait as SaveDataWet;
		isPlayerSource = saveDataWet.isPlayerSource;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		_owner = addTo;
		if (addTo is GenericTileObject)
		{
			Messenger.AddListener(Signals.HOUR_STARTED, OnHourStartedForWetFloor);
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		TryListenForBiomeEffect();
		UpdateVisualsOnAdd(_owner);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		_owner = addedTo;
		addedTo.traitContainer.RemoveStatusAndStacks(addedTo, "Overheating");
		if (addedTo is TileObject)
		{
			if (addedTo is GenericTileObject genericTileObject)
			{
				genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.CLEAN_UP);
				if (genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand)
				{
					ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
				}
				Messenger.AddListener(Signals.HOUR_STARTED, OnHourStartedForWetFloor);
			}
			addedTo.traitContainer.RemoveStatusAndStacks(addedTo, "Poisoned");
			if (addedTo is Crops crops)
			{
				crops.AdjustGrowthRate(1);
			}
		}
		TryListenForBiomeEffect();
		UpdateVisualsOnAdd(addedTo);
	}

	public override void OnStackStatus(ITraitable addedTo)
	{
		base.OnStackStatus(addedTo);
		UpdateVisualsOnAdd(addedTo);
	}

	public override void OnStackStatusAddedButStackIsAtLimit(ITraitable addedTo)
	{
		base.OnStackStatusAddedButStackIsAtLimit(addedTo);
		UpdateVisualsOnAdd(addedTo);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		_owner = null;
		if (removedFrom is TileObject)
		{
			if (removedFrom is GenericTileObject genericTileObject)
			{
				genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.CLEAN_UP);
				Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, (TileObject)genericTileObject, removedBy);
				Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStartedForWetFloor);
			}
			if (removedFrom is Crops crops)
			{
				crops.AdjustGrowthRate(-1);
			}
		}
		StopListenForBiomeEffect();
		UpdateVisualsOnRemove(removedFrom);
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Wet wet)
		{
			dryer = wet.dryer;
		}
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (p_character.hasMarker && p_character.marker.isMoving && p_character.isNormalCharacter && GameUtilities.RollChance(1.5f))
		{
			return p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Stumble_Knockout, p_character);
		}
		return false;
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		if (dryer == p_character)
		{
			SetDryer(null);
		}
	}

	private void UpdateVisualsOnAdd(ITraitable addedTo)
	{
		if (addedTo is Character character && _statusIcon == null && character.hasMarker)
		{
			_statusIcon = character.marker.AddStatusIcon(name);
		}
		else if (addedTo is TileObject tileObject)
		{
			if (tileObject is GenericTileObject && tileObject.structureLocation.structureType != STRUCTURE_TYPE.OCEAN)
			{
				tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, InnerMapManager.Instance.assetManager.wetTile, 0.5f);
				tileObject.gridTileLocation.ClearAllSeamlessEdges(tileObject.gridTileLocation.parentMap);
			}
			else if (tileObject.tileObjectType != TILE_OBJECT_TYPE.WATER_WELL && tileObject.tileObjectType != TILE_OBJECT_TYPE.FISHING_SPOT && _statusIcon == null && addedTo.mapObjectVisual != null)
			{
				_statusIcon = addedTo.mapObjectVisual.AddStatusIcon(name);
			}
		}
	}

	private void UpdateVisualsOnRemove(ITraitable removedFrom)
	{
		if (removedFrom is Character { hasMarker: not false })
		{
			ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);
		}
		else if (removedFrom is TileObject tileObject)
		{
			if (tileObject is GenericTileObject && tileObject.structureLocation.structureType != STRUCTURE_TYPE.OCEAN)
			{
				tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, null);
				tileObject.gridTileLocation.CreateSeamlessEdgesForTile(tileObject.gridTileLocation.parentMap);
			}
			else if (_statusIcon != null)
			{
				ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);
			}
		}
	}

	public void SetDryer(Character character)
	{
		dryer = character;
		if (dryer == null)
		{
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
		}
		else
		{
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
		}
	}

	private void OnJobRemovedFromCharacter(JobQueueItem jqi, Character character)
	{
		if (dryer == character && jqi.jobType == JOB_TYPE.DRY_TILES)
		{
			SetDryer(null);
		}
	}

	private void TryListenForBiomeEffect()
	{
		if (!(_owner.gridTileLocation?.structure is Ocean))
		{
			Messenger.AddListener(AreaSignals.FREEZE_WET_OBJECTS, TryFreezeWetObject);
		}
	}

	private void StopListenForBiomeEffect()
	{
		Messenger.RemoveListener(AreaSignals.FREEZE_WET_OBJECTS, TryFreezeWetObject);
	}

	private void TryFreezeWetObject()
	{
		if (GameUtilities.RollChance(25) && _owner.gridTileLocation != null && _owner.gridTileLocation.mainBiomeType == BIOMES.SNOW)
		{
			_owner.traitContainer.AddTrait(_owner, "Frozen", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Ice);
		}
	}

	private void OnHourStartedForWetFloor()
	{
		if (_owner == null)
		{
			Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStartedForWetFloor);
		}
		else
		{
			PerformTreeRoll();
		}
	}

	public void PerformTreeRoll()
	{
		LocationGridTile gridTileLocation = _owner.gridTileLocation;
		if (gridTileLocation == null || gridTileLocation.tileObjectComponent.objHere != null || gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS || gridTileLocation.corruptionComponent.isCorrupted)
		{
			return;
		}
		int chance = 0;
		int chance2 = 0;
		if (gridTileLocation.groundType == LocationGridTile.Ground_Type.Snow_Dirt || gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass)
		{
			chance = 8;
			chance2 = 4;
		}
		else if (gridTileLocation.groundType == LocationGridTile.Ground_Type.Grass)
		{
			if (gridTileLocation.mainBiomeType == BIOMES.FOREST)
			{
				chance = 8;
				chance2 = 12;
			}
			else
			{
				chance = 12;
				chance2 = 6;
			}
		}
		if (GameUtilities.RollChance(chance))
		{
			TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.SMALL_TREE_OBJECT);
			gridTileLocation.structure.AddPOI(poi, gridTileLocation);
		}
		else if (GameUtilities.RollChance(chance2) && InnerMapManager.Instance.CanBigTreeBePlacedOnTile(gridTileLocation))
		{
			TileObject poi2 = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
			gridTileLocation.structure.AddPOI(poi2, gridTileLocation);
		}
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isPlayerSource = p_state;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
		_ = dryer;
	}
}
