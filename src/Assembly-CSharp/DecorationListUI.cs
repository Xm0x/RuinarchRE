using System.Collections;
using Inner_Maps;
using UnityEngine;
using UnityEngine.UI;

public class DecorationListUI : MonoBehaviour
{
	[SerializeField]
	private GridLayoutGroup decorationsLayoutGroup;

	[SerializeField]
	private GameObject tileOjectItemPrefab;

	[SerializeField]
	private UIHoverPosition tooltipPosition;

	[SerializeField]
	private ToggleGroup _toggleGroup;

	private TileObjectTypeItem[] _decorationItems;

	private DecorationsData _decorationsSkill;

	private TileObjectTypeItem _currentlyChosenNameplate;

	private Sprite _decorSprite;

	public TileObjectTypeItem currentlyChosenNameplate => _currentlyChosenNameplate;

	public void ShowDecorationsUI()
	{
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		if (PlayerManager.Instance.player.currentActivePlayerSpell != null)
		{
			PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		}
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(_decorationsSkill);
	}

	public void HideDecorationsUI()
	{
		if (PlayerManager.Instance.player.currentActivePlayerSpell == _decorationsSkill)
		{
			PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		}
		if (PlayerManager.Instance.player.currentActivePlayerSpell == _decorationsSkill)
		{
			PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		}
	}

	public void Open()
	{
		base.gameObject.SetActive(value: true);
		StartCoroutine(UpdateItemNameTexts());
	}

	public void Close()
	{
		if (_currentlyChosenNameplate != null)
		{
			_currentlyChosenNameplate.toggle.isOn = false;
		}
		base.gameObject.SetActive(value: false);
	}

	public void Initialize()
	{
		_decorationsSkill = PlayerSkillManager.Instance.GetBuildSkillData(PLAYER_SKILL_TYPE.DECORATIONS) as DecorationsData;
		_decorationItems = new TileObjectTypeItem[_decorationsSkill.decorationsType.Length];
		PopulateDecorationsList();
	}

	private void PopulateDecorationsList()
	{
		for (int i = 0; i < _decorationsSkill.decorationsType.Length; i++)
		{
			TILE_OBJECT_TYPE p_type = _decorationsSkill.decorationsType[i];
			_decorationItems[i] = CreateDecorationItems(p_type);
		}
	}

	private TileObjectTypeItem CreateDecorationItems(TILE_OBJECT_TYPE p_type)
	{
		TileObjectTypeItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(tileOjectItemPrefab.name, Vector3.zero, Quaternion.identity, decorationsLayoutGroup.transform).GetComponent<TileObjectTypeItem>();
		component.SetObject(p_type);
		component.SetAsToggle();
		component.SetToggleGroup(_toggleGroup);
		component.ClearAllHoverEnterActions();
		component.ClearAllHoverExitActions();
		component.AddHoverEnterItem(OnHoverEnterDecorationItem);
		component.AddHoverExitItem(OnHoverExitDecorationItem);
		component.AddOnToggleAction(OnToggleItem);
		return component;
	}

	private IEnumerator UpdateItemNameTexts()
	{
		yield return null;
		for (int i = 0; i < _decorationItems.Length; i++)
		{
			_decorationItems[i].UpdateNameText();
		}
	}

	private void OnHoverEnterDecorationItem(TileObjectTypeItem p_item)
	{
		PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(p_item.localizedName, string.Empty, InnerMapManager.Instance.GetCorruptedTileObjectAsset(p_item.type, POI_STATE.ACTIVE, p_getFirstAsset: true), tooltipPosition, autoReplaceText: true, "", new Vector2(64f, 64f));
	}

	private void OnHoverExitDecorationItem(TileObjectTypeItem p_item)
	{
		PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
	}

	private void OnToggleItem(TILE_OBJECT_TYPE p_type, bool p_state)
	{
		int indexOf = GetIndexOf(p_type);
		if (p_state)
		{
			_currentlyChosenNameplate = _decorationItems[indexOf];
			_decorSprite = InnerMapManager.Instance.GetCorruptedTileObjectAsset(_currentlyChosenNameplate.type, POI_STATE.ACTIVE, p_getFirstAsset: true);
			_decorationsSkill.SetChosenTileObjectTypeIndex(indexOf);
		}
		else
		{
			_currentlyChosenNameplate = null;
			_decorSprite = null;
			_decorationsSkill.SetChosenTileObjectTypeIndex(-1);
		}
		BaseBuildingManager.Instance.ResetDecorationRotation();
	}

	private int GetIndexOf(TILE_OBJECT_TYPE p_type)
	{
		for (int i = 0; i < _decorationItems.Length; i++)
		{
			if (_decorationItems[i].type == p_type)
			{
				return i;
			}
		}
		return -1;
	}

	public void UpdateDecorationItemsInteractableState(bool p_state)
	{
		for (int i = 0; i < _decorationItems.Length; i++)
		{
			TileObjectTypeItem tileObjectTypeItem = _decorationItems[i];
			if (tileObjectTypeItem != null)
			{
				if (!p_state)
				{
					tileObjectTypeItem.toggle.isOn = false;
				}
				tileObjectTypeItem.SetInteractableState(p_state);
			}
		}
	}
}
