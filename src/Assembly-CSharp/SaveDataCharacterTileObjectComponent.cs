using System;

[Serializable]
public class SaveDataCharacterTileObjectComponent : SaveData<CharacterTileObjectComponent>
{
	public string primaryBed;

	public string bedBeingUsed;

	public override void Save(CharacterTileObjectComponent data)
	{
		if (data.primaryBed != null)
		{
			primaryBed = data.primaryBed.persistentID;
		}
		if (data.bedBeingUsed != null)
		{
			bedBeingUsed = data.bedBeingUsed.persistentID;
		}
	}

	public override CharacterTileObjectComponent Load()
	{
		return new CharacterTileObjectComponent(this);
	}
}
