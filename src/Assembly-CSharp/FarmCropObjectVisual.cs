using UnityEngine;

public class FarmCropObjectVisual : TileObjectGameObject
{
	[SerializeField]
	private Sprite horizontalEmpty;

	[SerializeField]
	private Sprite horizontalGrowing;

	[SerializeField]
	private Sprite horizontalHarvestable;

	[SerializeField]
	private Sprite verticalEmpty;

	[SerializeField]
	private Sprite verticalGrowing;

	[SerializeField]
	private Sprite verticalHarvestable;

	private bool _isHorizontal;

	public override void UpdateTileObjectVisual(TileObject tileObject)
	{
		Crops obj = tileObject as Crops;
		obj.GetRipeningTicks();
		if (obj.currentGrowthState == Crops.Growth_State.Growing)
		{
			SetVisual(_isHorizontal ? horizontalGrowing : verticalGrowing);
		}
		else
		{
			SetVisual(_isHorizontal ? horizontalHarvestable : verticalHarvestable);
		}
	}

	public override void SetVisual(Sprite sprite, int p_spriteIndex = -1)
	{
		base.SetVisual(sprite, p_spriteIndex);
		_isHorizontal = sprite.name.Contains("#1");
	}
}
