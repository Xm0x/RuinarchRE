using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataFactionHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataFaction> _hub;

	public Dictionary<string, SaveDataFaction> hub => _hub;

	public SaveDataFactionHub()
	{
		_hub = new Dictionary<string, SaveDataFaction>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataFaction saveDataFaction && !_hub.ContainsKey(saveDataFaction.persistentID))
		{
			_hub.Add(saveDataFaction.persistentID, saveDataFaction);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataFaction saveDataFaction)
		{
			return _hub.Remove(saveDataFaction.persistentID);
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
		foreach (KeyValuePair<string, SaveDataFaction> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
