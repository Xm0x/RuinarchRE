using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Ocean : NaturalStructure
{
	public Ocean(Region location)
		: base(STRUCTURE_TYPE.OCEAN, location)
	{
	}

	public Ocean(Region location, SaveDataNaturalStructure data)
		: base(location, data)
	{
	}

	public override void CenterOnStructure()
	{
		if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != base.region.innerMap)
		{
			InnerMapManager.Instance.HideAreaMap();
		}
		if (!base.region.innerMap.isShowing)
		{
			InnerMapManager.Instance.ShowInnerMap(base.region);
		}
		if (base.occupiedArea != null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(base.occupiedArea.gridTileComponent.centerGridTile.centeredWorldLocation);
		}
	}

	public override void ShowSelectorOnStructure()
	{
	}

	public override bool TryGetMinimapColorForTileInStructure(LocationGridTile p_tile, out Color p_color)
	{
		if (p_tile.groundType == LocationGridTile.Ground_Type.Water_Deep)
		{
			p_color = GameUtilities.DeepWaterMinimapColor;
		}
		else if (p_tile.groundType == LocationGridTile.Ground_Type.Water_Mid)
		{
			p_color = GameUtilities.MidWaterMinimapColor;
		}
		else if (p_tile.groundType == LocationGridTile.Ground_Type.Water_Shallow)
		{
			p_color = GameUtilities.ShallowWaterMinimapColor;
		}
		else
		{
			p_color = GameUtilities.ShoreWaterMinimapColor;
		}
		return true;
	}

	protected override void OnTileAddedToStructure(LocationGridTile tile)
	{
		base.OnTileAddedToStructure(tile);
		tile.SetElevation(ELEVATION.WATER);
		tile.UpdateMinimapVisual(this);
	}

	protected override void OnTileRemovedFromStructure(LocationGridTile tile, LocationStructure removedFrom)
	{
		base.OnTileRemovedFromStructure(tile, removedFrom);
		tile.UpdateMinimapVisual(null);
	}
}
