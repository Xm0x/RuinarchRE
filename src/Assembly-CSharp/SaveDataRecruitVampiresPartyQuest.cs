using System;

[Serializable]
public class SaveDataRecruitVampiresPartyQuest : SaveDataPartyQuest
{
	public string targetSettlement;

	public bool isHunting;

	public GameDate expiryDate;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is RecruitVampiresPartyQuest recruitVampiresPartyQuest)
		{
			if (recruitVampiresPartyQuest.targetSettlement != null)
			{
				targetSettlement = recruitVampiresPartyQuest.targetSettlement.persistentID;
			}
			isHunting = recruitVampiresPartyQuest.isRecruiting;
			expiryDate = recruitVampiresPartyQuest.expiryDate;
		}
	}
}
