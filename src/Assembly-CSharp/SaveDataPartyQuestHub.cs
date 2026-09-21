using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataPartyQuestHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataPartyQuest> _hub;

	public Dictionary<string, SaveDataPartyQuest> hub => _hub;

	public SaveDataPartyQuestHub()
	{
		_hub = new Dictionary<string, SaveDataPartyQuest>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataPartyQuest saveDataPartyQuest && !_hub.ContainsKey(saveDataPartyQuest.persistentID))
		{
			_hub.Add(saveDataPartyQuest.persistentID, saveDataPartyQuest);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataPartyQuest saveDataPartyQuest)
		{
			return _hub.Remove(saveDataPartyQuest.persistentID);
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
		foreach (KeyValuePair<string, SaveDataPartyQuest> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
