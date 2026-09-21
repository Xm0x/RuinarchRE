namespace Inner_Maps.Location_Structures;

public class VampireCastle : ManMadeStructure
{
	public VampireCastle(Region location)
		: base(STRUCTURE_TYPE.VAMPIRE_CASTLE, location)
	{
	}

	public VampireCastle(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
	}

	protected override void OnAddResident(Character newResident)
	{
		base.OnAddResident(newResident);
		if (!GameManager.Instance.gameHasStarted)
		{
			return;
		}
		ProcessAllTileObjects(delegate(TileObject t)
		{
			if (t.isPreplaced)
			{
				t.UpdateOwners();
			}
		});
	}

	public override bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null)
	{
		if (base.AddPOI(poi, tileLocation))
		{
			if (poi is TileObject)
			{
				_ = poi.gridTileLocation;
			}
			return true;
		}
		return false;
	}

	public override bool RemovePOI(IPointOfInterest poi, Character removedBy = null, bool isPlayerSource = false)
	{
		if (base.RemovePOI(poi, removedBy, isPlayerSource))
		{
			_ = poi is TileObject;
			return true;
		}
		return false;
	}

	public override bool RemovePOIWithoutDestroying(IPointOfInterest poi)
	{
		if (base.RemovePOIWithoutDestroying(poi))
		{
			_ = poi is TileObject;
			return true;
		}
		return false;
	}

	public override bool RemovePOIDestroyVisualOnly(IPointOfInterest poi, Character remover = null)
	{
		if (base.RemovePOIDestroyVisualOnly(poi, remover))
		{
			_ = poi is TileObject;
			return true;
		}
		return false;
	}
}
