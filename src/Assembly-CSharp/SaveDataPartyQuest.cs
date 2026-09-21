public class SaveDataPartyQuest : SaveData<PartyQuest>, ISavableCounterpart
{
	public PARTY_QUEST_TYPE partyQuestType;

	public int minimumPartySize;

	public bool isWaitTimeOver;

	public string relatedBehaviour;

	public string assignedParty;

	public string madeInLocation;

	public bool isSuccessful;

	public string questCreator;

	public int priority;

	public bool isFactionWideQuest;

	public bool isDemonicQuest;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Party_Quest;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		persistentID = data.persistentID;
		partyQuestType = data.partyQuestType;
		minimumPartySize = data.minimumPartySize;
		isWaitTimeOver = data.isWaitTimeOver;
		isSuccessful = data.isSuccessful;
		priority = data.priority;
		isFactionWideQuest = data.isFactionWideQuest;
		isDemonicQuest = data.isDemonicQuest;
		relatedBehaviour = data.relatedBehaviour.ToString();
		if (data.assignedParty != null)
		{
			assignedParty = data.assignedParty.persistentID;
		}
		if (data.madeInLocation != null)
		{
			madeInLocation = data.madeInLocation.persistentID;
		}
		if (data.questCreator != null)
		{
			questCreator = data.questCreator.persistentID;
		}
	}

	public override PartyQuest Load()
	{
		return PartyManager.Instance.CreateNewPartyQuest(this);
	}

	public override void CleanUp()
	{
	}
}
