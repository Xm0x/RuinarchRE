using System;
using Inner_Maps;
using Object_Pools;
using UtilityScripts;

public class AnkhOfAnubis : Artifact
{
	public bool isActivated { get; private set; }

	public int ghostLimit { get; private set; }

	public override Type serializedData => typeof(SaveDataAnkhOfAnubis);

	public override bool canBeSeized => !isActivated;

	public AnkhOfAnubis()
		: base(ARTIFACT_TYPE.Ankh_Of_Anubis)
	{
		base.maxHP = 700;
		base.currentHP = base.maxHP;
		ghostLimit = GameUtilities.RandomBetweenTwoNumbers(20, 30);
		base.traitContainer.AddTrait(this, "Treasure");
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public AnkhOfAnubis(SaveDataAnkhOfAnubis data)
		: base(data)
	{
		isActivated = data.isActivated;
		ghostLimit = data.ghostLimit;
	}

	public override void ActivateTileObject()
	{
		if (gridTileLocation != null && !isActivated)
		{
			base.ActivateTileObject();
			isActivated = true;
			base.traitContainer.RemoveTrait(this, "Treasure");
			GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Ankh_Of_Anubis_Activate);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Artifact", "TileObjectAlerts_Table", "Ankh Of Anubis activate", LOG_TAG.Major);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
	}

	public override void OnTileObjectDroppedBy(Character inventoryOwner, LocationGridTile tile)
	{
		if (inventoryOwner.isDead && inventoryOwner.isNormalCharacter)
		{
			ActivateTileObject();
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (isActivated)
		{
			GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Ankh_Of_Anubis_Activate);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
		}
	}

	private void OnCharacterDeath(Character characterThatDied)
	{
		if (isActivated && gridTileLocation != null && characterThatDied.isNormalCharacter && base.currentRegion == characterThatDied.currentRegion && characterThatDied.hasMarker && characterThatDied.visuals.HasBlood())
		{
			CharacterManager.Instance.SpawnNewMonsterInstanceFrom(SUMMON_TYPE.Vengeful_Ghost, characterThatDied, null, null, gridTileLocation, FactionManager.Instance.undeadFaction);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Artifact", "TileObjectAlerts_Table", "Ankh Of Anubis spawn_vengeful", LOG_TAG.Life_Changes);
			log.AddToFillers(this, base.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, characterThatDied.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(gridTileLocation, log);
			LogPool.Release(log);
			ghostLimit--;
			EvaluateGhostLimit();
		}
	}

	private void EvaluateGhostLimit()
	{
		if (ghostLimit <= 0)
		{
			gridTileLocation.structure.RemovePOI(this);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Artifact", "TileObjectAlerts_Table", "Ankh Of Anubis deactivate", LOG_TAG.Major);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
	}
}
