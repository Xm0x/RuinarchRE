public class Desk : TileObject
{
	public Desk()
	{
		Initialize(TILE_OBJECT_TYPE.DESK);
		AddAdvertisedAction(INTERACTION_TYPE.SIT);
		AddAdvertisedAction(INTERACTION_TYPE.PLAY_CARDS);
	}

	public Desk(SaveDataTileObject data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Desk " + base.id;
	}

	public override void SetPOIState(POI_STATE state)
	{
		base.SetPOIState(state);
		if (gridTileLocation != null && mapVisual != null)
		{
			mapVisual.UpdateTileObjectVisual(this);
		}
	}

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		base.OnDoActionToObject(action);
		INTERACTION_TYPE goapType = action.goapType;
		if (goapType == INTERACTION_TYPE.SIT || goapType == INTERACTION_TYPE.STUDY_MAGIC)
		{
			AddUser(action.actor);
		}
	}

	public override void OnDoneActionToObject(ActualGoapNode action)
	{
		base.OnDoneActionToObject(action);
		INTERACTION_TYPE goapType = action.goapType;
		if (goapType == INTERACTION_TYPE.SIT || goapType == INTERACTION_TYPE.STUDY_MAGIC)
		{
			RemoveUser(action.actor);
		}
	}

	public override void OnCancelActionTowardsObject(ActualGoapNode action)
	{
		base.OnCancelActionTowardsObject(action);
		INTERACTION_TYPE goapType = action.goapType;
		if (goapType == INTERACTION_TYPE.SIT || goapType == INTERACTION_TYPE.STUDY_MAGIC)
		{
			RemoveUser(action.actor);
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.MAGIC_ACADEMY)
		{
			AddAdvertisedAction(INTERACTION_TYPE.STUDY_MAGIC);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.STUDY_MAGIC);
		}
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		RemoveAdvertisedAction(INTERACTION_TYPE.STUDY_MAGIC);
	}
}
