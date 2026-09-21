namespace Goap.Unique_Action_Data;

public class FirstAidCharacterUAD : UniqueActionData
{
	public bool usedPoisonedHealingPotion { get; private set; }

	public FirstAidCharacterUAD()
	{
		usedPoisonedHealingPotion = false;
	}

	public FirstAidCharacterUAD(SaveDataFirstAidCharacterUAD saveData)
	{
		usedPoisonedHealingPotion = saveData.usedPoisonedHealingPotion;
	}

	public void SetUsedPoisonedHealingPotion(bool state)
	{
		usedPoisonedHealingPotion = state;
	}

	public override SaveDataUniqueActionData Save()
	{
		SaveDataFirstAidCharacterUAD saveDataFirstAidCharacterUAD = new SaveDataFirstAidCharacterUAD();
		saveDataFirstAidCharacterUAD.Save(this);
		return saveDataFirstAidCharacterUAD;
	}
}
