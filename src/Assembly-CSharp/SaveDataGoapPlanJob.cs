using System.Collections.Generic;

public class SaveDataGoapPlanJob : SaveDataJobQueueItem
{
	public GoapEffect goal;

	public string targetPOIID;

	public OBJECT_TYPE targetPOIObjectType;

	public INTERACTION_TYPE targetInteractionType;

	public Dictionary<INTERACTION_TYPE, SaveDataOtherData[]> otherData;

	public bool shouldBeCancelledOnDeath;

	public bool isAssigned;

	public bool isAgitateJob;

	public bool isTriggeredByPlayer;

	public SaveDataGoapPlan saveDataGoapPlan;

	public Dictionary<INTERACTION_TYPE, List<ILocationSaveData>> priorityLocations { get; private set; }

	public override void Save(JobQueueItem job)
	{
		base.Save(job);
		GoapPlanJob goapPlanJob = job as GoapPlanJob;
		goal = goapPlanJob.goal;
		targetPOIID = ((goapPlanJob.targetPOI == null) ? string.Empty : goapPlanJob.targetPOI.persistentID);
		targetPOIObjectType = ((goapPlanJob.targetPOI != null) ? goapPlanJob.targetPOI.objectType : OBJECT_TYPE.Character);
		targetInteractionType = goapPlanJob.targetInteractionType;
		isAssigned = goapPlanJob.isAssigned;
		isAgitateJob = goapPlanJob.isAgitateJob;
		isTriggeredByPlayer = goapPlanJob.isTriggeredByPlayer;
		this.otherData = new Dictionary<INTERACTION_TYPE, SaveDataOtherData[]>();
		foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in goapPlanJob.otherData)
		{
			SaveDataOtherData[] array = new SaveDataOtherData[otherDatum.Value.Length];
			for (int i = 0; i < otherDatum.Value.Length; i++)
			{
				OtherData otherData = otherDatum.Value[i];
				if (otherData != null)
				{
					array[i] = otherData.Save();
				}
			}
			this.otherData.Add(otherDatum.Key, array);
		}
		if (goapPlanJob.priorityLocations != null)
		{
			priorityLocations = new Dictionary<INTERACTION_TYPE, List<ILocationSaveData>>();
			foreach (KeyValuePair<INTERACTION_TYPE, List<ILocation>> item2 in new Dictionary<INTERACTION_TYPE, List<ILocation>>(goapPlanJob.priorityLocations))
			{
				if (item2.Value != null)
				{
					priorityLocations.Add(item2.Key, new List<ILocationSaveData>());
					for (int j = 0; j < item2.Value.Count; j++)
					{
						ILocation location = item2.Value[j];
						ILocationSaveData item = new ILocationSaveData
						{
							persistentID = location.persistentID,
							objectType = location.objectType
						};
						priorityLocations[item2.Key].Add(item);
					}
				}
			}
		}
		shouldBeCancelledOnDeath = goapPlanJob.shouldBeCancelledOnDeath;
		if (goapPlanJob.assignedPlan != null)
		{
			saveDataGoapPlan = new SaveDataGoapPlan();
			saveDataGoapPlan.Save(goapPlanJob.assignedPlan);
		}
	}

	public override JobQueueItem Load()
	{
		return JobManager.Instance.CreateNewGoapPlanJob(this);
	}
}
