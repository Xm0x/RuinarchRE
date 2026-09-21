using System;

[Serializable]
public class SaveDataClaimHallowedGroundPartyQuest : SaveDataPartyQuest
{
	public string dropStructure;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is ClaimHallowedGroundPartyQuest { targetStructure: not null } claimHallowedGroundPartyQuest)
		{
			dropStructure = claimHallowedGroundPartyQuest.targetStructure.persistentID;
		}
	}
}
