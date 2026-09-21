using System;

[Serializable]
public class SaveDataExplorationPartyQuest : SaveDataPartyQuest
{
	public string targetStructure;

	public bool isExploring;

	public GameDate expiryDate;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is ExplorationPartyQuest explorationPartyQuest)
		{
			if (explorationPartyQuest.targetStructure != null)
			{
				targetStructure = explorationPartyQuest.targetStructure.persistentID;
			}
			isExploring = explorationPartyQuest.isExploring;
			expiryDate = explorationPartyQuest.expiryDate;
		}
	}
}
