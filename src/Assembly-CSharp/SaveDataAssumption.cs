using System;

[Serializable]
public class SaveDataAssumption : SaveData<Assumption>
{
	public string characterThatCreatedAssumptionID;

	public string targetCharacterID;

	public Log assumptionLog;

	public override void Save(Assumption data)
	{
		characterThatCreatedAssumptionID = data.characterThatCreatedAssumption.persistentID;
		targetCharacterID = data.targetCharacter.persistentID;
		assumptionLog = data.assumptionLog;
	}

	public override Assumption Load()
	{
		Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(characterThatCreatedAssumptionID);
		Character characterByPersistentID2 = CharacterManager.Instance.GetCharacterByPersistentID(targetCharacterID);
		Assumption assumption = new Assumption(characterByPersistentID, characterByPersistentID2);
		assumption.SetAssumptionLog(assumptionLog);
		return assumption;
	}
}
