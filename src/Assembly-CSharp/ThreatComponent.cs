using UtilityScripts;

public class ThreatComponent
{
	public const int MAX_THREAT = 100;

	private bool isDecreasingThreatPerHour;

	public Player player { get; private set; }

	public int threat { get; private set; }

	public ThreatComponent(Player player)
	{
		this.player = player;
		isDecreasingThreatPerHour = false;
	}

	public ThreatComponent()
	{
		isDecreasingThreatPerHour = false;
	}

	public void SetPlayer(Player player)
	{
		this.player = player;
	}

	public void AdjustThreatAndApplyModification(int amount)
	{
		AdjustThreat(amount);
	}

	public void AdjustThreat(int amount)
	{
	}

	private void OnMaxThreat()
	{
	}

	private void OnThreatIncreased()
	{
		if (!isDecreasingThreatPerHour)
		{
			isDecreasingThreatPerHour = true;
			Messenger.AddListener(Signals.HOUR_STARTED, DecreaseThreatPerHour);
		}
	}

	private void DecreaseThreatPerHour()
	{
		if (Utilities.IsEven(GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick)))
		{
			AdjustThreat(-1);
		}
	}

	private void OnThreatDecreased()
	{
		if (threat <= 0)
		{
			isDecreasingThreatPerHour = false;
			Messenger.Broadcast(PlayerSignals.THREAT_UPDATED);
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseThreatPerHour);
		}
		Messenger.Broadcast(PlayerSignals.STOP_THREAT_EFFECT);
	}

	public void SetThreatFromSave(int amount)
	{
		threat = amount;
	}
}
