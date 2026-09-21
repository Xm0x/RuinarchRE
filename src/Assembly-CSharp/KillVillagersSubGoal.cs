using System;

public class KillVillagersSubGoal : NumberTrackingSubGoal
{
	public override Type serializedData => typeof(SaveDataKillVillagersSubGoal);

	public KillVillagersSubGoal(SUB_GOAL p_subGoal, ACHIEVEMENT p_achievement)
		: base(100, p_subGoal, p_achievement)
	{
	}

	public KillVillagersSubGoal(SaveDataNumberTrackingSubGoal p_data)
		: base(p_data)
	{
	}

	protected override string CreateLocalizedDescriptiveName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Achievements_Table", base.subGoalType.ToStringEnum() + "_DESC");
		log.AddToFillers(null, base.intCurrentValue + "/" + base.intGoal, LOG_IDENTIFIER.STRING_1);
		return log.logText;
	}

	protected override void UniqueActionsOnIncreaseValue()
	{
		base.UniqueActionsOnIncreaseValue();
		RegenerateDescriptiveName();
	}
}
