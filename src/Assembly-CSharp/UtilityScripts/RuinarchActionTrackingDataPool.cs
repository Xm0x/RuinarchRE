using System.Collections.Generic;

namespace UtilityScripts;

public static class RuinarchActionTrackingDataPool
{
	private static readonly Queue<ActionTrackingData> pool = new Queue<ActionTrackingData>();

	public static ActionTrackingData Claim()
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				return pool.Dequeue();
			}
			ActionTrackingData actionTrackingData = new ActionTrackingData();
			actionTrackingData.Claim();
			return actionTrackingData;
		}
	}

	public static void Release(ActionTrackingData p_data)
	{
		lock (pool)
		{
			if (!pool.Contains(p_data))
			{
				p_data.Release();
				pool.Enqueue(p_data);
			}
		}
	}
}
