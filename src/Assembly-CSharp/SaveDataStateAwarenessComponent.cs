using System;

[Serializable]
public class SaveDataStateAwarenessComponent : SaveData<StateAwarenessComponent>
{
	public override void Save(StateAwarenessComponent data)
	{
	}

	public override StateAwarenessComponent Load()
	{
		return new StateAwarenessComponent(this);
	}
}
