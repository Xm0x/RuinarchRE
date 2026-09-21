using System;
using System.Collections.Generic;

namespace Locations.Area_Features;

[Serializable]
public class SaveDataGameFeature : SaveDataAreaFeature
{
	public SUMMON_TYPE summon;

	public List<string> ownedAnimals;

	public bool isGeneratingPerHour;

	public override void Save(AreaFeature tileFeature)
	{
		base.Save(tileFeature);
		GameFeature gameFeature = tileFeature as GameFeature;
		summon = gameFeature.animalTypeBeingSpawned;
		ownedAnimals = SaveUtilities.ConvertSavableListToIDs(gameFeature.ownedAnimals);
		isGeneratingPerHour = gameFeature.isGeneratingPerHour;
	}

	public override AreaFeature Load()
	{
		GameFeature obj = base.Load() as GameFeature;
		obj.SetSpawnType(summon);
		obj.LoadAnimals(ownedAnimals);
		obj.LoadGeneration(isGeneratingPerHour);
		return obj;
	}
}
