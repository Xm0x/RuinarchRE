using System.Collections.Generic;
using Inner_Maps;
using Locations;

namespace UtilityScripts;

public static class LocationAwarenessUtility
{
	private static List<ILocationAwareness> allLocationsToBeUpdated = new List<ILocationAwareness>();

	public static void AddToAwarenessList(IPointOfInterest poi, LocationGridTile gridTileLocation)
	{
		ILocationAwareness locationAwareness = null;
		if (gridTileLocation != null)
		{
			locationAwareness = ((gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS || gridTileLocation.structure.structureType == STRUCTURE_TYPE.OCEAN) ? gridTileLocation.area.locationAwareness : gridTileLocation.structure.locationAwareness);
			if (locationAwareness != null && poi.currentLocationAwareness != locationAwareness)
			{
				locationAwareness.AddAwarenessToMainList(poi);
			}
		}
	}

	public static void AddToAwarenessList(INTERACTION_TYPE actionType, IPointOfInterest targetPOI)
	{
		targetPOI.currentLocationAwareness?.AddAwarenessToMainList(actionType, targetPOI);
	}

	public static void RemoveFromAwarenessList(IPointOfInterest poi)
	{
		ILocationAwareness currentLocationAwareness = poi.currentLocationAwareness;
		if (currentLocationAwareness != null)
		{
			currentLocationAwareness.RemoveAwarenessFromMainList(poi);
			return;
		}
		LocationGridTile gridTileLocation = poi.gridTileLocation;
		if (gridTileLocation != null)
		{
			ILocationAwareness locationAwareness = null;
			((ILocationAwareness)((gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS || gridTileLocation.structure.structureType == STRUCTURE_TYPE.OCEAN) ? gridTileLocation.area.locationAwareness : gridTileLocation.structure.locationAwareness))?.RemoveAwarenessFromMainList(poi);
		}
	}

	public static void RemoveFromAwarenessList(INTERACTION_TYPE actionType, IPointOfInterest targetPOI)
	{
		targetPOI.currentLocationAwareness?.RemoveAwarenessFromMainList(actionType, targetPOI);
	}
}
