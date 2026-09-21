using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;

public class AreaFeatureComponent
{
	public List<AreaFeature> features { get; private set; }

	public AreaFeatureComponent()
	{
		features = new List<AreaFeature>();
	}

	public void AddFeature(AreaFeature feature, Area p_area)
	{
		if (!features.Contains(feature))
		{
			features.Add(feature);
			feature.OnAddFeature(p_area);
		}
	}

	public AreaFeature AddFeature(string featureName, Area p_area)
	{
		AreaFeature areaFeature = LandmarkManager.Instance.CreateAreaFeature<AreaFeature>(featureName);
		AddFeature(areaFeature, p_area);
		return areaFeature;
	}

	public bool RemoveFeature(AreaFeature feature, Area p_area)
	{
		if (features.Remove(feature))
		{
			feature.OnRemoveFeature(p_area);
			return true;
		}
		return false;
	}

	public bool RemoveFeature(string featureName, Area p_area)
	{
		AreaFeature feature = GetFeature(featureName);
		if (feature != null)
		{
			return RemoveFeature(feature, p_area);
		}
		return false;
	}

	public void RemoveAllFeatures(Area p_area)
	{
		for (int i = 0; i < features.Count; i++)
		{
			if (RemoveFeature(features[i], p_area))
			{
				i--;
			}
		}
	}

	public void RemoveAllFeaturesExcept(Area p_area, params string[] except)
	{
		for (int i = 0; i < features.Count; i++)
		{
			AreaFeature areaFeature = features[i];
			if (!except.Contains(areaFeature.name) && RemoveFeature(areaFeature, p_area))
			{
				i--;
			}
		}
	}

	public AreaFeature GetFeature(string featureName)
	{
		for (int i = 0; i < features.Count; i++)
		{
			AreaFeature areaFeature = features[i];
			if (areaFeature.GetType().Name == featureName || areaFeature.name == featureName)
			{
				return areaFeature;
			}
		}
		return null;
	}

	public T GetFeature<T>() where T : AreaFeature
	{
		for (int i = 0; i < features.Count; i++)
		{
			if (features[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public bool HasFeature(string featureName)
	{
		return GetFeature(featureName) != null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < features.Count; i++)
		{
			features[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
