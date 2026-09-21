using System.Collections.Generic;

public class SaveDataSettlementClassComponent : SaveData<SettlementClassComponent>
{
	public int currentClassOrderIndex;

	public List<string> currentResidentClasses;

	public GameDate morningScheduleDateForProcessingOfNeededClasses;

	public GameDate afternoonScheduleDateForProcessingOfNeededClasses;

	public override void Save(SettlementClassComponent data)
	{
		currentClassOrderIndex = data.currentClassOrderIndex;
		currentResidentClasses = new List<string>(data.currentResidentClasses);
		morningScheduleDateForProcessingOfNeededClasses = data.morningScheduleDateForProcessingOfNeededClasses;
		afternoonScheduleDateForProcessingOfNeededClasses = data.afternoonScheduleDateForProcessingOfNeededClasses;
	}

	public override SettlementClassComponent Load()
	{
		return new SettlementClassComponent(this);
	}
}
