using System;

[Serializable]
public class SaveDataCharacterStructureComponent : SaveData<CharacterStructureComponent>
{
	public override void Save(CharacterStructureComponent data)
	{
	}

	public override CharacterStructureComponent Load()
	{
		return new CharacterStructureComponent(this);
	}
}
