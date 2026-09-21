using System;
using System.Collections.Generic;
using UtilityScripts;

public class SchedulingManager : BaseMonoBehaviour
{
	public static SchedulingManager Instance;

	private Dictionary<GameDate, List<ScheduledAction>> schedules = new Dictionary<GameDate, List<ScheduledAction>>(new GameDateComparer());

	private GameDate checkGameDate;

	private List<ScheduledAction> _actionsToDo;

	private void Awake()
	{
		Instance = this;
		_actionsToDo = new List<ScheduledAction>();
	}

	protected override void OnDestroy()
	{
		schedules.Clear();
		schedules = null;
		_actionsToDo.Clear();
		_actionsToDo = null;
		base.OnDestroy();
		Instance = null;
	}

	public void StartScheduleCalls()
	{
		checkGameDate = GameManager.Instance.Today();
		Messenger.AddListener(Signals.CHECK_SCHEDULES, CheckSchedule);
	}

	private void CheckSchedule()
	{
		checkGameDate = GameManager.Instance.Today();
		if (schedules.ContainsKey(checkGameDate))
		{
			_actionsToDo.Clear();
			_actionsToDo.AddRange(schedules[checkGameDate]);
			DoAsScheduled(_actionsToDo);
			RemoveEntry(checkGameDate);
		}
	}

	internal string AddEntry(GameDate gameDate, Action act, object adder)
	{
		if (!schedules.ContainsKey(gameDate))
		{
			schedules.Add(gameDate, RuinarchListPool<ScheduledAction>.Claim(10));
		}
		string text = GenerateScheduleID();
		ScheduledAction scheduledAction = ObjectPoolManager.Instance.CreateNewScheduledAction();
		scheduledAction.SetData(text, act, adder);
		schedules[gameDate].Add(scheduledAction);
		return text;
	}

	internal void RemoveEntry(GameDate gameDate)
	{
		if (schedules.ContainsKey(gameDate))
		{
			List<ScheduledAction> list = schedules[gameDate];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					OnRemoveScheduledAction(list[i]);
				}
				RuinarchListPool<ScheduledAction>.Release(list);
			}
		}
		schedules.Remove(gameDate);
	}

	internal void RemoveSpecificEntry(int month, int day, int year, int hour, int continuousDays, Action act)
	{
		GameDate key = new GameDate(month, day, year, hour);
		if (!schedules.ContainsKey(key))
		{
			return;
		}
		List<ScheduledAction> list = schedules[key];
		for (int i = 0; i < list.Count; i++)
		{
			ScheduledAction scheduledAction = list[i];
			if (scheduledAction.action.Target == act.Target)
			{
				OnRemoveScheduledAction(scheduledAction);
				schedules[key].RemoveAt(i);
				break;
			}
		}
	}

	internal void RemoveSpecificEntry(GameDate date, Action act)
	{
		if (!schedules.ContainsKey(date))
		{
			return;
		}
		List<ScheduledAction> list = schedules[date];
		for (int i = 0; i < list.Count; i++)
		{
			ScheduledAction scheduledAction = list[i];
			if (scheduledAction.action.Target == act.Target)
			{
				OnRemoveScheduledAction(scheduledAction);
				schedules[date].RemoveAt(i);
				break;
			}
		}
	}

	private bool RemoveSpecificEntry(GameDate date, string id)
	{
		if (schedules.ContainsKey(date))
		{
			List<ScheduledAction> list = schedules[date];
			for (int i = 0; i < list.Count; i++)
			{
				ScheduledAction scheduledAction = list[i];
				if (scheduledAction.scheduleID == id)
				{
					OnRemoveScheduledAction(scheduledAction);
					schedules[date].RemoveAt(i);
					return true;
				}
			}
		}
		return false;
	}

	public bool RemoveSpecificEntry(string id)
	{
		foreach (KeyValuePair<GameDate, List<ScheduledAction>> schedule in schedules)
		{
			if (RemoveSpecificEntry(schedule.Key, id))
			{
				return true;
			}
		}
		return false;
	}

	private void OnRemoveScheduledAction(ScheduledAction p_sa)
	{
		ObjectPoolManager.Instance.ReturnScheduledActionToPool(p_sa);
	}

	private void EvaluateRemovingOfSchedules()
	{
	}

	private void DoAsScheduled(List<ScheduledAction> acts)
	{
		_ = acts.Count;
		int num = 0;
		for (int i = 0; i < acts.Count; i++)
		{
			ScheduledAction scheduledAction = acts[i];
			if (schedules[checkGameDate].Contains(scheduledAction) && scheduledAction.IsScheduleStillValid() && scheduledAction.action != null && scheduledAction.action.Target != null)
			{
				scheduledAction.action();
			}
			num++;
		}
	}

	public void ClearAllSchedulesBy(Character character)
	{
		foreach (KeyValuePair<GameDate, List<ScheduledAction>> schedule in schedules)
		{
			List<ScheduledAction> value = schedule.Value;
			for (int i = 0; i < value.Count; i++)
			{
				ScheduledAction scheduledAction = value[i];
				if (scheduledAction.scheduler == character)
				{
					OnRemoveScheduledAction(scheduledAction);
					value.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public void ClearAllSchedulesBy(object obj)
	{
		foreach (KeyValuePair<GameDate, List<ScheduledAction>> schedule in schedules)
		{
			List<ScheduledAction> value = schedule.Value;
			for (int i = 0; i < value.Count; i++)
			{
				ScheduledAction scheduledAction = value[i];
				if (scheduledAction.scheduler == obj)
				{
					OnRemoveScheduledAction(scheduledAction);
					value.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public string GenerateScheduleID()
	{
		return Guid.NewGuid().ToString("N");
	}
}
