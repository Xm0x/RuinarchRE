using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class ReligionComponent : CharacterComponent
{
	public static int Religious_Cultist_Belief_Threshold = 60;

	public static int Religious_Cult_Leader_Belief_Threshold = 90;

	public static int Base_Belief_Points = 10;

	public RELIGION religion { get; private set; }

	public Dictionary<RELIGION, int> beliefPoints { get; private set; }

	public ReligionComponent()
	{
		religion = RELIGION.None;
		beliefPoints = new Dictionary<RELIGION, int>();
	}

	public ReligionComponent(SaveDataReligionComponent data)
	{
		religion = data.religion;
		if (data.beliefPoints != null && data.beliefPoints.Count > 0)
		{
			beliefPoints = new Dictionary<RELIGION, int>(data.beliefPoints);
		}
		else
		{
			beliefPoints = new Dictionary<RELIGION, int>();
		}
	}

	public void Initialize()
	{
		SetDefaultReligion();
	}

	public void SubscribeListeners()
	{
		Messenger.AddListener<Character>(FactionSignals.FACTION_SET, OnCharacterFactionSet);
		Messenger.AddListener<Character>(FactionSignals.CHARACTER_BECAME_RELIGIOUS_CULT_LEADER, OnCharacterBecameReligiousCultLeader);
		Messenger.AddListener<RELIGION>(StructureSignals.HALLOWED_GROUND_CLAIMED, OnHallowedGroundClaimed);
	}

	public void UnsubscribeListeners()
	{
		Messenger.RemoveListener<Character>(FactionSignals.FACTION_SET, OnCharacterFactionSet);
		Messenger.RemoveListener<Character>(FactionSignals.CHARACTER_BECAME_RELIGIOUS_CULT_LEADER, OnCharacterBecameReligiousCultLeader);
		Messenger.RemoveListener<RELIGION>(StructureSignals.HALLOWED_GROUND_CLAIMED, OnHallowedGroundClaimed);
	}

	private void OnCharacterFactionSet(Character character)
	{
		if (character == base.owner)
		{
			Faction faction = character.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Undead)
			{
				ForceSetReligion(RELIGION.None);
			}
		}
	}

	private void OnCharacterBecameReligiousCultLeader(Character p_character)
	{
		if (base.owner != p_character && base.owner.characterClass.IsReligiousCultLeaderClass() && p_character.characterClass.className == base.owner.characterClass.className)
		{
			p_character.classComponent.AssignClass(p_character.classComponent.previousClassName);
		}
	}

	private void OnHallowedGroundClaimed(RELIGION p_religion)
	{
		if (religion == p_religion)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Religion Buff");
		}
		else
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Religion Buff");
		}
	}

	public void ChangeReligion(RELIGION p_religion)
	{
		ForceSetReligion(p_religion);
	}

	private void ForceSetReligion(RELIGION p_religion)
	{
		beliefPoints.Clear();
		IncreaseBeliefPoints(p_religion, Base_Belief_Points);
	}

	private void SetDefaultReligion()
	{
		switch (base.owner.race)
		{
		case RACE.HUMANS:
			IncreaseBeliefPoints(RELIGION.Divine_Worship, Base_Belief_Points);
			break;
		case RACE.ELVES:
			IncreaseBeliefPoints(RELIGION.Nature_Worship, Base_Belief_Points);
			break;
		default:
			IncreaseBeliefPoints(RELIGION.None, Base_Belief_Points);
			break;
		}
	}

	public static RELIGION GetDefaultReligionForRace(RACE race)
	{
		return race switch
		{
			RACE.HUMANS => RELIGION.Divine_Worship, 
			RACE.ELVES => RELIGION.Nature_Worship, 
			_ => RELIGION.None, 
		};
	}

	public bool IsTargetACultistOfDifferentReligion(Character p_targetCharacter)
	{
		if (p_targetCharacter.traitContainer.IsReligiousCultist(out var p_religion))
		{
			return p_religion != base.owner.religionComponent.religion;
		}
		return false;
	}

	public void ProcessReligiousCrimeTypes(ref CRIME_TYPE p_crimeType1, ref CRIME_TYPE p_crimeType2)
	{
		p_crimeType1 = CRIME_TYPE.Divine_Worship;
		p_crimeType2 = CRIME_TYPE.Nature_Worship;
		if (base.owner.religionComponent.religion == RELIGION.Divine_Worship)
		{
			p_crimeType1 = CRIME_TYPE.Demon_Worship;
			p_crimeType2 = CRIME_TYPE.Nature_Worship;
		}
		else if (base.owner.religionComponent.religion == RELIGION.Nature_Worship)
		{
			p_crimeType1 = CRIME_TYPE.Demon_Worship;
			p_crimeType2 = CRIME_TYPE.Divine_Worship;
		}
	}

	public void IncreaseBeliefPointsFromReligiousActions(RELIGION p_religion, INTERACTION_TYPE p_actionType, int p_amount = 1)
	{
		IncreaseBeliefPoints(p_religion, p_amount);
		switch (p_religion)
		{
		case RELIGION.Divine_Worship:
		case RELIGION.Nature_Worship:
			if (!CharacterManager.Instance.HasActiveReligiousCultistOfType(p_religion) && !CharacterManager.Instance.HasActiveReligiousCultLeaderOfType(p_religion))
			{
				int num = 0;
				if (base.owner.traitContainer.HasTrait("Devout"))
				{
					num += 50;
				}
				if (base.owner.moodComponent.moodState == MOOD_STATE.Critical)
				{
					num += 20;
				}
				if (GameUtilities.RollChance(num))
				{
					CheckForReligiousCultistCreation(p_religion, WorldSettings.Instance.worldSettingsData.villageSettings.GetInitialCultistThresholdForReligion(p_religion));
				}
			}
			else if (p_actionType == INTERACTION_TYPE.EVANGELIZE || p_actionType == INTERACTION_TYPE.LIBERATE)
			{
				CheckForReligiousCultistCreation(p_religion, Religious_Cultist_Belief_Threshold);
			}
			CheckForReligiousCultLeaderCreation(p_religion);
			break;
		case RELIGION.Demon_Worship:
			if (CharacterManager.Instance.HasActiveReligiousCultistOfType(p_religion) && (p_actionType == INTERACTION_TYPE.EVANGELIZE || p_actionType == INTERACTION_TYPE.LIBERATE))
			{
				CheckForReligiousCultistCreation(p_religion, Religious_Cultist_Belief_Threshold);
			}
			CheckForReligiousCultLeaderCreation(p_religion);
			break;
		}
	}

	private void CheckForReligiousCultLeaderCreation(RELIGION p_religion)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Religious_Cult_Leader_Spawn_Chance) && !CharacterManager.Instance.HasActiveReligiousCultLeaderOfType(p_religion) && FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion) == null && base.owner.traitContainer.IsReligiousCultist(p_religion) && GetBeliefPoints(p_religion) >= Religious_Cult_Leader_Belief_Threshold)
		{
			string cultLeaderClassNameForReligion = p_religion.GetCultLeaderClassNameForReligion();
			if (!string.IsNullOrEmpty(cultLeaderClassNameForReligion))
			{
				base.owner.classComponent.AssignClass(cultLeaderClassNameForReligion);
			}
		}
	}

	public bool CheckForReligiousCultistCreation(RELIGION p_religion, int p_neededPoints)
	{
		if (!base.owner.traitContainer.IsReligiousCultist() && GetBeliefPoints(p_religion) >= p_neededPoints)
		{
			if (base.owner.classComponent.IsStalkerCannotBeTurned())
			{
				return false;
			}
			string cultistTraitNameForReligion = p_religion.GetCultistTraitNameForReligion();
			if (!string.IsNullOrEmpty(cultistTraitNameForReligion))
			{
				return base.owner.traitContainer.AddTrait(base.owner, cultistTraitNameForReligion);
			}
		}
		return false;
	}

	public bool IncreaseBeliefPoints(RELIGION p_religion, int p_amount = 1)
	{
		if (!beliefPoints.ContainsKey(p_religion))
		{
			beliefPoints.Add(p_religion, 0);
		}
		beliefPoints[p_religion] += p_amount;
		bool num = UpdateReligionBasedOnBeliefPoints();
		if (num)
		{
			ProcessOnChangeReligion();
		}
		return num;
	}

	public bool DecreaseBeliefPoints(RELIGION p_religion, int p_amount = 1)
	{
		if (beliefPoints.ContainsKey(p_religion))
		{
			beliefPoints[p_religion] -= p_amount;
			if (beliefPoints[p_religion] <= 0)
			{
				beliefPoints.Remove(p_religion);
			}
			bool num = UpdateReligionBasedOnBeliefPoints();
			if (num)
			{
				ProcessOnChangeReligion();
			}
			return num;
		}
		return false;
	}

	public int GetBeliefPoints(RELIGION p_religion)
	{
		if (beliefPoints.ContainsKey(p_religion))
		{
			return beliefPoints[p_religion];
		}
		return 0;
	}

	private bool UpdateReligionBasedOnBeliefPoints()
	{
		RELIGION rELIGION = religion;
		int value = GetBeliefPoints(religion);
		RELIGION key = religion;
		foreach (KeyValuePair<RELIGION, int> beliefPoint in beliefPoints)
		{
			if (beliefPoint.Value > value)
			{
				key = beliefPoint.Key;
				value = beliefPoint.Value;
			}
		}
		religion = key;
		return rELIGION != religion;
	}

	private void ProcessOnChangeReligion()
	{
		if (!GameManager.Instance.gameHasStarted)
		{
			return;
		}
		if (base.owner.traitContainer.IsReligiousCultist())
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Demon Cultist");
			base.owner.traitContainer.RemoveTrait(base.owner, "Cleric");
			base.owner.traitContainer.RemoveTrait(base.owner, "Witch");
		}
		if (GridMap.Instance.mainRegion.GetRandomStructureOfType(STRUCTURE_TYPE.HALLOWED_GROUND) is Inner_Maps.Location_Structures.HallowedGround { claimedByReligion: not RELIGION.None } hallowedGround)
		{
			if (hallowedGround.claimedByReligion == religion)
			{
				base.owner.traitContainer.AddTrait(base.owner, "Religion Buff");
			}
			else
			{
				base.owner.traitContainer.RemoveTrait(base.owner, "Religion Buff");
			}
		}
		for (int i = 0; i < beliefPoints.Keys.Count; i++)
		{
			RELIGION key = beliefPoints.Keys.ElementAt(i);
			beliefPoints[key] /= 2;
		}
		base.owner.faction?.CheckIfCharacterStillFitsIdeology(base.owner, willLog: true, rollForGrudge: false);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
