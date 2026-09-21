using System;

[Serializable]
public class SaveDataHuntBeastPartyQuest : SaveDataPartyQuest
{
	public string targetStructure;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is HuntBeastPartyQuest { targetStructure: not null } huntBeastPartyQuest)
		{
			targetStructure = huntBeastPartyQuest.targetStructure.persistentID;
		}
	}
}
