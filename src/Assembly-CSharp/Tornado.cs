using System;
using Inner_Maps;

public class Tornado : MovingTileObject
{
	private TornadoMapObjectVisual _tornadoMapObjectVisual;

	public int radius { get; private set; }

	public GameDate expiryDate { get; private set; }

	public override string neutralizer => "Wind Master";

	protected override int affectedRange => 2;

	public override Type serializedData => typeof(SaveDataTornado);

	public Tornado()
	{
		Initialize(TILE_OBJECT_TYPE.TORNADO, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		SetRadius(2);
	}

	public Tornado(SaveDataTornado data)
		: base(data)
	{
		expiryDate = data.expiryDate;
		SetRadius(data.radius);
		base.hasExpired = data.hasExpired;
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_tornadoMapObjectVisual = mapVisual as TornadoMapObjectVisual;
	}

	public override void Neutralize()
	{
		_tornadoMapObjectVisual.Expire();
	}

	public void SetRadius(int radius)
	{
		this.radius = radius;
	}

	public void SetExpiryDate(GameDate expiry)
	{
		expiryDate = expiry;
	}

	public override void Expire()
	{
		if (!base.hasExpired)
		{
			base.Expire();
			Messenger.Broadcast<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
		}
	}

	public override string ToString()
	{
		return "Tornado " + base.id;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		base.traitContainer.AddTrait(this, "Dangerous");
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	protected override bool TryGetGridTileLocation(out LocationGridTile tile)
	{
		if (mapVisual != null)
		{
			TornadoMapObjectVisual tornadoMapObjectVisual = mapVisual as TornadoMapObjectVisual;
			if (tornadoMapObjectVisual.isSpawned)
			{
				tile = tornadoMapObjectVisual.gridTileLocation;
				return true;
			}
		}
		tile = null;
		return false;
	}
}
