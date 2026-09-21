using System;
using System.Collections.Generic;
using Traits;
using UtilityScripts;

[Serializable]
public class SaveDataTrait : SaveData<Trait>, ISavableCounterpart
{
	public string _persistentID;

	public OBJECT_TYPE _objectType;

	public string name;

	public List<string> responsibleCharacters;

	public INTERACTION_TYPE gainedFromDoingType;

	public bool isGainedFromDoingStealth;

	public string persistentID => _persistentID;

	public OBJECT_TYPE objectType => _objectType;

	public override void Save(Trait trait)
	{
		_persistentID = trait.persistentID;
		_objectType = trait.objectType;
		name = trait.name;
		if (trait.responsibleCharacters != null)
		{
			responsibleCharacters = RuinarchListPool<string>.Claim();
			for (int i = 0; i < trait.responsibleCharacters.Count; i++)
			{
				Character character = trait.responsibleCharacters[i];
				responsibleCharacters.Add(character.persistentID);
			}
		}
		gainedFromDoingType = trait.gainedFromDoingType;
		isGainedFromDoingStealth = trait.isGainedFromDoingStealth;
	}

	public override Trait Load()
	{
		return TraitManager.Instance.LoadTrait(this);
	}

	public override void CleanUp()
	{
		if (responsibleCharacters != null)
		{
			RuinarchListPool<string>.Release(responsibleCharacters);
			responsibleCharacters = null;
		}
	}
}
