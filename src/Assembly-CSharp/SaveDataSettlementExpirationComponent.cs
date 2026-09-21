using System;

[Serializable]
public class SaveDataSettlementExpirationComponent : SaveData<SettlementExpirationComponent>
{
	public GameDate expirationDate;

	public override void Save(SettlementExpirationComponent data)
	{
		expirationDate = data.expirationDate;
	}

	public override SettlementExpirationComponent Load()
	{
		return new SettlementExpirationComponent(this);
	}
}
