using EZObjectPools;
using TMPro;
using UnityEngine;
using UtilityScripts;

public class GoalUIItem : PooledObject, GoalEventDispatcher.IGoalListener
{
	[SerializeField]
	private TextMeshProUGUI lblGoalName;

	[SerializeField]
	private GameObject goCompleted;

	[SerializeField]
	private GameObject goalTaskItemPrefab;

	[SerializeField]
	private Transform goalTasksParent;

	public Goal goal { get; private set; }

	public void Initialize(Goal p_goal)
	{
		goal = p_goal;
		lblGoalName.text = p_goal.goalName;
		goCompleted.SetActive(p_goal.isCompleted);
		goal.eventDispatcher.SubscribeToGoal(this);
		CreateGoalTaskItems(goal);
	}

	public void OnGoalCompleted(Goal p_goal)
	{
		goCompleted.SetActive(value: false);
	}

	public void OnChildTaskPinStateChanged(Goal p_goal, GoalTask p_affectedTask, bool p_isPinned)
	{
	}

	private void CreateGoalTaskItems(Goal p_goal)
	{
		Utilities.DestroyChildrenObjectPool(goalTasksParent);
		for (int i = 0; i < p_goal.tasks.Length; i++)
		{
			GoalTask p_task = p_goal.tasks[i];
			ObjectPoolManager.Instance.InstantiateObjectFromPool(goalTaskItemPrefab.name, Vector3.zero, Quaternion.identity, goalTasksParent).GetComponent<GoalTaskItem>().Initialize(p_task);
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (goal != null)
		{
			goal.eventDispatcher.UnsubscribeToGoal(this);
		}
	}
}
