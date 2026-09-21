namespace Goap.Unique_Action_Data;

public class SaveDataHealSelfUAD : SaveDataUniqueActionData
{
	public bool usedPoisonedHealingPotion;

	public override void Save(UniqueActionData data)
	{
		base.Save(data);
		HealSelfUAD healSelfUAD = data as HealSelfUAD;
		usedPoisonedHealingPotion = healSelfUAD.usedPoisonedHealingPotion;
	}

	public override UniqueActionData Load()
	{
		return new HealSelfUAD(this);
	}
}
