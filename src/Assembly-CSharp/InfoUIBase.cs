using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public abstract class InfoUIBase : MonoBehaviour
{
	public Button backButton;

	public bool isShowing;

	private Action _openMenuAction;

	private Action _closeMenuAction;

	protected object _data;

	private IPlayerActionTarget _playerActionTarget;

	[Header("Actions")]
	[SerializeField]
	protected GameObject actionsGO;

	[SerializeField]
	protected RectTransform actionsTransform;

	[SerializeField]
	protected GameObject actionItemPrefab;

	private RuinarchToggle[] _toggles;

	protected List<ActionItem> activeActionItems = new List<ActionItem>();

	internal virtual void Initialize()
	{
		Messenger.AddListener<InfoUIBase>(UISignals.BEFORE_MENU_OPENED, BeforeMenuOpens);
		_toggles = GetComponentsInChildren<RuinarchToggle>(includeInactive: true);
	}

	protected void ListenToPlayerActionSignals()
	{
		Messenger.AddListener<PlayerAction>(PlayerSkillSignals.ON_EXECUTE_PLAYER_ACTION, OnExecutePlayerAction);
		Messenger.AddListener<IPlayerActionTarget>(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, ReloadPlayerActions);
		Messenger.AddListener(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS, ForceReloadPlayerActions);
		Messenger.AddListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, OnPlayerActionAddedToTarget);
		Messenger.AddListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, OnPlayerActionRemovedFromTarget);
	}

	private void OnReceiveHideMenuSignal()
	{
		if (isShowing)
		{
			OnClickCloseMenu();
		}
	}

	public virtual void OpenMenu()
	{
		Messenger.Broadcast(UISignals.BEFORE_MENU_OPENED, this);
		isShowing = true;
		bool activeSelf = base.gameObject.activeSelf;
		base.gameObject.SetActive(value: true);
		if (activeSelf && _toggles != null)
		{
			for (int i = 0; i < _toggles.Length; i++)
			{
				_toggles[i].FireToggleShownSignal();
			}
		}
		if (_openMenuAction != null)
		{
			_openMenuAction();
			_openMenuAction = null;
		}
		Messenger.Broadcast(UISignals.MENU_OPENED, this);
		UIManager.Instance.poiTestingUI.HideUI();
		UIManager.Instance.customDropdownList.Close();
		if (_data is Minion minion)
		{
			_playerActionTarget = minion.character;
		}
		else
		{
			_playerActionTarget = _data as IPlayerActionTarget;
		}
	}

	public virtual void CloseMenu()
	{
		isShowing = false;
		base.gameObject.SetActive(value: false);
		_closeMenuAction?.Invoke();
		_playerActionTarget = null;
		SetData(null);
		Messenger.Broadcast(UISignals.MENU_CLOSED, this);
	}

	public virtual void SetData(object data)
	{
		_data = data;
	}

	public virtual void ShowTooltip(GameObject objectHovered)
	{
	}

	protected virtual void OnExecutePlayerAction(PlayerAction action)
	{
		if (_playerActionTarget != null && _playerActionTarget.actions.Contains(action.type))
		{
			LoadActions(_playerActionTarget);
		}
	}

	public void OnClickCloseMenu()
	{
		CloseMenu();
	}

	private void BeforeMenuOpens(InfoUIBase baseToOpen)
	{
		if (isShowing && baseToOpen != this)
		{
			CloseMenu();
		}
	}

	public void AddCloseMenuAction(Action p_action)
	{
		_closeMenuAction = (Action)Delegate.Combine(_closeMenuAction, p_action);
	}

	public void AddOpenMenuAction(Action p_action)
	{
		_openMenuAction = (Action)Delegate.Combine(_openMenuAction, p_action);
	}

	protected void LoadActions(IPlayerActionTarget target)
	{
		if (!isShowing)
		{
			return;
		}
		List<SkillData> list = RuinarchListPool<SkillData>.Claim();
		for (int i = 0; i < activeActionItems.Count; i++)
		{
			ActionItem actionItem = activeActionItems[i];
			list.Add(actionItem.playerAction);
		}
		List<SkillData> list2 = RuinarchListPool<SkillData>.Claim();
		for (int j = 0; j < target.actions.Count; j++)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(target.actions[j]);
			if (ShouldCreateActionItem(skillData, target))
			{
				list2.Add(skillData);
			}
		}
		bool flag = list2.Count != list.Count;
		if (!flag)
		{
			for (int k = 0; k < list2.Count; k++)
			{
				PlayerAction item = list2[k] as PlayerAction;
				if (!list.Contains(item))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			RefreshActionItems(target);
		}
		else
		{
			for (int l = 0; l < activeActionItems.Count; l++)
			{
				ActionItem actionItem2 = activeActionItems[l];
				if (PlayerSkillManager.Instance.GetSkillData(actionItem2.playerAction.type) is PlayerAction playerAction)
				{
					actionItem2.RefreshAction(playerAction, target);
					actionItem2.SetInteractable(playerAction.CanPerformAbilityTo(target) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
					actionItem2.ForceUpdateCooldown();
				}
			}
		}
		RuinarchListPool<SkillData>.Release(list2);
		RuinarchListPool<SkillData>.Release(list);
		ActivateDeactivateActionGO();
	}

	private void RefreshActionItems(IPlayerActionTarget target)
	{
		for (int i = 0; i < activeActionItems.Count; i++)
		{
			ObjectPoolManager.Instance.DestroyObject(activeActionItems[i]);
		}
		activeActionItems.Clear();
		for (int j = 0; j < target.actions.Count; j++)
		{
			if (PlayerSkillManager.Instance.GetSkillData(target.actions[j]) is PlayerAction playerAction && ShouldCreateActionItem(playerAction, target))
			{
				ActionItem actionItem = AddNewAction(playerAction, target);
				actionItem.SetInteractable(playerAction.CanPerformAbilityTo(target) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
				actionItem.ForceUpdateCooldown();
			}
		}
	}

	protected virtual bool ShouldCreateActionItem(SkillData p_skill, IPlayerActionTarget p_target)
	{
		if (p_skill.IsValid(p_target))
		{
			return PlayerManager.Instance.player.playerSkillComponent.CanDoSkill(p_skill.type);
		}
		return false;
	}

	protected ActionItem AddNewAction(PlayerAction playerAction, IPlayerActionTarget target)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(actionItemPrefab.name, Vector3.zero, Quaternion.identity, actionsTransform);
		obj.SetActive(value: false);
		ActionItem component = obj.GetComponent<ActionItem>();
		component.SetAction(playerAction, target);
		activeActionItems.Add(component);
		return component;
	}

	public ActionItem GetActionItem(PlayerAction action)
	{
		for (int i = 0; i < activeActionItems.Count; i++)
		{
			ActionItem actionItem = activeActionItems[i];
			if (actionItem.playerAction == action)
			{
				return actionItem;
			}
		}
		return null;
	}

	private void OnPlayerActionAddedToTarget(PLAYER_SKILL_TYPE playerAction, IPlayerActionTarget actionTarget)
	{
		if (_playerActionTarget == actionTarget && isShowing)
		{
			LoadActions(actionTarget);
		}
	}

	private void OnPlayerActionRemovedFromTarget(PLAYER_SKILL_TYPE playerAction, IPlayerActionTarget actionTarget)
	{
		if (_playerActionTarget == actionTarget && isShowing)
		{
			LoadActions(actionTarget);
		}
	}

	private void ReloadPlayerActions(IPlayerActionTarget actionTarget)
	{
		if (_playerActionTarget == actionTarget && isShowing)
		{
			LoadActions(actionTarget);
		}
	}

	private void ForceReloadPlayerActions()
	{
		if (isShowing && _playerActionTarget != null)
		{
			LoadActions(_playerActionTarget);
		}
	}

	private void ActivateDeactivateActionGO()
	{
		actionsGO.SetActive(activeActionItems.Count > 0);
	}

	private void OnDestroy()
	{
		_closeMenuAction = null;
	}
}
