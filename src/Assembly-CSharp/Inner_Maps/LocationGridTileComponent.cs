namespace Inner_Maps;

public class LocationGridTileComponent
{
	public LocationGridTile owner { get; private set; }

	public void SetOwner(LocationGridTile owner)
	{
		this.owner = owner;
	}
}
