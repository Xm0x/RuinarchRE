namespace Goap.Unique_Action_Data;

public class CureCharacterUAD : UniqueActionData
{
	public bool usedPoisonedHealingPotion { get; private set; }

	public CureCharacterUAD()
	{
		usedPoisonedHealingPotion = false;
	}

	public CureCharacterUAD(SaveDataCureCharacterUAD saveData)
	{
		usedPoisonedHealingPotion = saveData.usedPoisonedHealingPotion;
	}

	public void SetUsedPoisonedHealingPotion(bool state)
	{
		usedPoisonedHealingPotion = state;
	}

	public override SaveDataUniqueActionData Save()
	{
		SaveDataCureCharacterUAD saveDataCureCharacterUAD = new SaveDataCureCharacterUAD();
		saveDataCureCharacterUAD.Save(this);
		return saveDataCureCharacterUAD;
	}
}
