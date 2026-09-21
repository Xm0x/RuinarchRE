using System;

[Serializable]
public class SaveDataSettlementVillageMigrationComponent : SaveData<SettlementVillageMigrationComponent>
{
	public int villageMigrationMeter;

	public int perHourIncrement;

	public int longTermModifier;

	public GameDate emptyVillageMigrationDate;

	public override void Save(SettlementVillageMigrationComponent data)
	{
		villageMigrationMeter = data.villageMigrationMeter;
		perHourIncrement = data.perHourIncrement;
		longTermModifier = data.longTermModifier;
		emptyVillageMigrationDate = data.emptyVillageMigrationDate;
	}

	public override SettlementVillageMigrationComponent Load()
	{
		return new SettlementVillageMigrationComponent(this);
	}
}
