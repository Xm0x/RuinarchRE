using System;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class SocialGathering : Gathering
{
	public LocationStructure targetStructure { get; private set; }

	public override IGatheringTarget target => targetStructure;

	public override Type serializedData => typeof(SaveDataSocialGathering);

	public SocialGathering()
		: base(GATHERING_TYPE.Social)
	{
		base.minimumGatheringSize = 5;
		base.waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(4);
		base.relatedBehaviour = typeof(SocialGatheringBehaviour);
		base.jobQueueOwnerType = JOB_OWNER.SETTLEMENT;
	}

	public SocialGathering(SaveDataSocialGathering data)
		: base(data)
	{
	}

	public override bool IsAllowedToJoin(Character character)
	{
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			if (!character.relationshipContainer.IsEnemiesWith(base.host) && !character.traitContainer.HasTrait("Agoraphobic"))
			{
				return character.limiterComponent.isSociable;
			}
			return false;
		}
		return false;
	}

	protected override void OnWaitTimeOver()
	{
		base.OnWaitTimeOver();
		DisbandGathering();
	}

	protected override void OnRemoveAttendee(Character member)
	{
		base.OnRemoveAttendee(member);
		if (base.attendees.Count <= 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < base.attendees.Count; i++)
		{
			if (base.attendees[i].currentStructure == targetStructure)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			DisbandGathering();
		}
	}

	protected override void OnDisbandGathering()
	{
		base.OnDisbandGathering();
		targetStructure?.SetHasActiveSocialGathering(state: false);
	}

	public void SetTargetStructure(LocationStructure structure)
	{
		if (targetStructure != structure)
		{
			targetStructure = structure;
			if (targetStructure != null)
			{
				targetStructure.SetHasActiveSocialGathering(state: true);
			}
		}
	}

	public override void LoadReferences(SaveDataGathering data)
	{
		base.LoadReferences(data);
		if (data is SaveDataSocialGathering saveDataSocialGathering && !string.IsNullOrEmpty(saveDataSocialGathering.targetStructure))
		{
			targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataSocialGathering.targetStructure);
		}
	}
}
