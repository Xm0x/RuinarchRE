using UtilityScripts;

public abstract class Nymph : Summon
{
	private string _currentEffectSchedule;

	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;

	protected Nymph(SUMMON_TYPE summonType, string className)
		: base(summonType, className, RACE.NYMPH, Utilities.GetRandomGender())
	{
	}

	protected Nymph(SaveDataSummon data)
		: base(data)
	{
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		base.jobComponent.TriggerCastMesmerizedToNearbyVillager(JOB_TYPE.AGITATED, out p_agitateJob);
		if (p_agitateJob is GoapPlanJob goapPlanJob)
		{
			goapPlanJob.SetIsAgitateJob(p_state: true);
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
			return true;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Mesmerize_No_Target);
		return false;
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Mesmerize_Villager_Tooltip.ToStringEnum();
	}
}
