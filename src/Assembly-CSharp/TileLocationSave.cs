using System;
using Inner_Maps;

[Serializable]
public struct TileLocationSave
{
	public bool hasValue;

	public int xPos;

	public int yPos;

	public TileLocationSave(LocationGridTile locationGridTile)
	{
		hasValue = true;
		xPos = locationGridTile.localPlace.x;
		yPos = locationGridTile.localPlace.y;
	}
}
