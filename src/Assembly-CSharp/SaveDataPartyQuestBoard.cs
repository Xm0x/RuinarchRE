using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataPartyQuestBoard : SaveData<PartyQuestBoard>
{
	public string owner;

	public List<string> availablePartyQuests;

	public override void Save(PartyQuestBoard data)
	{
		owner = data.owner.persistentID;
		availablePartyQuests = new List<string>();
		for (int i = 0; i < data.availablePartyQuests.Count; i++)
		{
			PartyQuest partyQuest = data.availablePartyQuests[i];
			if (partyQuest.target != null)
			{
				availablePartyQuests.Add(partyQuest.persistentID);
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(partyQuest);
			}
		}
	}

	public override PartyQuestBoard Load()
	{
		return new PartyQuestBoard(this);
	}
}
