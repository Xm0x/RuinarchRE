using System;

public class GoalEventDispatcher
{
	public interface IGoalListener
	{
		void OnGoalCompleted(Goal p_goal);

		void OnChildTaskPinStateChanged(Goal p_goal, GoalTask p_affectedTask, bool p_isPinned);
	}

	private Action<Goal> _onGoalCompleted;

	private Action<Goal, GoalTask, bool> _onChildTaskPinStateChanged;

	public void SubscribeToGoal(IGoalListener p_listener)
	{
		_onGoalCompleted = (Action<Goal>)Delegate.Combine(_onGoalCompleted, new Action<Goal>(p_listener.OnGoalCompleted));
		_onChildTaskPinStateChanged = (Action<Goal, GoalTask, bool>)Delegate.Combine(_onChildTaskPinStateChanged, new Action<Goal, GoalTask, bool>(p_listener.OnChildTaskPinStateChanged));
	}

	public void UnsubscribeToGoal(IGoalListener p_listener)
	{
		_onGoalCompleted = (Action<Goal>)Delegate.Remove(_onGoalCompleted, new Action<Goal>(p_listener.OnGoalCompleted));
		_onChildTaskPinStateChanged = (Action<Goal, GoalTask, bool>)Delegate.Remove(_onChildTaskPinStateChanged, new Action<Goal, GoalTask, bool>(p_listener.OnChildTaskPinStateChanged));
	}

	public void ExecuteOnGoalCompleted(Goal p_goal)
	{
		_onGoalCompleted?.Invoke(p_goal);
	}

	public void ExecuteOnChildTaskPinStateChanged(Goal p_goal, GoalTask p_affectedTask, bool p_isPinned)
	{
		_onChildTaskPinStateChanged?.Invoke(p_goal, p_affectedTask, p_isPinned);
	}
}
