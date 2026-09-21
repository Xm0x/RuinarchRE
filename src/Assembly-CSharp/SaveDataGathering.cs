using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataGathering : SaveData<Gathering>, ISavableCounterpart
{
	public string host;

	public GATHERING_TYPE gatheringType;

	public string gatheringName;

	public int waitTimeInTicks;

	public int minimumGatheringSize;

	public string relatedBehaviour;

	public JOB_OWNER jobQueueOwnerType;

	public string jobOwner;

	public List<string> attendees;

	public bool isWaitTimeOver;

	public bool isDisbanded;

	public bool isAlreadyWaiting;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Party;

	public override void Save(Gathering data)
	{
		persistentID = data.persistentID;
		host = data.host.persistentID;
		gatheringType = data.gatheringType;
		gatheringName = data.gatheringName;
		waitTimeInTicks = data.waitTimeInTicks;
		minimumGatheringSize = data.minimumGatheringSize;
		relatedBehaviour = data.relatedBehaviour.ToString();
		jobQueueOwnerType = data.jobQueueOwnerType;
		jobOwner = data.jobOwner.persistentID;
		isWaitTimeOver = data.isWaitTimeOver;
		isDisbanded = data.isDisbanded;
		isAlreadyWaiting = data.isAlreadyWaiting;
		attendees = new List<string>();
		for (int i = 0; i < data.attendees.Count; i++)
		{
			attendees.Add(data.attendees[i].persistentID);
		}
	}

	public override Gathering Load()
	{
		return CharacterManager.Instance.CreateNewGathering(this);
	}

	public override void CleanUp()
	{
	}
}
