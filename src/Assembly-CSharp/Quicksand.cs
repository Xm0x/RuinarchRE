using Inner_Maps;
using UnityEngine;

public class Quicksand : TileObject
{
	private QuicksandMapObjectVisual _quicksandMapVisual;

	public override string neutralizer => "Earth Master";

	public Quicksand()
	{
		Initialize(TILE_OBJECT_TYPE.QUICKSAND, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public Quicksand(SaveDataTileObject data)
		: base(data)
	{
	}

	protected override void CreateMapObjectVisual()
	{
		GameObject gameObject = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(base.tileObjectType);
		_quicksandMapVisual = gameObject.GetComponent<QuicksandMapObjectVisual>();
		mapVisual = _quicksandMapVisual;
	}

	public override void Neutralize()
	{
		_quicksandMapVisual.Expire();
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		base.traitContainer.AddTrait(this, "Dangerous");
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public override string ToString()
	{
		return "Quicksand";
	}
}
