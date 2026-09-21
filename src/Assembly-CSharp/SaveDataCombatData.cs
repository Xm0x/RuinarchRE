using System;

[Serializable]
public class SaveDataCombatData : SaveData<CombatData>
{
	public string reasonForCombat;

	public string avoidReasonLogKey;

	public string connectedAction;

	public bool isLethal;

	public bool attackBecauseOfCrime;

	public override void Save(CombatData data)
	{
		base.Save(data);
		reasonForCombat = data.reasonForCombat;
		avoidReasonLogKey = data.avoidReasonLogKey;
		isLethal = data.isLethal;
		attackBecauseOfCrime = data.attackBecauseOfCrime;
		if (data.connectedAction != null)
		{
			connectedAction = data.connectedAction.persistentID;
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.connectedAction);
		}
	}

	public override CombatData Load()
	{
		return new CombatData(this);
	}
}
