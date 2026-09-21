using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataGatheringHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataGathering> _hub;

	public Dictionary<string, SaveDataGathering> hub => _hub;

	public SaveDataGatheringHub()
	{
		_hub = new Dictionary<string, SaveDataGathering>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataGathering saveDataGathering && !_hub.ContainsKey(saveDataGathering.persistentID))
		{
			_hub.Add(saveDataGathering.persistentID, saveDataGathering);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataGathering saveDataGathering)
		{
			return _hub.Remove(saveDataGathering.persistentID);
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
		foreach (KeyValuePair<string, SaveDataGathering> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
