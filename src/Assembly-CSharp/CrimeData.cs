using System;
using System.Collections.Generic;
using Crime_System;
using Inner_Maps.Location_Structures;
using Interrupts;
using Traits;
using UtilityScripts;

public class CrimeData : ISavable
{
	public string persistentID { get; private set; }

	public CRIME_SEVERITY crimeSeverity { get; private set; }

	public CRIME_TYPE crimeType { get; private set; }

	public CRIME_STATUS crimeStatus { get; private set; }

	public ICrimeable crime { get; private set; }

	public Character criminal { get; private set; }

	public IPointOfInterest target { get; private set; }

	public Faction targetFaction { get; private set; }

	public Character judge { get; private set; }

	public Character reporter { get; private set; }

	public List<Character> witnesses { get; private set; }

	public List<Faction> factionsThatConsidersWanted { get; private set; }

	public bool isRemoved { get; private set; }

	public bool hasAuthoritiesReachedADecision { get; private set; }

	public CrimeType crimeTypeObj => CrimeManager.Instance.GetCrimeType(crimeType);

	public OBJECT_TYPE objectType => OBJECT_TYPE.Crime;

	public Type serializedData => typeof(SaveDataCrimeData);

	public CrimeData(CRIME_TYPE crimeType, CRIME_SEVERITY crimeSeverity, ICrimeable crime, Character criminal, IPointOfInterest target, Faction targetFaction)
	{
		persistentID = Utilities.GetNewUniqueID();
		this.crimeType = crimeType;
		this.crimeSeverity = crimeSeverity;
		SetCrime(crime);
		this.criminal = criminal;
		this.target = target;
		this.targetFaction = targetFaction;
		witnesses = new List<Character>();
		factionsThatConsidersWanted = new List<Faction>();
		SetCrimeStatus(CRIME_STATUS.Unpunished);
		SubscribeToListeners();
		DatabaseManager.Instance.crimeDatabase.AddCrime(this);
	}

	public CrimeData(SaveDataCrimeData data)
	{
		witnesses = new List<Character>();
		factionsThatConsidersWanted = new List<Faction>();
		persistentID = data.persistentID;
		crimeSeverity = data.crimeSeverity;
		crimeType = data.crimeType;
		crimeStatus = data.crimeStatus;
		isRemoved = data.isRemoved;
		hasAuthoritiesReachedADecision = data.hasAuthoritiesReachedADecision;
		if (!isRemoved)
		{
			SubscribeToListeners();
		}
	}

	private void SetCrime(ICrimeable crime)
	{
		this.crime = crime;
		if (this.crime is ActualGoapNode actualGoapNode)
		{
			actualGoapNode.SetIsUsedAsCrime(p_state: true);
		}
	}

	private void SubscribeToListeners()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	private void UnsubscribeFromListeners()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	private void OnCharacterDied(Character character)
	{
		if (!isRemoved && IsWitness(character) && !HasWanted() && AreAllWitnessesDead())
		{
			criminal.crimeComponent.RemoveCrime(this);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "dead_witnesses", LOG_TAG.Crimes, LOG_TAG.Life_Changes);
			log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, crimeTypeObj.name, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		if (IsWitness(p_character))
		{
			witnesses.Remove(p_character);
		}
		if (judge == p_character)
		{
			judge = null;
		}
		if (reporter == p_character)
		{
			reporter = null;
		}
	}

	public void SetCrimeStatus(CRIME_STATUS status)
	{
		if (crimeStatus != status)
		{
			crimeStatus = status;
			if (crimeStatus == CRIME_STATUS.Unpunished)
			{
				criminal.SetHasUnresolvedCrime(state: true);
			}
			else
			{
				criminal.SetHasUnresolvedCrime(state: false);
			}
		}
	}

	public void SetJudge(Character character)
	{
		judge = character;
	}

	public void SetReporter(Character character)
	{
		reporter = character;
	}

	public string GetCrimeDataDescription()
	{
		string text = crimeTypeObj.name + " - " + crimeStatus.ToStringEnumWithSpace();
		text = text + "\n\t" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Wanted") + ": " + GetFactionsThatConsidersWantedAsText();
		return text + "\n\t" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Witnessed") + ": " + GetWitnessesAsText();
	}

	private string GetFactionsThatConsidersWantedAsText()
	{
		string text = "None";
		if (factionsThatConsidersWanted.Count > 0)
		{
			text = string.Empty;
			for (int i = 0; i < factionsThatConsidersWanted.Count; i++)
			{
				if (i > 0)
				{
					text += ", ";
				}
				text += factionsThatConsidersWanted[i].nameWithColor;
			}
		}
		return text;
	}

	private string GetWitnessesAsText()
	{
		string text = "None";
		if (witnesses.Count > 0)
		{
			text = string.Empty;
			for (int i = 0; i < witnesses.Count; i++)
			{
				if (i > 0)
				{
					text += ", ";
				}
				text += witnesses[i].name;
			}
		}
		return text;
	}

