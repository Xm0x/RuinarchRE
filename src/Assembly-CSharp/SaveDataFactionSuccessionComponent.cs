using System;

[Serializable]
public class SaveDataFactionSuccessionComponent : SaveData<FactionSuccessionComponent>
{
	public string[] successors;

	public int[] successorWeights;

	public bool hasFirstDayStartedTriggered;

	public override void Save(FactionSuccessionComponent data)
	{
		hasFirstDayStartedTriggered = data.hasFirstDayStartedTriggered;
		successors = new string[data.successors.Length];
		for (int i = 0; i < data.successors.Length; i++)
		{
			Character character = data.successors[i];
			if (character != null)
			{
				successors[i] = character.persistentID;
			}
			else
			{
				successors[i] = string.Empty;
			}
		}
		successorWeights = new int[data.successorWeights.Length];
		for (int j = 0; j < data.successorWeights.Length; j++)
		{
			successorWeights[j] = data.successorWeights[j];
		}
	}

	public override FactionSuccessionComponent Load()
	{
		return new FactionSuccessionComponent(this);
	}
}
