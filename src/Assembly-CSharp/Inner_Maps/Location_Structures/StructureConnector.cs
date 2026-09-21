using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class StructureConnector : MonoBehaviour
{
	private bool _isOpen;

	private LocationGridTile _tileLocation;

	private bool _isPartOfLocationStructureObject;

	public bool isOpen => _isOpen;

	public LocationGridTile tileLocation => _tileLocation;

	public bool isPartOfLocationStructureObject => _isPartOfLocationStructureObject;

	private void Awake()
	{
		_isOpen = true;
	}

	public void SetOpenState(bool state)
	{
		_isOpen = state;
	}

	public void SetIsPartOfLocationStructureObject(bool p_state)
	{
		_isPartOfLocationStructureObject = p_state;
	}

	public void OnPlaceConnector(InnerTileMap innerTileMap)
	{
		_tileLocation = innerTileMap.GetTileFromWorldPosition(base.transform.position);
		if (_tileLocation != null)
		{
			_tileLocation.AddStructureConnector(this);
			Messenger.AddListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_PLACED, OnStructureConnectorPlaced);
			Messenger.AddListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_REMOVED, OnStructureConnectorRemoved);
			Messenger.Broadcast(StructureSignals.STRUCTURE_CONNECTOR_PLACED, _tileLocation);
		}
	}

	private void OnStructureConnectorPlaced(LocationGridTile placedOnTile)
	{
		if (placedOnTile == _tileLocation && placedOnTile.connectorsOnTile > 1)
		{
			SetOpenState(state: false);
		}
	}

	private void OnStructureConnectorRemoved(LocationGridTile placedOnTile)
	{
		if (placedOnTile == _tileLocation && placedOnTile.connectorsOnTile == 1)
		{
			SetOpenState(state: true);
		}
	}

	public void LoadReferencesForStructureObjects(SaveDataStructureConnector saveData, InnerTileMap innerTileMap)
	{
		_isOpen = saveData.isOpen;
		_isPartOfLocationStructureObject = saveData.isPartOfLocationStructureObject;
		_tileLocation = innerTileMap.GetTileFromWorldPosition(base.transform.position);
		if (_tileLocation != null)
		{
			_tileLocation.area.structureComponent.AddStructureConnector(this);
		}
		Messenger.AddListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_PLACED, OnStructureConnectorPlaced);
		Messenger.AddListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_REMOVED, OnStructureConnectorRemoved);
	}

	public void LoadConnectorForTileObjects(InnerTileMap innerTileMap)
	{
		_tileLocation = innerTileMap.GetTileFromWorldPosition(base.transform.position);
		if (_tileLocation != null)
		{
			_tileLocation.area.structureComponent.AddStructureConnector(this);
			Messenger.AddListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_PLACED, OnStructureConnectorPlaced);
			Messenger.AddListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_REMOVED, OnStructureConnectorRemoved);
		}
	}

	public void Reset()
	{
		if (_tileLocation != null)
		{
			Messenger.RemoveListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_PLACED, OnStructureConnectorPlaced);
			Messenger.RemoveListener<LocationGridTile>(StructureSignals.STRUCTURE_CONNECTOR_REMOVED, OnStructureConnectorRemoved);
			_tileLocation.RemoveStructureConnector(this);
			Messenger.Broadcast(StructureSignals.STRUCTURE_CONNECTOR_REMOVED, _tileLocation);
		}
		_isOpen = true;
		_tileLocation = null;
	}

	public override string ToString()
	{
		return $"Connector at {_tileLocation}";
	}
}
