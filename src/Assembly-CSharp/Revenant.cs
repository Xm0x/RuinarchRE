using System;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class Revenant : Summon
{
	public override Type serializedData => typeof(SaveDataRevenant);

	public List<Character> betrayers { get; private set; }

	public Revenant()
		: base(SUMMON_TYPE.Revenant, "Revenant", RACE.REVENANT, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
		betrayers = new List<Character>();
	}

	public Revenant(string className)
		: base(SUMMON_TYPE.Revenant, className, RACE.REVENANT, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
		betrayers = new List<Character>();
	}

	public Revenant(SaveDataRevenant data)
		: base(data)
	{
		betrayers = new List<Character>();
	}

	public override void Initialize()
	{
		base.Initialize();
		base.isWildMonster = false;
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		if (data is SaveDataRevenant saveDataRevenant)
		{
			for (int i = 0; i < saveDataRevenant.betrayers.Count; i++)
			{
				Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(saveDataRevenant.betrayers[i]);
				if (characterByPersistentID != null)
				{
					betrayers.Add(characterByPersistentID);
				}
			}
		}
		base.LoadReferences(data);
	}

	public override void LoadReferencesMainThread(SaveDataCharacter data)
	{
		base.LoadReferencesMainThread(data);
		base.visuals.SetHasBlood(state: false);
	}

	protected override void OnTickEnded()
	{
		base.OnTickEnded();
		PerTickOutsideCombatHPRecovery();
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		int numberOfGhostsInHome = GetNumberOfGhostsInHome();
		if (numberOfGhostsInHome < 5)
		{
			int amount = 5 - numberOfGhostsInHome;
			TriggerSpawnGhosts(JOB_TYPE.AGITATED, amount, out p_agitateJob);
			if (p_agitateJob is GoapPlanJob goapPlanJob)
			{
				goapPlanJob.SetIsAgitateJob(p_state: true);
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
				return true;
			}
			return false;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_1);
		return false;
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		betrayers.Remove(p_character);
	}

	public void AddBetrayer(Character character)
	{
		betrayers.Add(character);
	}

	public Character GetRandomBetrayer()
	{
		if (betrayers.Count > 0)
		{
			return betrayers[UnityEngine.Random.Range(0, betrayers.Count)];
		}
		return null;
	}

	private bool TriggerSpawnGhosts(JOB_TYPE p_jobType, int amount, out JobQueueItem p_producedJob)
	{
		p_producedJob = null;
		if (!base.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.SPAWN_GHOST, this, this);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.SPAWN_GHOST, new object[1] { amount });
			goapPlanJob.SetDoNotRecalculate(state: true);
			p_producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TrySpawnGhost(ref string p_log)
	{
		if (GetNumberOfGhostsInHome() < 5)
		{
			Character randomBetrayer = GetRandomBetrayer();
			LocationGridTile tileToSpawnGhostRelativeToThis = GetTileToSpawnGhostRelativeToThis();
			Faction faction = base.faction ?? FactionManager.Instance.undeadFaction;
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, faction, base.homeSettlement, base.homeRegion, null, "", bypassIdeologyChecking: true);
			(summon as Ghost).SetBetrayedBy(randomBetrayer);
			CharacterManager.Instance.PlaceSummonInitially(summon, tileToSpawnGhostRelativeToThis);
			Messenger.Broadcast(CharacterSignals.GHOST_SPAWNED_THAT_COUNTS_FOR_TASK, summon);
			return true;
		}
		return false;
	}

	public void SpawnGhosts(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			Character randomBetrayer = GetRandomBetrayer();
			LocationGridTile tileToSpawnGhostRelativeToThis = GetTileToSpawnGhostRelativeToThis();
			Faction faction = base.faction ?? FactionManager.Instance.undeadFaction;
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, faction, base.homeSettlement, base.homeRegion, null, "", bypassIdeologyChecking: true);
			(summon as Ghost).SetBetrayedBy(randomBetrayer);
			CharacterManager.Instance.PlaceSummonInitially(summon, tileToSpawnGhostRelativeToThis);
			Messenger.Broadcast(CharacterSignals.GHOST_SPAWNED_THAT_COUNTS_FOR_TASK, summon);
		}
	}

	private int GetNumberOfGhostsInHome()
	{
		int num = 0;
		if (base.homeSettlement != null)
		{
			num = base.homeSettlement.GetNumberOfResidentsThatIsAliveMonsterAndMonsterTypeIs(SUMMON_TYPE.Ghost);
		}
		else if (base.homeStructure != null)
		{
			num = base.homeStructure.GetNumberOfResidentsThatIsAliveMonsterAndMonsterTypeIs(SUMMON_TYPE.Ghost);
		}
		else if (HasTerritory())
		{
			for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
			{
				Character character = CharacterManager.Instance.allCharacters[i];
				if (!character.isDead && character.IsTerritory(base.territory) && character is Summon { summonType: SUMMON_TYPE.Ghost })
				{
					num++;
				}
			}
		}
		return num;
	}

	private LocationGridTile GetTileToSpawnGhostRelativeToThis()
	{
		LocationGridTile locationGridTile = null;
		if (base.homeSettlement != null)
		{
			locationGridTile = base.homeSettlement.GetRandomArea().GetRandomPassableTile();
		}
		else if (base.homeStructure != null)
		{
			locationGridTile = base.homeStructure.GetRandomPassableTile();
		}
		else if (HasTerritory())
		{
			locationGridTile = base.territory.GetRandomPassableTile();
		}
		if (locationGridTile == null)
		{
			locationGridTile = base.gridTileLocation;
		}
		return locationGridTile;
	}

	public override void CleanUp()
	{
		base.CleanUp();
		betrayers?.Clear();
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		betrayers.Contains(p_character);
	}
}
