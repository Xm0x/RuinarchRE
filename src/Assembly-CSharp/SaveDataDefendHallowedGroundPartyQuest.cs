using System;

[Serializable]
public class SaveDataDefendHallowedGroundPartyQuest : SaveDataPartyQuest
{
	public string targetStructure;

	public bool isDefending;

	public GameDate expiryDate;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is DefendHallowedGroundPartyQuest defendHallowedGroundPartyQuest)
		{
			if (defendHallowedGroundPartyQuest.targetStructure != null)
			{
				targetStructure = defendHallowedGroundPartyQuest.targetStructure.persistentID;
			}
			isDefending = defendHallowedGroundPartyQuest.isDefending;
			expiryDate = defendHallowedGroundPartyQuest.expiryDate;
		}
	}
}
