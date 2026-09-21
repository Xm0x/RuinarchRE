using System;
using UnityEngine;

namespace UtilityScripts;

[Serializable]
public class RuinarchCooldown
{
	public string cooldownName;

	public GameDate cooldownStart;

	public GameDate cooldownEnd;

	public int totalTicksInCooldown;

	public int currentCooldownProgress;

	public RuinarchCooldown(string p_name)
	{
		cooldownName = p_name;
	}

	public void LoadStart()
	{
		Messenger.AddListener(Signals.TICK_ENDED, TimerTick);
	}

	public void Start(GameDate p_start, GameDate p_end)
	{
		cooldownStart = p_start;
		cooldownEnd = p_end;
		totalTicksInCooldown = p_start.GetTickDifference(p_end);
		currentCooldownProgress = 0;
		Messenger.AddListener(Signals.TICK_ENDED, TimerTick);
		Debug.Log("Started " + cooldownName + ". ETA is " + p_end.ToString());
	}

	private void TimerTick()
	{
		currentCooldownProgress++;
		if (IsFinished())
		{
			TimerHasReachedEnd();
		}
	}

	private void TimerHasReachedEnd()
	{
		Stop();
	}

	public bool IsFinished()
	{
		return currentCooldownProgress == totalTicksInCooldown;
	}

	public int GetRemainingTicks()
	{
		return totalTicksInCooldown - currentCooldownProgress;
	}

	public string GetRemainingTimeString()
	{
		int remainingTicks = GetRemainingTicks();
		return GameManager.GetTimeAsWholeDuration(remainingTicks) + " " + GameManager.GetTimeIdentifierAsWholeDuration(remainingTicks);
	}

	public void Stop()
	{
		cooldownStart = default(GameDate);
		cooldownEnd = default(GameDate);
		currentCooldownProgress = 0;
		totalTicksInCooldown = 0;
		Messenger.RemoveListener(Signals.TICK_ENDED, TimerTick);
	}
}
