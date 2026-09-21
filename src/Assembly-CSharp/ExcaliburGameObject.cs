using UnityEngine;

public class ExcaliburGameObject : TileObjectGameObject
{
	[Header("Excalibur Specific")]
	[SerializeField]
	private Sprite lockedSprite;

	[SerializeField]
	private Sprite unlockedSprite;

	public override void UpdateTileObjectVisual(TileObject tileObject)
	{
		SetVisual(lockedSprite);
	}
}
