namespace Goap.Unique_Action_Data;

public class SaveDataFirstAidCharacterUAD : SaveDataUniqueActionData
{
	public bool usedPoisonedHealingPotion;

	public override void Save(UniqueActionData data)
	{
		base.Save(data);
		FirstAidCharacterUAD firstAidCharacterUAD = data as FirstAidCharacterUAD;
		usedPoisonedHealingPotion = firstAidCharacterUAD.usedPoisonedHealingPotion;
	}

	public override UniqueActionData Load()
	{
		return new FirstAidCharacterUAD(this);
	}
}
