using System;
using System.Collections.Generic;
using Factions.Faction_Succession;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Factions.Faction_Types;

public abstract class FactionType
{
	public readonly string name;

	public readonly FACTION_TYPE type;

	public readonly List<FactionIdeology> ideologies;

	public readonly List<string> combatantClasses;

	public readonly List<string> civilianClasses;

	public readonly Dictionary<CRIME_TYPE, CRIME_SEVERITY> crimes;

	public FactionSuccession succession { get; protected set; }

	public bool hasCrimes { get; protected set; }

	public abstract RESOURCE mainResource { get; }

	public virtual bool usesCorruptedStructures => false;

	public virtual bool shouldShowFactionEmblem => true;

	public virtual bool usesBothWoodAndStoneResources => false;

	public virtual Type serializedData => typeof(SaveDataFactionType);

	public string displayName => LocalizationManager.Instance.GetLocalizedValue("Faction_Table", type.ToStringEnum());

	protected FactionType(FACTION_TYPE type)
	{
		this.type = type;
		name = Utilities.NormalizeStringUpperCaseFirstLetters(type.ToStringEnum());
		ideologies = new List<FactionIdeology>();
		combatantClasses = new List<string>();
		civilianClasses = new List<string>();
		crimes = new Dictionary<CRIME_TYPE, CRIME_SEVERITY>();
		succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.None);
	}

	public FactionType(FACTION_TYPE type, SaveDataFactionType data)
	{
		this.type = type;
		name = Utilities.NormalizeStringUpperCaseFirstLetters(type.ToStringEnum());
		ideologies = new List<FactionIdeology>();
		combatantClasses = new List<string>();
		civilianClasses = new List<string>();
		succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.None);
		for (int i = 0; i < data.ideologies.Count; i++)
		{
			SaveDataFactionIdeology saveDataFactionIdeology = data.ideologies[i];
			ideologies.Add(saveDataFactionIdeology.Load());
		}
		crimes = ((data.crimes != null) ? new Dictionary<CRIME_TYPE, CRIME_SEVERITY>(data.crimes) : new Dictionary<CRIME_TYPE, CRIME_SEVERITY>());
		hasCrimes = data.hasCrimes;
		succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.None);
	}

	public abstract void SetAsDefault(Faction p_faction);

	public abstract void SetFixedData();

	public FactionIdeology AddIdeology(FACTION_IDEOLOGY ideology, Faction p_faction)
	{
		if (!HasIdeology(ideology))
		{
			FactionIdeology factionIdeology = FactionManager.Instance.CreateIdeology<FactionIdeology>(ideology);
			AddIdeologyBase(factionIdeology, p_faction);
			return factionIdeology;
		}
		return null;
	}

	public void AddIdeology(FactionIdeology ideology, Faction p_faction)
	{
		if (!HasIdeology(ideology.ideologyType))
		{
			AddIdeologyBase(ideology, p_faction);
		}
	}

	private void AddIdeologyBase(FactionIdeology ideology, Faction p_faction)
	{
		ideologies.Add(ideology);
		ideology.OnAddFactionIdeology(this, p_faction);
	}

	public void RemoveIdeology(FACTION_IDEOLOGY ideology, Faction p_faction)
	{
		if (HasIdeology(ideology, out var factionIdeology))
		{
			ideologies.Remove(factionIdeology);
			factionIdeology.OnRemoveFactionIdeology(this, p_faction);
		}
	}

	public void RemoveAllIdeologies(Faction p_faction)
	{
		for (int i = 0; i < ideologies.Count; i++)
		{
			ideologies[i].OnRemoveFactionIdeology(this, p_faction);
		}
		ideologies.Clear();
	}

	public bool HasIdeology(FACTION_IDEOLOGY ideology)
	{
		for (int i = 0; i < ideologies.Count; i++)
		{
			if (ideologies[i].ideologyType == ideology)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasIdeology(FACTION_IDEOLOGY ideology1, FACTION_IDEOLOGY ideology2)
	{
		for (int i = 0; i < ideologies.Count; i++)
		{
			FactionIdeology factionIdeology = ideologies[i];
			if (factionIdeology.ideologyType == ideology1 || factionIdeology.ideologyType == ideology2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasIdeology(FACTION_IDEOLOGY ideology, out FactionIdeology factionIdeology)
	{
		for (int i = 0; i < ideologies.Count; i++)
		{
			FactionIdeology factionIdeology2 = ideologies[i];
			if (factionIdeology2.ideologyType == ideology)
			{
				factionIdeology = factionIdeology2;
				return true;
			}
		}
		factionIdeology = null;
		return false;
	}

	public bool HasPeaceTypeIdeology()
	{
		for (int i = 0; i < ideologies.Count; i++)
		{
			if (ideologies[i].ideologyType.IsPeaceType())
			{
				return true;
			}
		}
		return false;
	}

	public FactionIdeology GetFactionIdeology(FACTION_IDEOLOGY p_ideologyType)
	{
		for (int i = 0; i < ideologies.Count; i++)
		{
			FactionIdeology factionIdeology = ideologies[i];
			if (factionIdeology.ideologyType == p_ideologyType)
			{
				return factionIdeology;
			}
		}
		return null;
	}

	public CRIME_SEVERITY GetCrimeSeverity(Character actor, IPointOfInterest target, CRIME_TYPE crimeType)
	{
		if (hasCrimes)
		{
			if (crimes.ContainsKey(crimeType))
			{
				return crimes[crimeType];
			}
			return CRIME_SEVERITY.None;
		}
		return CRIME_SEVERITY.Unapplicable;
	}

	public CRIME_SEVERITY GetCrimeSeverity(CRIME_TYPE crimeType)
	{
		if (hasCrimes)
		{
			if (crimes.ContainsKey(crimeType))
			{
				return crimes[crimeType];
			}
			return CRIME_SEVERITY.None;
		}
		return CRIME_SEVERITY.Unapplicable;
	}

	public void AddCrime(CRIME_TYPE type, CRIME_SEVERITY severity, bool shouldBroadcastSignal = false)
	{
		if (!crimes.ContainsKey(type))
		{
			crimes.Add(type, severity);
		}
		else
		{
			crimes[type] = severity;
		}
	}

	public bool RemoveCrime(CRIME_TYPE type)
	{
		if (crimes.ContainsKey(type))
		{
			return crimes.Remove(type);
		}
		return false;
	}

	public abstract CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType);

	public CRIME_TYPE GetRandomNonReligionSeriousCrime()
	{
		List<CRIME_TYPE> list = RuinarchListPool<CRIME_TYPE>.Claim();
		foreach (KeyValuePair<CRIME_TYPE, CRIME_SEVERITY> crime in crimes)
		{
			if (crime.Value == CRIME_SEVERITY.Serious && !crime.Key.IsReligiousCrime())
			{
				list.Add(crime.Key);
			}
		}
		if (list.Count > 0)
		{
			return CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<CRIME_TYPE>.Release(list);
		return CRIME_TYPE.None;
	}

	public bool IsActionConsideredACrime(CRIME_TYPE p_crimeType)
	{
		return GetCrimeSeverity(p_crimeType).IsConsideredACrime();
	}

	public void PopulateCrimeTypesBySeverity(List<CRIME_TYPE> crimeTypes, CRIME_SEVERITY p_severity)
	{
		if (!hasCrimes)
		{
			return;
		}
		foreach (KeyValuePair<CRIME_TYPE, CRIME_SEVERITY> crime in crimes)
		{
			if (crime.Value == p_severity)
			{
				crimeTypes.Add(crime.Key);
			}
		}
	}

	public void PopulateCrimeTypesBySeverity(List<CRIME_TYPE> crimeTypes, CRIME_SEVERITY p_severity1, CRIME_SEVERITY p_severity2)
	{
		if (!hasCrimes)
		{
			return;
		}
		foreach (KeyValuePair<CRIME_TYPE, CRIME_SEVERITY> crime in crimes)
		{
			if (crime.Value == p_severity1 || crime.Value == p_severity2)
			{
				crimeTypes.Add(crime.Key);
			}
		}
	}

	public virtual StructureSetting ProcessStructureSetting(StructureSetting p_setting, NPCSettlement p_settlement)
	{
		return p_setting;
	}

	public virtual StructureSetting CreateStructureSettingForStructure(STRUCTURE_TYPE structureType, NPCSettlement p_settlement)
	{
		RESOURCE resource = (structureType.RequiresResourceToBuild(RESOURCE.STONE) ? RESOURCE.STONE : RESOURCE.NONE);
		if (structureType == STRUCTURE_TYPE.VAMPIRE_CASTLE)
		{
			resource = RESOURCE.STONE;
		}
		return new StructureSetting(structureType, resource);
	}

	public void AddCombatantClass(string className)
	{
		combatantClasses.Add(className);
	}

	public void RemoveCombatantClass(string className)
	{
		combatantClasses.Remove(className);
	}

	public void AddCivilianClass(string className)
	{
		civilianClasses.Add(className);
	}

	public void RemoveCivilianClass(string className)
	{
		civilianClasses.Remove(className);
	}

	public bool IsCivilian(string className)
	{
		if (civilianClasses != null)
		{
			return civilianClasses.Contains(className);
		}
		return false;
	}

	public virtual int GetAdditionalMigrationMeterGain(NPCSettlement p_settlement)
	{
		return 0;
	}

	protected int GetMigrationMeterGainBasedOnUnoccupiedDwellings(NPCSettlement p_settlement)
	{
		return Mathf.Min(p_settlement.GetUnoccupiedDwellingCount(), 3);
	}

	public virtual void ProcessNewMember(Character character)
	{
	}

	public virtual void ProcessOnFactionLeaderChanged(ILeader p_previousLeader, ILeader p_newLeader)
	{
	}

	public bool IsReligiousCultForReligion(RELIGION p_religion)
	{
		return p_religion switch
		{
			RELIGION.Demon_Worship => type == FACTION_TYPE.Demon_Cult, 
			RELIGION.Divine_Worship => type == FACTION_TYPE.Divine_Church, 
			RELIGION.Nature_Worship => type == FACTION_TYPE.Wiccans, 
			_ => false, 
		};
	}

	public virtual void RecruitProcess(Character p_actor, Character p_target, Character p_factionLeader, out JobQueueItem p_producedJob)
	{
		KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
	}

	protected void KillProcess(Character p_actor, Character p_target, Character p_factionLeader, out JobQueueItem p_producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECRUIT, INTERACTION_TYPE.EXECUTE, p_target, p_actor);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetDoNotRecalculate(state: true);
		p_producedJob = goapPlanJob;
	}

	public void LoadReferencesInMainThread(SaveDataFactionType data)
	{
		for (int i = 0; i < data.ideologies.Count; i++)
		{
			SaveDataFactionIdeology p_type = data.ideologies[i];
			ideologies[i].LoadReferencesInMainThread(p_type);
		}
	}
}
