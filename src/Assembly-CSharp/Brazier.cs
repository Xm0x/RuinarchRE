using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Brazier : TileObject
{
	private InnerMapLight m_innerMapLight;

	public InnerMapLight InnerMap
	{
		get
		{
			if (m_innerMapLight == null)
			{
				m_innerMapLight = baseMapObjectVisual.GetComponentInChildren<InnerMapLight>(includeInactive: true);
			}
			return m_innerMapLight;
		}
	}

	public Brazier()
	{
		Initialize(TILE_OBJECT_TYPE.BRAZIER);
	}

	public Brazier(SaveDataTileObject data)
		: base(data)
	{
	}

	protected override void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
		base.OnPlaceTileObjectAtTile(tile);
		if (tile.structure is ManMadeStructure { structureObjectHasOwnLight: not false })
		{
			DisableInnerMapLight();
		}
		else
		{
			EnableInnerMapLight();
		}
	}

	private void EnableInnerMapLight()
	{
		if (InnerMap != null)
		{
			InnerMap.gameObject.SetActive(value: true);
		}
	}

	private void DisableInnerMapLight()
	{
		if (InnerMap != null)
		{
			InnerMap.gameObject.SetActive(value: false);
		}
	}
}
