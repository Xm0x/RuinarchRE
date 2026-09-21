using System;

public class GoalTaskEventDispatcher
{
	public interface IGoalTaskListener
	{
		void OnGoalTaskNameUpdated(GoalTask p_task);

		void OnGoalTaskCompleted(GoalTask p_task);

		void OnGoalTaskPinStateChanged(GoalTask p_task, bool p_isPinned);
	}

	private Action<GoalTask> _onGoalTaskNameUpdated;

	private Action<GoalTask> _onTaskCompleted;

	private Action<GoalTask, bool> _onTaskPinStateChanged;

	public void SubscribeToGoalTask(IGoalTaskListener p_listener)
	{
		_onGoalTaskNameUpdated = (Action<GoalTask>)Delegate.Combine(_onGoalTaskNameUpdated, new Action<GoalTask>(p_listener.OnGoalTaskNameUpdated));
		_onTaskCompleted = (Action<GoalTask>)Delegate.Combine(_onTaskCompleted, new Action<GoalTask>(p_listener.OnGoalTaskCompleted));
		_onTaskPinStateChanged = (Action<GoalTask, bool>)Delegate.Combine(_onTaskPinStateChanged, new Action<GoalTask, bool>(p_listener.OnGoalTaskPinStateChanged));
	}

	public void UnsubscribeToGoalTask(IGoalTaskListener p_listener)
	{
		_onGoalTaskNameUpdated = (Action<GoalTask>)Delegate.Remove(_onGoalTaskNameUpdated, new Action<GoalTask>(p_listener.OnGoalTaskNameUpdated));
		_onTaskCompleted = (Action<GoalTask>)Delegate.Remove(_onTaskCompleted, new Action<GoalTask>(p_listener.OnGoalTaskCompleted));
		_onTaskPinStateChanged = (Action<GoalTask, bool>)Delegate.Remove(_onTaskPinStateChanged, new Action<GoalTask, bool>(p_listener.OnGoalTaskPinStateChanged));
	}

	public void ExecuteOnGoalTaskNameUpdated(GoalTask p_task)
	{
		_onGoalTaskNameUpdated?.Invoke(p_task);
	}

	public void ExecuteOnGoalTaskCompleted(GoalTask p_task)
	{
		_onTaskCompleted?.Invoke(p_task);
	}

	public void ExecuteOnGoalTaskPinStateChanged(GoalTask p_task, bool p_isPinned)
	{
		_onTaskPinStateChanged?.Invoke(p_task, p_isPinned);
	}
}
