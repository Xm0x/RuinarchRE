using System;

[Serializable]
public class SaveDataActionIntel : SaveData<ActionIntel>
{
	public string node;

	public override void Save(ActionIntel data)
	{
		node = data.node.persistentID;
		SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.node);
	}

	public override ActionIntel Load()
	{
		return new ActionIntel(this);
	}
}
