using System;

[Serializable]
public class SaveDataFactionIdeology : SaveData<FactionIdeology>
{
	public FACTION_IDEOLOGY ideologyType;

	public int daysPassedSettlementEvent;

	public EXCLUSIVE_IDEOLOGY_CATEGORIES category;

	public RACE raceRequirement;

	public GENDER genderRequirement;

	public RELIGION religionRequirement;

	public string traitRequirement;

	public override void Save(FactionIdeology data)
	{
		ideologyType = data.ideologyType;
		if (data is Exclusive exclusive)
		{
			category = exclusive.category;
			raceRequirement = exclusive.raceRequirement;
			genderRequirement = exclusive.genderRequirement;
			traitRequirement = exclusive.traitRequirement;
			religionRequirement = exclusive.religionRequirement;
		}
	}

	public override FactionIdeology Load()
	{
		FactionIdeology factionIdeology = FactionManager.Instance.CreateIdeology<FactionIdeology>(ideologyType);
		factionIdeology.SetSavedData(this);
		if (factionIdeology is Exclusive exclusive)
		{
			if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE)
			{
				exclusive.LoadRequirement(raceRequirement);
				return factionIdeology;
			}
			if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER)
			{
				exclusive.LoadRequirement(genderRequirement);
				return factionIdeology;
			}
			if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT)
			{
				exclusive.LoadRequirement(traitRequirement);
				return factionIdeology;
			}
			if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RELIGION)
			{
				exclusive.LoadRequirement(religionRequirement);
			}
		}
		return factionIdeology;
	}
}
