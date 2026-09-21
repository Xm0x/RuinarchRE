using System;

namespace UtilityScripts;

[Serializable]
public class RuinarchTimer : RuinarchProgressable
{
	public string timerName;

	public GameDate timerStart;

	public GameDate timerEnd;

	public bool hasStarted;

	private Action _onTimerEndAction;

	public override string progressableName => timerName;

	public int totalTicksInTimer => totalValue;

	public int currentTimerProgress => currentValue;

	public override BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Progress_Bar;

	public RuinarchTimer(string p_name)
	{
		timerName = p_name;
	}

	public void SetTimerName(string p_name)
	{
		timerName = p_name;
	}

	public void LoadStart(Action p_endAction = null)
	{
		Load();
		hasStarted = true;
		_onTimerEndAction = p_endAction;
		Messenger.AddListener(Signals.TICK_ENDED, TimerTick);
	}

	public void Start(GameDate p_start, GameDate p_end, Action p_endAction = null)
	{
		timerStart = p_start;
		timerEnd = p_end;
		hasStarted = true;
		int tickDifference = p_start.GetTickDifference(p_end);
		Setup(0, tickDifference);
		_onTimerEndAction = p_endAction;
		Messenger.AddListener(Signals.TICK_ENDED, TimerTick);
	}

	public bool IsFinished()
	{
		return currentTimerProgress == totalTicksInTimer;
	}

	private void TimerTick()
	{
		if (IsComplete())
		{
			TimerHasReachedEnd();
		}
		IncreaseProgress(1);
	}

	private void TimerHasReachedEnd()
	{
		Action onTimerEndAction = _onTimerEndAction;
		Stop();
		onTimerEndAction?.Invoke();
	}

	public float GetCurrentTimerProgressPercent()
	{
		float num = (float)currentTimerProgress / (float)totalTicksInTimer;
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		return num;
	}

	public int GetRemainingTicks()
	{
		return totalTicksInTimer - currentTimerProgress;
	}

	public string GetRemainingTimeString()
	{
		int remainingTicks = GetRemainingTicks();
		return GameManager.GetTimeAsWholeDuration(remainingTicks) + " " + GameManager.GetTimeIdentifierAsWholeDuration(remainingTicks);
	}

	public string GetTimerEndString()
	{
		return timerEnd.ToString() ?? "";
	}

	public void Stop()
	{
		Reset();
		timerStart = default(GameDate);
		timerEnd = default(GameDate);
		hasStarted = false;
		_onTimerEndAction = null;
		Messenger.RemoveListener(Signals.TICK_ENDED, TimerTick);
	}
}
