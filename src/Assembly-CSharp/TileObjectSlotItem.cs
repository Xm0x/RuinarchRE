using UnityEngine;

public class TileObjectSlotItem : MonoBehaviour
{
	private TileObject parentObj;

	private TileObjectSlotSetting settings;

	[SerializeField]
	private SpriteRenderer slotVisual;

	public Character user { get; private set; }

	public SpriteRenderer spriteRenderer => slotVisual;

	public void ApplySettings(TileObject parentObj, TileObjectSlotSetting settings)
	{
		this.parentObj = parentObj;
		this.settings = settings;
		base.name = $"{parentObj} - {settings.slotName}";
		slotVisual.sprite = settings.slotAsset;
		slotVisual.sortingOrder = 39;
		base.transform.localRotation = Quaternion.Euler(settings.assetRotation);
		UnusedPosition();
	}

	public void SetSlotColor(Color color)
	{
		slotVisual.color = color;
	}

	private void UnusedPosition()
	{
		base.transform.localPosition = settings.unusedPosition;
	}

	private void UsedPosition()
	{
		base.transform.localPosition = settings.usedPosition;
	}

	public void Use(Character character)
	{
		user = character;
		UsedPosition();
		user.marker.pathfindingAI.Teleport(base.transform.position);
		if (parentObj is Table && parentObj.mapVisual.usedSprite.name.Contains("bartop"))
		{
			character.marker.Rotate(Quaternion.Euler(0f, 0f, 0f));
		}
		else
		{
			user.marker.LookAt(parentObj.gridTileLocation.centeredWorldLocation);
		}
	}

	public void StopUsing()
	{
		if (user != null)
		{
			UnusedPosition();
			user = null;
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = user;
	}
}
