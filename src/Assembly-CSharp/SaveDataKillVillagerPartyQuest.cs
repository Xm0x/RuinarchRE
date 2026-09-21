using System;

[Serializable]
public class SaveDataKillVillagerPartyQuest : SaveDataPartyQuest
{
	public string targetCharacter;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is KillVillagerPartyQuest { targetCharacter: not null } killVillagerPartyQuest)
		{
			targetCharacter = killVillagerPartyQuest.targetCharacter.persistentID;
		}
	}
}
