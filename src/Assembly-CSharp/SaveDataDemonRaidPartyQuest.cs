using System;

[Serializable]
public class SaveDataDemonRaidPartyQuest : SaveDataPartyQuest
{
	public string targetSettlement;

	public bool isRaiding;

	public GameDate expiryDate;

	public DEMON_RAID_TYPE raidType;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is DemonRaidPartyQuest demonRaidPartyQuest)
		{
			isRaiding = demonRaidPartyQuest.isRaiding;
			expiryDate = demonRaidPartyQuest.expiryDate;
			raidType = demonRaidPartyQuest.raidType;
			if (demonRaidPartyQuest.targetSettlement != null)
			{
				targetSettlement = demonRaidPartyQuest.targetSettlement.persistentID;
			}
		}
	}
}
