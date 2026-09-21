using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Interrupts;
using UtilityScripts;

public class CrimeComponent : CharacterComponent
{
	public List<CrimeData> witnessedCrimes { get; private set; }

	public List<CrimeData> reportedCrimes { get; private set; }

	public List<CrimeData> activeCrimes { get; protected set; }

	public List<CrimeData> previousCrimes { get; protected set; }

	public bool hasReportedCrime { get; private set; }

	public GameDate dateToReportCrimeAgain { get; private set; }

	public CrimeComponent()
	{
		witnessedCrimes = new List<CrimeData>();
		reportedCrimes = new List<CrimeData>();
		activeCrimes = new List<CrimeData>();
		previousCrimes = new List<CrimeData>();
	}

	public CrimeComponent(SaveDataCrimeComponent data)
	{
		witnessedCrimes = new List<CrimeData>();
		reportedCrimes = new List<CrimeData>();
		activeCrimes = new List<CrimeData>();
		previousCrimes = new List<CrimeData>();
		hasReportedCrime = data.hasReportedCrime;
		dateToReportCrimeAgain = data.dateToReportCrimeAgain;
	}

	public void OnCharacterDied()
	{
		RemoveAllActiveCrimes(removeTrait: true);
	}

	public void OnFactionActiveStateChanged(Faction p_faction)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.IsWantedBy(p_faction) && crimeData.RemoveFactionThatConsidersWanted(p_faction) && crimeData.factionsThatConsidersWanted.Count == 0)
			{
				crimeData.OnCrimeRemoved();
				previousCrimes.Add(crimeData);
				activeCrimes.RemoveAt(i);
				Messenger.Broadcast(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, base.owner, crimeData);
				i--;
			}
		}
		ProcessAfterRemovingCrime();
		if (activeCrimes.Count <= 0)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Criminal");
		}
	}

	public void SubscribeToPermanentSignals()
	{
		Messenger.AddListener<CrimeData>(CharacterSignals.CRIME_REMOVED_FROM_DATABASE, OnCrimeRemovedFromDatabase);
	}

	public void UnsubscribeToPermanentSignals()
	{
		Messenger.RemoveListener<CrimeData>(CharacterSignals.CRIME_REMOVED_FROM_DATABASE, OnCrimeRemovedFromDatabase);
	}

	private void OnCrimeRemovedFromDatabase(CrimeData p_crime)
	{
		p_crime.OnCrimeRemoved();
		activeCrimes.Remove(p_crime);
		previousCrimes.Remove(p_crime);
		witnessedCrimes.Remove(p_crime);
		reportedCrimes.Remove(p_crime);
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		List<CrimeData> list = RuinarchListPool<CrimeData>.Claim();
		if (witnessedCrimes.Count > 0)
		{
			list.AddRange(witnessedCrimes);
		}
		if (reportedCrimes.Count > 0)
		{
			list.AddRange(reportedCrimes);
		}
		if (previousCrimes.Count > 0)
		{
			list.AddRange(previousCrimes);
		}
		if (activeCrimes.Count > 0)
		{
			list.AddRange(activeCrimes);
		}
		List<CrimeData> list2 = RuinarchListPool<CrimeData>.Claim();
		for (int i = 0; i < list.Count; i++)
		{
			CrimeData crimeData = list[i];
			if (crimeData.IsCrimeDataInvalid())
			{
				witnessedCrimes.Remove(crimeData);
				reportedCrimes.Remove(crimeData);
				previousCrimes.Remove(crimeData);
				activeCrimes.Remove(crimeData);
				list2.Add(crimeData);
			}
			else if (crimeData.crime is ActualGoapNode actualGoapNode)
			{
				actualGoapNode.DisconnectFromCharacter(p_character);
				if (actualGoapNode.IsNodeObjectInvalid() || actualGoapNode.IsCharacterReferenced(p_character))
				{
					witnessedCrimes.Remove(crimeData);
					reportedCrimes.Remove(crimeData);
					previousCrimes.Remove(crimeData);
					activeCrimes.Remove(crimeData);
					list2.Add(crimeData);
				}
			}
			else if (crimeData.crime is InterruptHolder interruptHolder)
			{
				interruptHolder.DisconnectFromCharacter(p_character);
				if (interruptHolder.IsCharacterReferenced(p_character) || interruptHolder.IsImportantDataNull())
				{
					witnessedCrimes.Remove(crimeData);
					reportedCrimes.Remove(crimeData);
					previousCrimes.Remove(crimeData);
					activeCrimes.Remove(crimeData);
					list2.Add(crimeData);
				}
			}
		}
		RuinarchListPool<CrimeData>.Release(list);
		for (int j = 0; j < list2.Count; j++)
		{
			CrimeData crimeData2 = list2[j];
			if (crimeData2.criminal != null)
			{
				Messenger.Broadcast(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, crimeData2.criminal, crimeData2);
			}
			DatabaseManager.Instance.crimeDatabase.RemoveCrime(crimeData2);
		}
		RuinarchListPool<CrimeData>.Release(list2);
	}

	public void AddWitnessedCrime(CrimeData data)
	{
		witnessedCrimes.Add(data);
	}

	public void RemoveWitnessedCrime(CrimeData data)
	{
		witnessedCrimes.Remove(data);
	}

	public void AddReportedCrime(CrimeData data)
	{
		reportedCrimes.Add(data);
	}

	public bool IsReported(CrimeData data)
	{
		if (data.isRemoved)
		{
			return true;
		}
		return reportedCrimes.Contains(data);
	}

	public bool HasUnreportedCrime()
	{
		for (int i = 0; i < witnessedCrimes.Count; i++)
		{
			CrimeData item = witnessedCrimes[i];
			if (!reportedCrimes.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasUnreportedCrimeOf(Character criminal)
	{
		for (int i = 0; i < witnessedCrimes.Count; i++)
		{
			CrimeData crimeData = witnessedCrimes[i];
			if (crimeData.criminal == criminal && !reportedCrimes.Contains(crimeData))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanCreateReportCrimeJob(Character actor, IPointOfInterest target, CrimeData crimeData, ICrimeable crime)
	{
		string opinionLabel = base.owner.relationshipContainer.GetOpinionLabel(actor);
		CRIME_SEVERITY crimeSeverity = crimeData.crimeSeverity;
		if (opinionLabel == "Close Friend" && crimeSeverity != CRIME_SEVERITY.Heinous)
		{
			return false;
		}
		if (opinionLabel == "Friend" && crimeSeverity != CRIME_SEVERITY.Heinous && crimeSeverity != CRIME_SEVERITY.Serious)
		{
			return false;
		}
		if (base.owner.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor) && crimeSeverity != CRIME_SEVERITY.Heinous)
		{
			return false;
		}
		if (base.owner.characterClass.className == "Shaman" && (opinionLabel == "Close Friend" || opinionLabel == "Friend") && (crimeData.crimeType == CRIME_TYPE.Vampire || crimeData.crimeType == CRIME_TYPE.Werewolf))
		{
			return false;
		}
		return true;
	}

	public CrimeData GetCrimeDataOf(ICrimeable crime)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crime == crime)
			{
				return crimeData;
			}
		}
		return null;
	}

	public CrimeData GetExistingActiveCrimeData(IPointOfInterest p_target, CRIME_TYPE p_crimeType)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (p_crimeType == CRIME_TYPE.Vampire || p_crimeType == CRIME_TYPE.Werewolf)
			{
				if (crimeData.crimeType == p_crimeType)
				{
					return crimeData;
				}
			}
			else if (crimeData.target == p_target && crimeData.crimeType == p_crimeType)
			{
				return crimeData;
			}
		}
		return null;
	}

	public CrimeData AddCrime(CRIME_TYPE crimeType, CRIME_SEVERITY crimeSeverity, ICrimeable crime, Character criminal, IPointOfInterest target, Faction targetFaction, REACTION_STATUS reactionStatus)
	{
		RemoveAllUnwantedCrimesBeforeAddingNewOne();
		CrimeData crimeData = new CrimeData(crimeType, crimeSeverity, crime, criminal, target, targetFaction);
		activeCrimes.Add(crimeData);
		crimeData.OnCrimeAdded();
		ProcessAfterAddingCrime();
		return crimeData;
	}

	private void RemoveAllUnwantedCrimesBeforeAddingNewOne()
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (!crimeData.HasWanted() && crimeData.hasAuthoritiesReachedADecision && !crimeData.IsThereWitnessThatHasCurrentReportCrimeForThis())
			{
				crimeData.OnCrimeRemoved();
				activeCrimes.RemoveAt(i);
				Messenger.Broadcast(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, base.owner, crimeData);
				DatabaseManager.Instance.crimeDatabase.RemoveCrime(crimeData);
				i--;
			}
		}
		ProcessAfterRemovingCrime();
	}

	public void RemoveAllCrimesWantedBy(Faction faction)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.IsWantedBy(faction))
			{
				crimeData.OnCrimeRemoved();
				previousCrimes.Add(crimeData);
				activeCrimes.RemoveAt(i);
				Messenger.Broadcast(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, base.owner, crimeData);
				i--;
			}
		}
		ProcessAfterRemovingCrime();
		if (activeCrimes.Count <= 0)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Criminal");
		}
	}

	public void RemoveAllActiveCrimes(bool removeTrait)
	{
		while (activeCrimes.Count > 0)
		{
			CrimeData crimeData = activeCrimes[0];
			crimeData.OnCrimeRemoved();
			previousCrimes.Add(crimeData);
			activeCrimes.RemoveAt(0);
			Messenger.Broadcast(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, base.owner, crimeData);
		}
		ProcessAfterRemovingCrime();
		if (removeTrait)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Criminal");
		}
	}

	public bool RemoveCrime(CrimeData crimeData)
	{
		if (activeCrimes.Remove(crimeData))
		{
			crimeData.OnCrimeRemoved();
			previousCrimes.Add(crimeData);
			if (activeCrimes.Count <= 0)
			{
				base.owner.traitContainer.RemoveTrait(base.owner, "Criminal");
			}
			ProcessAfterRemovingCrime();
			Messenger.Broadcast(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, base.owner, crimeData);
			return true;
		}
		return false;
	}

	public bool HasCrime(CRIME_SEVERITY severity)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			if (activeCrimes[i].crimeSeverity == severity)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCrime(CRIME_SEVERITY severity1, CRIME_SEVERITY severity2)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeSeverity == severity1 || crimeData.crimeSeverity == severity2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCrime(CRIME_TYPE p_type)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			if (activeCrimes[i].crimeType == p_type)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCrime(CRIME_TYPE p_type1, CRIME_TYPE p_type2)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeType == p_type1 || crimeData.crimeType == p_type2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCrime(CRIME_TYPE p_type1, CRIME_TYPE p_type2, CRIME_TYPE p_type3)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeType == p_type1 || crimeData.crimeType == p_type2 || crimeData.crimeType == p_type3)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCrime(CRIME_TYPE p_type1, CRIME_TYPE p_type2, CRIME_TYPE p_type3, CRIME_TYPE p_type4, CRIME_TYPE p_type5)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeType == p_type1 || crimeData.crimeType == p_type2 || crimeData.crimeType == p_type3 || crimeData.crimeType == p_type4 || crimeData.crimeType == p_type5)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsWantedBy(Faction faction)
	{
		if (faction == null)
		{
			return false;
		}
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			if (activeCrimes[i].IsWantedBy(faction))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsWantedBy(Faction faction, CRIME_TYPE p_type1)
	{
		if (faction == null)
		{
			return false;
		}
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeType == p_type1 && crimeData.IsWantedBy(faction))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsWantedBy(Faction faction, CRIME_TYPE p_type1, CRIME_TYPE p_type2, CRIME_TYPE p_type3, CRIME_TYPE p_type4, CRIME_TYPE p_type5)
	{
		if (faction == null)
		{
			return false;
		}
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if ((crimeData.crimeType == p_type1 || crimeData.crimeType == p_type2 || crimeData.crimeType == p_type3 || crimeData.crimeType == p_type4 || crimeData.crimeType == p_type5) && crimeData.IsWantedBy(faction))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsWantedBy(Faction faction, CRIME_TYPE p_type1, CRIME_TYPE p_type2, CRIME_TYPE p_type3, CRIME_TYPE p_type4)
	{
		if (faction == null)
		{
			return false;
		}
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if ((crimeData.crimeType == p_type1 || crimeData.crimeType == p_type2 || crimeData.crimeType == p_type3 || crimeData.crimeType == p_type4) && crimeData.IsWantedBy(faction))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWantedCrime()
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			if (activeCrimes[i].HasWanted())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWantedCrimeType(CRIME_TYPE crimeType1, CRIME_TYPE crimeType2, CRIME_TYPE crimeType3, CRIME_TYPE crimeType4)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if ((crimeData.crimeType == crimeType1 || crimeData.crimeType == crimeType2 || crimeData.crimeType == crimeType3 || crimeData.crimeType == crimeType4) && crimeData.HasWanted())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWantedCrimeBy(Faction faction, CRIME_STATUS status)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeStatus == status && crimeData.IsWantedBy(faction))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWantedCrimeBy(Faction faction, CRIME_SEVERITY severity1, CRIME_SEVERITY severity2)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if ((crimeData.crimeSeverity == severity1 || crimeData.crimeSeverity == severity2) && crimeData.IsWantedBy(faction))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsTargetOfACrime(IPointOfInterest poi)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			if (activeCrimes[i].target == poi)
			{
				return true;
			}
		}
		return false;
	}

	public List<CrimeData> GetListOfCrimesWantedBy(Faction faction)
	{
		List<CrimeData> list = null;
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.IsWantedBy(faction))
			{
				if (list == null)
				{
					list = RuinarchListPool<CrimeData>.Claim();
				}
				list.Add(crimeData);
			}
		}
		return list;
	}

	public List<string> GetListOfCrimeNamesWantedByNoDuplicates(Faction faction)
	{
		List<string> list = null;
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.IsWantedBy(faction))
			{
				if (list == null)
				{
					list = new List<string>();
				}
				string item = crimeData.crimeType.ToStringEnumWithSpace();
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public List<CrimeData> GetListOfCrimesWantedBy(Faction faction, CRIME_STATUS status)
	{
		List<CrimeData> list = null;
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeStatus == status && crimeData.IsWantedBy(faction))
			{
				if (list == null)
				{
					list = new List<CrimeData>();
				}
				list.Add(crimeData);
			}
		}
		return list;
	}

	public CrimeData GetFirstCrimeWantedBy(Faction faction, CRIME_STATUS status)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeStatus == status && crimeData.IsWantedBy(faction))
			{
				return crimeData;
			}
		}
		return null;
	}

	public CrimeData GetFirstCrimeWantedBy(Faction faction)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.IsWantedBy(faction))
			{
				return crimeData;
			}
		}
		return null;
	}

	public void SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(Faction faction, CRIME_STATUS status, Character judge)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeStatus == CRIME_STATUS.Unpunished && crimeData.IsWantedBy(faction))
			{
				crimeData.SetCrimeStatus(status);
				crimeData.SetJudge(judge);
			}
		}
	}

	public bool IsCrimeAlreadyWitnessedBy(Character character, ICrimeable crime)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crime == crime)
			{
				return crimeData.IsWitness(character);
			}
		}
		for (int j = 0; j < previousCrimes.Count; j++)
		{
			CrimeData crimeData2 = previousCrimes[j];
			if (crimeData2.crime == crime)
			{
				return crimeData2.IsWitness(character);
			}
		}
		return false;
	}

	public bool IsCrimeAlreadyWitnessedBy(Character character, CRIME_TYPE crimeType)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeType == crimeType)
			{
				return crimeData.IsWitness(character);
			}
		}
		return false;
	}

	public bool IsAnActiveCrimeWitnessedBy(Character p_witness)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			if (activeCrimes[i].IsWitness(p_witness))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnActiveCrimeWitnessedBy(Character character, CRIME_STATUS p_status)
	{
		for (int i = 0; i < activeCrimes.Count; i++)
		{
			CrimeData crimeData = activeCrimes[i];
			if (crimeData.crimeStatus == p_status && crimeData.IsWitness(character))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE crimeType, Character except = null)
	{
		if ((bool)base.owner.marker)
		{
			for (int i = 0; i < base.owner.marker.inVisionCharacters.Count; i++)
			{
				Character character = base.owner.marker.inVisionCharacters[i];
				if (character != base.owner && except != character && !(character is Animal) && character.petComponent.petOwner == null && !base.owner.IsHostileWith(character) && character.limiterComponent.canWitness && (base.owner.marker.visionColliderComponent.IsTheSameStructureOrSameOpenSpaceWithPOI(character) || base.owner.marker.IsCharacterInLineOfSightWith(character)) && CrimeManager.Instance.GetCrimeSeverity(character, base.owner, base.owner, crimeType).IsConsideredACrime())
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public void FleeToAllNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(Character character, CRIME_TYPE crimeType, string reason = "")
	{
		if (!character.marker)
		{
			return;
		}
		for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
		{
			Character character2 = character.marker.inVisionCharacters[i];
			if (character2 != character && !character.IsHostileWith(character2) && CrimeManager.Instance.GetCrimeSeverity(character2, character, character, crimeType).IsConsideredACrime())
			{
				if (string.IsNullOrEmpty(reason))
				{
					reason = "Avoiding Witnesses";
				}
				character.combatComponent.Flight(character2, reason);
			}
		}
	}

	public void FleeToAllVillagerInRangeThatConsidersCrimeTypeACrime(Character character, CRIME_TYPE crimeType, string reason = "")
	{
		if (!character.marker)
		{
			return;
		}
		for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
		{
			Character character2 = character.marker.inVisionCharacters[i];
			if (character2 != character && character.race.IsSapient() && CrimeManager.Instance.GetCrimeSeverity(character2, character, character, crimeType).IsConsideredACrime())
			{
				if (string.IsNullOrEmpty(reason))
				{
					reason = "Avoiding Witnesses";
				}
				character.combatComponent.Flight(character2, reason);
			}
		}
	}

	public void SetHasReportedCrime(bool state)
	{
		if (hasReportedCrime == state)
		{
			return;
		}
		hasReportedCrime = state;
		if (hasReportedCrime)
		{
			dateToReportCrimeAgain = GameManager.Instance.Today().AddTicks(96);
			SchedulingManager.Instance.AddEntry(dateToReportCrimeAgain, delegate
			{
				SetHasReportedCrime(state: false);
			}, base.owner);
		}
	}

	private void ProcessAfterAddingCrime()
	{
		if (!base.owner.isDead && HasCrime(CRIME_SEVERITY.Heinous, CRIME_SEVERITY.Serious) && base.owner.faction == FactionManager.Instance.vagrantFaction)
		{
			PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<BanditsEvent>().AdjustNumberOfAliveVagrantsWithSeriousOrHeinousCrime(1);
		}
	}

	private void ProcessAfterRemovingCrime()
	{
		if (!base.owner.isDead && !HasCrime(CRIME_SEVERITY.Heinous, CRIME_SEVERITY.Serious) && base.owner.faction == FactionManager.Instance.vagrantFaction)
		{
			PlayerManager.Instance?.player?.playerSkillComponent.GetPrismEvent<BanditsEvent>().AdjustNumberOfAliveVagrantsWithSeriousOrHeinousCrime(-1);
		}
	}

	public void LoadReferences(SaveDataCrimeComponent data)
	{
		if (data.witnessedCrimes != null)
		{
			for (int i = 0; i < data.witnessedCrimes.Count; i++)
			{
				CrimeData crimeByPersistentID = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.witnessedCrimes[i]);
				witnessedCrimes.Add(crimeByPersistentID);
			}
		}
		if (data.reportedCrimes != null)
		{
			for (int j = 0; j < data.reportedCrimes.Count; j++)
			{
				CrimeData crimeByPersistentID2 = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.reportedCrimes[j]);
				reportedCrimes.Add(crimeByPersistentID2);
			}
		}
		if (data.activeCrimes != null)
		{
			for (int k = 0; k < data.activeCrimes.Count; k++)
			{
				CrimeData crimeByPersistentID3 = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.activeCrimes[k]);
				activeCrimes.Add(crimeByPersistentID3);
			}
		}
		if (data.previousCrimes != null)
		{
			for (int l = 0; l < data.previousCrimes.Count; l++)
			{
				CrimeData crimeByPersistentID4 = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.previousCrimes[l]);
				previousCrimes.Add(crimeByPersistentID4);
			}
		}
	}

	public void LoadReferencesMainThread(SaveDataCrimeComponent data)
	{
		if (hasReportedCrime)
		{
			SchedulingManager.Instance.AddEntry(dateToReportCrimeAgain, delegate
			{
				SetHasReportedCrime(state: false);
			}, base.owner);
		}
	}

	public override void CleanUp()
	{
		base.CleanUp();
		witnessedCrimes?.Clear();
		reportedCrimes?.Clear();
		activeCrimes?.Clear();
		previousCrimes?.Clear();
	}

	public void CleanUpAllCrimesByThisCharacter()
	{
		List<CrimeData> list = RuinarchListPool<CrimeData>.Claim();
		if (activeCrimes.Count > 0)
		{
			list.AddRange(activeCrimes);
		}
		if (previousCrimes.Count > 0)
		{
			list.AddRange(previousCrimes);
		}
		for (int i = 0; i < list.Count; i++)
		{
			CrimeData crimeData = list[i];
			if (crimeData.criminal != null)
			{
				Messenger.Broadcast(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, crimeData.criminal, crimeData);
			}
			DatabaseManager.Instance.crimeDatabase.RemoveCrime(crimeData);
		}
		RuinarchListPool<CrimeData>.Release(list);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < witnessedCrimes.Count; i++)
		{
			witnessedCrimes[i].CheckIfStructureIsStillReferenced(p_structure);
		}
		for (int j = 0; j < reportedCrimes.Count; j++)
		{
			reportedCrimes[j].CheckIfStructureIsStillReferenced(p_structure);
		}
		for (int k = 0; k < activeCrimes.Count; k++)
		{
			activeCrimes[k].CheckIfStructureIsStillReferenced(p_structure);
		}
		for (int l = 0; l < previousCrimes.Count; l++)
		{
			previousCrimes[l].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		for (int i = 0; i < witnessedCrimes.Count; i++)
		{
			witnessedCrimes[i].CheckIfCharacterIsStillReferenced(p_character);
		}
		for (int j = 0; j < reportedCrimes.Count; j++)
		{
			reportedCrimes[j].CheckIfCharacterIsStillReferenced(p_character);
		}
		for (int k = 0; k < activeCrimes.Count; k++)
		{
			activeCrimes[k].CheckIfCharacterIsStillReferenced(p_character);
		}
		for (int l = 0; l < previousCrimes.Count; l++)
		{
			previousCrimes[l].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
