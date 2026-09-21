using System.Collections.Generic;
using System.Threading;
using Threads;

public class MultiThreadPool : BaseMonoBehaviour
{
	public static MultiThreadPool Instance;

	public static readonly object THREAD_LOCKER = new object();

	private Queue<Multithread> generalFunctionsToBeResolved;

	private Queue<SQLWorkerItem> logFunctionsToBeResolved;

	private WaitCallback _callback;

	private Timer timer;

	private void Awake()
	{
		Instance = this;
		_callback = ThreadQueueFunction;
		generalFunctionsToBeResolved = new Queue<Multithread>();
		logFunctionsToBeResolved = new Queue<SQLWorkerItem>();
	}

	protected override void OnDestroy()
	{
		generalFunctionsToBeResolved = null;
		logFunctionsToBeResolved = null;
		base.OnDestroy();
		Instance = null;
	}

	private void LateUpdate()
	{
		lock (generalFunctionsToBeResolved)
		{
			if (generalFunctionsToBeResolved.Count > 0)
			{
				generalFunctionsToBeResolved.Dequeue().FinishMultithread();
			}
		}
		lock (logFunctionsToBeResolved)
		{
			if (logFunctionsToBeResolved.Count > 0)
			{
				SQLWorkerItem sQLWorkerItem = logFunctionsToBeResolved.Dequeue();
				sQLWorkerItem.FinishMultithread();
				ObjectPoolManager.Instance.ReturnLogDatabaseThreadToPool(sQLWorkerItem);
			}
		}
	}

	public void AddToThreadPool(Multithread multiThread)
	{
		ThreadPool.QueueUserWorkItem(_callback, multiThread);
	}

	private void ThreadQueueFunction(object p_thread)
	{
		Multithread multithread = p_thread as Multithread;
		multithread.DoMultithread();
		if (multithread is SQLWorkerItem item)
		{
			lock (logFunctionsToBeResolved)
			{
				logFunctionsToBeResolved.Enqueue(item);
				return;
			}
		}
		lock (generalFunctionsToBeResolved)
		{
			generalFunctionsToBeResolved.Enqueue(multithread);
		}
	}

	public bool IsThereStillFunctionsToBeResolved()
	{
		if (generalFunctionsToBeResolved.Count <= 0)
		{
			return logFunctionsToBeResolved.Count > 0;
		}
		return true;
	}
}
