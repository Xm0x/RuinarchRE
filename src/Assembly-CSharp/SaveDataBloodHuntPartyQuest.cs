using System;

[Serializable]
public class SaveDataBloodHuntPartyQuest : SaveDataPartyQuest
{
	public string targetSettlement;

	public bool isHunting;

	public GameDate expiryDate;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is BloodHuntPartyQuest bloodHuntPartyQuest)
		{
			if (bloodHuntPartyQuest.targetSettlement != null)
			{
				targetSettlement = bloodHuntPartyQuest.targetSettlement.persistentID;
			}
			isHunting = bloodHuntPartyQuest.isHunting;
			expiryDate = bloodHuntPartyQuest.expiryDate;
		}
	}
}
