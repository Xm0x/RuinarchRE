using System;

[Serializable]
public class BaseSaveDataHub
{
	public virtual bool AddToSave<T>(T data)
	{
		return false;
	}

	public virtual bool RemoveFromSave<T>(T data)
	{
		return false;
	}

	public virtual ISavableCounterpart GetData(string persistentID)
	{
		return null;
	}

	public virtual void CleanUp()
	{
	}
}
