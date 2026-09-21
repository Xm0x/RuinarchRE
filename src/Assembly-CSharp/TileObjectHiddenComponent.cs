using Inner_Maps;
using Inner_Maps.Location_Structures;

public class TileObjectHiddenComponent : TileObjectComponent
{
	public bool isHidden { get; private set; }

	public bool affectAlpha { get; private set; }

	public TileObjectHiddenComponent()
	{
	}

	public TileObjectHiddenComponent(SaveDataTileObjectHiddenComponent data)
	{
		isHidden = data.isHidden;
		affectAlpha = data.affectAlpha;
	}

	public void SetIsHidden(TileObject owner, bool state, bool affectAlpha = true)
	{
		this.affectAlpha = affectAlpha;
		if (isHidden != state)
		{
			isHidden = state;
			LocationGridTile gridTileLocation = owner.gridTileLocation;
			if (gridTileLocation != null)
			{
				gridTileLocation.structure.RemovePOI(owner);
				gridTileLocation.structure.AddPOI(owner, gridTileLocation);
			}
			OnSetHiddenState(owner);
		}
	}

	public void OnSetHiddenState(TileObject owner)
	{
		if (!affectAlpha)
		{
			return;
		}
		BaseMapObjectVisual mapObjectVisual = owner.mapObjectVisual;
		if ((bool)mapObjectVisual)
		{
			if (isHidden)
			{
				mapObjectVisual.SetVisualAlpha(0.5f);
			}
			else
			{
				mapObjectVisual.SetVisualAlpha(1f);
			}
		}
	}

	public void LoadSecondWave(TileObject owner)
	{
		OnSetHiddenState(owner);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
