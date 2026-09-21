using Ruinarch;
using UnityEngine;

public class TileObjectNameplateItem : NameplateItem<TileObject>
{
	[Header("Basic Data")]
	[SerializeField]
	private TileObjectPortrait tileObjectPortrait;

	public override void SetObject(TileObject o)
	{
		base.SetObject(o);
		tileObjectPortrait.SetTileObject(o);
		tileObjectPortrait.SetRightClickAction(OnRightClickPortrait);
		UpdateBasicData();
	}

	private void UpdateBasicData()
	{
		mainLbl.text = obj.name;
	}

	private void OnRightClickPortrait(TileObject p_tileObject)
	{
		UIManager.Instance.ShowPlayerActionContextMenu(p_tileObject, InputManager.Instance.mousePosition, p_isScreenPosition: true);
	}

	public void SetPosition(UIHoverPosition position)
	{
		UIManager.Instance.PositionTooltip(position, base.gameObject, base.transform as RectTransform);
	}
}
