using System;

[Serializable]
public class SaveDataCharacterPartyComponent : SaveData<CharacterPartyComponent>
{
	public string currentParty;

	public override void Save(CharacterPartyComponent data)
	{
		if (data.hasParty)
		{
			currentParty = data.currentParty.persistentID;
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentParty);
		}
	}

	public override CharacterPartyComponent Load()
	{
		return new CharacterPartyComponent(this);
	}
}
