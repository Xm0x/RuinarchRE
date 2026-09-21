using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class ThinWallGameObject : TileObjectGameObject
{
	[SerializeField]
	private SpriteRenderer[] _spriteRenderers;

	[SerializeField]
	private BoxCollider2D[] _unpassableColliders;

	[SerializeField]
	private bool _lockWallVisual;

	public SpriteRenderer[] spriteRenderers
	{
		get
		{
			if (_spriteRenderers == null)
			{
				_spriteRenderers = base.transform.GetComponentsInChildren<SpriteRenderer>();
			}
			return _spriteRenderers;
		}
	}

	private void Awake()
	{
		if (_spriteRenderers == null || _spriteRenderers.Length == 0)
		{
			_spriteRenderers = base.transform.GetComponentsInChildren<SpriteRenderer>();
		}
		if (_unpassableColliders == null || _unpassableColliders.Length == 0)
		{
			_unpassableColliders = objectVisual.transform.GetComponentsInChildren<BoxCollider2D>();
		}
		if (visionTrigger == null)
		{
			visionTrigger = base.transform.GetComponentInChildren<WallObjectVisionTrigger>();
		}
		particleEffectParent = objectVisual.transform;
		visionTrigger.gameObject.SetActive(value: false);
	}

	public void UpdateWallAssets(ThinWall structureWallObject)
	{
		if (_lockWallVisual)
		{
			return;
		}
		for (int i = 0; i < spriteRenderers.Length; i++)
		{
			SpriteRenderer spriteRenderer = spriteRenderers[i];
			string assetName = (spriteRenderer.sprite.name.Contains("vertical") ? "vertical" : ((!spriteRenderer.sprite.name.Contains("horizontal")) ? "corner" : "horizontal"));
			WallAsset wallAsset = InnerMapManager.Instance.GetWallAsset(structureWallObject.madeOf, assetName);
			if (structureWallObject.currentHP == structureWallObject.maxHP)
			{
				spriteRenderer.sprite = wallAsset.undamaged;
			}
			else
			{
				spriteRenderer.sprite = wallAsset.damaged;
			}
		}
	}

	public void ResetWallAssets(WALL_RESOURCE resource)
	{
		if (!_lockWallVisual)
		{
			for (int i = 0; i < spriteRenderers.Length; i++)
			{
				SpriteRenderer spriteRenderer = spriteRenderers[i];
				string assetName = ((!spriteRenderer.sprite.name.Contains("vertical")) ? ((!spriteRenderer.sprite.name.Contains("horizontal")) ? "corner" : "horizontal") : "vertical");
				WallAsset wallAsset = InnerMapManager.Instance.GetWallAsset(resource, assetName);
				spriteRenderer.sprite = wallAsset.undamaged;
			}
		}
	}

	public void UpdateWallAssets(WALL_RESOURCE madeOf)
	{
		if (!_lockWallVisual)
		{
			for (int i = 0; i < spriteRenderers.Length; i++)
			{
				SpriteRenderer spriteRenderer = spriteRenderers[i];
				WallAsset wallAsset = InnerMapManager.Instance.GetWallAsset(madeOf, spriteRenderer.sprite.name);
				spriteRenderer.sprite = wallAsset.undamaged;
			}
		}
	}

	public void UpdateSortingOrders(int sortingOrder)
	{
		for (int i = 0; i < spriteRenderers.Length; i++)
		{
			SpriteRenderer spriteRenderer = spriteRenderers[i];
			if (spriteRenderer.gameObject.name.Contains("Details"))
			{
				if (spriteRenderer.sprite.name.Contains("vine"))
				{
					spriteRenderer.sortingOrder = sortingOrder + 2;
				}
				else
				{
					spriteRenderer.sortingOrder = sortingOrder - 1;
				}
			}
			else if (spriteRenderer.sprite.name.Contains("corner"))
			{
				spriteRenderer.sortingOrder = sortingOrder + 1;
			}
			else
			{
				spriteRenderer.sortingOrder = sortingOrder;
			}
		}
	}

	public void UpdateWallState(ThinWall structureWallObject)
	{
		if (structureWallObject.currentHP == 0)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public void SetWallColor(Color color)
	{
		for (int i = 0; i < spriteRenderers.Length; i++)
		{
			spriteRenderers[i].color = color;
		}
	}

	public void SetUnpassableColliderState(bool state)
	{
		for (int i = 0; i < _unpassableColliders.Length; i++)
		{
			_unpassableColliders[i].enabled = state;
		}
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < _unpassableColliders.Length; i++)
		{
			_unpassableColliders[i].enabled = true;
		}
		if (particleEffectParent != null)
		{
			Utilities.DestroyChildren(particleEffectParent);
		}
		visionTrigger.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
	}

	public override void UpdateTileObjectVisual(TileObject obj)
	{
	}

	public bool IsVerticalWall()
	{
		SpriteRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].sprite.name.Contains("vertical"))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHorizontalWall()
	{
		SpriteRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].sprite.name.Contains("horizontal"))
			{
				return true;
			}
		}
		return false;
	}

	public void SetSizeAndOffsetOfUnpassableColliders(Vector2 p_offset, Vector2 p_size)
	{
		BoxCollider2D[] componentsInChildren = objectVisual.transform.GetComponentsInChildren<BoxCollider2D>();
		foreach (BoxCollider2D boxCollider2D in componentsInChildren)
		{
			if (boxCollider2D.gameObject.layer == LayerMask.NameToLayer("Unpassable"))
			{
				boxCollider2D.size = p_size;
				boxCollider2D.offset = p_offset;
			}
		}
	}
}
