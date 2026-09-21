using System;

public class CreateBoneGolem : GoalTask
{
	public override Type serializedData => typeof(SaveDataCreateBoneGolem);

	public CreateBoneGolem()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Bone_Golem_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Bone_Golem_Task_Tooltip");
	}

	public CreateBoneGolem(SaveDataCreateBoneGolem p_data)
		: base(p_data)
	{
	}

	public override void StartTask()
	{
		Messenger.AddListener<Summon>(MonsterSignals.BONE_GOLEM_SPAWNED_FROM_TEMPLE, OnBoneGolemSpawnedFromTemple);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Summon>(MonsterSignals.BONE_GOLEM_SPAWNED_FROM_TEMPLE, OnBoneGolemSpawnedFromTemple);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Bone_Golem_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Bone_Golem_Task_Tooltip");
	}

	private void OnBoneGolemSpawnedFromTemple(Summon p_summon)
	{
		if (p_summon.summonType == SUMMON_TYPE.Bone_Golem)
		{
			CompleteTask();
		}
	}
}
