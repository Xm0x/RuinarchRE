using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AfflictionUpgradeData
{
	[HideInInspector]
	public List<AFFLICTION_UPGRADE_BONUS> bonuses = new List<AFFLICTION_UPGRADE_BONUS>();

	[HideInInspector]
	public List<int> rateChance = new List<int>();

	[HideInInspector]
	public List<int> crowdNumber = new List<int>();

	[HideInInspector]
	public List<int> napsDuration = new List<int>();

	[HideInInspector]
	public List<int> numberOfCriteria = new List<int>();

	[HideInInspector]
	public List<int> hungerRate = new List<int>();

	[HideInInspector]
	public List<OPINIONS> opinionTrigger = new List<OPINIONS>();

	[HideInInspector]
	public List<LIST_OF_CRITERIA> listOfCriteria = new List<LIST_OF_CRITERIA>();

	public int GetHungerRatePerLevel(int p_currentLevel)
	{
		if (hungerRate == null || hungerRate.Count <= 0)
		{
			return -1;
		}
		if (p_currentLevel >= hungerRate.Count)
		{
			return hungerRate[hungerRate.Count - 1];
		}
		return hungerRate[p_currentLevel];
	}

	public float GetNapsDurationPerLevel(int p_currentLevel)
	{
		if (napsDuration == null || napsDuration.Count <= 0)
		{
			return -1f;
		}
		if (p_currentLevel >= napsDuration.Count)
		{
			return napsDuration[napsDuration.Count - 1];
		}
		return napsDuration[p_currentLevel];
	}

	public float GetRateChancePerLevel(int p_currentLevel)
	{
		if (rateChance == null || rateChance.Count <= 0)
		{
			return -1f;
		}
		if (p_currentLevel >= rateChance.Count)
		{
			return rateChance[rateChance.Count - 1];
		}
		return rateChance[p_currentLevel];
	}

	public int GetCrowdNumberPerLevel(int p_currentLevel)
	{
		if (crowdNumber == null || crowdNumber.Count <= 0)
		{
			return -1;
		}
		if (p_currentLevel >= crowdNumber.Count)
		{
			return crowdNumber[crowdNumber.Count - 1];
		}
		return crowdNumber[p_currentLevel];
	}

	public OPINIONS GetOpinionTriggerPerLevel(int p_currentLevel)
	{
		if (opinionTrigger == null || opinionTrigger.Count <= 0)
		{
			return OPINIONS.NoOne;
		}
		if (p_currentLevel >= opinionTrigger.Count)
		{
			return opinionTrigger[opinionTrigger.Count - 1];
		}
		return opinionTrigger[p_currentLevel];
	}

	public List<OPINIONS> GetAllOpinionsTrigger()
	{
		return opinionTrigger;
	}

	public bool HasOpinionTriggerForLevel(OPINIONS p_opinion, int p_level)
	{
		return opinionTrigger.HasValueInListUntilIndex(p_level, p_opinion);
	}

	public void PopulateOpinionTriggerForLevel(List<OPINIONS> p_opinions, int p_level)
	{
		for (int i = 0; i <= p_level; i++)
		{
			OPINIONS item = opinionTrigger[i];
			p_opinions.Add(item);
		}
	}

	public void PopulateAvailableCriteriaForLevel(List<LIST_OF_CRITERIA> p_criteria, int p_level)
	{
		for (int i = 0; i <= p_level; i++)
		{
			LIST_OF_CRITERIA item = listOfCriteria[i];
			p_criteria.Add(item);
		}
	}
}
