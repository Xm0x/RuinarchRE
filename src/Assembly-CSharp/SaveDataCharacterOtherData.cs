public class SaveDataCharacterOtherData : SaveDataOtherData
{
	public string characterID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		CharacterOtherData characterOtherData = data as CharacterOtherData;
		characterID = characterOtherData.character.persistentID;
	}

	public override OtherData Load()
	{
		return new CharacterOtherData(this);
	}
}
