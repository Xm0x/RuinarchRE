using System;
using Object_Pools;

[Serializable]
public class SaveDataNotification
{
	public string logID;

	public int tickShown;

	public bool isIntel;

	public bool isActionIntel;

	public SaveDataActionIntel actionIntel;

	public SaveDataInterruptIntel interruptIntel;

	public void Save(PlayerNotificationItem notif)
	{
		logID = notif.logPersistentID;
		if (DatabaseManager.Instance.mainSQLDatabase.GetLogWithPersistentID(logID) != null && notif is IntelNotificationItem intelNotificationItem)
		{
			isIntel = true;
			if (intelNotificationItem.intel is ActionIntel data)
			{
				isActionIntel = true;
				actionIntel = new SaveDataActionIntel();
				actionIntel.Save(data);
			}
			else if (intelNotificationItem.intel is InterruptIntel data2)
			{
				isActionIntel = false;
				interruptIntel = new SaveDataInterruptIntel();
				interruptIntel.Save(data2);
			}
		}
	}

	public void Load()
	{
		Log log = DatabaseManager.Instance.mainSQLDatabase.GetLogWithPersistentID(logID);
		if (log != null)
		{
			if (isIntel)
			{
				IIntel intel = null;
				intel = ((!isActionIntel) ? ((IIntel)new InterruptIntel(DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(interruptIntel.interruptHolder))) : ((IIntel)new ActionIntel(DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(actionIntel.node))));
				UIManager.Instance.ShowPlayerNotification(intel, in log, tickShown);
			}
			else
			{
				UIManager.Instance.ShowPlayerNotification(in log, tickShown);
			}
			LogPool.Release(log);
		}
	}
}
