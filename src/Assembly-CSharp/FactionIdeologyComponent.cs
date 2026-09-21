using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class FactionIdeologyComponent : FactionComponent
{
	public List<FactionIdeology> currentIdeologies => base.owner.factionType.ideologies;

	public CRIME_TYPE lastRemovedNonReligionCrimeTypeForEvilOrPsychopath { get; private set; }

	public FactionIdeologyComponent()
	{
	}

	public FactionIdeologyComponent(SaveDataFactionIdeologyComponent data)
	{
		lastRemovedNonReligionCrimeTypeForEvilOrPsychopath = data.lastRemovedNonReligionCrimeTypeForEvilOrPsychopath;
	}

	public bool DoesCharacterFitCurrentIdeologies(Character character)
	{
		if (currentIdeologies == null)
		{
			return true;
		}
		for (int i = 0; i < currentIdeologies.Count; i++)
		{
			FactionIdeology factionIdeology = currentIdeologies[i];
			if (factionIdeology != null && !factionIdeology.DoesCharacterFitIdeology(character))
			{
				return false;
			}
		}
		return true;
	}

	public bool DoesCharacterFitCurrentIdeologiesIgnoreReligion(Character character)
	{
		if (currentIdeologies == null)
		{
			return true;
		}
		for (int i = 0; i < currentIdeologies.Count; i++)
		{
			FactionIdeology factionIdeology = currentIdeologies[i];
			if (factionIdeology != null && !factionIdeology.IsReligionType() && !factionIdeology.DoesCharacterFitIdeology(character))
			{
				return false;
			}
		}
		return true;
	}

	public bool DoesCharacterFitCurrentIdeologies(PreCharacterData character)
	{
		if (currentIdeologies == null)
		{
			return true;
		}
		for (int i = 0; i < currentIdeologies.Count; i++)
		{
			FactionIdeology factionIdeology = currentIdeologies[i];
			if (factionIdeology != null && !factionIdeology.DoesCharacterFitIdeology(character))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasIdeology(FACTION_IDEOLOGY p_ideologyType)
	{
		return base.owner.factionType.HasIdeology(p_ideologyType);
	}

	public void OnLeaderBecameCultist(Character leader)
	{
		Faction faction = leader.faction;
		if (!leader.traitContainer.IsReligiousCultist(out var p_religion) || !base.owner.isMajorNonPlayer)
		{
			return;
		}
		Faction faction2 = null;
		if (FactionManager.Instance.FactionLeaderCultistProcessing(leader, p_religion, out var wasFactionMergedWithReligiousCult, out var createdReligiousCultFaction))
		{
			string key = (createdReligiousCultFaction ? "convert_to_cult" : "merged_with_cult");
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", key, LOG_TAG.Major);
			log.AddToFillers(leader, leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			faction2 = FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion);
			log.AddToFillers(faction2, faction2.name, LOG_IDENTIFIER.FACTION_2);
			log.AddToFillers(null, p_religion.GetCultistTraitNameForReligion(), LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		if (wasFactionMergedWithReligiousCult)
		{
			return;
		}
		if (faction2 != null)
		{
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "cannot_join_demon_cult", LOG_TAG.Major);
			log2.AddToFillers(leader, leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			log2.AddToFillers(faction2, faction2.name, LOG_IDENTIFIER.STRING_1);
			log2.AddLogToDatabase(releaseLogAfter: true);
		}
		if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges)
		{
			FactionIdeology factionIdeology = null;
			for (int i = 0; i < base.owner.factionType.ideologies.Count; i++)
			{
				FactionIdeology factionIdeology2 = base.owner.factionType.ideologies[i];
				if (factionIdeology2.ideologyType == FACTION_IDEOLOGY.Exclusive || factionIdeology2.ideologyType == FACTION_IDEOLOGY.Inclusive)
				{
					factionIdeology = factionIdeology2;
					break;
				}
			}
			base.owner.factionType.RemoveAllIdeologies(base.owner);
			FactionManager.Instance.RerollPeaceTypeIdeology(base.owner, leader);
			if (factionIdeology != null)
			{
				base.owner.factionType.AddIdeology(factionIdeology, base.owner);
			}
			else
			{
				FactionManager.Instance.RerollInclusiveTypeIdeology(base.owner, leader);
			}
			FactionManager.Instance.RerollReligionTypeIdeology(base.owner, leader);
		}
		FactionManager.Instance.RevalidateFactionCrimes(base.owner, leader);
		base.owner.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
		base.owner.GetRelationshipWith(FactionManager.Instance.retaliatorFaction).SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
		Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "ideology_change", LOG_TAG.Life_Changes);
		log3.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.FACTION_1);
		log3.AddLogToDatabase(releaseLogAfter: true);
		Log log4 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "relation_change", LOG_TAG.Life_Changes);
		log4.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.FACTION_1);
		log4.AddLogToDatabase(releaseLogAfter: true);
		Messenger.Broadcast(FactionSignals.FACTION_IDEOLOGIES_CHANGED, base.owner);
		Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, base.owner);
	}

	public void OnFactionMemberDied(Character p_deadCharacter)
	{
		List<FactionIdeology> list = currentIdeologies;
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].OnFactionMemberDied(p_deadCharacter);
			}
		}
	}

	public FactionIdeology GetFactionIdeology(FACTION_IDEOLOGY p_ideologyType)
	{
		return base.owner.factionType.GetFactionIdeology(p_ideologyType);
	}

	public void SetLastRemovedNonReligionCrimeTypeForEvilOrPsychopath(CRIME_TYPE p_crimeType)
	{
		lastRemovedNonReligionCrimeTypeForEvilOrPsychopath = p_crimeType;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}

	public void OnDisbandFaction()
	{
	}
}
