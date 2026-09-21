public class SaveDataSettlementResourcesComponent : SaveData<SettlementResourcesComponent>
{
	public override void Save(SettlementResourcesComponent data)
	{
	}

	public override SettlementResourcesComponent Load()
	{
		return new SettlementResourcesComponent(this);
	}
}
