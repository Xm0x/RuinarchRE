public class Book : TileObject
{
	public Book()
	{
		Initialize(TILE_OBJECT_TYPE.BOOK);
	}

	public Book(SaveDataTileObject data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Book " + base.id;
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

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		base.OnDoActionToObject(action);
		if (action.goapType == INTERACTION_TYPE.STUDY_MAGIC)
		{
			AddUser(action.actor);
		}
	}

	public override void OnDoneActionToObject(ActualGoapNode action)
	{
		base.OnDoneActionToObject(action);
		if (action.goapType == INTERACTION_TYPE.STUDY_MAGIC)
		{
			RemoveUser(action.actor);
		}
	}

	public override void OnCancelActionTowardsObject(ActualGoapNode action)
	{
		base.OnCancelActionTowardsObject(action);
		if (action.goapType == INTERACTION_TYPE.STUDY_MAGIC)
		{
			RemoveUser(action.actor);
		}
	}
}
