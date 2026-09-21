using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataCharacterJobTriggerComponent : SaveData<CharacterJobTriggerComponent>
{
	public List<JOB_TYPE> ableJobs;

	public Dictionary<INTERACTION_TYPE, ActionTrackingData> numOfTimesActionDone;

	public List<string> obtainPersonalItemUnownedRandomList;

	public bool hasStartedScreamCheck;

	public bool doNotDoRecoverHPJob;

	public int producedFish;

	public override void Save(CharacterJobTriggerComponent data)
	{
		ableJobs = RuinarchListPool<JOB_TYPE>.Claim();
		if (data.ableJobs != null && data.ableJobs.Count > 0)
		{
			ableJobs.AddRange(data.ableJobs);
		}
		numOfTimesActionDone = new Dictionary<INTERACTION_TYPE, ActionTrackingData>(data.actionTracker);
		obtainPersonalItemUnownedRandomList = RuinarchListPool<string>.Claim();
		if (data.obtainPersonalItemUnownedRandomList != null && data.obtainPersonalItemUnownedRandomList.Count > 0)
		{
			obtainPersonalItemUnownedRandomList.AddRange(data.obtainPersonalItemUnownedRandomList);
		}
		hasStartedScreamCheck = data.hasStartedScreamCheck;
		doNotDoRecoverHPJob = data.doNotDoRecoverHPJob;
		producedFish = data.producedFish;
	}

	public override CharacterJobTriggerComponent Load()
	{
		return new CharacterJobTriggerComponent(this);
	}

	public override void CleanUp()
	{
		if (ableJobs != null)
		{
			RuinarchListPool<JOB_TYPE>.Release(ableJobs);
			ableJobs = null;
		}
		numOfTimesActionDone?.Clear();
		numOfTimesActionDone = null;
		if (obtainPersonalItemUnownedRandomList != null)
		{
			RuinarchListPool<string>.Release(obtainPersonalItemUnownedRandomList);
			obtainPersonalItemUnownedRandomList = null;
		}
	}
}
