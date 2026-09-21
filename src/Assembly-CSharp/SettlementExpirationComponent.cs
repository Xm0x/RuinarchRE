using Inner_Maps.Location_Structures;

public class SettlementExpirationComponent : NPCSettlementComponent
{
	private string _expirationScheduleKey;

	public GameDate expirationDate { get; private set; }

	public SettlementExpirationComponent()
	{
	}

	public SettlementExpirationComponent(SaveDataSettlementExpirationComponent p_data)
	{
		expirationDate = p_data.expirationDate;
	}

	public void LoadReferences(SaveDataSettlementExpirationComponent p_data)
	{
		if (expirationDate.hasValue)
		{
			ScheduleExpiry(expirationDate);
		}
	}

	public void OnSettlementAbandoned()
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE)
		{
			GameDate p_targetDate = GameManager.Instance.Today();
			p_targetDate.AddDays(3);
			ScheduleExpiry(p_targetDate);
		}
	}

	public void OnSettlementUnabandoned()
	{
		CancelCurrentExpiry();
	}

	private void ScheduleExpiry(GameDate p_targetDate)
	{
		CancelCurrentExpiry();
		expirationDate = p_targetDate;
		_expirationScheduleKey = SchedulingManager.Instance.AddEntry(p_targetDate, TriggerExpiry, base.owner);
	}

	public void CancelCurrentExpiry()
	{
		if (!string.IsNullOrEmpty(_expirationScheduleKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expirationScheduleKey);
			_expirationScheduleKey = string.Empty;
			expirationDate = default(GameDate);
		}
	}

	private void TriggerExpiry()
	{
		if (base.owner.residents.Count <= 0)
		{
			base.owner.DestroySettlement();
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
