using System;

[Serializable]
public struct AssumptionData
{
	public INTERACTION_TYPE assumedActionType;

	public string actorID;

	public string targetID;

	public POINT_OF_INTEREST_TYPE targetPOIType;

	public AssumptionData(INTERACTION_TYPE actionType, Character actor, IPointOfInterest target)
	{
		assumedActionType = actionType;
		actorID = actor.persistentID;
		targetID = target.persistentID;
		targetPOIType = target.poiType;
	}
}
