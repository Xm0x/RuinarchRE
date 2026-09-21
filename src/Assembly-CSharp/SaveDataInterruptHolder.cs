using System;
using System.Collections.Generic;
using Interrupts;
using Object_Pools;

[Serializable]
public class SaveDataInterruptHolder : SaveData<InterruptHolder>, ISavableCounterpart
{
	public INTERRUPT interruptType;

	public string actorID;

	public string targetID;

	public POINT_OF_INTEREST_TYPE targetPOIType;

	public string disguisedActorID;

	public string disguisedTargetID;

	public Log effectLog;

	public string identifier;

	public SaveDataRumor rumor;

	public bool hasRumor;

	public List<string> awareCharacterIDs;

	public string reason;

	public CRIME_TYPE crimeType;

	public bool shouldNotBeObjectPooled;

	public List<LOG_TAG> logTags;

	public bool isSupposedToBeInPool;

	public bool isIntel;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Interrupt;

	public override void Save(InterruptHolder data)
	{
		persistentID = data.persistentID;
		interruptType = data.interrupt.type;
		actorID = data.actor.persistentID;
		if (data.target != null)
		{
			targetID = data.target.persistentID;
			targetPOIType = data.target.poiType;
		}
		identifier = data.identifier;
		reason = data.reason;
		crimeType = data.crimeType;
		shouldNotBeObjectPooled = data.shouldNotBeObjectPooled;
		isIntel = data.isIntel;
		disguisedActorID = string.Empty;
		disguisedTargetID = string.Empty;
		if (data.disguisedActor != null)
		{
			disguisedActorID = data.disguisedActor.persistentID;
		}
		if (data.disguisedTarget != null)
		{
			disguisedTargetID = data.disguisedTarget.persistentID;
		}
		effectLog = null;
		if (data.effectLog != null)
		{
			effectLog = LogPool.Claim();
			effectLog.Copy(data.effectLog);
		}
		if (data.rumor != null)
		{
			hasRumor = true;
			rumor = new SaveDataRumor();
			rumor.Save(data.rumor);
		}
		awareCharacterIDs = new List<string>();
		if (data.awareCharacters != null && data.awareCharacters.Count > 0)
		{
			for (int i = 0; i < data.awareCharacters.Count; i++)
			{
				Character character = data.awareCharacters[i];
				if (character == null)
				{
					data.awareCharacters.RemoveAt(i);
					i--;
				}
				else
				{
					awareCharacterIDs.Add(character.persistentID);
				}
			}
		}
		logTags = new List<LOG_TAG>(data.logTags);
		isSupposedToBeInPool = data.isSupposedToBeInPool;
	}

	public override InterruptHolder Load()
	{
		return new InterruptHolder(this);
	}

	public override void CleanUp()
	{
		if (effectLog != null)
		{
			LogPool.Release(effectLog);
		}
	}
}
