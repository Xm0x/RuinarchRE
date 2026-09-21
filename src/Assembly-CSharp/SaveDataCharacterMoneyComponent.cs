using System;

[Serializable]
public class SaveDataCharacterMoneyComponent : SaveData<CharacterMoneyComponent>
{
	public int coins;

	public override void Save(CharacterMoneyComponent data)
	{
		coins = data.coins;
	}

	public override CharacterMoneyComponent Load()
	{
		return new CharacterMoneyComponent(this);
	}
}
