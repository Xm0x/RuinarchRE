using System;

[Serializable]
public class SaveDataCharacterStateComponent : SaveData<CharacterStateComponent>
{
	public override void Save(CharacterStateComponent data)
	{
	}

	public override CharacterStateComponent Load()
	{
		return new CharacterStateComponent(this);
	}
}
