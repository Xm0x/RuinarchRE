public class SaveDataManaRegenComponent : SaveData<ManaRegenComponent>
{
	public int manaPitCount;

	public int maxMana;

	public GameDate nextManaRegen;

	public override void Save(ManaRegenComponent data)
	{
		base.Save(data);
		manaPitCount = data.GetManaPitCount();
		maxMana = data.GetMaxMana();
		nextManaRegen = data.nextManaRegenDate;
	}
}
