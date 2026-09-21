namespace Goap.Unique_Action_Data;

public class HealSelfUAD : UniqueActionData
{
	public bool usedPoisonedHealingPotion { get; private set; }

	public HealSelfUAD()
	{
		usedPoisonedHealingPotion = false;
	}

	public HealSelfUAD(SaveDataHealSelfUAD saveData)
	{
		usedPoisonedHealingPotion = saveData.usedPoisonedHealingPotion;
	}

	public void SetUsedPoisonedHealingPotion(bool state)
	{
		usedPoisonedHealingPotion = state;
	}

	public override SaveDataUniqueActionData Save()
	{
		SaveDataHealSelfUAD saveDataHealSelfUAD = new SaveDataHealSelfUAD();
		saveDataHealSelfUAD.Save(this);
		return saveDataHealSelfUAD;
	}
}
