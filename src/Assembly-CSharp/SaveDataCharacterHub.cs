using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataCharacterHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataCharacter> _hub;

	public Dictionary<string, SaveDataCharacter> hub => _hub;

	public SaveDataCharacterHub()
	{
		_hub = new Dictionary<string, SaveDataCharacter>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataCharacter saveDataCharacter && !_hub.ContainsKey(saveDataCharacter.persistentID))
		{
			_hub.Add(saveDataCharacter.persistentID, saveDataCharacter);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataCharacter saveDataCharacter)
		{
			return _hub.Remove(saveDataCharacter.persistentID);
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
		foreach (KeyValuePair<string, SaveDataCharacter> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
