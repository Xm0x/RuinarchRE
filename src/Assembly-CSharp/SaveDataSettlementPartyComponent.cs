public class SaveDataSettlementPartyComponent : SaveData<SettlementPartyComponent>
{
	public GameDate scheduleDateForProcessingOfPartyQuests;

	public override void Save(SettlementPartyComponent data)
	{
		scheduleDateForProcessingOfPartyQuests = data.scheduleDateForProcessingOfPartyQuests;
	}

	public override SettlementPartyComponent Load()
	{
		return new SettlementPartyComponent(this);
	}
}
