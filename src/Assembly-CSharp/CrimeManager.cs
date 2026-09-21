using System;
using System.Collections.Generic;
using Crime_System;
using Interrupts;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CrimeManager : BaseMonoBehaviour
{
	public static CrimeManager Instance;

	private Dictionary<CRIME_SEVERITY, CrimeSeverity> _crimeSeverities;

	private Dictionary<CRIME_TYPE, CrimeType> _crimeTypes;

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public void Initialize()
	{
		ConstructCrimeSeverities();
		ConstructCrimeTypes();
	}

	private void ConstructCrimeSeverities()
	{
		_crimeSeverities = new Dictionary<CRIME_SEVERITY, CrimeSeverity>();
		CRIME_SEVERITY[] enumValues = CollectionUtilities.GetEnumValues<CRIME_SEVERITY>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			CRIME_SEVERITY key = enumValues[i];
			Type type = Type.GetType("Crime_System." + Utilities.NotNormalizedConversionEnumToStringNoSpaces(key.ToString()) + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			if (type != null)
			{
				CrimeSeverity value = Activator.CreateInstance(type) as CrimeSeverity;
				_crimeSeverities.Add(key, value);
			}
		}
	}

	private void ConstructCrimeTypes()
	{
		_crimeTypes = new Dictionary<CRIME_TYPE, CrimeType>();
		CRIME_TYPE[] enumValues = CollectionUtilities.GetEnumValues<CRIME_TYPE>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			CRIME_TYPE key = enumValues[i];
			Type type = Type.GetType("Crime_System." + Utilities.NotNormalizedConversionEnumToStringNoSpaces(key.ToString()) + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			if (type != null)
			{
				CrimeType value = Activator.CreateInstance(type) as CrimeType;
				_crimeTypes.Add(key, value);
			}
		}
	}

	private bool ShouldCreateCrimeStatus(CRIME_SEVERITY severity)
	{
		if (severity == CRIME_SEVERITY.Infraction || severity == CRIME_SEVERITY.None || severity == CRIME_SEVERITY.Unapplicable)
		{
			return false;
		}
		return true;
	}

	public string MakeCharacterACriminal(CRIME_TYPE crimeType, CRIME_SEVERITY crimeSeverity, ICrimeable crime, Character witness, Character criminal, IPointOfInterest target, Faction targetFaction, REACTION_STATUS reactionStatus, Criminal criminalTrait)
	{
		if (criminalTrait == null)
		{
			criminal.traitContainer.AddTrait(criminal, "Criminal");
		}
		CrimeData crimeData = criminal.crimeComponent.GetCrimeDataOf(crime);
		if (crimeData == null)
		{
			if (crime is InterruptHolder interruptHolder)
			{
				interruptHolder.SetShouldNotBeObjectPooled(state: true);
			}
			crimeData = criminal.crimeComponent.AddCrime(crimeType, crimeSeverity, crime, criminal, target, targetFaction, reactionStatus);
			CrimeType crimeTypeObj = crimeData.crimeTypeObj;
			if (!(crime is ActualGoapNode { isAssumption: not false }))
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "become_criminal", LOG_TAG.Crimes, LOG_TAG.Life_Changes);
				log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(null, crimeTypeObj.localizedAccuseText, LOG_IDENTIFIER.STRING_1);
				log.AddToFillers(witness, witness.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFrom(criminal, log);
				LogPool.Release(log);
			}
			Messenger.Broadcast(CharacterSignals.CHARACTER_ACCUSED_OF_CRIME, criminal, crimeType, witness);
		}
		return ProcessWitnessCrime(witness, crimeData);
	}

	private string ProcessWitnessCrime(Character p_witness, CrimeData p_crimeData)
	{
		string result = string.Empty;
		if (!p_crimeData.IsWitness(p_witness))
		{
			p_crimeData.AddWitness(p_witness);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (p_witness.isNormalCharacter && p_witness.faction != null && p_witness.faction.isMajorNonPlayer && (p_witness.isFactionLeader || p_witness.isSettlementRuler))
			{
				flag = true;
				if (!p_crimeData.criminal.crimeComponent.IsWantedBy(p_witness.faction, p_crimeData.crimeType))
				{
					flag2 = WantedOrNotDecisionMaking(p_witness, p_crimeData.criminal, p_witness.faction, p_crimeData, p_crimeData.crimeSeverity);
				}
				else
				{
					flag3 = true;
				}
			}
			if (flag)
			{
				result = (flag3 ? "Already_Wanted_Same_Crime" : ((!flag2) ? "Will_Not_Punish_Report_Crime" : ((p_crimeData.crimeType != CRIME_TYPE.Animal_Killing) ? "Wanted_Decision_Crime" : "Wanted_Decision_Animal_Killing")));
			}
			else if (!p_crimeData.isRemoved && !p_witness.crimeComponent.IsReported(p_crimeData))
			{
				result = ((!p_witness.jobComponent.TryCreateReportCrimeJob(p_crimeData.criminal, p_crimeData.target, p_crimeData, p_crimeData.crime)) ? "Will_Not_Punish_Report_Crime" : ((p_crimeData.crimeType != CRIME_TYPE.Animal_Killing) ? "Will_Report_Crime" : "Will_Report_Animal_Killing"));
			}
		}
		return result;
	}

	public bool WantedOrNotDecisionMaking(Character authority, Character criminal, Faction authorityFaction, CrimeData crimeData, CRIME_SEVERITY crimeSeverity)
	{
		string opinionLabel = authority.relationshipContainer.GetOpinionLabel(criminal);
		string empty = string.Empty;
		string value = string.Empty;
		if (crimeSeverity == CRIME_SEVERITY.Heinous)
		{
			empty = "wanted";
		}
		else if (opinionLabel == "Close Friend")
		{
			value = LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", "Close Friend");
			empty = ((crimeSeverity != CRIME_SEVERITY.Serious) ? "not_wanted" : (GameUtilities.RollChance(75) ? "wanted" : "not_wanted"));
		}
		else
		{
			bool flag = authority.relationshipContainer.IsFamilyMember(criminal);
			bool flag2 = authority.relationshipContainer.IsLoverOrAffair(criminal);
			if ((flag || flag2) && !authority.relationshipContainer.IsEnemiesWith(criminal))
			{
				value = (flag ? LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", "Relative") : LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", "Lover"));
				empty = ((crimeSeverity != CRIME_SEVERITY.Serious) ? "not_wanted" : (GameUtilities.RollChance(75) ? "wanted" : "not_wanted"));
			}
			else if (opinionLabel == "Friend")
			{
				value = LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", "Friend");
				empty = ((UnityEngine.Random.Range(0, 100) >= authority.relationshipContainer.GetTotalOpinion(criminal)) ? "wanted" : "not_wanted");
			}
			else
			{
				empty = "wanted";
			}
		}
		if (empty == "wanted")
		{
			crimeData.AddFactionThatConsidersWanted(authorityFaction);
		}
		if (empty != string.Empty)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", empty, LOG_TAG.Life_Changes, LOG_TAG.Crimes, LOG_TAG.Major);
			log.AddToFillers(authority, authority.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddToFillers(null, crimeData.crimeTypeObj.localizedName, LOG_IDENTIFIER.STRING_1);
			if (empty == "not_wanted")
			{
				log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_2);
			}
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(criminal, log);
			LogPool.Release(log);
		}
		crimeData.SetHasAuthoritiesReachedADecision(p_state: true);
		return empty == "wanted";
	}

	public string ReactToCrime(Character witness, Character actor, IPointOfInterest target, Faction targetFaction, CRIME_TYPE crimeType, ICrimeable crime, REACTION_STATUS reactionStatus)
	{
		string result = string.Empty;
		if (witness == actor)
		{
			return result;
		}
		if (crimeType == CRIME_TYPE.Unset || crimeType == CRIME_TYPE.None)
		{
			return result;
		}
		if (witness.faction != null && actor.faction != null)
		{
			bool flag = false;
			if ((reactionStatus != REACTION_STATUS.WITNESSED) ? witness.IsHostileWithCheckingForInformed(actor) : witness.IsHostileWith(actor))
			{
				return result;
			}
		}
		if (actor.crimeComponent.IsCrimeAlreadyWitnessedBy(witness, crime))
		{
			return result;
		}
		CrimeType crimeType2 = GetCrimeType(crimeType);
		CRIME_SEVERITY cRIME_SEVERITY = CRIME_SEVERITY.None;
		CRIME_SEVERITY personalCrimeSeverity = GetPersonalCrimeSeverity(witness, actor, target, crimeType);
		CRIME_SEVERITY factionCrimeSeverity = GetFactionCrimeSeverity(witness, actor, target, crimeType);
		cRIME_SEVERITY = ((personalCrimeSeverity != CRIME_SEVERITY.Unapplicable) ? personalCrimeSeverity : factionCrimeSeverity);
		if (cRIME_SEVERITY.IsConsideredACrime())
		{
			if ((actor.isSettlementRuler || actor.isFactionLeader) && actor.faction == witness.faction)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "actor_leader_do_nothing", LOG_TAG.Crimes);
				log.AddToFillers(witness, witness.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
				return result;
			}
			string text = GetCrimeSeverity(cRIME_SEVERITY).EffectAndReaction(witness, actor, target, crimeType2, crime, reactionStatus);
			if (text != string.Empty)
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "emotions_crime_" + reactionStatus.ToStringEnumLowercase(), LOG_TAG.Crimes, LOG_TAG.Life_Changes);
				switch (reactionStatus)
				{
				case REACTION_STATUS.INFORMED:
					log2.AddTag(LOG_TAG.Informed);
					break;
				case REACTION_STATUS.WITNESSED:
					log2.AddTag(LOG_TAG.Witnessed);
					break;
				}
				log2.AddToFillers(witness, witness.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddToFillers(null, Utilities.GetFirstFewEmotionsAndComafy(text, 2), LOG_IDENTIFIER.STRING_1);
				log2.AddLogToDatabase(releaseLogAfter: true);
			}
			if (actor.isDead)
			{
				return result;
			}
			crimeType2.ProcessReactionOnAccuse(witness, actor, target);
			CrimeData existingActiveCrimeData = actor.crimeComponent.GetExistingActiveCrimeData(target, crimeType);
			if (existingActiveCrimeData != null)
			{
				result = ProcessWitnessCrime(witness, existingActiveCrimeData);
			}
			else if (ShouldCreateCrimeStatus(cRIME_SEVERITY))
			{
				Criminal criminalTrait = null;
				if (actor.traitContainer.HasTrait("Criminal"))
				{
					criminalTrait = actor.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
				}
				result = MakeCharacterACriminal(crimeType, cRIME_SEVERITY, crime, witness, actor, target, targetFaction, reactionStatus, criminalTrait);
			}
			else if (cRIME_SEVERITY == CRIME_SEVERITY.Infraction)
			{
				result = "Being_Naughty";
			}
		}
		else if (reactionStatus == REACTION_STATUS.INFORMED)
		{
			if (crime != null && crime.isIntel && factionCrimeSeverity.IsConsideredACrime() && !personalCrimeSeverity.IsConsideredACrime())
			{
				result = ((crimeType == CRIME_TYPE.Vampire && witness.traitContainer.HasTrait("Hemophiliac")) ? ((!RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor)) ? "Go_Vampires" : "Ooh_La_La") : ((crimeType == CRIME_TYPE.Werewolf && witness.traitContainer.HasTrait("Lycanphiliac")) ? ((!RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor)) ? "Go_Werewolves" : "Ooh_La_La") : ((crimeType == CRIME_TYPE.Arson && witness.traitContainer.HasTrait("Pyromaniac")) ? "Burn_Baby_Burn" : ((crimeType == CRIME_TYPE.Cannibalism && witness.traitContainer.HasTrait("Cannibal")) ? "Taste_Of_Flesh" : ((crimeType != CRIME_TYPE.Murder || !witness.traitContainer.HasTrait("Psychopath")) ? "Do_Not_Tell_Others" : "Another_Killer")))));
			}
		}
		return result;
	}

	public CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType)
	{
		if (!actor.isNormalCharacter)
		{
			return CRIME_SEVERITY.None;
		}
		CrimeType crimeType2 = GetCrimeType(crimeType);
		CRIME_SEVERITY cRIME_SEVERITY = CRIME_SEVERITY.Unapplicable;
		if (witness.faction != null)
		{
			cRIME_SEVERITY = witness.faction.GetCrimeSeverity(actor, target, crimeType);
		}
		CRIME_SEVERITY cRIME_SEVERITY2 = CRIME_SEVERITY.Unapplicable;
		if (crimeType2 != null)
		{
			cRIME_SEVERITY2 = crimeType2.GetCrimeSeverity(witness, actor, target);
		}
		CRIME_SEVERITY result = cRIME_SEVERITY2;
		if (cRIME_SEVERITY2 == CRIME_SEVERITY.Unapplicable)
		{
			result = cRIME_SEVERITY;
		}
		return result;
	}

	public CRIME_SEVERITY GetPersonalCrimeSeverity(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType)
	{
		if (!actor.isNormalCharacter)
		{
			return CRIME_SEVERITY.None;
		}
		CrimeType crimeType2 = GetCrimeType(crimeType);
		CRIME_SEVERITY result = CRIME_SEVERITY.Unapplicable;
		if (crimeType2 != null)
		{
			result = crimeType2.GetCrimeSeverity(witness, actor, target);
		}
		return result;
	}

	public CRIME_SEVERITY GetFactionCrimeSeverity(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType)
	{
		if (!actor.isNormalCharacter)
		{
			return CRIME_SEVERITY.None;
		}
		CRIME_SEVERITY result = CRIME_SEVERITY.Unapplicable;
		if (witness.faction != null)
		{
			result = witness.faction.GetCrimeSeverity(actor, target, crimeType);
		}
		return result;
	}

	public bool IsConsideredACrimeByCharacter(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType)
	{
		return GetCrimeSeverity(witness, actor, target, crimeType).IsConsideredACrime();
	}

	public bool IsConsideredACrimeByCharacter(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType1, CRIME_TYPE crimeType2)
	{
		if (GetCrimeSeverity(witness, actor, target, crimeType1).IsConsideredACrime())
		{
			return true;
		}
		if (GetCrimeSeverity(witness, actor, target, crimeType2).IsConsideredACrime())
		{
			return true;
		}
		return false;
	}

	public bool IsConsideredACrimeByCharacter(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType1, CRIME_TYPE crimeType2, CRIME_TYPE crimeType3)
	{
		if (GetCrimeSeverity(witness, actor, target, crimeType1).IsConsideredACrime())
		{
			return true;
		}
		if (GetCrimeSeverity(witness, actor, target, crimeType2).IsConsideredACrime())
		{
			return true;
		}
		if (GetCrimeSeverity(witness, actor, target, crimeType3).IsConsideredACrime())
		{
			return true;
		}
		return false;
	}

	public CrimeSeverity GetCrimeSeverity(CRIME_SEVERITY severityType)
	{
		if (_crimeSeverities.ContainsKey(severityType))
		{
			return _crimeSeverities[severityType];
		}
		return null;
	}

	public CrimeType GetCrimeType(CRIME_TYPE crimeType)
	{
		if (_crimeTypes.ContainsKey(crimeType))
		{
			return _crimeTypes[crimeType];
		}
		return null;
	}
}
