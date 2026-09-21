using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataReactionComponent : SaveData<ReactionComponent>
{
	public List<string> charactersThatSawThisDead;

	public string disguisedCharacter;

	public bool isHidden;

	public bool hasBeenShockedByDemonicStructure;

	public override void Save(ReactionComponent data)
	{
		charactersThatSawThisDead = RuinarchListPool<string>.Claim();
		for (int i = 0; i < data.charactersThatSawThisDead.Count; i++)
		{
			charactersThatSawThisDead.Add(data.charactersThatSawThisDead[i].persistentID);
		}
		if (data.disguisedCharacter != null)
		{
			disguisedCharacter = data.disguisedCharacter.persistentID;
		}
		isHidden = data.isHidden;
		hasBeenShockedByDemonicStructure = data.hasBeenShockedByDemonicStructure;
	}

	public override ReactionComponent Load()
	{
		return new ReactionComponent(this);
	}

	public override void CleanUp()
	{
		if (charactersThatSawThisDead != null)
		{
			RuinarchListPool<string>.Release(charactersThatSawThisDead);
			charactersThatSawThisDead = null;
		}
	}
}
