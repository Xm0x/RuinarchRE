using System;

public abstract class DailySchedule
{
	public abstract DailyScheduleSection[] schedule { get; }

	public DAILY_SCHEDULE GetScheduleType(int p_tick)
	{
		for (int i = 0; i < schedule.Length; i++)
		{
			DailyScheduleSection dailyScheduleSection = schedule[i];
			if (dailyScheduleSection.time.IsInRange(p_tick))
			{
				return dailyScheduleSection.scheduleType;
			}
		}
		throw new Exception($"Could not find schedule type for tick {p_tick.ToString()} on schedule {GetType()}");
	}

	public int GetStartTickOfScheduleType(DAILY_SCHEDULE p_scheduleType)
	{
		for (int i = 0; i < schedule.Length; i++)
		{
			DailyScheduleSection dailyScheduleSection = schedule[i];
			if (dailyScheduleSection.scheduleType == p_scheduleType)
			{
				return dailyScheduleSection.time.GetStartTick();
			}
		}
		return -1;
	}

	public int GetEndTickOfScheduleType(DAILY_SCHEDULE p_scheduleType)
	{
		for (int i = 0; i < schedule.Length; i++)
		{
			DailyScheduleSection dailyScheduleSection = schedule[i];
			if (dailyScheduleSection.scheduleType == p_scheduleType)
			{
				return dailyScheduleSection.time.GetEndTick();
			}
		}
		return -1;
	}

	public DailyScheduleSection GetScheduleSection(int p_tick)
	{
		for (int i = 0; i < schedule.Length; i++)
		{
			DailyScheduleSection dailyScheduleSection = schedule[i];
			if (dailyScheduleSection.time.IsInRange(p_tick))
			{
				return dailyScheduleSection;
			}
		}
		throw new Exception($"Could not find schedule type for tick {p_tick.ToString()} on schedule {GetType()}");
	}

	public bool IsInFirstHourOfCurrentScheduleType(int p_tick)
	{
		int startTick = GetScheduleSection(p_tick).time.GetStartTick();
		TickRange tickRange = new TickRange(startTick, startTick);
		tickRange.IncreaseEndTick(20);
		return tickRange.IsInRange(p_tick);
	}

	public string GetScheduleSummary()
	{
		string text = GetType().ToString();
		for (int i = 0; i < schedule.Length; i++)
		{
			DailyScheduleSection arg = schedule[i];
			text = $"{text}\n\t{arg}";
		}
		return text;
	}
}
