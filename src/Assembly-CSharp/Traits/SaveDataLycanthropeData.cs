using System;
using System.Collections.Generic;

namespace Traits;

[Serializable]
public class SaveDataLycanthropeData : SaveData<LycanthropeData>
{
	public string activeForm;

	public string limboForm;

	public string lycanthropeForm;

	public string originalForm;

	public string plainWolf;

	public string direWolf;

	public bool dislikesBeingLycan;

	public bool isMaster;

	public List<string> awareCharacterIDs;

	public bool isInWerewolfForm;

	public override void Save(LycanthropeData data)
	{
		activeForm = data.activeForm.persistentID;
		limboForm = data.limboForm.persistentID;
		lycanthropeForm = data.lycanthropeForm.persistentID;
		originalForm = data.originalForm.persistentID;
		plainWolf = data.plainWolf?.persistentID;
		direWolf = data.direWolf?.persistentID;
		dislikesBeingLycan = data.dislikesBeingLycan;
		isMaster = data.isMaster;
		isInWerewolfForm = data.isInWerewolfForm;
		awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(data.awareCharacters);
	}

	public override LycanthropeData Load()
	{
		Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(originalForm);
		Character characterByPersistentID2 = CharacterManager.Instance.GetCharacterByPersistentID(lycanthropeForm);
		Character character = null;
		if (!string.IsNullOrEmpty(plainWolf))
		{
			character = CharacterManager.Instance.GetCharacterByPersistentID(plainWolf);
		}
		Character character2 = null;
		if (!string.IsNullOrEmpty(direWolf))
		{
			character2 = CharacterManager.Instance.GetCharacterByPersistentID(direWolf);
		}
		Character character3 = characterByPersistentID;
		Character character4 = characterByPersistentID2;
		if (activeForm == lycanthropeForm)
		{
			character3 = characterByPersistentID2;
			character4 = characterByPersistentID;
		}
		else
		{
			character3 = characterByPersistentID;
			character4 = characterByPersistentID2;
		}
		LycanthropeData lycanthropeData = new LycanthropeData(characterByPersistentID, characterByPersistentID2, character3, character4, character, character2);
		lycanthropeData.LoadDislikesBeingLycan(dislikesBeingLycan);
		lycanthropeData.LoadIsMaster(isMaster);
		lycanthropeData.LoadIsInWerewolfForm(isInWerewolfForm);
		lycanthropeData.LoadAwareCharacters(SaveUtilities.ConvertIDListToCharacters(awareCharacterIDs));
		return lycanthropeData;
	}
}
