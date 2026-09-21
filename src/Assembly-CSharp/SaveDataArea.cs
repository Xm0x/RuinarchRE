using System;
using System.Collections.Generic;
using Locations.Area_Features;
using UtilityScripts;

[Serializable]
public class SaveDataArea : SaveData<Area>
{
	public AreaData areaData;

	public List<SaveDataAreaFeature> tileFeatureSaveData;

	public override void Save(Area p_data)
	{
		areaData = p_data.areaData;
		tileFeatureSaveData = RuinarchListPool<SaveDataAreaFeature>.Claim();
		for (int i = 0; i < p_data.featureComponent.features.Count; i++)
		{
			AreaFeature areaFeature = p_data.featureComponent.features[i];
			SaveDataAreaFeature saveDataAreaFeature = SaveManager.ConvertAreaFeatureToSaveData(areaFeature);
			saveDataAreaFeature.Save(areaFeature);
			tileFeatureSaveData.Add(saveDataAreaFeature);
		}
	}

	public override Area Load()
	{
		return new Area(this);
	}

	public override void CleanUp()
	{
		areaData = null;
		if (tileFeatureSaveData != null)
		{
			RuinarchListPool<SaveDataAreaFeature>.Release(tileFeatureSaveData);
			tileFeatureSaveData = null;
		}
	}
}
