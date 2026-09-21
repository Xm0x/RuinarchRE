using System;

public class SubGoalEventDispatcher
{
	public interface ISubGoalListener
	{
		void OnSubGoalNameUpdated(SubGoal p_task);

		void OnSubGoalCompleted(SubGoal p_subGoal);

		void OnSubGoalPinStateChanged(SubGoal p_subGoal, bool p_isPinned);
	}

	private Action<SubGoal> _onSubGoalNameUpdated;

	private Action<SubGoal> _onSubGoalCompleted;

	private Action<SubGoal, bool> _onSubGoalPinStateChanged;

	public void SubscribeToSubGoal(ISubGoalListener p_listener)
	{
		_onSubGoalNameUpdated = (Action<SubGoal>)Delegate.Combine(_onSubGoalNameUpdated, new Action<SubGoal>(p_listener.OnSubGoalNameUpdated));
		_onSubGoalCompleted = (Action<SubGoal>)Delegate.Combine(_onSubGoalCompleted, new Action<SubGoal>(p_listener.OnSubGoalCompleted));
		_onSubGoalPinStateChanged = (Action<SubGoal, bool>)Delegate.Combine(_onSubGoalPinStateChanged, new Action<SubGoal, bool>(p_listener.OnSubGoalPinStateChanged));
	}

	public void UnsubscribeToSubGoal(ISubGoalListener p_listener)
	{
		_onSubGoalNameUpdated = (Action<SubGoal>)Delegate.Remove(_onSubGoalNameUpdated, new Action<SubGoal>(p_listener.OnSubGoalNameUpdated));
		_onSubGoalCompleted = (Action<SubGoal>)Delegate.Remove(_onSubGoalCompleted, new Action<SubGoal>(p_listener.OnSubGoalCompleted));
		_onSubGoalPinStateChanged = (Action<SubGoal, bool>)Delegate.Remove(_onSubGoalPinStateChanged, new Action<SubGoal, bool>(p_listener.OnSubGoalPinStateChanged));
	}

	public void ExecuteOnSubGoalNameUpdated(SubGoal p_subGoal)
	{
		_onSubGoalNameUpdated?.Invoke(p_subGoal);
	}

	public void ExecuteOnSubGoalCompleted(SubGoal p_subGoal)
	{
		_onSubGoalCompleted?.Invoke(p_subGoal);
	}

	public void ExecuteOnSubGoalPinStateChanged(SubGoal p_subGoal, bool p_isPinned)
	{
		_onSubGoalPinStateChanged?.Invoke(p_subGoal, p_isPinned);
	}
}
