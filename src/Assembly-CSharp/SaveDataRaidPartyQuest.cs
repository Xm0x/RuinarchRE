using System;

[Serializable]
public class SaveDataRaidPartyQuest : SaveDataPartyQuest
{
	public string targetSettlement;

	public bool isRaiding;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is RaidPartyQuest raidPartyQuest)
		{
			isRaiding = raidPartyQuest.isRaiding;
			if (raidPartyQuest.targetSettlement != null)
			{
				targetSettlement = raidPartyQuest.targetSettlement.persistentID;
			}
		}
	}
}
