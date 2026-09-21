using System.Collections.Generic;

public class SaveDataTraitHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataTrait> _hub;

	public Dictionary<string, SaveDataTrait> hub => _hub;

	public SaveDataTraitHub()
	{
		_hub = new Dictionary<string, SaveDataTrait>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataTrait saveDataTrait && !_hub.ContainsKey(saveDataTrait.persistentID))
		{
			_hub.Add(saveDataTrait.persistentID, saveDataTrait);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataTrait saveDataTrait)
		{
			return _hub.Remove(saveDataTrait.persistentID);
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
		foreach (KeyValuePair<string, SaveDataTrait> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
