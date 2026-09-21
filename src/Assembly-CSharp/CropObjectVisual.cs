using Inner_Maps;

public class CropObjectVisual : TileObjectGameObject
{
	public override void UpdateTileObjectVisual(TileObject obj)
	{
		Crops crops = obj as Crops;
		if (crops.currentGrowthState == Crops.Growth_State.Growing)
		{
			ShowSprite(POI_STATE.INACTIVE, obj);
		}
		else if (crops.currentGrowthState == Crops.Growth_State.Ripe)
		{
			ShowSprite(POI_STATE.ACTIVE, obj);
		}
	}

	protected void ShowSprite(POI_STATE p_state, TileObject p_object)
	{
		SetVisual(InnerMapManager.Instance.GetTileObjectAsset(p_object, p_state, p_object.gridTileLocation.mainBiomeType, p_object.gridTileLocation?.corruptionComponent.isCorrupted ?? false));
	}
}
