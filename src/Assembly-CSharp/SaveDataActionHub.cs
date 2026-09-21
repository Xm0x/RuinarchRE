using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataActionHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataActualGoapNode> _hub;

	public Dictionary<string, SaveDataActualGoapNode> hub => _hub;

	public SaveDataActionHub()
	{
		_hub = new Dictionary<string, SaveDataActualGoapNode>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataActualGoapNode saveDataActualGoapNode && !_hub.ContainsKey(saveDataActualGoapNode.persistentID))
		{
			_hub.Add(saveDataActualGoapNode.persistentID, saveDataActualGoapNode);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataActualGoapNode saveDataActualGoapNode)
		{
			return _hub.Remove(saveDataActualGoapNode.persistentID);
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
		foreach (KeyValuePair<string, SaveDataActualGoapNode> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
