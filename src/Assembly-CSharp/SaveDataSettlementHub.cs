using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataSettlementHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataBaseSettlement> _hub;

	public Dictionary<string, SaveDataBaseSettlement> hub => _hub;

	public SaveDataSettlementHub()
	{
		_hub = new Dictionary<string, SaveDataBaseSettlement>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataBaseSettlement saveDataBaseSettlement && !_hub.ContainsKey(saveDataBaseSettlement._persistentID))
		{
			_hub.Add(saveDataBaseSettlement._persistentID, saveDataBaseSettlement);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataBaseSettlement saveDataBaseSettlement)
		{
			return _hub.Remove(saveDataBaseSettlement._persistentID);
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
		foreach (KeyValuePair<string, SaveDataBaseSettlement> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
