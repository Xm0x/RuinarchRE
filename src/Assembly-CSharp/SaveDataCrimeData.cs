using System;
using System.Collections.Generic;
using Interrupts;

[Serializable]
public class SaveDataCrimeData : SaveData<CrimeData>, ISavableCounterpart
{
	public CRIME_SEVERITY crimeSeverity;

	public CRIME_TYPE crimeType;

	public CRIME_STATUS crimeStatus;

	public bool isRemoved;

	public bool hasAuthoritiesReachedADecision;

	public string crime;

	public CRIMABLE_TYPE crimableType;

	public string criminal;

	public string target;

	public POINT_OF_INTEREST_TYPE targetPOIType;

	public string targetFaction;

	public string judge;

	public string reporter;

	public List<string> witnesses;

	public List<string> factionsThatConsidersWanted;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Crime;

	public override void Save(CrimeData data)
	{
		persistentID = data.persistentID;
		crimeSeverity = data.crimeSeverity;
		crimeType = data.crimeType;
		crimeStatus = data.crimeStatus;
		isRemoved = data.isRemoved;
		hasAuthoritiesReachedADecision = data.hasAuthoritiesReachedADecision;
		if (data.crime != null)
		{
			crime = data.crime.persistentID;
			crimableType = data.crime.crimableType;
			if (data.crime is ActualGoapNode data2)
			{
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data2);
			}
			else if (data.crime is InterruptHolder data3)
			{
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data3);
			}
		}
		if (data.criminal != null)
		{
			criminal = data.criminal.persistentID;
		}
		if (data.target != null)
		{
			target = data.target.persistentID;
			targetPOIType = data.target.poiType;
		}
		if (data.targetFaction != null)
		{
			targetFaction = data.targetFaction.persistentID;
		}
		if (data.judge != null)
		{
			judge = data.judge.persistentID;
		}
		if (data.reporter != null)
		{
			reporter = data.reporter.persistentID;
		}
		witnesses = new List<string>();
		for (int i = 0; i < data.witnesses.Count; i++)
		{
			witnesses.Add(data.witnesses[i].persistentID);
		}
		factionsThatConsidersWanted = new List<string>();
		for (int j = 0; j < data.factionsThatConsidersWanted.Count; j++)
		{
			factionsThatConsidersWanted.Add(data.factionsThatConsidersWanted[j].persistentID);
		}
	}

	public override CrimeData Load()
	{
		return new CrimeData(this);
	}

	public override void CleanUp()
	{
	}
}
