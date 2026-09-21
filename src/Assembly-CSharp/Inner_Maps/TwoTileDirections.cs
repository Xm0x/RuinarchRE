using System;

namespace Inner_Maps;

[Serializable]
public struct TwoTileDirections
{
	public GridNeighbourDirection from;

	public GridNeighbourDirection to;

	public TwoTileDirections(GridNeighbourDirection from, GridNeighbourDirection to)
	{
		this.from = from;
		this.to = to;
	}
}
