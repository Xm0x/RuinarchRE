using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class ActionTrackingData
{
	public int numberOfTimesActionDone;

	public List<GameDate> decreaseSchedules;

	public void IncreaseActionCounter(GameDate p_expiryDate)
	{
		numberOfTimesActionDone++;
		decreaseSchedules.Add(p_expiryDate);
	}

	public void DecreaseActionCounter()
	{
		if (numberOfTimesActionDone > 0)
		{
			numberOfTimesActionDone--;
			if (decreaseSchedules.Count > 0)
			{
				decreaseSchedules.RemoveAt(0);
			}
		}
	}

	public void Claim()
	{
		numberOfTimesActionDone = 0;
		decreaseSchedules = RuinarchListPool<GameDate>.Claim();
	}

	public void Release()
	{
		numberOfTimesActionDone = 0;
		RuinarchListPool<GameDate>.Release(decreaseSchedules);
	}
}
