using System;

[Serializable]
public class SaveDataMonsterInvadeGathering : SaveDataGathering
{
	public string targetStructure;

	public string targetHex;

	public string hexForJoining;

	public bool isInvading;

	public override void Save(Gathering data)
	{
		base.Save(data);
		if (data is MonsterInvadeGathering monsterInvadeGathering)
		{
			isInvading = monsterInvadeGathering.isInvading;
			if (monsterInvadeGathering.targetStructure != null)
			{
				targetStructure = monsterInvadeGathering.targetStructure.persistentID;
			}
			if (monsterInvadeGathering.targetArea != null)
			{
				targetHex = monsterInvadeGathering.targetArea.persistentID;
			}
			if (monsterInvadeGathering.areaForJoining != null)
			{
				hexForJoining = monsterInvadeGathering.areaForJoining.persistentID;
			}
		}
	}
}
