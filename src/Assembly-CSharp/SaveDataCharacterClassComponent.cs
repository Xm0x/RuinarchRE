using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataCharacterClassComponent : SaveData<CharacterClassComponent>
{
	public string className;

	public string previousClassName;

	public bool shouldChangeClass;

	public List<string> ableClasses;

	public int devastationCounter;

	public CHARACTER_CATEGORY hunterKillingSpecialization;

	public StalkerBonuses stalkerBonuses;

	public override void Save(CharacterClassComponent data)
	{
		className = data.characterClass.className;
		previousClassName = data.previousClassName;
		shouldChangeClass = data.shouldChangeClass;
		devastationCounter = data.devastationCounter;
		hunterKillingSpecialization = data.hunterKillingSpecialization;
		stalkerBonuses = data.stalkerBonuses;
		ableClasses = RuinarchListPool<string>.Claim();
		if (data.ableClasses != null && data.ableClasses.Count > 0)
		{
			ableClasses.AddRange(data.ableClasses);
		}
	}

	public override CharacterClassComponent Load()
	{
		return new CharacterClassComponent(this);
	}

	public override void CleanUp()
	{
		if (ableClasses != null)
		{
			RuinarchListPool<string>.Release(ableClasses);
			ableClasses = null;
		}
	}
}
