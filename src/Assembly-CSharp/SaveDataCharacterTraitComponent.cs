using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataCharacterTraitComponent : SaveData<CharacterTraitComponent>
{
	public bool hasAgoraphobicReactedThisTick;

	public bool willProcessPlayerSourceChaosOrb;

	public bool isOtherTick;

	public List<string> obsessedCharacterIDs;

	public override void Save(CharacterTraitComponent data)
	{
		hasAgoraphobicReactedThisTick = data.hasAgoraphobicReactedThisTick;
		willProcessPlayerSourceChaosOrb = data.willProcessPlayerSourceChaosOrb;
		isOtherTick = data.isOtherTick;
		obsessedCharacterIDs = RuinarchListPool<string>.Claim();
		if (data.obsessedCharacterIDs != null && data.obsessedCharacterIDs.Count > 0)
		{
			obsessedCharacterIDs.AddRange(data.obsessedCharacterIDs);
		}
	}

	public override CharacterTraitComponent Load()
	{
		return new CharacterTraitComponent(this);
	}

	public override void CleanUp()
	{
		if (obsessedCharacterIDs != null)
		{
			RuinarchListPool<string>.Release(obsessedCharacterIDs);
			obsessedCharacterIDs = null;
		}
	}
}
