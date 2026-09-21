using System;

[Serializable]
public class ActualGoapNodeTestingData
{
	public string lastPersistentID;

	public string lastActorName;

	public string resetCallStack;

	public INTERACTION_TYPE actionType;

	public override string ToString()
	{
		return "PID: " + lastPersistentID + "\nActor: " + lastActorName + "\nAction Type: " + actionType.ToString() + "\nReset Call Stack: " + resetCallStack;
	}
}
