public class Succubus : SeducerSummon
{
	public const string ClassName = "Succubus";

	public Succubus()
		: base(SUMMON_TYPE.Succubus, GENDER.FEMALE, "Succubus")
	{
	}

	public Succubus(string className)
		: base(SUMMON_TYPE.Succubus, GENDER.FEMALE, className)
	{
	}

	public Succubus(SaveDataSummon data)
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
		Character randomCharacterThatIsThisGenderVillagerAndNotDead = base.currentRegion.GetRandomCharacterThatIsThisGenderVillagerAndNotDead(GENDER.FEMALE);
		base.jobComponent.TriggerCastSeducedToNearbyVillager(JOB_TYPE.IDLE, out p_agitateJob, GENDER.MALE);
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
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Seduce_Male_No_Target);
		return false;
	}
}
