namespace Player_Input;

public abstract class PlayerInputModule
{
	public abstract void OnUpdate();

	public virtual void OnModuleAdded()
	{
	}

	public virtual void OnModuleRemoved()
	{
	}
}
