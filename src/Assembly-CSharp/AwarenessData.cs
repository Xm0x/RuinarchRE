using System;

[Serializable]
public class AwarenessData
{
	public string targetPersistentID;

	public AWARENESS_STATE state;

	public GameDate currentPresumedDeadDate;

	public string presumedDeadScheduleKey;

	public AwarenessData()
	{
		SetAwarenessState(AWARENESS_STATE.Available);
	}

	public void SetTarget(Character p_character)
	{
		targetPersistentID = p_character.persistentID;
	}

	public void SetAwarenessState(AWARENESS_STATE state)
	{
		this.state = state;
	}

	public void SetPresumedDeadDate(GameDate p_date, string p_scheduleKey)
	{
		currentPresumedDeadDate = p_date;
		presumedDeadScheduleKey = p_scheduleKey;
	}
}