	public void OnCrimeAdded()
	{
	}

	public void OnCrimeRemoved()
	{
		isRemoved = true;
		UnsubscribeFromListeners();
	}

	public bool IsCrimeFabricated()
	{
		if (crime is ActualGoapNode actualGoapNode)
		{
			return actualGoapNode.isFabricated;
		}
		if (crime is InterruptHolder interruptHolder)
		{
			return interruptHolder.isRumor;
		}
		return false;
	}

	public bool IsThereWitnessThatHasCurrentReportCrimeForThis()
	{
		if (witnesses != null)
		{
			for (int i = 0; i < witnesses.Count; i++)
			{
				Character character = witnesses[i];
				if (character.isDead)
				{
					continue;
				}
				for (int j = 0; j < character.jobQueue.jobsInQueue.Count; j++)
				{
					if (character.jobQueue.jobsInQueue[j] is GoapPlanJob goapPlanJob && goapPlanJob.HasOtherDataRelatedTo(this))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void SetHasAuthoritiesReachedADecision(bool p_state)
	{
		hasAuthoritiesReachedADecision = p_state;
	}

	public bool IsWitness(Character character)
	{
		return witnesses.Contains(character);
	}

	public void AddWitness(Character character)
	{
		witnesses.Add(character);
		character.crimeComponent.AddWitnessedCrime(this);
	}

	public void RemoveWitness(Character character)
	{
		witnesses.Remove(character);
		character.crimeComponent.RemoveWitnessedCrime(this);
	}

	private bool AreAllWitnessesDead()
	{
		if (witnesses.Count > 0)
		{
			for (int i = 0; i < witnesses.Count; i++)
			{
				if (!witnesses[i].isDead)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void RemoveWitnessFromZap(Character character)
	{
		if (!isRemoved && IsWitness(character))
		{
			RemoveWitness(character);
			if (!HasWanted() && (AreAllWitnessesDead() || witnesses.Count <= 0))
			{
				criminal.crimeComponent.RemoveCrime(this);
			}
		}
	}

	public void AddFactionThatConsidersWanted(Faction faction)
	{
		if (factionsThatConsidersWanted.Contains(faction))
		{
			return;
		}
		factionsThatConsidersWanted.Add(faction);
		if (criminal.homeSettlement != null && criminal.partyComponent.hasParty && criminal.homeSettlement.owner == faction && !criminal.isDead)
		{
			criminal.interruptComponent.TriggerInterrupt(INTERRUPT.Removed_From_Party, criminal, "", null, "Removed_From_Party_Criminal");
		}
		if (criminal.isSettlementRuler && criminal.homeSettlement.owner == faction)
		{
			criminal.homeSettlement.SetRuler(null);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "no_longer_settlement_ruler", LOG_TAG.Crimes, LOG_TAG.Life_Changes);
			log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			criminal.logComponent.RegisterLog(log, releaseAfter: true);
		}
		if (faction.leader == criminal)
		{
			faction.SetLeader(null);
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "no_longer_faction_leader", LOG_TAG.Crimes, LOG_TAG.Life_Changes);
			log2.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			criminal.logComponent.RegisterLog(log2, releaseAfter: true);
		}
		if (target is Character crimeTarget && crime is ActualGoapNode actualGoapNode)
		{
			CRIME_SEVERITY cRIME_SEVERITY = faction.GetCrimeSeverity(criminal, target, crimeType);
			if (faction.factionType.type != FACTION_TYPE.Vagrants && criminal.faction != null && criminal.faction.factionType.type != FACTION_TYPE.Vagrants)
			{
				faction.CheckForWar(criminal.faction, cRIME_SEVERITY, criminal, crimeTarget, actualGoapNode);
			}
		}
		if (crimeType == CRIME_TYPE.Vampire)
		{
			Traits.Vampire traitOrStatus = criminal.traitContainer.GetTraitOrStatus<Traits.Vampire>("Vampire");
			if (traitOrStatus != null)
			{
				for (int i = 0; i < faction.characters.Count; i++)
				{
					Character character = faction.characters[i];
					if (character != criminal)
					{
						traitOrStatus.AddAwareCharacter(character);
					}
				}
			}
		}
		else if (crimeType == CRIME_TYPE.Werewolf)
		{
			LycanthropeData lycanData = criminal.lycanData;
			if (lycanData != null)
			{
				for (int j = 0; j < faction.characters.Count; j++)
				{
					Character character2 = faction.characters[j];
					if (character2 != criminal)
					{
						lycanData.AddAwareCharacter(character2);
					}
				}
			}
		}
		criminal.movementComponent.RedetermineFactionsToAvoid(criminal);
		Messenger.Broadcast(FactionSignals.BECOME_WANTED_CRIMINAL_OF_FACTION, faction, criminal, this);
	}

	public bool IsWantedBy(Faction faction)
	{
		return factionsThatConsidersWanted.Contains(faction);
	}

	public bool HasWanted()
	{
		return factionsThatConsidersWanted.Count > 0;
	}

	public bool RemoveFactionThatConsidersWanted(Faction p_faction)
	{
		return factionsThatConsidersWanted.Remove(p_faction);
	}

	public void LoadReferences(SaveDataCrimeData data)
	{
		if (!string.IsNullOrEmpty(data.crime))
		{
			if (data.crimableType == CRIMABLE_TYPE.Action)
			{
				crime = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.crime);
			}
			else if (data.crimableType == CRIMABLE_TYPE.Interrupt)
			{
				crime = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.crime);
			}
		}
		if (!string.IsNullOrEmpty(data.criminal))
		{
			criminal = CharacterManager.Instance.GetCharacterByPersistentID(data.criminal);
		}
		if (!string.IsNullOrEmpty(data.target))
		{
			if (data.targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				target = CharacterManager.Instance.GetCharacterByPersistentID(data.target);
			}
			else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				target = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.target);
			}
		}
		if (!string.IsNullOrEmpty(data.targetFaction))
		{
			targetFaction = FactionManager.Instance.GetFactionByPersistentID(data.targetFaction);
		}
		if (!string.IsNullOrEmpty(data.judge))
		{
			judge = CharacterManager.Instance.GetCharacterByPersistentID(data.judge);
		}
		if (!string.IsNullOrEmpty(data.reporter))
		{
			reporter = CharacterManager.Instance.GetCharacterByPersistentID(data.reporter);
		}
		for (int i = 0; i < data.witnesses.Count; i++)
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(data.witnesses[i]);
			if (characterByPersistentID != null)
			{
				witnesses.Add(characterByPersistentID);
			}
		}
		for (int j = 0; j < data.factionsThatConsidersWanted.Count; j++)
		{
			Faction factionByPersistentID = FactionManager.Instance.GetFactionByPersistentID(data.factionsThatConsidersWanted[j]);
			factionsThatConsidersWanted.Add(factionByPersistentID);
		}
	}

