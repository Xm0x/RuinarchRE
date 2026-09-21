using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

[Serializable]
public class SaveDataMoodComponent : SaveData<MoodComponent>
{
	public int moodValue;

	public bool isInNormalMood;

	public bool isInLowMood;

	public bool isInCriticalMood;

	public bool executeMoodChangeEffects;

	public bool isInCriticalBreak;

	public bool hasMoodChanged;

	public CRITICAL_BREAK_ACTION currentCriticalBreak;

	public string criticalBreakTargetID;

	public Type criticalBreakTargetType;

	public Dictionary<string, List<MoodModification>> allMoodModifications;

	public override void Save(MoodComponent data)
	{
		moodValue = data.moodValue;
		isInNormalMood = data.isInNormalMood;
		isInLowMood = data.isInLowMood;
		isInCriticalMood = data.isInCriticalMood;
		executeMoodChangeEffects = data.executeMoodChangeEffects;
		isInCriticalBreak = data.isInCriticalBreak;
		hasMoodChanged = data.hasMoodChanged;
		currentCriticalBreak = data.currentCriticalBreak;
		if (data.criticalBreakTarget is Character character)
		{
			criticalBreakTargetID = character.persistentID;
			criticalBreakTargetType = typeof(Character);
		}
		else if (data.criticalBreakTarget is LocationStructure locationStructure)
		{
			criticalBreakTargetID = locationStructure.persistentID;
			criticalBreakTargetType = typeof(LocationStructure);
		}
		allMoodModifications = new Dictionary<string, List<MoodModification>>();
		if (data.allMoodModifications == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<MoodModification>> allMoodModification in data.allMoodModifications)
		{
			List<MoodModification> value = allMoodModification.Value;
			if (value != null && value.Count > 0)
			{
				List<MoodModification> list = new List<MoodModification>(allMoodModification.Value.Capacity);
				for (int i = 0; i < value.Count; i++)
				{
					MoodModification moodModification = value[i];
					MoodModification moodModification2 = new MoodModification();
					moodModification2.SetData(moodModification.modification, moodModification.expiryDate, moodModification.flavorText);
					list.Add(moodModification2);
				}
				allMoodModifications.Add(allMoodModification.Key, list);
			}
		}
	}

	public override MoodComponent Load()
	{
		return new MoodComponent(this);
	}

	public override void CleanUp()
	{
		allMoodModifications?.Clear();
		allMoodModifications = null;
	}
}
