using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataCrimeHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataCrimeData> _hub;

	public Dictionary<string, SaveDataCrimeData> hub => _hub;

	public SaveDataCrimeHub()
	{
		_hub = new Dictionary<string, SaveDataCrimeData>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataCrimeData saveDataCrimeData && !_hub.ContainsKey(saveDataCrimeData.persistentID))
		{
			_hub.Add(saveDataCrimeData.persistentID, saveDataCrimeData);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataCrimeData saveDataCrimeData)
		{
			return _hub.Remove(saveDataCrimeData.persistentID);
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
		foreach (KeyValuePair<string, SaveDataCrimeData> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
