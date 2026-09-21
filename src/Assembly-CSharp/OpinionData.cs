using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class OpinionData : SharedOpinionModifierEventDispatcher.IExpiryListener
{
	public Dictionary<string, int> allOpinions;

	[NonSerialized]
	public List<SharedOpinionModifier> sharedOpinions;

	public int compatibilityValue;

	public int totalOpinion => TotalAllOpinions() + TotalSharedOpinions();

	public OpinionData()
	{
		allOpinions = new Dictionary<string, int>(10);
		allOpinions.Add("Base", 0);
		sharedOpinions = new List<SharedOpinionModifier>(10);
		compatibilityValue = -1;
	}

	public void RandomizeBaseOpinionBasedOnCompatibility()
	{
		if (GameUtilities.RollChance(35))
		{
			switch (compatibilityValue)
			{
			case 0:
				SetOpinion("Base", GameUtilities.RandomBetweenTwoNumbers(-10, 0));
				break;
			case 1:
				SetOpinion("Base", GameUtilities.RandomBetweenTwoNumbers(0, 20));
				break;
			case 2:
				SetOpinion("Base", GameUtilities.RandomBetweenTwoNumbers(10, 40));
				break;
			case 3:
				SetOpinion("Base", GameUtilities.RandomBetweenTwoNumbers(20, 60));
				break;
			case 4:
				SetOpinion("Base", GameUtilities.RandomBetweenTwoNumbers(40, 80));
				break;
			case 5:
				SetOpinion("Base", GameUtilities.RandomBetweenTwoNumbers(50, 100));
				break;
			}
		}
		else
		{
			SetOpinion("Base", GameUtilities.RandomBetweenTwoNumbers(20, 60));
		}
	}

	private int TotalAllOpinions()
	{
		int num = 0;
		foreach (int value in allOpinions.Values)
		{
			num += value;
		}
		return num;
	}

	private int TotalSharedOpinions()
	{
		int num = 0;
		for (int i = 0; i < sharedOpinions.Count; i++)
		{
			num += sharedOpinions[i].modifierValue;
		}
		return num;
	}

	public void AdjustOpinion(string text, int value)
	{
		if (allOpinions.ContainsKey(text))
		{
			allOpinions[text] += value;
		}
		else
		{
			allOpinions.Add(text, value);
		}
	}

	public void SetOpinion(string text, int value)
	{
		if (allOpinions.ContainsKey(text))
		{
			allOpinions[text] = value;
		}
		else
		{
			allOpinions.Add(text, value);
		}
	}

	public bool RemoveOpinion(string text)
	{
		if (allOpinions.ContainsKey(text))
		{
			return allOpinions.Remove(text);
		}
		return false;
	}

	public bool HasOpinion(string text)
	{
		return allOpinions.ContainsKey(text);
	}

	public void SetCompatibilityValue(int value)
	{
		compatibilityValue = value;
	}

	public string GetOpinionLabel()
	{
		if (totalOpinion > 70)
		{
			return "Close Friend";
		}
		if (totalOpinion > 20 && totalOpinion <= 70)
		{
			return "Friend";
		}
		if (totalOpinion > -21 && totalOpinion <= 20)
		{
			return "Acquaintance";
		}
		if (totalOpinion > -71 && totalOpinion <= -21)
		{
			return "Enemy";
		}
		if (totalOpinion <= -71)
		{
			return "Rival";
		}
		return string.Empty;
	}

	public void AddSharedOpinion(SharedOpinionModifier p_modifier)
	{
		if (!sharedOpinions.Contains(p_modifier))
		{
			sharedOpinions.Add(p_modifier);
			p_modifier.eventDispatcher.SubscribeToExpiryEvent(this);
		}
	}

	public bool RemoveSharedOpinion(SharedOpinionModifier p_modifier)
	{
		if (sharedOpinions.Remove(p_modifier))
		{
			p_modifier.eventDispatcher.UnsubscribeToExpiryEvent(this);
			return true;
		}
		return false;
	}

	public T GetFirstSharedOpinionOfType<T>() where T : SharedOpinionModifier
	{
		for (int i = 0; i < sharedOpinions.Count; i++)
		{
			if (sharedOpinions[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public void LoadSharedOpinion(SharedOpinionModifier p_modifier)
	{
		if (!sharedOpinions.Contains(p_modifier))
		{
			sharedOpinions.Add(p_modifier);
			p_modifier.eventDispatcher.SubscribeToExpiryEvent(this);
		}
	}

	public void Initialize()
	{
	}

	public void Reset()
	{
		allOpinions.Clear();
		sharedOpinions.Clear();
		compatibilityValue = 0;
	}

	public void OnSharedOpinionModifierExpired(SharedOpinionModifier p_modifier)
	{
		RemoveSharedOpinion(p_modifier);
	}
}
