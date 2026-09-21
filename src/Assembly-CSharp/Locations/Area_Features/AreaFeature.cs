using System;

namespace Locations.Area_Features;

public class AreaFeature
{
	public string name { get; protected set; }

	public string description { get; protected set; }

	public virtual Type serializedData => typeof(SaveDataAreaFeature);

	public virtual void OnAddFeature(Area p_area)
	{
	}

	public virtual void OnRemoveFeature(Area p_area)
	{
	}

	public virtual void OnDemolishLandmark(Area p_area, LANDMARK_TYPE demolishedLandmarkType)
	{
	}

	public virtual void GameStartActions(Area p_area)
	{
	}

	public virtual void LoadedGameStartActions(Area p_area)
	{
		GameStartActions(p_area);
	}

	public override string ToString()
	{
		return name;
	}

	public virtual string GetTestingData()
	{
		return string.Empty;
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
