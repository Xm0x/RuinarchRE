using System;

[Serializable]
public class SaveData<T> : BaseSaveData
{
	public virtual void Save(T data)
	{
	}

	public virtual T Load()
	{
		return default(T);
	}

	public virtual void CleanUp()
	{
	}
}
