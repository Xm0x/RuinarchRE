using Inner_Maps;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UtilityScripts;

public class TileObjectTypeItem : NameplateItem<TILE_OBJECT_TYPE>
{
	[SerializeField]
	private Image _img;

	[SerializeField]
	private MaxRectTransform _maxRectTransform;

	private string _localizedName;

	private UnityEvent<TileObjectTypeItem> _onHoverEnter = new UnityEvent<TileObjectTypeItem>();

	private UnityEvent<TileObjectTypeItem> _onHoverExit = new UnityEvent<TileObjectTypeItem>();

	public TILE_OBJECT_TYPE type { get; private set; }

	public string localizedName => _localizedName;

	public override void SetObject(TILE_OBJECT_TYPE o)
	{
		base.SetObject(o);
		type = o;
		_localizedName = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", type.ToStringEnumWithSpace());
		base.name = _localizedName;
		button.name = _localizedName;
		base.toggle.name = _localizedName;
		mainLbl.text = _localizedName;
		_img.sprite = InnerMapManager.Instance.GetCorruptedTileObjectAsset(type, POI_STATE.ACTIVE, p_getFirstAsset: true);
	}

	public override void Reset()
	{
		base.Reset();
		button.name = "Button";
		base.toggle.name = "Toggle";
		_onHoverEnter.RemoveAllListeners();
		_onHoverExit.RemoveAllListeners();
	}

	public void UpdateNameText()
	{
		if (mainLbl.rectTransform.sizeDelta.x >= _maxRectTransform.maxX)
		{
			string text = mainLbl.text;
			mainLbl.text = text.Replace(' ', '\n');
		}
	}

	public void AddHoverEnterItem(UnityAction<TileObjectTypeItem> p_action)
	{
		_onHoverEnter.AddListener(p_action);
	}

	public void RemoveHoverEnterItem(UnityAction<TileObjectTypeItem> p_action)
	{
		_onHoverEnter.RemoveListener(p_action);
	}

	public void AddHoverExitItem(UnityAction<TileObjectTypeItem> p_action)
	{
		_onHoverExit.AddListener(p_action);
	}

	public void RemoveHoverExitItem(UnityAction<TileObjectTypeItem> p_action)
	{
		_onHoverExit.RemoveListener(p_action);
	}

	public override void OnHoverEnter()
	{
		_onHoverEnter.Invoke(this);
	}

	public override void OnHoverExit()
	{
		_onHoverExit.Invoke(this);
	}
}
