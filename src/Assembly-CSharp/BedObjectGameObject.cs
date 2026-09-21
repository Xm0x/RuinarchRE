using Inner_Maps;
using UnityEngine;

public class BedObjectGameObject : TileObjectGameObject
{
	private Sprite _usedBedSprite;

	public BedMarkerNameplate nameplate { get; private set; }

	public TileObject bedTileObject => base.obj;

	public Rect usedBedRect => _usedBedSprite.rect;

	public override void Initialize(TileObject tileObject)
	{
		base.Initialize(tileObject);
		CreateNameplate();
	}

	public override void Reset()
	{
		base.Reset();
		ObjectPoolManager.Instance.DestroyObject(nameplate);
		nameplate = null;
		_usedBedSprite = null;
	}

	public override void UpdateTileObjectVisual(TileObject bed)
	{
		int userCount = bed.GetUserCount();
		if (userCount == 0)
		{
			if (_usedBedSprite == null)
			{
				SetVisual(InnerMapManager.Instance.GetTileObjectAsset(bed, bed.state, bed.gridTileLocation.mainBiomeType, bed.gridTileLocation?.corruptionComponent.isCorrupted ?? false));
			}
			else
			{
				BedSpriteSetting bedSpriteSettings = InnerMapManager.Instance.GetTileObjectScriptableObject<BedTileObjectScriptableObject>(bed.tileObjectType).GetBedSpriteSettings(_usedBedSprite);
				SetVisual(bedSpriteSettings.spriteUnoccupied);
			}
		}
		else if (userCount > 0)
		{
			BedSpriteSetting bedSpriteSettings2 = InnerMapManager.Instance.GetTileObjectScriptableObject<BedTileObjectScriptableObject>(bed.tileObjectType).GetBedSpriteSettings(_usedBedSprite);
			switch (userCount)
			{
			case 1:
				SetVisual(bedSpriteSettings2.sprite1Sleeping);
				break;
			case 2:
				SetVisual(bedSpriteSettings2.sprite2Sleeping);
				break;
			}
		}
		if (nameplate != null)
		{
			nameplate.UpdateMarkerNameplate(bed);
		}
	}

	public override Sprite GetSeizeSprite(IPointOfInterest poi)
	{
		if (poi is TileObject tileObject)
		{
			return InnerMapManager.Instance.GetTileObjectScriptableObject<BedTileObjectScriptableObject>(tileObject.tileObjectType).GetBedSpriteSettings(_usedBedSprite).spriteUnoccupied;
		}
		return _usedBedSprite;
	}

	private void CreateNameplate()
	{
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool("BedMarkerNameplate", base.transform.position, Quaternion.identity, UIManager.Instance.characterMarkerNameplateParent);
		nameplate = gameObject.GetComponent<BedMarkerNameplate>();
		nameplate.Initialize(this);
	}

	public override void SetVisual(Sprite sprite, int p_spriteIndex = -1)
	{
		base.SetVisual(sprite, p_spriteIndex);
		if (sprite != null)
		{
			_usedBedSprite = sprite;
		}
	}

	public override void OnSeizeVisual(IPointOfInterest poi)
	{
		base.OnSeizeVisual(poi);
		if (nameplate != null && poi is BaseBed baseBed)
		{
			nameplate.UpdateMarkerNameplate(baseBed);
		}
	}
}
