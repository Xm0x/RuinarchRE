using System;

[Serializable]
public class SaveDataExterminationPartyQuest : SaveDataPartyQuest
{
	public string targetStructure;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is ExterminationPartyQuest { targetStructure: not null } exterminationPartyQuest)
		{
			targetStructure = exterminationPartyQuest.targetStructure.persistentID;
		}
	}
}
