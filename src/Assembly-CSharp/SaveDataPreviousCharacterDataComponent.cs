public class SaveDataPreviousCharacterDataComponent : SaveData<PreviousCharacterDataComponent>
{
	public string previousHomeStructureID;

	public string previousHomeSettlementID;

	public string previousFactionID;

	public string homeSettlementOnDeathID;

	public INTERACTION_TYPE previousActionNodeType;

	public JOB_TYPE previousJobType;

	public override void Save(PreviousCharacterDataComponent data)
	{
		previousHomeStructureID = data.previousHomeStructure?.persistentID ?? string.Empty;
		previousHomeSettlementID = data.previousHomeSettlement?.persistentID ?? string.Empty;
		previousFactionID = data.previousFaction?.persistentID ?? string.Empty;
		homeSettlementOnDeathID = data.homeSettlementOnDeath?.persistentID ?? string.Empty;
		previousActionNodeType = data.previousActionNodeType;
		previousJobType = data.previousJobType;
	}

	public override PreviousCharacterDataComponent Load()
	{
		return new PreviousCharacterDataComponent(this);
	}
}
