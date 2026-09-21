using System;

[Serializable]
public class SaveDataDemonRescuePartyQuest : SaveDataPartyQuest
{
	public string targetCharacter;

	public string targetDemonicStructure;

	public bool isReleasing;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is DemonRescuePartyQuest demonRescuePartyQuest)
		{
			isReleasing = demonRescuePartyQuest.isReleasing;
			targetDemonicStructure = demonRescuePartyQuest.targetDemonicStructure?.persistentID;
			if (demonRescuePartyQuest.targetCharacter != null)
			{
				targetCharacter = demonRescuePartyQuest.targetCharacter.persistentID;
			}
		}
	}
}
