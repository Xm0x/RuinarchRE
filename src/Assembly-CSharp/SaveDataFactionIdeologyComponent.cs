using System;

[Serializable]
public class SaveDataFactionIdeologyComponent : SaveData<FactionIdeologyComponent>
{
	public CRIME_TYPE lastRemovedNonReligionCrimeTypeForEvilOrPsychopath;

	public override void Save(FactionIdeologyComponent data)
	{
		lastRemovedNonReligionCrimeTypeForEvilOrPsychopath = data.lastRemovedNonReligionCrimeTypeForEvilOrPsychopath;
	}

	public override FactionIdeologyComponent Load()
	{
		return new FactionIdeologyComponent(this);
	}
}
