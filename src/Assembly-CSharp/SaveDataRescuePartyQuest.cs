using System;

[Serializable]
public class SaveDataRescuePartyQuest : SaveDataPartyQuest
{
	public string targetCharacter;

	public bool isReleasing;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is RescuePartyQuest rescuePartyQuest)
		{
			isReleasing = rescuePartyQuest.isReleasing;
			if (rescuePartyQuest.targetCharacter != null)
			{
				targetCharacter = rescuePartyQuest.targetCharacter.persistentID;
			}
		}
	}
}
