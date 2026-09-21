using System;

[Serializable]
public class SaveDataScorpion : SaveDataSummon
{
	public string heldCharacter;

	public bool hasPulledForTheDay;

	public GameDate nextPullDate;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Scorpion scorpion)
		{
			if (scorpion.heldCharacter != null)
			{
				heldCharacter = scorpion.heldCharacter.persistentID;
			}
			hasPulledForTheDay = scorpion.hasPulledForTheDay;
			nextPullDate = scorpion.nextPullDate;
		}
	}
}
