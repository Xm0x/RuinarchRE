using System.Collections.Generic;

namespace Object_Pools;

public static class LogPool
{
	private static readonly Queue<Log> logPool = new Queue<Log>(10000);

	public static void WarmUp(int p_amount)
	{
		for (int i = 0; i < p_amount; i++)
		{
			logPool.Enqueue(new Log());
		}
	}

	public static Log Claim()
	{
		lock (logPool)
		{
			if (logPool.Count > 0)
			{
				return logPool.Dequeue();
			}
			return new Log();
		}
	}

	public static void Release(Log p_log)
	{
		lock (logPool)
		{
			p_log.Reset();
			logPool.Enqueue(p_log);
		}
	}

	public static int GetCurrentLogsInPool()
	{
		return logPool.Count;
	}
}