	public void TryTriggerGrudgeAgainstJudgeOrReporter()
	{
		if (GameUtilities.RollChance(50))
		{
			if (!TriggerGrudgeAgainstJudge())
			{
				TriggerGrudgeAgainstReporter();
			}
		}
		else if (!TriggerGrudgeAgainstReporter())
		{
			TriggerGrudgeAgainstJudge();
		}
	}

	private bool TriggerGrudgeAgainstJudge()
	{
		if (judge != null && criminal.relationshipContainer.SetHasGrudgeAgainst(criminal, judge, p_state: true))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Grudge", "Relationships_Table", "Grudge_Punished_Fabricated_Judge", LOG_TAG.Social, LOG_TAG.Crimes);
			log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(judge, judge.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			return true;
		}
		return false;
	}

	private bool TriggerGrudgeAgainstReporter()
	{
		if (reporter != null && criminal.relationshipContainer.SetHasGrudgeAgainst(criminal, reporter, p_state: true))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Grudge", "Relationships_Table", "Grudge_Punished_Fabricated_Reporter", LOG_TAG.Social, LOG_TAG.Crimes);
			log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(reporter, reporter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			return true;
		}
		return false;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		IsStructureReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		IsCharacterReferenced(p_character);
	}

	public bool IsStructureReferenced(LocationStructure p_structure)
	{
		if (crime.IsStructureReferenced(p_structure))
		{
			return true;
		}
		return false;
	}

	public bool IsCharacterReferenced(Character p_character)
	{
		if (criminal == p_character)
		{
			return true;
		}
		if (target == p_character)
		{
			return true;
		}
		if (judge == p_character)
		{
			return true;
		}
		if (reporter == p_character)
		{
			return true;
		}
		if (witnesses.Contains(p_character))
		{
			return true;
		}
		if (crime.IsCharacterReferenced(p_character))
		{
			return true;
		}
		return false;
	}

	public bool IsCrimeDataInvalid()
	{
		if (crime == null)
		{
			return true;
		}
		if (crime.IsImportantDataNull())
		{
			return true;
		}
		if (criminal == null)
		{
			return true;
		}
		if (target == null)
		{
			return true;
		}
		return false;
	}

	public void CleanUp()
	{
		if (crime is ActualGoapNode actualGoapNode)
		{
			actualGoapNode.SetIsUsedAsCrime(p_state: false);
			if (actualGoapNode.isSupposedToBeInPool)
			{
				actualGoapNode.ProcessReturnToPool();
			}
		}
		else if (crime is InterruptHolder { isSupposedToBeInPool: not false } interruptHolder)
		{
			ObjectPoolManager.Instance.TryReturnInterruptToPool(interruptHolder);
		}
		crime = null;
		criminal = null;
		target = null;
		targetFaction = null;
		judge = null;
		reporter = null;
		witnesses?.Clear();
		factionsThatConsidersWanted?.Clear();
		UnsubscribeFromListeners();
	}
}
