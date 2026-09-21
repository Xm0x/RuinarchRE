using System;
using Inner_Maps.Location_Structures;

namespace Inner_Maps;

public class GridTileEventDispatcher : LocationGridTileComponent
{
	public interface ICharacterTileListener
	{
		void OnCharacterEnteredTile(Character p_character, LocationGridTile p_tile);

		void OnCharacterLeftTile(Character p_character, LocationGridTile p_tile);
	}

	public interface ITileObjectTileListener
	{
		void OnTileObjectPlacedOnTile(TileObject p_tileObject, LocationGridTile p_tile);

		void OnTileObjectRemovedFromTile(TileObject p_tileObject, LocationGridTile p_tile);
	}

	public interface ITileStructureListener
	{
		void OnTileChangedStructure(LocationStructure p_newStructure, LocationStructure p_oldStructure, LocationGridTile p_tile);
	}

	private Action<Character, LocationGridTile> _characterEnteredTile;

	private Action<Character, LocationGridTile> _characterLeftTile;

	private Action<TileObject, LocationGridTile> _tileObjectPlacedOnTile;

	private Action<TileObject, LocationGridTile> _tileObjectRemovedFromTile;

	private Action<LocationStructure, LocationStructure, LocationGridTile> _tileChangedStructure;

	public void SubscribeToCharacterTileEvents(ICharacterTileListener p_characterTileListener)
	{
		_characterEnteredTile = (Action<Character, LocationGridTile>)Delegate.Combine(_characterEnteredTile, new Action<Character, LocationGridTile>(p_characterTileListener.OnCharacterEnteredTile));
		_characterLeftTile = (Action<Character, LocationGridTile>)Delegate.Combine(_characterLeftTile, new Action<Character, LocationGridTile>(p_characterTileListener.OnCharacterLeftTile));
	}

	public void UnsubscribeToCharacterTileEvents(ICharacterTileListener p_characterTileListener)
	{
		_characterEnteredTile = (Action<Character, LocationGridTile>)Delegate.Remove(_characterEnteredTile, new Action<Character, LocationGridTile>(p_characterTileListener.OnCharacterEnteredTile));
		_characterLeftTile = (Action<Character, LocationGridTile>)Delegate.Remove(_characterLeftTile, new Action<Character, LocationGridTile>(p_characterTileListener.OnCharacterLeftTile));
	}

	public void ExecuteCharacterEnteredTile(Character p_character, LocationGridTile p_tile)
	{
		_characterEnteredTile?.Invoke(p_character, p_tile);
	}

	public void ExecuteCharacterLeftTile(Character p_character, LocationGridTile p_tile)
	{
		_characterLeftTile?.Invoke(p_character, p_tile);
	}

	public void SubscribeToTileObjectTileEvents(ITileObjectTileListener p_characterTileListener)
	{
		_tileObjectPlacedOnTile = (Action<TileObject, LocationGridTile>)Delegate.Combine(_tileObjectPlacedOnTile, new Action<TileObject, LocationGridTile>(p_characterTileListener.OnTileObjectPlacedOnTile));
		_tileObjectRemovedFromTile = (Action<TileObject, LocationGridTile>)Delegate.Combine(_tileObjectRemovedFromTile, new Action<TileObject, LocationGridTile>(p_characterTileListener.OnTileObjectRemovedFromTile));
	}

	public void UnsubscribeToTileObjectTileEvents(ITileObjectTileListener p_characterTileListener)
	{
		_tileObjectPlacedOnTile = (Action<TileObject, LocationGridTile>)Delegate.Remove(_tileObjectPlacedOnTile, new Action<TileObject, LocationGridTile>(p_characterTileListener.OnTileObjectPlacedOnTile));
		_tileObjectRemovedFromTile = (Action<TileObject, LocationGridTile>)Delegate.Remove(_tileObjectRemovedFromTile, new Action<TileObject, LocationGridTile>(p_characterTileListener.OnTileObjectRemovedFromTile));
	}

	public void ExecuteTileObjectPlacedEvent(TileObject p_tileObject, LocationGridTile p_tile)
	{
		_tileObjectPlacedOnTile?.Invoke(p_tileObject, p_tile);
	}

	public void ExecuteTileObjectRemovedEvent(TileObject p_tileObject, LocationGridTile p_tile)
	{
		_tileObjectRemovedFromTile?.Invoke(p_tileObject, p_tile);
	}

	public void SubscribeToTileChangedStructureEvent(ITileStructureListener p_characterTileListener)
	{
		_tileChangedStructure = (Action<LocationStructure, LocationStructure, LocationGridTile>)Delegate.Combine(_tileChangedStructure, new Action<LocationStructure, LocationStructure, LocationGridTile>(p_characterTileListener.OnTileChangedStructure));
	}

	public void UnsubscribeToTileChangedStructureEvent(ITileStructureListener p_characterTileListener)
	{
		_tileChangedStructure = (Action<LocationStructure, LocationStructure, LocationGridTile>)Delegate.Remove(_tileChangedStructure, new Action<LocationStructure, LocationStructure, LocationGridTile>(p_characterTileListener.OnTileChangedStructure));
	}

	public void ExecuteTileChangedStructureEvent(LocationStructure p_newStructure, LocationStructure p_oldStructure, LocationGridTile p_tile)
	{
		_tileChangedStructure?.Invoke(p_newStructure, p_oldStructure, p_tile);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
