using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataPartyHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataParty> _hub;

	public Dictionary<string, SaveDataParty> hub => _hub;

	public SaveDataPartyHub()
	{
		_hub = new Dictionary<string, SaveDataParty>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataParty saveDataParty && !_hub.ContainsKey(saveDataParty.persistentID))
		{
			_hub.Add(saveDataParty.persistentID, saveDataParty);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataParty saveDataParty)
		{
			return _hub.Remove(saveDataParty.persistentID);
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
		foreach (KeyValuePair<string, SaveDataParty> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
