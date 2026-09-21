public class CharacterOtherData : OtherData
{
	public Character character { get; private set; }

	public override object obj => character;

	public CharacterOtherData(Character character)
	{
		this.character = character;
	}

	public CharacterOtherData(SaveDataCharacterOtherData saveData)
	{
		character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveData.characterID);
	}

	public override SaveDataOtherData Save()
	{
		SaveDataCharacterOtherData saveDataCharacterOtherData = new SaveDataCharacterOtherData();
		saveDataCharacterOtherData.Save(this);
		return saveDataCharacterOtherData;
	}

	public override void CleanUp()
	{
		character = null;
	}
}
