using System;
using Inner_Maps.Location_Structures;

public class LocationStructureEventDispatcher
{
	public interface IDestroyedListener
	{
		void OnStructureDestroyed(LocationStructure p_structure);
	}

	private Action<LocationStructure> _structureDestroyed;

	public void SubscribeToStructureDestroyed(IDestroyedListener p_listener)
	{
		_structureDestroyed = (Action<LocationStructure>)Delegate.Combine(_structureDestroyed, new Action<LocationStructure>(p_listener.OnStructureDestroyed));
	}

	public void UnsubscribeToStructureDestroyed(IDestroyedListener p_listener)
	{
		_structureDestroyed = (Action<LocationStructure>)Delegate.Remove(_structureDestroyed, new Action<LocationStructure>(p_listener.OnStructureDestroyed));
	}

	public void ExecuteStructureDestroyed(LocationStructure p_structure)
	{
		_structureDestroyed?.Invoke(p_structure);
	}

	public void CleanUp()
	{
		_structureDestroyed = null;
	}
}
