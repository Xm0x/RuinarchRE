using System.Collections.Generic;

namespace Locations;

public interface ILocationAwareness
{
	Dictionary<INTERACTION_TYPE, List<IPointOfInterest>> awareness { get; }

	void AddSpecificAwarenessToPendingAddList(INTERACTION_TYPE actionType, IPointOfInterest poi);

	bool AddAwarenessToPendingAddList(IPointOfInterest poi);

	bool RemoveAwarenessFromPendingAddList(IPointOfInterest poi);

	void AddSpecificAwarenessToPendingRemoveList(INTERACTION_TYPE actionType, IPointOfInterest poi);

	bool AddAwarenessToPendingRemoveList(IPointOfInterest poi);

	bool RemoveAwarenessFromPendingRemoveList(IPointOfInterest poi);

	List<IPointOfInterest> GetListOfPOIBasedOnActionType(INTERACTION_TYPE actionType);

	bool AddAwarenessToMainList(IPointOfInterest poi);

	bool AddAwarenessToMainList(INTERACTION_TYPE actionType, IPointOfInterest pointOfInterest);

	void RemoveAwarenessFromMainList(IPointOfInterest poi);

	void RemoveAwarenessFromMainList(INTERACTION_TYPE actionType, IPointOfInterest pointOfInterest);

	bool HasAwareness(INTERACTION_TYPE actionType, IPointOfInterest poi);

	void CheckIfCharacterIsStillReferenced(Character p_character);
}
