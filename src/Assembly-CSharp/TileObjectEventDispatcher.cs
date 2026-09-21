using System;
using Inner_Maps.Location_Structures;
using Traits;

public class TileObjectEventDispatcher
{
	public interface IDestroyedListener
	{
		void OnTileObjectDestroyed(TileObject p_tileObject);
	}

	public interface ITraitListener
	{
		void OnTileObjectGainedTrait(TileObject p_tileObject, Trait p_trait);

		void OnTileObjectLostTrait(TileObject p_tileObject, Trait p_trait);
	}

	private Action<TileObject> _tileObjectDestroyed;

	private Action<TileObject, Trait> _tileObjectGainedTrait;

	private Action<TileObject, Trait> _tileObjectLostTrait;

	public void SubscribeToTileObjectDestroyed(IDestroyedListener p_listener)
	{
		_tileObjectDestroyed = (Action<TileObject>)Delegate.Combine(_tileObjectDestroyed, new Action<TileObject>(p_listener.OnTileObjectDestroyed));
	}

	public void UnsubscribeToTileObjectDestroyed(IDestroyedListener p_listener)
	{
		_tileObjectDestroyed = (Action<TileObject>)Delegate.Remove(_tileObjectDestroyed, new Action<TileObject>(p_listener.OnTileObjectDestroyed));
	}

	public void ExecuteTileObjectDestroyed(TileObject p_tileObject)
	{
		_tileObjectDestroyed?.Invoke(p_tileObject);
	}

	public void SubscribeToTileObjectGainedTrait(ITraitListener p_listener)
	{
		_tileObjectGainedTrait = (Action<TileObject, Trait>)Delegate.Combine(_tileObjectGainedTrait, new Action<TileObject, Trait>(p_listener.OnTileObjectGainedTrait));
	}

	public void UnsubscribeToTileObjectGainedTrait(ITraitListener p_listener)
	{
		_tileObjectGainedTrait = (Action<TileObject, Trait>)Delegate.Remove(_tileObjectGainedTrait, new Action<TileObject, Trait>(p_listener.OnTileObjectGainedTrait));
	}

	public void ExecuteTileObjectGainedTrait(TileObject p_tileObject, Trait p_trait)
	{
		_tileObjectGainedTrait?.Invoke(p_tileObject, p_trait);
	}

	public void SubscribeToTileObjectLostTrait(ITraitListener p_listener)
	{
		_tileObjectLostTrait = (Action<TileObject, Trait>)Delegate.Combine(_tileObjectLostTrait, new Action<TileObject, Trait>(p_listener.OnTileObjectLostTrait));
	}

	public void UnsubscribeToTileObjectLostTrait(ITraitListener p_listener)
	{
		_tileObjectLostTrait = (Action<TileObject, Trait>)Delegate.Remove(_tileObjectLostTrait, new Action<TileObject, Trait>(p_listener.OnTileObjectLostTrait));
	}

	public void ExecuteTileObjectLostTrait(TileObject p_tileObject, Trait p_trait)
	{
		_tileObjectLostTrait?.Invoke(p_tileObject, p_trait);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
