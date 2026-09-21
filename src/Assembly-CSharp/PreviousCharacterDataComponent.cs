using Inner_Maps.Location_Structures;

public class PreviousCharacterDataComponent : CharacterComponent
{
	private LocationStructure _previousHomeStructure;

	private NPCSettlement _previousHomeSettlement;

	private Faction _previousFaction;

	private NPCSettlement _homeSettlementOnDeath;

	public LocationStructure previousHomeStructure => _previousHomeStructure;

	public NPCSettlement previousHomeSettlement => _previousHomeSettlement;

	public Faction previousFaction => _previousFaction;

	public INTERACTION_TYPE previousActionNodeType { get; private set; }

	public JOB_TYPE previousJobType { get; private set; }

	public NPCSettlement homeSettlementOnDeath => _homeSettlementOnDeath;

	public PreviousCharacterDataComponent()
	{
	}

	public PreviousCharacterDataComponent(SaveDataPreviousCharacterDataComponent data)
	{
		previousActionNodeType = data.previousActionNodeType;
		previousJobType = data.previousJobType;
	}

	public void LoadReferences(SaveDataPreviousCharacterDataComponent data)
	{
		if (!string.IsNullOrEmpty(data.previousHomeStructureID))
		{
			_previousHomeStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.previousHomeStructureID);
		}
		if (!string.IsNullOrEmpty(data.previousHomeSettlementID))
		{
			_previousHomeSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.previousHomeSettlementID) as NPCSettlement;
			if (_previousHomeSettlement != null)
			{
				Messenger.AddListener<NPCSettlement>(SettlementSignals.SETTLEMENT_ABANDONED, OnSettlementAbandoned);
			}
		}
		if (!string.IsNullOrEmpty(data.previousFactionID))
		{
			_previousFaction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(data.previousFactionID);
		}
		if (!string.IsNullOrEmpty(data.homeSettlementOnDeathID))
		{
			_homeSettlementOnDeath = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.homeSettlementOnDeathID) as NPCSettlement;
		}
	}

	public void SubscribeToListeners()
	{
		Messenger.AddListener<NPCSettlement>(SettlementSignals.SETTLEMENT_ABANDONED, OnSettlementAbandoned);
	}

	public void UnsubscribeToListeners()
	{
		Messenger.RemoveListener<NPCSettlement>(SettlementSignals.SETTLEMENT_ABANDONED, OnSettlementAbandoned);
	}

	public void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (_previousHomeStructure == p_structure)
		{
			SetPreviousHomeStructure(null);
		}
	}

	public void OnChangeFactionTo(Faction p_newFaction)
	{
		if (p_newFaction != null && previousHomeSettlement != null && p_newFaction.ownedSettlements.Contains(previousHomeSettlement))
		{
			SetPreviousHomeSettlement(null);
		}
	}

	public void SetPreviousHomeStructure(LocationStructure p_structure)
	{
		_previousHomeStructure = p_structure;
	}

	public void SetPreviousHomeSettlement(NPCSettlement p_settlement)
	{
		_previousHomeSettlement = p_settlement;
	}

	public void SetHomeSettlementOnDeath(NPCSettlement p_settlement)
	{
		_homeSettlementOnDeath = p_settlement;
	}

	private void OnSettlementAbandoned(NPCSettlement p_settlement)
	{
		if (previousHomeSettlement == p_settlement)
		{
			SetPreviousHomeSettlement(null);
		}
	}

	public void DisconnectFromSettlement(NPCSettlement p_settlement)
	{
		if (previousHomeSettlement == p_settlement)
		{
			SetPreviousHomeSettlement(null);
		}
		if (_homeSettlementOnDeath == p_settlement)
		{
			SetHomeSettlementOnDeath(p_settlement);
		}
	}

	public void SetPreviousFaction(Faction p_faction)
	{
		_previousFaction = p_faction;
	}

	public void SetPreviousActionType(INTERACTION_TYPE p_type)
	{
		previousActionNodeType = p_type;
	}

	public void SetPreviousJobType(JOB_TYPE p_type)
	{
		previousJobType = p_type;
	}

	public bool IsPreviousJobOrActionReturnHome()
	{
		if (!previousActionNodeType.IsReturnHome())
		{
			return previousJobType.IsReturnHome();
		}
		return true;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = _previousHomeStructure;
	}
}
