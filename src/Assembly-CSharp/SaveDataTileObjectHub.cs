using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataTileObjectHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataTileObject> _hub;

	public Dictionary<string, SaveDataTileObject> hub => _hub;

	public SaveDataTileObjectHub()
	{
		_hub = new Dictionary<string, SaveDataTileObject>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataTileObject saveDataTileObject && !_hub.ContainsKey(saveDataTileObject.persistentID))
		{
			_hub.Add(saveDataTileObject.persistentID, saveDataTileObject);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataTileObject saveDataTileObject)
		{
			return _hub.Remove(saveDataTileObject.persistentID);
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
		foreach (KeyValuePair<string, SaveDataTileObject> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
