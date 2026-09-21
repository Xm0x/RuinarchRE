using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace Locations;

public class LocationAwareness : ILocationAwareness
{
	public Dictionary<INTERACTION_TYPE, List<IPointOfInterest>> awareness { get; private set; }

	public LocationAwareness()
	{
		awareness = new Dictionary<INTERACTION_TYPE, List<IPointOfInterest>>(400);
	}

	public void AddSpecificAwarenessToPendingAddList(INTERACTION_TYPE actionType, IPointOfInterest poi)
	{
	}

	public bool AddAwarenessToPendingAddList(IPointOfInterest poi)
	{
		return false;
	}

	public bool RemoveAwarenessFromPendingAddList(IPointOfInterest poi)
	{
		return false;
	}

	public void AddSpecificAwarenessToPendingRemoveList(INTERACTION_TYPE actionType, IPointOfInterest poi)
	{
	}

	public bool AddAwarenessToPendingRemoveList(IPointOfInterest poi)
	{
		return false;
	}

	public bool RemoveAwarenessFromPendingRemoveList(IPointOfInterest poi)
	{
		return false;
	}

	public List<IPointOfInterest> GetListOfPOIBasedOnActionType(INTERACTION_TYPE actionType)
	{
		if (awareness.ContainsKey(actionType))
		{
			return awareness[actionType];
		}
		return null;
	}

	public bool AddAwarenessToMainList(IPointOfInterest pointOfInterest)
	{
		if (pointOfInterest == null || pointOfInterest.advertisedActions == null)
		{
			return false;
		}
		if (pointOfInterest is Character character)
		{
			character.SetCurrentLocationAwareness(this);
			return true;
		}
		if (pointOfInterest.advertisedActions.Count > 0)
		{
			for (int i = 0; i < pointOfInterest.advertisedActions.Count; i++)
			{
				INTERACTION_TYPE iNTERACTION_TYPE = pointOfInterest.advertisedActions[i];
				if (!HasAwareness(iNTERACTION_TYPE, pointOfInterest))
				{
					if (!awareness.ContainsKey(iNTERACTION_TYPE))
					{
						awareness.Add(iNTERACTION_TYPE, new List<IPointOfInterest>(100));
					}
					awareness[iNTERACTION_TYPE].Add(pointOfInterest);
				}
			}
			pointOfInterest.SetCurrentLocationAwareness(this);
			return true;
		}
		return false;
	}

	public void RemoveAwarenessFromMainList(IPointOfInterest pointOfInterest)
	{
		if (!(pointOfInterest is Character))
		{
			for (int i = 0; i < pointOfInterest.advertisedActions.Count; i++)
			{
				INTERACTION_TYPE key = pointOfInterest.advertisedActions[i];
				if (awareness.ContainsKey(key))
				{
					awareness[key].Remove(pointOfInterest);
				}
			}
		}
		if (pointOfInterest.currentLocationAwareness == this)
		{
			pointOfInterest.SetCurrentLocationAwareness(null);
		}
	}

	public bool AddAwarenessToMainList(INTERACTION_TYPE actionType, IPointOfInterest pointOfInterest)
	{
		if (!HasAwareness(actionType, pointOfInterest))
		{
			if (!awareness.ContainsKey(actionType))
			{
				awareness.Add(actionType, new List<IPointOfInterest>(100));
			}
			awareness[actionType].Add(pointOfInterest);
		}
		return true;
	}

	public void RemoveAwarenessFromMainList(INTERACTION_TYPE actionType, IPointOfInterest pointOfInterest)
	{
		if (awareness.ContainsKey(actionType))
		{
			awareness[actionType].Remove(pointOfInterest);
		}
	}

	public bool HasAwareness(INTERACTION_TYPE actionType, IPointOfInterest poi)
	{
		if (awareness.ContainsKey(actionType))
		{
			List<IPointOfInterest> list = awareness[actionType];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == poi)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void CleanUp()
	{
		if (awareness.Count <= 0)
		{
			return;
		}
		foreach (List<IPointOfInterest> value in awareness.Values)
		{
			for (int i = 0; i < value.Count; i++)
			{
				IPointOfInterest pointOfInterest = value[i];
				if (pointOfInterest.currentLocationAwareness == this)
				{
					pointOfInterest.SetCurrentLocationAwareness(null);
				}
			}
		}
		awareness.Clear();
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		foreach (KeyValuePair<INTERACTION_TYPE, List<IPointOfInterest>> item in awareness)
		{
			item.Value.Contains(p_character);
		}
	}
}
