using System.Collections.Generic;

public class SaveDataJobHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataJobQueueItem> _hub;

	public Dictionary<string, SaveDataJobQueueItem> hub => _hub;

	public SaveDataJobHub()
	{
		_hub = new Dictionary<string, SaveDataJobQueueItem>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataJobQueueItem saveDataJobQueueItem && !_hub.ContainsKey(saveDataJobQueueItem.persistentID))
		{
			_hub.Add(saveDataJobQueueItem.persistentID, saveDataJobQueueItem);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataJobQueueItem saveDataJobQueueItem)
		{
			return _hub.Remove(saveDataJobQueueItem.persistentID);
		}
		return false;
	}

	public override ISavableCounterpart GetData(string persistentID)
	{
		if (_hub.ContainsKey(persistentID))
		{
			return _hub[persistentID];
		}
		return null;
	}

	public override void CleanUp()
	{
		foreach (KeyValuePair<string, SaveDataJobQueueItem> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
