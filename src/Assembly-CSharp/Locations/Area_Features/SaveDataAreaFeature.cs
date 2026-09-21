namespace Locations.Area_Features;

public class SaveDataAreaFeature
{
	public string tileFeatureName;

	public virtual void Save(AreaFeature areaFeature)
	{
		tileFeatureName = areaFeature.GetType().Name;
	}

	public virtual AreaFeature Load()
	{
		return LandmarkManager.Instance.CreateAreaFeature<AreaFeature>(tileFeatureName);
	}
}
