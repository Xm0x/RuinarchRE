public class CombatData
{
	public string reasonForCombat;

	public string avoidReasonLogKey;

	public ActualGoapNode connectedAction;

	public bool isLethal;

	public bool attackBecauseOfCrime;

	public CombatData()
	{
		Initialize();
	}

	public CombatData(SaveDataCombatData data)
	{
		reasonForCombat = data.reasonForCombat;
		avoidReasonLogKey = data.avoidReasonLogKey;
		isLethal = data.isLethal;
		attackBecauseOfCrime = data.attackBecauseOfCrime;
		if (!string.IsNullOrEmpty(data.connectedAction))
		{
			connectedAction = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.connectedAction);
		}
	}

	public void Initialize()
	{
		reasonForCombat = string.Empty;
		avoidReasonLogKey = string.Empty;
		connectedAction = null;
		isLethal = false;
		attackBecauseOfCrime = false;
	}

	public void Reset()
	{
		ActualGoapNode actualGoapNode = connectedAction;
		bool num = actualGoapNode != null;
		if (num)
		{
			actualGoapNode.SetIsStillProcessing(p_state: false);
		}
		Initialize();
		if (num && actualGoapNode.isSupposedToBeInPool)
		{
			actualGoapNode.ProcessReturnToPool();
		}
	}

	public void SetFightData(string reasonForCombat, ActualGoapNode connectedAction, bool isLethal, bool willAttackBecauseOfCrime)
	{
		if (connectedAction != null)
		{
			connectedAction.SetIsStillProcessing(p_state: true);
		}
		this.reasonForCombat = reasonForCombat;
		this.connectedAction = connectedAction;
		this.isLethal = isLethal;
		attackBecauseOfCrime = willAttackBecauseOfCrime;
	}

	public void SetFlightData(string avoidReason)
	{
		avoidReasonLogKey = avoidReason;
	}
}
