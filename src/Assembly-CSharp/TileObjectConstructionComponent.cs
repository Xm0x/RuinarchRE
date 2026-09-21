using Inner_Maps;
using Inner_Maps.Location_Structures;

public class TileObjectConstructionComponent : TileObjectComponent
{
	private int _currentConstructionTick;

	private int _totalNeededConstructionTicks;

	private GameDate _expirationDate;

	private string _expirationScheduleKey;

	public int currentConstructionTick => _currentConstructionTick;

	public int totalNeededConstructionTicks => _totalNeededConstructionTicks;

	public GameDate expirationDate => _expirationDate;

	public TileObjectConstructionComponent(TileObject p_tileObject)
	{
		_currentConstructionTick = 0;
		_totalNeededConstructionTicks = TileObjectDB.GetTileObjectData(p_tileObject.tileObjectType).constructionTimeInTicks;
	}

	public TileObjectConstructionComponent(SaveDataTileObjectConstructionComponent data)
	{
		_totalNeededConstructionTicks = data.totalNeededConstructionTicks;
		_expirationDate = data.expirationDate;
	}

	public void LoadSecondWave(TileObject p_tileObject, SaveDataTileObjectConstructionComponent data)
	{
		if (_expirationDate.hasValue)
		{
			_expirationScheduleKey = SchedulingManager.Instance.AddEntry(_expirationDate, delegate
			{
				Expire(p_tileObject);
			}, this);
		}
		_currentConstructionTick = data.currentConstructionTick;
		if (p_tileObject is GenericTileObject { hasStartedBuildingBlueprintOnTile: not false } genericTileObject)
		{
			if (genericTileObject.constructionProgress != null)
			{
				genericTileObject.constructionProgress.SetConstructionProgress((float)(_currentConstructionTick + 1) / (float)_totalNeededConstructionTicks);
			}
		}
		else if (p_tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
		{
			TileObjectGameObject tileObjectGameObject = p_tileObject.mapObjectVisual as TileObjectGameObject;
			if (tileObjectGameObject != null)
			{
				tileObjectGameObject.SetConstructionProgress((float)(_currentConstructionTick + 1) / (float)_totalNeededConstructionTicks);
			}
		}
	}

	public void SetNeededConstructionTicks(int p_amount)
	{
		_totalNeededConstructionTicks = p_amount;
	}

	public void IncreaseConstructionTick(int p_amount, TileObject p_tileObject)
	{
		_currentConstructionTick += p_amount;
		float constructionProgress = (float)(_currentConstructionTick + 1) / (float)_totalNeededConstructionTicks;
		if (p_tileObject is GenericTileObject genericTileObject)
		{
			genericTileObject.constructionProgress.SetConstructionProgress(constructionProgress);
		}
		else if (p_tileObject.mapObjectVisual is TileObjectGameObject tileObjectGameObject)
		{
			tileObjectGameObject.SetConstructionProgress(constructionProgress);
		}
	}

	public int GetRemainingTicksForConstruction()
	{
		return _totalNeededConstructionTicks - _currentConstructionTick;
	}

	public void OnObjectSetAsBuilt()
	{
		_currentConstructionTick = _totalNeededConstructionTicks;
	}

	public void OnObjectSetAsUnbuilt()
	{
		_currentConstructionTick = 0;
	}

	public void OnConstructionCancelled(TileObject p_tileObject)
	{
		if (string.IsNullOrEmpty(_expirationScheduleKey))
		{
			ScheduleExpiration(p_tileObject);
		}
	}

	public void OnConstructionResumed(TileObject p_tileObject)
	{
		CancelExpiration(p_tileObject);
	}

	private void ScheduleExpiration(TileObject tileObject)
	{
		_expirationDate = GameManager.Instance.Today();
		_expirationDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(48));
		_expirationScheduleKey = SchedulingManager.Instance.AddEntry(_expirationDate, delegate
		{
			Expire(tileObject);
		}, this);
	}

	private void CancelExpiration(TileObject tileObject)
	{
		if (!string.IsNullOrEmpty(_expirationScheduleKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expirationScheduleKey);
			_expirationScheduleKey = string.Empty;
			_expirationDate = default(GameDate);
		}
	}

	private void Expire(TileObject tileObject)
	{
		_expirationScheduleKey = string.Empty;
		_expirationDate = default(GameDate);
		LocationGridTile gridTileLocation = tileObject.gridTileLocation;
		if (gridTileLocation != null)
		{
			gridTileLocation.structure.RemovePOI(tileObject);
			tileObject.resourceStorageComponent.SpawnResourcesInStorage(gridTileLocation);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
