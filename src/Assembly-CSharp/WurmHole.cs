using System;

public class WurmHole : TileObject
{
	public WurmHole wurmHoleConnection { get; private set; }

	public override Type serializedData => typeof(SaveDataWurmHole);

	public override bool canBeSeized => false;

	public WurmHole()
	{
		Initialize(TILE_OBJECT_TYPE.WURM_HOLE);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public WurmHole(SaveDataWurmHole data)
		: base(data)
	{
	}

	public void SetWurmHoleConnection(WurmHole wurmHole)
	{
		wurmHoleConnection = wurmHole;
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		if (wurmHoleConnection.gridTileLocation != null)
		{
			wurmHoleConnection.gridTileLocation.structure.RemovePOI(wurmHoleConnection);
		}
	}

	public void TravelThroughWurmHole(Character character)
	{
		character.movementComponent.SetCameFromWurmHole(state: true);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Tile Object", "TileObjectAlerts_Table", "Wurm Hole teleported", LOG_TAG.Work, null);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(this, base.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddToFillers(wurmHoleConnection, wurmHoleConnection.name, LOG_IDENTIFIER.CHARACTER_3);
		log.AddLogToDatabase(releaseLogAfter: true);
		if (gridTileLocation != null)
		{
			GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Teleport);
		}
		character.jobQueue.CancelAllJobs();
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)character, "");
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)character, "");
		character.combatComponent.ClearHostilesInRange();
		character.combatComponent.ClearAvoidInRange();
		CharacterManager.Instance.Teleport(character, wurmHoleConnection.gridTileLocation);
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		if (data is SaveDataWurmHole saveDataWurmHole && !string.IsNullOrEmpty(saveDataWurmHole.wurmHoleConnection))
		{
			wurmHoleConnection = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataWurmHole.wurmHoleConnection) as WurmHole;
		}
	}
}
