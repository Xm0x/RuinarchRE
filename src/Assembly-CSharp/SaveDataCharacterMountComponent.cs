using System;

[Serializable]
public class SaveDataCharacterMountComponent : SaveData<CharacterMountComponent>
{
	public string mountedCharacter;

	public BuffStatsBonus mountStatsBonus;

	public override void Save(CharacterMountComponent data)
	{
		mountedCharacter = data.mountedCharacterPersistentID;
		mountStatsBonus = data.mountStatsBonus;
	}

	public override CharacterMountComponent Load()
	{
		return new CharacterMountComponent(this);
	}
}
