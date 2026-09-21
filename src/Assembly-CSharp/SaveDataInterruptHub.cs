using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataInterruptHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataInterruptHolder> _hub;

	public Dictionary<string, SaveDataInterruptHolder> hub => _hub;

	public SaveDataInterruptHub()
	{
		_hub = new Dictionary<string, SaveDataInterruptHolder>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataInterruptHolder saveDataInterruptHolder && !_hub.ContainsKey(saveDataInterruptHolder.persistentID))
		{
			_hub.Add(saveDataInterruptHolder.persistentID, saveDataInterruptHolder);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataInterruptHolder saveDataInterruptHolder)
		{
			return _hub.Remove(saveDataInterruptHolder.persistentID);
		}
		return false;
	}

	public override ISavableCounterpart GetData(string persistendID)
	{
		if (_hub.ContainsKey(persistendID))
		{
			return _hub[persistendID];
		}
		return null;
	}

	public override void CleanUp()
	{
		foreach (KeyValuePair<string, SaveDataInterruptHolder> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
