using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataSharedOpinionModifierHub : BaseSaveDataHub
{
	public Dictionary<string, SaveDataSharedOpinionModifier> _hub;

	public Dictionary<string, SaveDataSharedOpinionModifier> hub => _hub;

	public SaveDataSharedOpinionModifierHub()
	{
		_hub = new Dictionary<string, SaveDataSharedOpinionModifier>();
	}

	public override bool AddToSave<T>(T data)
	{
		if (data is SaveDataSharedOpinionModifier saveDataSharedOpinionModifier && !_hub.ContainsKey(saveDataSharedOpinionModifier.persistentID))
		{
			_hub.Add(saveDataSharedOpinionModifier.persistentID, saveDataSharedOpinionModifier);
			return true;
		}
		return false;
	}

	public override bool RemoveFromSave<T>(T data)
	{
		if (data is SaveDataSharedOpinionModifier saveDataSharedOpinionModifier)
		{
			return _hub.Remove(saveDataSharedOpinionModifier.persistentID);
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
		foreach (KeyValuePair<string, SaveDataSharedOpinionModifier> item in _hub)
		{
			item.Value.CleanUp();
		}
		_hub.Clear();
		_hub = null;
	}
}
