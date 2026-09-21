using System;

public class SaveDataDailyScheduleComponent : SaveData<DailyScheduleComponent>
{
	public Type scheduleType;

	public override void Save(DailyScheduleComponent data)
	{
		base.Save(data);
		scheduleType = data.schedule.GetType();
	}

	public override DailyScheduleComponent Load()
	{
		return new DailyScheduleComponent();
	}
}
