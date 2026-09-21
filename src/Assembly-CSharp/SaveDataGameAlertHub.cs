using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataGameAlertHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataGameAlert> _hub;

	public Dictionary<string, SaveDataGameAlert> hub => _hub;

	public SaveDataGameAlertHub()
	{
		_hub = new Dictionary<string, SaveDataGameAlert>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataGameAlert saveDataGameAlert && !_hub.ContainsKey(saveDataGameAlert.persistentID))
		{
			_hub.Add(saveDataGameAlert.persistentID, saveDataGameAlert);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataGameAlert saveDataGameAlert)
		{
			return _hub.Remove(saveDataGameAlert.persistentID);
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
		foreach (KeyValuePair<string, SaveDataGameAlert> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
