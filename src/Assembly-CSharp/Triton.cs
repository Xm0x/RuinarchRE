using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Triton : Summon
{
	public const string ClassName = "Triton";

	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;

	public override Type serializedData => typeof(SaveDataTriton);

	public LocationGridTile spawnLocationTile { get; private set; }

	public Triton()
		: base(SUMMON_TYPE.Triton, "Triton", RACE.TRITON, Utilities.GetRandomGender())
	{
	}

	public Triton(string className)
		: base(SUMMON_TYPE.Triton, className, RACE.TRITON, Utilities.GetRandomGender())
	{
	}

	public Triton(SaveDataSummon data)
		: base(data)
	{
	}

	public override void OnPlaceSummon(LocationGridTile tile)
	{
		base.OnPlaceSummon(tile);
		if (spawnLocationTile == null)
		{
			List<LocationGridTile> checkedTiles = RuinarchListPool<LocationGridTile>.Claim();
			spawnLocationTile = tile.GetNeareastTileFromThisThatIsPassableOrHasNoWallsAndIsNotInOcean(checkedTiles);
			if (spawnLocationTile == null)
			{
				spawnLocationTile = tile;
			}
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetEnableDigging(state: true);
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
		}
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		base.LoadReferences(data);
		if (data is SaveDataTriton saveDataTriton && saveDataTriton.tileLocationSave.hasValue)
		{
			spawnLocationTile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveDataTriton.tileLocationSave);
		}
	}

	private void OnJobRemovedFromQueue(JobQueueItem job, Character character)
	{
		if (character != this || job.jobType != JOB_TYPE.TRITON_KIDNAP)
		{
			return;
		}
		IPointOfInterest poiTarget = job.poiTarget;
		if (poiTarget != null)
		{
			Prisoner traitOrStatus = poiTarget.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus != null && traitOrStatus.prisonerOfCharacter == this)
			{
				poiTarget.traitContainer.RemoveRestrainAndImprison(poiTarget);
			}
		}
	}

	public void TriggerTritonKidnap(Character targetCharacter)
	{
		base.jobComponent.TriggerTritonKidnap(targetCharacter, spawnLocationTile.structure, spawnLocationTile);
	}
}
