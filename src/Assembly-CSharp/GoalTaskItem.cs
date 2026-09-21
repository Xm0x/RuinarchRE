using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GoalTaskItem : PooledObject, GoalTaskEventDispatcher.IGoalTaskListener, IPointerClickHandler, IEventSystemHandler
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

	private GoalTask _task;

	public void Initialize(GoalTask p_task)
	{
		_task = p_task;
		toggle.SetIsOnWithoutNotify(p_task.isComplete);
		lblTaskName.text = p_task.taskName;
		_task.eventDispatcher.SubscribeToGoalTask(this);
		envelopContent.Execute();
		goPinned.SetActive(_task.isPinned);
		hoverHandler.SetOnHoverOverAction(OnHoverOverTask);
		hoverHandler.SetOnHoverOutAction(OnHoverOutTask);
	}

	public void OnGoalTaskNameUpdated(GoalTask p_task)
	{
		lblTaskName.text = p_task.taskName;
	}

	public void OnGoalTaskCompleted(GoalTask p_task)
	{
		toggle.SetIsOnWithoutNotify(p_task.isComplete);
	}

	public void OnGoalTaskPinStateChanged(GoalTask p_task, bool p_isPinned)
	{
		goPinned.SetActive(p_isPinned);
	}

	private void OnHoverOverTask()
	{
		UIManager.Instance.ShowSmallInfo(_task.taskTooltip, "", autoReplaceText: false);
	}

	private void OnHoverOutTask()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_task != null)
		{
			if (!_task.isPinned)
			{
				_task.Pin();
			}
			else
			{
				_task.UnPin();
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		_task.eventDispatcher.UnsubscribeToGoalTask(this);
		_task = null;
		hoverHandler.ClearHoverActions();
	}
}
