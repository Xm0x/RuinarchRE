using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class CrimeDatabase
{
	public Dictionary<string, CrimeData> allCrimes { get; }

	public CrimeDatabase()
	{
		allCrimes = new Dictionary<string, CrimeData>();
	}

	public void AddCrime(CrimeData crime)
	{
		if (!allCrimes.ContainsKey(crime.persistentID))
		{
			allCrimes.Add(crime.persistentID, crime);
		}
	}

	public void RemoveCrime(CrimeData crime)
	{
		if (allCrimes.Remove(crime.persistentID))
		{
			Messenger.Broadcast(CharacterSignals.CRIME_REMOVED_FROM_DATABASE, crime);
			crime.CleanUp();
		}
	}

	public CrimeData GetCrimeByPersistentID(string id)
	{
		if (allCrimes.ContainsKey(id))
		{
			return allCrimes[id];
		}
		throw new NullReferenceException("Trying to get a crime from the database with id " + id + " but the crime is not loaded");
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		foreach (KeyValuePair<string, CrimeData> allCrime in allCrimes)
		{
			CrimeData value = allCrime.Value;
			value.CheckIfStructureIsStillReferenced(p_structure);
			value.IsCrimeDataInvalid();
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		foreach (KeyValuePair<string, CrimeData> allCrime in allCrimes)
		{
			CrimeData value = allCrime.Value;
			value.CheckIfCharacterIsStillReferenced(p_character);
			value.IsCrimeDataInvalid();
		}
	}
}
