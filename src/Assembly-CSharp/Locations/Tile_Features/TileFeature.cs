namespace Locations.Tile_Features;

public class TileFeature
{
	public string name { get; protected set; }

	public string description { get; protected set; }

	public virtual void OnAddFeature(Area tile)
	{
	}

	public virtual void OnRemoveFeature(Area tile)
	{
	}

	public virtual void OnDemolishLandmark(Area tile, LANDMARK_TYPE demolishedLandmarkType)
	{
	}

	public virtual void GameStartActions(Area tile)
	{
	}

	public virtual void LoadedGameStartActions(Area tile)
	{
		GameStartActions(tile);
	}
}
