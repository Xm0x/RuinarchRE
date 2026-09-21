using System;
using Object_Pools;

[Serializable]
public class MoodModification
{
	public int modification;

	public GameDate expiryDate;

	public Log flavorText;

	public void SetData(int modification, GameDate expiryDate, Log flavorText)
	{
		this.modification = modification;
		this.expiryDate = expiryDate;
		this.flavorText = flavorText;
	}

	public void UpdateLog(Log p_log)
	{
		if (flavorText != null)
		{
			LogPool.Release(flavorText);
		}
		flavorText = p_log;
	}

	public void Reset()
	{
		modification = 0;
		expiryDate = default(GameDate);
		flavorText = null;
	}
}
