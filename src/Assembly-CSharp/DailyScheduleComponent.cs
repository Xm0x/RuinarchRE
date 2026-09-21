using Inner_Maps.Location_Structures;

public class DailyScheduleComponent : CharacterComponent
{
	public DailySchedule schedule { get; private set; }

	public DailyScheduleComponent()
	{
		schedule = CharacterManager.Instance.GetDailySchedule<NonPartyMemberSchedule>();
	}

	public void LoadReferences(SaveDataDailyScheduleComponent p_data)
	{
		schedule = CharacterManager.Instance.GetDailySchedule(p_data.scheduleType);
	}

	private void UpdateDailySchedule(Character p_character)
	{
		if (p_character.partyComponent.hasParty)
		{
			if (p_character.partyComponent.currentParty.isActive && (p_character.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Night_Patrol || p_character.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Blood_Hunt || p_character.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Recruit_Vampires))
			{
				SetSchedule(CharacterManager.Instance.GetDailySchedule<NightPartyMemberSchedule>());
			}
			else
			{
				SetSchedule(CharacterManager.Instance.GetDailySchedule<PartyMemberSchedule>());
			}
		}
		else if (p_character.traitContainer.HasTrait("Nocturnal"))
		{
			SetSchedule(CharacterManager.Instance.GetDailySchedule<NocturnalSchedule>());
		}
		else
		{
			SetSchedule(CharacterManager.Instance.GetDailySchedule<NonPartyMemberSchedule>());
		}
	}

	private void SetSchedule(DailySchedule p_schedule)
	{
		if (schedule != p_schedule)
		{
			DAILY_SCHEDULE scheduleType = schedule.GetScheduleType(GameManager.Instance.currentTick);
			DAILY_SCHEDULE scheduleType2 = p_schedule.GetScheduleType(GameManager.Instance.currentTick);
			schedule = p_schedule;
			if (scheduleType == DAILY_SCHEDULE.Sleep && scheduleType2 != DAILY_SCHEDULE.Sleep && base.owner.currentJob != null && (base.owner.currentJob.jobType == JOB_TYPE.ENERGY_RECOVERY_NORMAL || base.owner.currentJob.jobType == JOB_TYPE.ENERGY_RECOVERY_URGENT))
			{
				base.owner.currentJob.CancelJob("Wake_Up");
			}
		}
	}

	public void OnCharacterGainedNocturnal(Character p_character)
	{
		UpdateDailySchedule(p_character);
	}

	public void OnCharacterLostNocturnal(Character p_character)
	{
		UpdateDailySchedule(p_character);
	}

	public void OnCharacterJoinedParty(Character p_character)
	{
		UpdateDailySchedule(p_character);
	}

	public void OnCharacterLeftParty(Character p_character)
	{
		UpdateDailySchedule(p_character);
	}

	public void OnPartyAcceptedQuest(Character p_character, PartyQuest p_quest)
	{
		UpdateDailySchedule(p_character);
	}

	public void OnPartyEndQuest(Character p_character, PartyQuest p_quest)
	{
		UpdateDailySchedule(p_character);
	}

	public void OnHourStarted(Character p_character)
	{
		GameDate gameDate = GameManager.Instance.Today();
		gameDate.ReduceTicks(1);
		DAILY_SCHEDULE scheduleType = schedule.GetScheduleType(gameDate.tick);
		DAILY_SCHEDULE scheduleType2 = schedule.GetScheduleType(GameManager.Instance.currentTick);
		if (scheduleType == DAILY_SCHEDULE.Sleep && scheduleType2 != DAILY_SCHEDULE.Sleep && p_character.currentJob != null && (p_character.currentJob.jobType == JOB_TYPE.ENERGY_RECOVERY_NORMAL || p_character.currentJob.jobType == JOB_TYPE.ENERGY_RECOVERY_URGENT))
		{
			p_character.currentJob.CancelJob("Wake_Up");
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
