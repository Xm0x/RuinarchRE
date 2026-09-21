public static class CharacterSignals
{
	public static string CHARACTER_DEATH = "OnCharacterDied";

	public static string CHARACTER_CREATED = "OnCharacterCreated";

	public static string ROLE_CHANGED = "OnCharacterRoleChanged";

	public static string CHARACTER_REMOVED = "OnCharacterRemoved";

	public static string CHARACTER_OBTAINED_ITEM = "OnCharacterObtainItem";

	public static string CHARACTER_LOST_ITEM = "OnCharacterLostItem";

	public static string CHARACTER_TRAIT_ADDED = "OnCharacterTraitAdded";

	public static string CHARACTER_TRAIT_REMOVED = "OnCharacterTraitRemoved";

	public static string CHARACTER_TRAIT_STACKED = "OnCharacterTraitStacked";

	public static string CHARACTER_TRAIT_UNSTACKED = "OnCharacterTraitUnstacked";

	public static string CHARACTER_ADJUSTED_HP = "OnAdjustedHP";

	public static string CHARACTER_MIGRATED_HOME = "OnCharacterChangedHome";

	public static string CHARACTER_CHANGED_RACE = "OnCharacterChangedRace";

	public static string CHARACTER_ARRIVED_AT_STRUCTURE = "OnCharacterArrivedAtStructure";

	public static string CHARACTER_LEFT_STRUCTURE = "OnCharacterLeftStructure";

	public static string RELATIONSHIP_CREATED = "OnCharacterGainedRelationship";

	public static string RELATIONSHIP_TYPE_ADDED = "OnCharacterGainedRelationshipType";

	public static string RELATIONSHIP_REMOVED = "OnCharacterRemovedRelationship";

	public static string FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI = "OnForceCancelAllJobTypesTargetingPOI";

	public static string FORCE_CANCEL_ALL_JOBS_TARGETING_POI = "OnForceCancelAllJobsTargetingPOI";

	public static string FORCE_CANCEL_ALL_JOBS_TARGETING_POI_EXCEPT_SELF = "OnForceCancelAllJobsTargetingPOIExceptSelf";

	public static string FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI = "OnForceCancelAllActionsTargetingPOI";

	public static string STOP_CURRENT_ACTION_TARGETING_POI = "OnStopCurrentActionTargetingPOI";

	public static string STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR = "OnStopCurrentActionTargetingPOIExceptActor";

	public static string CHARACTER_STARTED_STATE = "OnCharacterStartedState";

	public static string CHARACTER_PAUSED_STATE = "OnCharacterPausedState";

	public static string CHARACTER_ENDED_STATE = "OnCharacterEndedState";

	public static string DETERMINE_COMBAT_REACTION = "DetermineCombatReaction";

	public static string START_FLEE = "OnStartFlee";

	public static string CHARACTER_CLASS_CHANGE = "CharacterClassChange";

	public static string BEFORE_SEIZING_POI = "BeforeSeizingPOI";

	public static string ON_SEIZE_POI = "OnSeizePOI";

	public static string ON_UNSEIZE_POI = "OnUnseizePOI";

	public static string UPDATE_CHARACTER_AWARENESS_STATE = "OnUpdateCharacterAwarenessState";

	public static string CHARACTER_PRESUMED_DEAD = "OnCharacterPresumedDead";

	public static string ON_SET_AS_FACTION_LEADER = "OnSetAsFactionLeader";

	public static string STARTED_TRAVELLING = "OnStartedTravelling";

	public static string ON_FACTION_LEADER_REMOVED = "OnFactionLeaderRemoved";

	public static string ON_SET_AS_SETTLEMENT_RULER = "OnSetAsSettlementLeader";

	public static string ON_SETTLEMENT_RULER_REMOVED = "OnSettlementRulerRemoved";

	public static string ON_SWITCH_FROM_LIMBO = "OnSwitchFromLimbo";

	public static string INCREASE_THREAT_THAT_SEES_POI = "IncreaseThreatThatSeesPOI";

	public static string UPDATE_MOVEMENT_STATE = "OnUpdateMovementState";

	public static string MOOD_SUMMARY_MODIFIED = "OnMoodSummaryModified";

	public static string CHARACTER_REMOVED_FROM_VISION = "OnCharacterRemovedFromVision";

	public static string CHARACTER_WAS_HIT = "OnCharacterHit";

	public static string CHARACTER_RETURNED_TO_LIFE = "OnCharacterReturnedToLife";

	public static string CHARACTER_BECOMES_MINION_OR_SUMMON = "OnCharacterBecomesMinionOrSummon";

	public static string CHARACTER_BECOMES_NON_MINION_OR_SUMMON = "OnCharacterBecomesNonMinionOrSummon";

	public static string CHARACTER_FINISHED_JOB_SUCCESSFULLY = "OnCharacterFinishedJob";

	public static string OPINION_INCREASED = "OnOpinionIncreased";

	public static string OPINION_DECREASED = "OnOpinionDecreased";

	public static string OPINION_ADDED = "OnOpinionAdded";

	public static string OPINION_REMOVED = "OnOpinionRemoved";

	public static string OPINION_LABEL_DECREASED = "OnOpinionLabelDecreased";

	public static string CHARACTER_ENTERED_AREA = "OnCharacterEnteredArea";

	public static string CHARACTER_EXITED_AREA = "OnCharacterExitedArea";

	public static string CHARACTER_CAN_NO_LONGER_MOVE = "OnCharacterCannotMove";

	public static string CHARACTER_CAN_MOVE_AGAIN = "OnCharacterCannotMove";

	public static string INTERRUPT_FINISHED = "OnInterruptFinished";

	public static string CHARACTER_SAW = "OnCharacterSaw";

	public static string CHARACTER_CAN_NO_LONGER_PERFORM = "OnCharacterCannotPerform";

	public static string CHARACTER_CAN_PERFORM_AGAIN = "OnCharacterCanPerform";

	public static string REPROCESS_POI = "ReprocessPOI";

	public static string CHARACTER_REMOVED_BEHAVIOUR = "OnCharacterRemovedBehaviour";

	public static string CHARACTER_CAN_NO_LONGER_PERSONAL_PATROL = "OnCharacterCanNoLongerCombat";

	public static string CHARACTER_BECOME_DEMON_CULTIST = "OnCharacterBecomeDemonCultist";

	public static string CHARACTER_NO_LONGER_CULTIST = "OnCharacterNoLongerCultist";

	public static string CHARACTER_DISGUISED = "OnCharacterDisguised";

	public static string CHARACTER_MARKER_DESTROYED = "OnCharacterMarkerDestroyed";

	public static string CHARACTER_MARKER_EXPIRED = "OnCharacterMarkerExpired";

	public static string CHARACTER_ACCUSED_OF_CRIME = "OnCharacterAccusedOfCrime";

	public static string CHARACTER_CHANGED_NAME = "OnCharacterChangedName";

	public static string RENAME_CHARACTER = "OnRenameCharacter";

	public static string NECROMANCER_SPAWNED = "OnNecromancerSpawned";

	public static string CHARACTER_HIT_DEMONIC_STRUCTURE = "OnCharacterHitDemonicStructure";

	public static string HEALTH_CRITICALLY_LOW = "OnHealthCriticallyLow";

	public static string CHARACTER_TICK_ENDED_MOVEMENT = "OnTickEndedCharacterMovement";

	public static string PROCESS_ALL_UNPOROCESSED_POIS = "ProcessAllUnprocessedPOIS";

	public static string CHARACTER_TICK_ENDED = "OnCharacterTickEnded";

	public static string CHARACTER_INFO_REVEALED = "OnCharacterInfoRevealed";

	public static string TOGGLE_CHARACTER_MARKER_NAMEPLATE = "OnToggleCharacterMarkerNameplate";

	public static string ON_CHARACTER_RAISE_DEAD_BY_NECRO = "OnCharacterRaiseDeadByNecro";

	public static string TRY_CREATE_BURY_JOBS = "CheckBuryJob";

	public static string CHARACTER_FINISHED_DEVASTATION_RITUAL = "OnCharacterFinishedDevastationRitual";

	public static string ON_CHARACTER_TAMED = "OnCharacterTamed";

	public static string CHARACTER_PRAY_SUCCESS = "OnCharacterPraySuccess";

	public static string CHARACTER_BECAME_VAMPIRE = "OnCharacterBecameVampire";

	public static string CHARACTER_MEDDLER_SCHEME_SUCCESSFUL = "OnCharacterMedlerSchemeSuccessful";

	public static string LYCANTHROPE_SHED_WOLF_PELT = "OnLycanthropeShedWolfPelt";

	public static string WEAPON_UNEQUIPPED = "OnWeaponUnequipped";

	public static string ARMOR_UNEQUIPPED = "OnArmorUnequipped";

	public static string ACCESSORY_UNEQUIPPED = "OnAccessoryUnequipped";

	public static string CHARACTER_EQUIPPED_ITEM = "OnCharacterEquippedItem";

	public static string SHARED_OPINION_MODIFIER_DECREASED = "OnSharedOpinionModifierDecreased";

	public static string SHARED_OPINION_MODIFIER_INCREASED = "OnSharedOpinionModifierIncreased";

	public static string UPDATE_CHARACTER_PORTRAITS = "UpdateCharacterPortraits";

	public static string ACTIVE_RELIGIOUS_CULTISTS_UPDATED = "ActiveReligiousCultistsUpdated";

	public static string GHOST_SPAWNED_THAT_COUNTS_FOR_TASK = "GhostSpawnedThatCountsForTask";

	public static string REVENANT_SPAWNED_THAT_COUNTS_FOR_TASK = "RevenantSpawnedThatCountsForTask";

	public static string BECAME_WEREWOLF_VIA_PELT = "BecameWerewolfViaPelt";

	public static string PLAGUE_FATALITY_ACTIVATED = "OnPlagueFatalityActivated";

	public static string CHARACTER_DIED_FROM_PLAYER_SOURCE = "CharacterDiedFromPlayerSource";

	public static string DISCONNECT_FROM_CHARACTER = "DisconnectFromCharacter";

	public static string CRIME_REMOVED_FROM_DATABASE = "CrimeRemovedFromDatabase";

	public static string SHARED_OPINION_REMOVED_FROM_DATABASE = "OnSharedOpinionRemovedFromDatabase";

	public static string CHARACTER_REMOVED_FROM_ALIVE_VILLAGERS = "CharacterRemovedFromAliveVillagers";

	public static string CHARACTER_ADDED_TO_ALIVE_VILLAGERS = "CharacterAddedToAliveVillagers";
}
