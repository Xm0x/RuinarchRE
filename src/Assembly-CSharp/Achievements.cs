using System;
using UtilityScripts;

[Serializable]
public class Achievements
{
	public Achievement[] achievements { get; private set; }

	public void CreateNewAchievements()
	{
		ACHIEVEMENT[] enumValues = CollectionUtilities.GetEnumValues<ACHIEVEMENT>();
		achievements = new Achievement[enumValues.Length];
		for (int i = 0; i < enumValues.Length; i++)
		{
			achievements[i] = new Achievement(enumValues[i]);
		}
	}
}
