using System;

[Serializable]
public class SaveDataRumor : SaveData<Rumor>
{
	public string characterThatCreatedRumorID;

	public string targetCharacterID;

	public override void Save(Rumor data)
	{
		characterThatCreatedRumorID = data.characterThatCreatedRumor.persistentID;
		targetCharacterID = data.targetCharacter.persistentID;
	}

	public override Rumor Load()
	{
		Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(characterThatCreatedRumorID);
		Character characterByPersistentID2 = CharacterManager.Instance.GetCharacterByPersistentID(targetCharacterID);
		return new Rumor(characterByPersistentID, characterByPersistentID2);
	}
}
