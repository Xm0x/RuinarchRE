using System;

[Serializable]
public class SaveDataBountyHuntPartyQuest : SaveDataPartyQuest
{
	public string targetCharacter;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is BountyHuntPartyQuest { targetCharacter: not null } bountyHuntPartyQuest)
		{
			targetCharacter = bountyHuntPartyQuest.targetCharacter.persistentID;
		}
	}
}
