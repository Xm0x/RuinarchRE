using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubGoalItem : PooledObject, IPointerClickHandler, IEventSystemHandler, SubGoalEventDispatcher.ISubGoalListener
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private TextMeshProUGUI lblTaskName;

	[SerializeField]
	private EnvelopContentUnityUI envelopContent;

	[SerializeField]
	private HoverHandler hoverHandler;

	[SerializeField]
	private GameObject goPinned;

	private SubGoal _subGoal;

	public SubGoal subGoal => _subGoal;

	public void Initialize(SubGoal p_subGoal)
	{
		_subGoal = p_subGoal;
		toggle.SetIsOnWithoutNotify(p_subGoal.isComplete);
		UpdateName();
		_subGoal.subGoalEventDispatcher.SubscribeToSubGoal(this);
		envelopContent.Execute();
		goPinned.SetActive(_subGoal.isPinned);
		hoverHandler.SetOnHoverOverAction(OnHoverOverTask);
		hoverHandler.SetOnHoverOutAction(OnHoverOutTask);
	}

	private void UpdateName()
	{
		lblTaskName.text = _subGoal.localizedDescriptiveName;
	}

	private void OnHoverOverTask()
	{
		UIManager.Instance.ShowSmallInfo("<b>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "SubGoal_Reward_Title") + ":</b> " + _subGoal.completionReward.GetCostStringWithIcon(), "", autoReplaceText: false);
	}

	private void OnHoverOutTask()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_subGoal != null)
		{
			if (!_subGoal.isPinned)
			{
				_subGoal.Pin();
			}
			else
			{
				_subGoal.UnPin();
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		_subGoal.subGoalEventDispatcher.UnsubscribeToSubGoal(this);
		_subGoal = null;
		hoverHandler.ClearHoverActions();
	}

	public void OnSubGoalNameUpdated(SubGoal p_task)
	{
		UpdateName();
	}

	public void OnSubGoalCompleted(SubGoal p_subGoal)
	{
		toggle.SetIsOnWithoutNotify(p_subGoal.isComplete);
	}

	public void OnSubGoalPinStateChanged(SubGoal p_subGoal, bool p_isPinned)
	{
		goPinned.SetActive(_subGoal.isPinned);
	}
}
