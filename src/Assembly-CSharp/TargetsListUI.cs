using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class TargetsListUI : PopupMenuBase, BookmarkableEventDispatcher.IListener
{
	[SerializeField]
	private Toggle tglTargets;

	[SerializeField]
	private UIHoverPosition tooltipPos;

	[SerializeField]
	private ScrollRect listScrollRect;

	[SerializeField]
	private GameObject prefabStoredTargetPortrait;

	public override void Open()
	{
		base.Open();
		tglTargets.SetIsOnWithoutNotify(value: true);
	}

	public override void Close()
	{
		base.Close();
		tglTargets.SetIsOnWithoutNotify(value: false);
	}

	protected override void OnGameObjectEnabled()
	{
		base.OnGameObjectEnabled();
		Messenger.Broadcast(UISignals.TOP_UI_ENABLED);
	}

	protected override void OnGameObjectDisabled()
	{
		base.OnGameObjectDisabled();
		Messenger.Broadcast(UISignals.TOP_UI_DISABLED);
	}

	public void Initialize()
	{
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnPlayerRemovedStoredTarget);
		for (int i = 0; i < PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets.Count; i++)
		{
			IStoredTarget p_target = PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets[i];
			if (!HasStoredTargetItem(p_target))
			{
				CreatePortraitForStoredTarget(p_target);
			}
		}
	}

	private void OnHoverOver(IStoredTarget p_target)
	{
		if (p_target is Character character)
		{
			UIManager.Instance.ShowCharacterNameplateTooltip(character, tooltipPos);
		}
		else if (p_target is TileObject tileObject)
		{
			UIManager.Instance.ShowTileObjectNameplateTooltip(tileObject, tooltipPos);
		}
		else if (p_target is LocationStructure structure)
		{
			UIManager.Instance.ShowStructureNameplateTooltip(structure, tooltipPos);
		}
	}

	private void OnHoverOut(IStoredTarget p_target)
	{
		if (p_target is Character)
		{
			UIManager.Instance.HideCharacterNameplateTooltip();
			return;
		}
		if (p_target is TileObject)
		{
			UIManager.Instance.HideTileObjectNameplateTooltip();
			return;
		}
		if (p_target is LocationStructure)
		{
			UIManager.Instance.HideStructureNameplateTooltip();
			return;
		}
		UIManager.Instance.HideCharacterNameplateTooltip();
		UIManager.Instance.HideTileObjectNameplateTooltip();
		UIManager.Instance.HideStructureNameplateTooltip();
	}

	private void OnPlayerRemovedStoredTarget(IStoredTarget p_target)
	{
		p_target.bookmarkEventDispatcher.Unsubscribe(this, p_target);
		DestroyPortraitOfStoredTarget(p_target);
	}

	private void OnPlayerStoredTarget(IStoredTarget p_target)
	{
		p_target.bookmarkEventDispatcher.Subscribe(this, p_target);
		CreatePortraitForStoredTarget(p_target);
	}

	private void UpdateNameOfSpecificItem(IStoredTarget p_target)
	{
		StoredTargetPortrait[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<StoredTargetPortrait>(listScrollRect.content.gameObject);
		foreach (StoredTargetPortrait storedTargetPortrait in componentsInDirectChildren)
		{
			if (storedTargetPortrait.storedTarget == p_target)
			{
				storedTargetPortrait.UpdateName();
				break;
			}
		}
	}

	private bool HasStoredTargetItem(IStoredTarget p_target)
	{
		StoredTargetPortrait[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<StoredTargetPortrait>(listScrollRect.content.gameObject);
		for (int i = 0; i < componentsInDirectChildren.Length; i++)
		{
			if (componentsInDirectChildren[i].storedTarget == p_target)
			{
				return true;
			}
		}
		return false;
	}

	public void OnBookmarkRemoved(IBookmarkable p_bookmarkable)
	{
		if (p_bookmarkable is IStoredTarget p_target)
		{
			DestroyPortraitOfStoredTarget(p_target);
		}
	}

	public void OnBookmarkChangedName(IBookmarkable p_bookmarkable)
	{
		if (p_bookmarkable is IStoredTarget p_target)
		{
			UpdateNameOfSpecificItem(p_target);
		}
	}

	private void CreatePortraitForStoredTarget(IStoredTarget p_target)
	{
		ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabStoredTargetPortrait.name, Vector3.zero, Quaternion.identity, listScrollRect.content).GetComponent<StoredTargetPortrait>().SetStoredTarget(p_target);
	}

	private void DestroyPortraitOfStoredTarget(IStoredTarget p_target)
	{
		StoredTargetPortrait[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<StoredTargetPortrait>(listScrollRect.content.gameObject);
		foreach (StoredTargetPortrait storedTargetPortrait in componentsInDirectChildren)
		{
			if (storedTargetPortrait.storedTarget == p_target)
			{
				ObjectPoolManager.Instance.DestroyObject(storedTargetPortrait);
				break;
			}
		}
	}
}
