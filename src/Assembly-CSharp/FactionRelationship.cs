using System;

public class FactionRelationship
{
	protected Faction _faction1;

	protected Faction _faction2;

	private int _relationshipStatInt;

	public FACTION_RELATIONSHIP_STATUS relationshipStatus => (FACTION_RELATIONSHIP_STATUS)_relationshipStatInt;

	public int relationshipStatInt => _relationshipStatInt;

	public Faction faction1 => _faction1;

	public Faction faction2 => _faction2;

	public string localizedRelationshipStatus => LocalizationManager.Instance.GetLocalizedValue("Faction_Table", relationshipStatus.ToStringEnum());

	public FactionRelationship(Faction faction1, Faction faction2, int relationshipStatInt = 0)
	{
		_faction1 = faction1;
		_faction2 = faction2;
		_relationshipStatInt = relationshipStatInt;
	}

	public bool SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS newStatus)
	{
		if (newStatus == relationshipStatus)
		{
			return false;
		}
		if ((_faction1.factionType.type == FACTION_TYPE.Vagrants || _faction2.factionType.type == FACTION_TYPE.Vagrants) && newStatus == FACTION_RELATIONSHIP_STATUS.Hostile && (faction1.factionType.type == FACTION_TYPE.Human_Empire || faction1.factionType.type == FACTION_TYPE.Elven_Kingdom || faction2.factionType.type == FACTION_TYPE.Human_Empire || faction2.factionType.type == FACTION_TYPE.Elven_Kingdom))
		{
			throw new Exception("Setting relationship between " + _faction2.name + " and " + _faction1.name + " to Hostile!");
		}
		FACTION_RELATIONSHIP_STATUS arg = relationshipStatus;
		_relationshipStatInt = (int)newStatus;
		Messenger.Broadcast(FactionSignals.CHANGE_FACTION_RELATIONSHIP, _faction1, _faction2, relationshipStatus, arg);
		return true;
	}

	public void ImproveRelationshipByAStep()
	{
		switch (relationshipStatus)
		{
		case FACTION_RELATIONSHIP_STATUS.Hostile:
			SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Neutral);
			break;
		case FACTION_RELATIONSHIP_STATUS.Neutral:
			SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case FACTION_RELATIONSHIP_STATUS.Friendly:
			break;
		}
	}
}
