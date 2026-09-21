using UnityEngine;

public class BigSpawningPit : TileObject
{
	public override Vector2 selectableSize => new Vector2(1.7f, 1.7f);

	public override Vector3 worldPosition
	{
		get
		{
			Vector3 position = mapVisual.transform.position;
			position.x += 0.5f;
			position.y += 0.5f;
			return position;
		}
	}

	public BigSpawningPit()
	{
		Initialize(TILE_OBJECT_TYPE.BIG_SPAWNING_PIT);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public BigSpawningPit(SaveDataTileObject data)
		: base(data)
	{
	}
}
