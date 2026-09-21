namespace Goap.Unique_Action_Data;

public class SaveDataCureCharacterUAD : SaveDataUniqueActionData
{
	public bool usedPoisonedHealingPotion;

	public override void Save(UniqueActionData data)
	{
		base.Save(data);
		CureCharacterUAD cureCharacterUAD = data as CureCharacterUAD;
		usedPoisonedHealingPotion = cureCharacterUAD.usedPoisonedHealingPotion;
	}

	public override UniqueActionData Load()
	{
		return new CureCharacterUAD(this);
	}
}
