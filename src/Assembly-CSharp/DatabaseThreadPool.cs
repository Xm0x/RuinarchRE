using System.Collections.Generic;
using System.Threading;
using Threads;

public class DatabaseThreadPool : BaseMonoBehaviour
{
	public static DatabaseThreadPool Instance;

	private static readonly object THREAD_LOCKER = new object();

	private Queue<SQLWorkerItem> functionsToBeRunInThread;

	private Queue<SQLWorkerItem> functionsToBeResolved;

	private Thread newThread;

	private bool isRunning;

	private void Awake()
	{
		Instance = this;
		isRunning = true;
		functionsToBeRunInThread = new Queue<SQLWorkerItem>();
		functionsToBeResolved = new Queue<SQLWorkerItem>();
	}

	protected override void OnDestroy()
	{
		isRunning = false;
		functionsToBeRunInThread.Clear();
		functionsToBeResolved.Clear();
		base.OnDestroy();
		Instance = null;
	}

	private void LateUpdate()
	{
		if (functionsToBeResolved.Count > 0)
		{
			SQLWorkerItem sQLWorkerItem = functionsToBeResolved.Dequeue();
			sQLWorkerItem.FinishMultithread();
			ObjectPoolManager.Instance.ReturnLogDatabaseThreadToPool(sQLWorkerItem);
		}
	}

	public void AddToThreadPool(SQLWorkerItem multiThread)
	{
		functionsToBeRunInThread.Enqueue(multiThread);
	}

	private void RunThread()
	{
		while (isRunning)
		{
			lock (THREAD_LOCKER)
			{
				if (functionsToBeRunInThread != null && functionsToBeRunInThread.Count > 0)
				{
					SQLWorkerItem sQLWorkerItem = functionsToBeRunInThread.Dequeue();
					sQLWorkerItem?.DoMultithread();
					functionsToBeResolved.Enqueue(sQLWorkerItem);
				}
			}
		}
	}
}
