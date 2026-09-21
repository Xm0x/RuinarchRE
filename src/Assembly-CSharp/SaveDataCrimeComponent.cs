using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataCrimeComponent : SaveData<CrimeComponent>
{
	public List<string> witnessedCrimes;

	public List<string> reportedCrimes;

	public List<string> activeCrimes;

	public List<string> previousCrimes;

	public bool hasReportedCrime;

	public GameDate dateToReportCrimeAgain;

	public override void Save(CrimeComponent data)
	{
		hasReportedCrime = data.hasReportedCrime;
		dateToReportCrimeAgain = data.dateToReportCrimeAgain;
		if (data.witnessedCrimes != null)
		{
			witnessedCrimes = RuinarchListPool<string>.Claim();
			for (int i = 0; i < data.witnessedCrimes.Count; i++)
			{
				CrimeData crimeData = data.witnessedCrimes[i];
				if (crimeData.crime != null && crimeData.crime.actor != null && !(crimeData.crime is ActualGoapNode { hasBeenReset: not false }))
				{
					witnessedCrimes.Add(crimeData.persistentID);
					SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crimeData);
				}
			}
		}
		if (data.reportedCrimes != null)
		{
			reportedCrimes = RuinarchListPool<string>.Claim();
			for (int j = 0; j < data.reportedCrimes.Count; j++)
			{
				CrimeData crimeData2 = data.reportedCrimes[j];
				if (crimeData2.crime != null && crimeData2.crime.actor != null && !(crimeData2.crime is ActualGoapNode { hasBeenReset: not false }))
				{
					reportedCrimes.Add(crimeData2.persistentID);
					SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crimeData2);
				}
			}
		}
		if (data.activeCrimes != null)
		{
			activeCrimes = RuinarchListPool<string>.Claim();
			for (int k = 0; k < data.activeCrimes.Count; k++)
			{
				CrimeData crimeData3 = data.activeCrimes[k];
				if (crimeData3.crime != null && crimeData3.crime.actor != null && !(crimeData3.crime is ActualGoapNode { hasBeenReset: not false }))
				{
					activeCrimes.Add(crimeData3.persistentID);
					SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crimeData3);
				}
			}
		}
		if (data.previousCrimes == null)
		{
			return;
		}
		previousCrimes = RuinarchListPool<string>.Claim();
		for (int l = 0; l < data.previousCrimes.Count; l++)
		{
			CrimeData crimeData4 = data.previousCrimes[l];
			if (crimeData4.crime != null && crimeData4.crime.actor != null && !(crimeData4.crime is ActualGoapNode { hasBeenReset: not false }))
			{
				previousCrimes.Add(crimeData4.persistentID);
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crimeData4);
			}
		}
	}

	public override CrimeComponent Load()
	{
		return new CrimeComponent(this);
	}

	public override void CleanUp()
	{
		if (witnessedCrimes != null)
		{
			RuinarchListPool<string>.Release(witnessedCrimes);
			witnessedCrimes = null;
		}
		if (reportedCrimes != null)
		{
			RuinarchListPool<string>.Release(reportedCrimes);
			reportedCrimes = null;
		}
		if (activeCrimes != null)
		{
			RuinarchListPool<string>.Release(activeCrimes);
			activeCrimes = null;
		}
		if (previousCrimes != null)
		{
			RuinarchListPool<string>.Release(previousCrimes);
			previousCrimes = null;
		}
	}
}
