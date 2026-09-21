using System.Collections.Generic;
using Inner_Maps;
using Locations;
using Logs;
using Traits;
using UnityEngine;

public interface IPointOfInterest : ITraitable, IDamageable, ISelectable, ILogFiller, IGCollectable
{
	new string persistentID { get; }

	OBJECT_TYPE objectType { get; }

	new string name { get; }

	int id { get; }

	string nameWithID { get; }

	bool isDead { get; }

	bool isHidden { get; }

	bool isBeingSeized { get; }

	int numOfNonSecretActionsBeingPerformedOnThis { get; }

	Vector3 attackRangePosition { get; }

	ILocationAwareness currentLocationAwareness { get; }

	POINT_OF_INTEREST_TYPE poiType { get; }

	POI_STATE state { get; }

	Region currentRegion { get; }

	Faction factionOwner { get; }

	Character characterOwner { get; }

	Character isBeingCarriedBy { get; }

	LogComponent logComponent { get; }

	GameObject visualGO { get; }

	new LocationGridTile gridTileLocation { get; }

	List<JobQueueItem> allJobsTargetingThis { get; }

	ResourceStorageComponent resourceStorageComponent { get; }

	void SetGridTileLocation(LocationGridTile tile);

	void AddJobTargetingThis(JobQueueItem job);

	bool RemoveJobTargetingThis(JobQueueItem job);

	bool HasJobTargetingThis(JOB_TYPE jobType);

	bool HasJobTargetingThis(JOB_TYPE jobType1, JOB_TYPE jobType2);

	bool HasJobTargetingThis(JOB_TYPE jobType1, JOB_TYPE jobType2, JOB_TYPE jobType3);

	void SetPOIState(POI_STATE state);

	bool IsAvailable();

	LocationGridTile GetNearestUnoccupiedTileFromThis();

	GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, GoapPlanJob job, ref int cost, ref string log);

	bool CanAdvertiseActionToActor(Character actor, GoapAction action, GoapPlanJob job);

	bool IsValidCombatTargetFor(IPointOfInterest source);

	bool IsStillConsideredPartOfAwarenessByCharacter(Character character);

	bool IsOwnedBy(Character character);

	void OnPlacePOI();

	void OnLoadPlacePOI();

	void OnDestroyPOI(Character p_destroyer = null);

	void OnSeizePOI(bool wasUnseizedFromCharacter);

	void OnUnseizePOI(LocationGridTile tileLocation);

	void CancelRemoveStatusFeedAndRepairJobsTargetingThis();

	void AdjustNumOfNonSecretActionsBeingPerformedOnThis(int amount);

	bool IsPOICurrentlyTargetedByOtherCharacterPerformingAction();

	bool IsPOICurrentlyTargetedByAPerformingAction(params JOB_TYPE[] jobType);

	bool IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE p_jobType, GoapEffect p_goapEffect);

	bool Advertises(INTERACTION_TYPE type);

	void SetCurrentLocationAwareness(ILocationAwareness locationAwareness);

	bool IsUnpassable();

	bool CanBeSeenBy(Character p_character);

	void OnAddedAsUnprocessedPOI(Character p_characterThatAddedPOI);
}
