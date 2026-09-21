public class Incubus : SeducerSummon
{
	public const string ClassName = "Incubus";

	public Incubus()
		: base(SUMMON_TYPE.Incubus, GENDER.MALE, "Incubus")
	{
	}

	public Incubus(string className)
		: base(SUMMON_TYPE.Incubus, GENDER.MALE, className)
	{
	}

	public Incubus(SaveDataSummon data)
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
		Character randomCharacterThatIsThisGenderVillagerAndNotDead = base.currentRegion.GetRandomCharacterThatIsThisGenderVillagerAndNotDead(GENDER.MALE);
		base.jobComponent.TriggerCastSeducedToNearbyVillager(JOB_TYPE.IDLE, out p_agitateJob, GENDER.FEMALE);
		if (p_agitateJob is GoapPlanJob goapPlanJob)
		{
			if (randomCharacterThatIsThisGenderVillagerAndNotDead == null)
			{
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_1);
				return false;
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "Disguise disguise success_description", LOG_TAG.Work, null);
			log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(randomCharacterThatIsThisGenderVillagerAndNotDead, randomCharacterThatIsThisGenderVillagerAndNotDead.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			base.reactionComponent.SetDisguisedCharacter(randomCharacterThatIsThisGenderVillagerAndNotDead);
			goapPlanJob.SetIsAgitateJob(p_state: true);
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
			return true;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Seduce_Female_No_Target);
		return false;
	}
}
