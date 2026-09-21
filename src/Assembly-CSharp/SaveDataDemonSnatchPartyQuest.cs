using System;

[Serializable]
public class SaveDataDemonSnatchPartyQuest : SaveDataPartyQuest
{
	public string targetCharacter;

	public string dropStructure;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is DemonSnatchPartyQuest demonSnatchPartyQuest)
		{
			if (demonSnatchPartyQuest.targetCharacter != null)
			{
				targetCharacter = demonSnatchPartyQuest.targetCharacter.persistentID;
			}
			if (demonSnatchPartyQuest.dropStructure != null)
			{
				dropStructure = demonSnatchPartyQuest.dropStructure.persistentID;
			}
		}
	}
}
