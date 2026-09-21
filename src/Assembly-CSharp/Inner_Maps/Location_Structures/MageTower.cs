using System;
using System.Collections.Generic;
using Characters.Components;
using Object_Pools;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class MageTower : ManMadeStructure, CharacterEventDispatcher.ILocationListener, CharacterEventDispatcher.IDeathListener
{
	private GameObject structureProtectionParticleEffect;

	private uint _sfxID;

	public List<string> spawnedGolemIDs { get; private set; }

	public string protectorID { get; private set; }

	public override Type serializedData => typeof(SaveDataMageTower);

	public MageTower(Region location)
		: base(STRUCTURE_TYPE.MAGE_TOWER, location)
	{
		AddStructureTag(STRUCTURE_TAG.Treasure);
		AddStructureTag(STRUCTURE_TAG.Magic_Power_Up);
		AddStructureTag(STRUCTURE_TAG.Counterattack);
		AddStructureTag(STRUCTURE_TAG.Shelter);
		spawnedGolemIDs = new List<string>();
		protectorID = string.Empty;
	}

	public MageTower(Region location, SaveDataMageTower data)
		: base(location, data)
	{
		if (data.spawnedGolemIDs != null && data.spawnedGolemIDs.Count > 0)
		{
			spawnedGolemIDs = new List<string>(data.spawnedGolemIDs);
		}
		else
		{
			spawnedGolemIDs = new List<string>();
		}
		protectorID = data.protectorID;
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		SetIsProtected(p_state: false);
		SetProtector(null);
	}

	protected override void OnSetProtected(Character p_protector)
	{
		base.OnSetProtected(p_protector);
		if (base.isProtected)
		{
			SetProtector(p_protector);
			if (spawnedGolemIDs.Count <= 0)
			{
				SummonProtectorGolems(p_protector);
			}
			AddProtectionParticleEffect();
			_sfxID = AkSoundEngine.PostEvent("Play_Devastation_Ritual", base.structureObj.gameObject);
		}
		else
		{
			RemoveProtectionParticleEffect();
			AkSoundEngine.StopPlayingID(_sfxID);
		}
	}

	protected override void OnSetProtected()
	{
		base.OnSetProtected();
		if (!base.isProtected)
		{
			RemoveProtectionParticleEffect();
		}
	}

	private void SetProtector(Character p_protector)
	{
		Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(protectorID);
		if (p_protector != null)
		{
			protectorID = p_protector.persistentID;
		}
		else
		{
			protectorID = string.Empty;
		}
		Character characterByPersistentID2 = CharacterManager.Instance.GetCharacterByPersistentID(protectorID);
		if (characterByPersistentID != null)
		{
			characterByPersistentID.eventDispatcher.UnsubscribeToCharacterLeftStructure(this);
			characterByPersistentID.eventDispatcher.UnsubscribeToCharacterDied(this);
		}
		if (characterByPersistentID2 != null)
		{
			characterByPersistentID2.eventDispatcher.SubscribeToCharacterLeftStructure(this);
			characterByPersistentID2.eventDispatcher.SubscribeToCharacterDied(this);
		}
		else
		{
			UnsummonProtectorGolems();
		}
	}

	public override bool CanBeDamagedByPlayerSpells()
	{
		return false;
	}

	public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource)
	{
	}

	public override void OnTileRepaired(LocationGridTile tile, int amount)
	{
	}

	public override bool DoesTileContributeToDamage(LocationGridTile tile)
	{
		return false;
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		if (!string.IsNullOrEmpty(protectorID))
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(protectorID);
			characterByPersistentID.eventDispatcher.SubscribeToCharacterLeftStructure(this);
			characterByPersistentID.eventDispatcher.SubscribeToCharacterDied(this);
		}
	}

	private void SummonProtectorGolems(Character p_protector)
	{
		if (base.hasBeenDestroyed)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			LocationGridTile locationGridTile = GetRandomPassableTile();
			if (locationGridTile == null)
			{
				locationGridTile = GetRandomTile();
			}
			if (locationGridTile != null)
			{
				Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Golem, p_protector.faction, null, GridMap.Instance.mainRegion, this, "", bypassIdeologyChecking: true);
				summon.SetDestroyMarkerOnDeath(state: true);
				CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
				summon.traitContainer.RemoveTrait(summon, "Hibernating");
				summon.traitContainer.RemoveTrait(summon, "Indestructible");
				summon.behaviourComponent.ChangeDefaultBehaviourSet("Structure Protector Behaviour");
				GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
				spawnedGolemIDs.Add(summon.persistentID);
			}
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Locations", "LocationAlerts_Table", "mage_tower_summon_golems", LOG_TAG.Major);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
		LogPool.Release(log);
	}

	private void UnsummonProtectorGolems()
	{
		while (spawnedGolemIDs.Count > 0)
		{
			string text = spawnedGolemIDs[0];
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(text);
			if (characterByPersistentID != null && !characterByPersistentID.isDead)
			{
				characterByPersistentID.Death("disappear");
			}
			spawnedGolemIDs.RemoveAt(0);
		}
	}

	private void AddProtectionParticleEffect()
	{
		if (!structureProtectionParticleEffect)
		{
			LocationGridTile centerTile = GameUtilities.GetCenterTile(base.tiles, GridMap.Instance.mainRegion.innerMap.map);
			structureProtectionParticleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile, PARTICLE_EFFECT.Structure_Protection);
		}
	}

	private void RemoveProtectionParticleEffect()
	{
		if ((bool)structureProtectionParticleEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(structureProtectionParticleEffect);
			structureProtectionParticleEffect = null;
		}
	}

	public void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure)
	{
		if (p_leftStructure == this && p_character.persistentID == protectorID)
		{
			SetProtector(null);
		}
	}

	public void OnCharacterArrivedAtStructure(Character p_character, LocationStructure p_leftStructure)
	{
	}

	public void OnCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement)
	{
	}

	public void OnCharacterSubscribedToDied(Character p_character)
	{
		if (p_character.persistentID == protectorID)
		{
			SetProtector(null);
		}
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			spawnedGolemIDs.Clear();
			base.CleanUp();
		}
	}
}
