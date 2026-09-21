using System;
using System.Collections.Generic;
using Character_Talents;
using UtilityScripts;

[Serializable]
public class SaveDataCharacterTalentComponent : SaveData<CharacterTalentComponent>
{
	public List<SaveDataCharacterTalent> allTalents;

	public override void Save(CharacterTalentComponent data)
	{
		allTalents = RuinarchListPool<SaveDataCharacterTalent>.Claim();
		for (int i = 0; i < data.allTalents.Count; i++)
		{
			CharacterTalent data2 = data.allTalents[i];
			SaveDataCharacterTalent saveDataCharacterTalent = new SaveDataCharacterTalent();
			saveDataCharacterTalent.Save(data2);
			allTalents.Add(saveDataCharacterTalent);
		}
	}

	public override CharacterTalentComponent Load()
	{
		return new CharacterTalentComponent(this);
	}

	public override void CleanUp()
	{
		if (allTalents != null)
		{
			RuinarchListPool<SaveDataCharacterTalent>.Release(allTalents);
			allTalents = null;
		}
	}
}
