using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Character_Talents;
using Characters.Components;
using Factions.Faction_Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Interrupts;
using JetBrains.Annotations;
using Locations;
using Locations.Settlements;
using Logs;
using Object_Pools;
using Plague.Transmission;
using Traits;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UtilityScripts;

public class Character : Relatable, ILeader, ISavable, IPointOfInterest, ITraitable, IDamageable, ISelectable, ILogFiller, IGCollectable, IJobOwner, IPlayerActionTarget, IObjectManipulator, IPartyQuestTarget, IGatheringTarget, IStoredTarget, IBookmarkable, IContextMenuItem, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	private int _id;

	private string _name;

	private string _uiString;

	protected bool _isDead;

	private GENDER _gender;

	private HAIR_COLOR _hairColorType;

	private RaceData _raceSetting;

	private Faction _faction;

	private Minion _minion;

	private LocationStructure _currentStructure;

	private Region _currentRegion;

	public LocationGridTile deathTilePosition { get; protected set; }

	public string persistentID { get; private set; }

	public CharacterVisuals visuals { get; private set; }

	public int currentHP { get; private set; }

	public int doNotRecoverPassiveHP { get; private set; }

	public SEXUALITY sexuality { get; private set; }

	public bool canPersonalPatrol { get; private set; }

	public int numOfNonSecretActionsBeingPerformedOnThis { get; private set; }

	public Region homeRegion { get; protected set; }

	public NPCSettlement homeSettlement { get; protected set; }

	public LocationStructure homeStructure { get; protected set; }

	public CharacterMarker marker { get; private set; }

	public JobQueueItem currentJob { get; private set; }

	public GoapPlan currentPlan { get; private set; }

	public ActualGoapNode currentActionNode { get; private set; }

	public JobQueue jobQueue { get; private set; }

	public TileObject tileObjectLocation { get; private set; }

	public CharacterTrait defaultCharacterTrait { get; private set; }

	public List<INTERACTION_TYPE> advertisedActions { get; private set; }

	public List<TileObject> items { get; private set; }

	public List<TileObject> ownedItems { get; private set; }

	public List<JobQueueItem> allJobsTargetingThis { get; private set; }

	public bool hasUnresolvedCrime { get; protected set; }

	public bool isConversing { get; protected set; }

	public bool isInLimbo { get; protected set; }

	public bool isLimboCharacter { get; protected set; }

	public bool destroyMarkerOnDeath { get; protected set; }

	public bool isWanderer { get; private set; }

	public bool hasBeenRaisedFromDead { get; private set; }

	public bool hasSubscribedToSignals { get; private set; }

	public bool shouldDoActionOnFirstTickUponLoadGame { get; private set; }

	public bool isStoredAsTarget { get; private set; }

	public bool isDeadReference { get; private set; }

	public bool isRecruitedByAMajorFaction { get; private set; }

	public bool isWildMonster { get; protected set; }

	public Log deathLog { get; private set; }

	public bool wasKilledByPlayerSource { get; protected set; }

	public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; private set; }

	public Area territory { get; private set; }

	public LycanthropeData lycanData { get; protected set; }

	public Necromancer necromancerTrait { get; protected set; }

	public POI_STATE state { get; private set; }

	public ILocationAwareness currentLocationAwareness { get; private set; }

	public Vector2Int gridTileLocalPosition { get; private set; }

	public Vector3 gridTileWorldPosition { get; private set; }

	public bool hasMarker { get; private set; }

	public List<PLAYER_SKILL_TYPE> afflictionsSkillsInflictedByPlayer { get; set; }

	public LocationStructure deployedAtStructure { get; private set; }

	public Tombstone grave { get; private set; }

	public INTERACTION_TYPE causeOfDeath { get; set; }

	public PLAYER_SKILL_TYPE skillCauseOfDeath { get; set; }

	public PLAYER_SKILL_TYPE resonancePower { get; set; }

	public bool hasBeenCleanedUp { get; private set; }

	public TrapStructure trapStructure { get; private set; }

	public GoapPlanner planner { get; private set; }

	public CharacterClassComponent classComponent { get; private set; }

	public CharacterNeedsComponent needsComponent { get; private set; }

	public CharacterStructureComponent structureComponent { get; private set; }

	public CharacterStateComponent stateComponent { get; private set; }

	public NonActionEventsComponent nonActionEventsComponent { get; private set; }

	public InterruptComponent interruptComponent { get; private set; }

	public BehaviourComponent behaviourComponent { get; private set; }

	public MoodComponent moodComponent { get; private set; }

	public CharacterJobTriggerComponent jobComponent { get; private set; }

	public ReactionComponent reactionComponent { get; private set; }

	public LogComponent logComponent { get; private set; }

	public CombatComponent combatComponent { get; protected set; }

	public RumorComponent rumorComponent { get; private set; }

	public AssumptionComponent assumptionComponent { get; private set; }

	public MovementComponent movementComponent { get; private set; }

	public StateAwarenessComponent stateAwarenessComponent { get; private set; }

	public CarryComponent carryComponent { get; private set; }

	public CharacterPartyComponent partyComponent { get; private set; }

	public GatheringComponent gatheringComponent { get; private set; }

	public CharacterTileObjectComponent tileObjectComponent { get; private set; }

	public CrimeComponent crimeComponent { get; private set; }

	public ReligionComponent religionComponent { get; private set; }

	public LimiterComponent limiterComponent { get; private set; }

	public PiercingAndResistancesComponent piercingAndResistancesComponent { get; private set; }

	public CharacterEventDispatcher eventDispatcher { get; private set; }

	public PreviousCharacterDataComponent previousCharacterDataComponent { get; private set; }

	public CharacterTraitComponent traitComponent { get; private set; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; private set; }

	public EquipmentComponent equipmentComponent { get; private set; }

	public CharacterMoneyComponent moneyComponent { get; private set; }

	public ResourceStorageComponent resourceStorageComponent { get; private set; }

	public DailyScheduleComponent dailyScheduleComponent { get; private set; }

	public CharacterTalentComponent talentComponent { get; private set; }

	public VillagerWantsComponent villagerWantsComponent { get; private set; }

	public PetComponent petComponent { get; private set; }

	public CharacterMountComponent mountComponent { get; private set; }

	public string bookmarkName
	{
		get
		{
			if (lycanData == null)
			{
				return visuals.GetCharacterNameWithIconAndColor();
			}
			return lycanData.activeForm.visuals.GetCharacterNameWithIconAndColor();
		}
	}

	public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Character;

	public STORED_TARGET_TYPE storedTargetType
	{
		get
		{
			if (!(this is Summon))
			{
				return STORED_TARGET_TYPE.Character;
			}
			return STORED_TARGET_TYPE.Monster;
		}
	}

	public bool isTargetted { get; set; }

	public string iconRichText
	{
		get
		{
			if (lycanData == null)
			{
				return visuals.GetCharacterStringIcon();
			}
			return lycanData.activeForm.visuals.GetCharacterStringIcon();
		}
	}

	public virtual Type serializedData => typeof(SaveDataCharacter);

	public virtual string name => _name;

	public string raceClassName => GetDefaultRaceClassName();

	public override string relatableName => _name;

	public override int id => _id;

	public override GENDER gender => _gender;

	public HAIR_COLOR hairColorType => _hairColorType;

	public string firstNameWithColor => GetFirstNameWithColor();

	public string nameWithID => name;

	public bool isDead => _isDead;

	public bool isFactionLeader
	{
		get
		{
			if (faction != null)
			{
				return faction.leader == this;
			}
			return false;
		}
	}

	public bool isHoldingItem => items.Count > 0;

	public bool isAtHomeRegion
	{
		get
		{
			if (currentRegion == homeRegion)
			{
				return !carryComponent.masterCharacter.movementComponent.isTravellingInWorld;
			}
			return false;
		}
	}

	public bool isAtHomeStructure
	{
		get
		{
			if (currentStructure == homeStructure)
			{
				return homeStructure != null;
			}
			return false;
		}
	}

	public bool isPartOfHomeFaction
	{
		get
		{
			if (homeRegion != null && faction != null)
			{
				return homeRegion.IsFactionHere(faction);
			}
			return false;
		}
	}

	public bool isFactionless
	{
		get
		{
			if (faction != null)
			{
				return FactionManager.Instance.wildMonsterFaction == faction;
			}
			return true;
		}
	}

	public bool isSettlementRuler
	{
		get
		{
			if (homeSettlement != null)
			{
				return homeSettlement.ruler == this;
			}
			return false;
		}
	}

	public bool isHidden => reactionComponent.isHidden;

	public bool isBeingSeized
	{
		get
		{
			if (PlayerManager.Instance.player != null)
			{
				return PlayerManager.Instance.player.seizeComponent.seizedPOI == this;
			}
			return false;
		}
	}

	public bool isLycanthrope => lycanData != null;

	public bool isInWerewolfForm
	{
		get
		{
			if (isLycanthrope)
			{
				return lycanData.isInWerewolfForm;
			}
			return false;
		}
	}

	public bool isInVampireBatForm => IsInVampireBatForm();

	public bool isNormalCharacter
	{
		get
		{
			if (!(this is Summon) && minion == null)
			{
				return !characterClass.IsZombie();
			}
			return false;
		}
	}

	public bool isVillager
	{
		get
		{
			if (isNormalCharacter)
			{
				return race != RACE.RATMAN;
			}
			return false;
		}
	}

	public bool isNormalAndNotAlliedWithPlayer
	{
		get
		{
			if (isNormalCharacter)
			{
				return !isAlliedWithPlayer;
			}
			return false;
		}
	}

	public bool isNotSummonAndDemon
	{
		get
		{
			if (!(this is Summon))
			{
				return minion == null;
			}
			return false;
		}
	}

	public bool isNotSummonAndDemonAndZombie
	{
		get
		{
			if (!(this is Summon) && minion == null)
			{
				return characterClass.IsZombie();
			}
			return false;
		}
	}

	public bool isConsideredRatman
	{
		get
		{
			Faction obj = faction;
			if (obj != null && obj.factionType.type == FACTION_TYPE.Ratmen)
			{
				return race == RACE.RATMAN;
			}
			return false;
		}
	}

	public bool isMonsterOrRatmanOrUndead
	{
		get
		{
			if (faction != null)
			{
				if ((race == RACE.RATMAN || this is Summon || faction.factionType.type == FACTION_TYPE.Undead) && faction.factionType.type != FACTION_TYPE.Demons)
				{
					return !faction.isMajorFaction;
				}
				return false;
			}
			return false;
		}
	}

	public bool canBeTargetedByLandActions
	{
		get
		{
			if (!movementComponent.isFlying && !reactionComponent.isHidden)
			{
				return !traitContainer.HasTrait("Disabler", "DeMooder");
			}
			return false;
		}
	}

	public int maxHP => combatComponent.maxHP;

	public bool isInfoUnlocked { get; set; }

	public Vector3 worldPosition => marker.transform.position;

	public Vector2 selectableSize => visuals.selectableSize;

	public Vector3 attackRangePosition => worldPosition;

	public Transform worldObject
	{
		get
		{
			if (!hasMarker)
			{
				return null;
			}
			return marker.transform;
		}
	}

	public POINT_OF_INTEREST_TYPE poiType => POINT_OF_INTEREST_TYPE.CHARACTER;

	public RACE race => _raceSetting.race;

	public JOB_OWNER ownerType => JOB_OWNER.CHARACTER;

	public CharacterClass characterClass => classComponent.characterClass;

	public RaceData raceSetting => _raceSetting;

	public Faction faction => _faction;

	public Faction factionOwner => _faction;

	public Minion minion => _minion;

	public BaseSettlement currentSettlement
	{
		get
		{
			if (gridTileLocation != null)
			{
				if (gridTileLocation.structure.settlementLocation != null)
				{
					return gridTileLocation.structure.settlementLocation;
				}
				return areaLocation.GetCurrentSettlementOfCharacter(this);
			}
			return null;
		}
	}

	public ProjectileReceiver projectileReceiver
	{
		get
		{
			if (hasMarker && marker.visionTrigger != null)
			{
				return marker.visionTrigger.projectileReceiver;
			}
			return null;
		}
	}

	public Character isBeingCarriedBy => carryComponent.isBeingCarriedBy;

	public JobTriggerComponent jobTriggerComponent => jobComponent;

	public GameObject visualGO => marker.gameObject;

	public Character characterOwner => null;

	public BaseMapObjectVisual mapObjectVisual => marker;

	public bool isAlliedWithPlayer => IsAlliedWithPlayer();

	public bool isNotHostileWithPlayer => IsNotHostileWithPlayer();

	public Region currentRegion
	{
		get
		{
			Character character = carryComponent.isBeingCarriedBy;
			if (character != null)
			{
				return character.currentRegion;
			}
			return _currentRegion;
		}
	}

	public LocationGridTile gridTileLocation
	{
		get
		{
			if (!hasMarker)
			{
				return null;
			}
			Character character = carryComponent.isBeingCarriedBy;
			if (character != null)
			{
				return character.gridTileLocation;
			}
			return GetLocationGridTileByXY(gridTileLocalPosition.x, gridTileLocalPosition.y);
		}
	}

	public Area areaLocation => gridTileLocation?.area;

	public LocationStructure currentStructure
	{
		get
		{
			Character character = carryComponent.isBeingCarriedBy;
			if (character != null)
			{
				return character.currentStructure;
			}
			return _currentStructure;
		}
	}

	public bool isVagrant
	{
		get
		{
			if (faction != null)
			{
				return faction.factionType.type == FACTION_TYPE.Vagrants;
			}
			return false;
		}
	}

	public bool isVagrantOrFactionless
	{
		get
		{
			if (faction != null)
			{
				return FactionManager.Instance.vagrantFaction == faction;
			}
			return true;
		}
	}

	public Faction prevFaction => previousCharacterDataComponent.previousFaction;

	public Sprite contextMenuIcon => null;

	public string contextMenuName => bookmarkName;

	public int contextMenuColumn => 1;

	public List<IContextMenuItem> subMenus => null;

	public string uiString => GetUIString();

	public bool isTrappedInsideDemonicRoom
	{
		get
		{
			StructureRoom room;
			if (currentStructure != null && (currentStructure.structureType == STRUCTURE_TYPE.KENNEL || currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS))
			{
				return currentStructure.IsTilePartOfARoom(gridTileLocation, out room);
			}
			return false;
		}
	}

	public ITraitContainer traitContainer { get; private set; }

	public TraitProcessor traitProcessor => TraitManager.characterTraitProcessor;

	public List<PLAYER_SKILL_TYPE> actions { get; protected set; }

	public Character(string className, RACE race, GENDER gender, SEXUALITY sexuality, HAIR_COLOR p_hairColorType, int id = -1)
		: this()
	{
		skillCauseOfDeath = PLAYER_SKILL_TYPE.NONE;
		persistentID = Utilities.GetNewUniqueID();
		_id = ((id == -1) ? Utilities.SetID(this) : Utilities.SetID(this, id));
		_gender = gender;
		classComponent.AssignClass(className, isInitial: true);
		AssignRace(race, isInitial: true);
		SetSexuality(sexuality);
		SetHairColorType(p_hairColorType);
		visuals = new CharacterVisuals(this);
		needsComponent.UpdateBaseStaminaDecreaseRate();
		combatComponent.UpdateBasicData(resetHP: true);
		structureComponent = new CharacterStructureComponent();
		structureComponent.SetOwner(this);
		afflictionsSkillsInflictedByPlayer = new List<PLAYER_SKILL_TYPE>();
		combatComponent.ApplyInitialBonusStrengthAndIntelligenceOnCreation();
	}

	public Character(string className, RACE race, GENDER gender, HAIR_COLOR p_hairColorType)
		: this()
	{
		skillCauseOfDeath = PLAYER_SKILL_TYPE.NONE;
		persistentID = Utilities.GetNewUniqueID();
		_id = Utilities.SetID(this);
		_gender = gender;
		classComponent.AssignClass(className, isInitial: true);
		AssignRace(race, isInitial: true);
		GenerateSexuality();
		SetHairColorType(p_hairColorType);
		visuals = new CharacterVisuals(this);
		needsComponent.UpdateBaseStaminaDecreaseRate();
		combatComponent.UpdateBasicData(resetHP: true);
		structureComponent = new CharacterStructureComponent();
		structureComponent.SetOwner(this);
		afflictionsSkillsInflictedByPlayer = new List<PLAYER_SKILL_TYPE>();
		combatComponent.ApplyInitialBonusStrengthAndIntelligenceOnCreation();
	}

	private Character()
	{
		SetIsDead(isDead: false);
		CreateTraitContainer();
		skillCauseOfDeath = PLAYER_SKILL_TYPE.NONE;
		advertisedActions = new List<INTERACTION_TYPE>();
		items = new List<TileObject>();
		ownedItems = new List<TileObject>();
		allJobsTargetingThis = new List<JobQueueItem>();
		forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
		SetPOIState(POI_STATE.ACTIVE);
		jobQueue = new JobQueue(this);
		trapStructure = new TrapStructure();
		planner = new GoapPlanner(this);
		jobComponent = new CharacterJobTriggerComponent();
		jobComponent.SetOwner(this);
		logComponent = new LogComponent();
		classComponent = new CharacterClassComponent();
		classComponent.SetOwner(this);
		needsComponent = new CharacterNeedsComponent();
		needsComponent.SetOwner(this);
		stateComponent = new CharacterStateComponent();
		stateComponent.SetOwner(this);
		nonActionEventsComponent = new NonActionEventsComponent();
		nonActionEventsComponent.SetOwner(this);
		interruptComponent = new InterruptComponent();
		interruptComponent.SetOwner(this);
		behaviourComponent = new BehaviourComponent();
		behaviourComponent.SetOwner(this);
		moodComponent = new MoodComponent();
		moodComponent.SetOwner(this);
		reactionComponent = new ReactionComponent();
		reactionComponent.SetOwner(this);
		combatComponent = new CombatComponent();
		combatComponent.SetOwner(this);
		rumorComponent = new RumorComponent();
		rumorComponent.SetOwner(this);
		assumptionComponent = new AssumptionComponent();
		assumptionComponent.SetOwner(this);
		movementComponent = new MovementComponent();
		movementComponent.SetOwner(this);
		stateAwarenessComponent = new StateAwarenessComponent();
		stateAwarenessComponent.SetOwner(this);
		carryComponent = new CarryComponent();
		carryComponent.SetOwner(this);
		partyComponent = new CharacterPartyComponent();
		partyComponent.SetOwner(this);
		gatheringComponent = new GatheringComponent();
		gatheringComponent.SetOwner(this);
		tileObjectComponent = new CharacterTileObjectComponent();
		tileObjectComponent.SetOwner(this);
		crimeComponent = new CrimeComponent();
		crimeComponent.SetOwner(this);
		religionComponent = new ReligionComponent();
		religionComponent.SetOwner(this);
		limiterComponent = new LimiterComponent();
		limiterComponent.SetOwner(this);
		piercingAndResistancesComponent = new PiercingAndResistancesComponent();
		piercingAndResistancesComponent.SetOwner(this);
		previousCharacterDataComponent = new PreviousCharacterDataComponent();
		previousCharacterDataComponent.SetOwner(this);
		traitComponent = new CharacterTraitComponent();
		traitComponent.SetOwner(this);
		moneyComponent = new CharacterMoneyComponent();
		moneyComponent.SetOwner(this);
		mountComponent = new CharacterMountComponent();
		mountComponent.SetOwner(this);
		petComponent = new PetComponent();
		petComponent.SetOwner(this);
		eventDispatcher = new CharacterEventDispatcher();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		equipmentComponent = new EquipmentComponent();
		resourceStorageComponent = new ResourceStorageComponent();
		dailyScheduleComponent = new DailyScheduleComponent();
		dailyScheduleComponent.SetOwner(this);
		resonancePower = PlayerManager.Instance.GetRandomResonancePower(this);
	}

	public Character(SaveDataCharacter data)
	{
		skillCauseOfDeath = PLAYER_SKILL_TYPE.NONE;
		shouldDoActionOnFirstTickUponLoadGame = true;
		advertisedActions = new List<INTERACTION_TYPE>();
		items = new List<TileObject>();
		ownedItems = new List<TileObject>();
		allJobsTargetingThis = new List<JobQueueItem>();
		forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
		jobQueue = new JobQueue(this);
		planner = new GoapPlanner(this);
		_raceSetting = RaceManager.Instance.GetRaceData(data.race);
		CreateTraitContainer();
		persistentID = data.persistentID;
		_id = Utilities.SetID(this, data.id);
		_name = data.firstName;
		_isDead = data.isDead;
		_gender = data.gender;
		_hairColorType = data.hairColorType;
		sexuality = data.sexuality;
		currentHP = data.currentHP;
		doNotRecoverPassiveHP = data.doNotRecoverHP;
		advertisedActions = new List<INTERACTION_TYPE>(data.advertisedActions);
		canPersonalPatrol = data.canCombat;
		hasUnresolvedCrime = data.hasUnresolvedCrime;
		isInLimbo = data.isInLimbo;
		isLimboCharacter = data.isLimboCharacter;
		destroyMarkerOnDeath = data.destroyMarkerOnDeath;
		isWanderer = data.isWanderer;
		hasBeenRaisedFromDead = data.hasBeenRaisedFromDead;
		state = data.state;
		causeOfDeath = data.causeOfDeath;
		isStoredAsTarget = data.isStoredAsTarget;
		isDeadReference = data.isDeadReference;
		isRecruitedByAMajorFaction = data.isRecruitedByAMajorFaction;
		if (data.afflictionsSkillsInflictedByPlayer != null && data.afflictionsSkillsInflictedByPlayer.Count > 0)
		{
			afflictionsSkillsInflictedByPlayer = new List<PLAYER_SKILL_TYPE>(data.afflictionsSkillsInflictedByPlayer);
		}
		else
		{
			afflictionsSkillsInflictedByPlayer = new List<PLAYER_SKILL_TYPE>();
		}
		trapStructure = data.trapStructure.Load();
		classComponent = data.classComponent.Load();
		classComponent.SetOwner(this);
		needsComponent = data.needsComponent.Load();
		needsComponent.SetOwner(this);
		structureComponent = data.structureComponent.Load();
		structureComponent.SetOwner(this);
		stateComponent = data.stateComponent.Load();
		stateComponent.SetOwner(this);
		nonActionEventsComponent = data.nonActionEventsComponent.Load();
		nonActionEventsComponent.SetOwner(this);
		interruptComponent = data.interruptComponent.Load();
		interruptComponent.SetOwner(this);
		behaviourComponent = data.behaviourComponent.Load();
		behaviourComponent.SetOwner(this);
		moodComponent = data.moodComponent.Load();
		moodComponent.SetOwner(this);
		jobComponent = data.jobComponent.Load();
		jobComponent.SetOwner(this);
		reactionComponent = data.reactionComponent.Load();
		reactionComponent.SetOwner(this);
		logComponent = data.logComponent.Load();
		combatComponent = data.combatComponent.Load();
		combatComponent.SetOwner(this);
		rumorComponent = data.rumorComponent.Load();
		rumorComponent.SetOwner(this);
		assumptionComponent = data.assumptionComponent.Load();
		assumptionComponent.SetOwner(this);
		movementComponent = data.movementComponent.Load();
		movementComponent.SetOwner(this);
		stateAwarenessComponent = data.stateAwarenessComponent.Load();
		stateAwarenessComponent.SetOwner(this);
		carryComponent = data.carryComponent.Load();
		carryComponent.SetOwner(this);
		partyComponent = data.partyComponent.Load();
		partyComponent.SetOwner(this);
		gatheringComponent = data.gatheringComponent.Load();
		gatheringComponent.SetOwner(this);
		tileObjectComponent = data.tileObjectComponent.Load();
		tileObjectComponent.SetOwner(this);
		crimeComponent = data.crimeComponent.Load();
		crimeComponent.SetOwner(this);
		religionComponent = data.religionComponent.Load();
		religionComponent.SetOwner(this);
		limiterComponent = data.limiterComponent.Load();
		limiterComponent.SetOwner(this);
		piercingAndResistancesComponent = data.piercingAndResistancesComponent.Load();
		piercingAndResistancesComponent.SetOwner(this);
		previousCharacterDataComponent = data.previousCharacterDataComponent.Load();
		previousCharacterDataComponent.SetOwner(this);
		traitComponent = data.traitComponent.Load();
		traitComponent.SetOwner(this);
		moneyComponent = data.moneyComponent.Load();
		moneyComponent.SetOwner(this);
		dailyScheduleComponent = data.dailyScheduleComponent.Load();
		dailyScheduleComponent.SetOwner(this);
		mountComponent = data.mountComponent.Load();
		mountComponent.SetOwner(this);
		if (data.talentComponent != null)
		{
			talentComponent = data.talentComponent.Load();
			talentComponent.SetOwner(this);
		}
		if (data.villagerWantsComponent != null)
		{
			villagerWantsComponent = data.villagerWantsComponent.Load();
			villagerWantsComponent.SetOwner(this);
		}
		resonancePower = data.resonancePower;
		wasKilledByPlayerSource = data.wasKilledByPlayerSource;
		eventDispatcher = new CharacterEventDispatcher();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		resourceStorageComponent = data.resourceStorageComponent.Load();
		petComponent = data.petComponent.Load();
		petComponent.SetOwner(this);
	}

	public virtual void Initialize()
	{
		visuals.Initialize();
		ConstructDefaultPlayerActions();
		OnUpdateRace();
		classComponent.OnUpdateCharacterClass();
		moodComponent.SetMoodValue(50);
		if (needsComponent.HasNeeds())
		{
			needsComponent.Initialize();
		}
		religionComponent.Initialize();
		moneyComponent.Initialize();
		if (race != RACE.DEMON)
		{
			talentComponent = new CharacterTalentComponent();
			talentComponent.SetOwner(this);
			talentComponent.ConstructAllTalents();
			talentComponent.RandomizeInitialTalents(this);
			if (race.IsSapient())
			{
				RandomizeEquipmentOnMigration(this);
			}
		}
		if (race.IsSapient() || race == RACE.RATMAN)
		{
			villagerWantsComponent = new VillagerWantsComponent();
			villagerWantsComponent.SetOwner(this);
			villagerWantsComponent.Initialize(this);
			if (race == RACE.RATMAN)
			{
				isInfoUnlocked = true;
			}
		}
	}

	private void SetHairColorType(HAIR_COLOR p_hairColorType)
	{
		_hairColorType = p_hairColorType;
	}

	private void RandomizeEquipmentOnMigration(Character p_character)
	{
		float weaponEquipmentChance = PortalBonusDataHandler.Instance.portalBonus.migrationBonusPerLevel[0].weaponEquipmentChance;
		float armorEquipmentChance = PortalBonusDataHandler.Instance.portalBonus.migrationBonusPerLevel[0].armorEquipmentChance;
		float accessoryEquipmentChance = PortalBonusDataHandler.Instance.portalBonus.migrationBonusPerLevel[0].accessoryEquipmentChance;
		if (GameManager.Instance.gameHasStarted)
		{
			ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
			weaponEquipmentChance = PortalBonusDataHandler.Instance.portalBonus.migrationBonusPerLevel[thePortal.level - 1].weaponEquipmentChance;
			armorEquipmentChance = PortalBonusDataHandler.Instance.portalBonus.migrationBonusPerLevel[thePortal.level - 1].armorEquipmentChance;
			accessoryEquipmentChance = PortalBonusDataHandler.Instance.portalBonus.migrationBonusPerLevel[thePortal.level - 1].accessoryEquipmentChance;
		}
		CharacterClass characterClass = p_character.characterClass;
		if (GameUtilities.RollChance(weaponEquipmentChance))
		{
			List<TILE_OBJECT_TYPE> list = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
			for (int i = 0; i < characterClass.craftableWeapons.Count; i++)
			{
				TILE_OBJECT_TYPE tILE_OBJECT_TYPE = characterClass.craftableWeapons[i];
				if (!EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(tILE_OBJECT_TYPE.ToStringEnumWithSpace()).isLegendary)
				{
					list.Add(tILE_OBJECT_TYPE);
				}
			}
			if (list.Count > 0)
			{
				TILE_OBJECT_TYPE randomElement = CollectionUtilities.GetRandomElement(list);
				EquipmentItem item = InnerMapManager.Instance.CreateNewTileObject<EquipmentItem>(randomElement);
				p_character.ObtainItem(item);
			}
			RuinarchListPool<TILE_OBJECT_TYPE>.Release(list);
		}
		if (GameUtilities.RollChance(armorEquipmentChance))
		{
			List<TILE_OBJECT_TYPE> list2 = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
			for (int j = 0; j < characterClass.craftableArmors.Count; j++)
			{
				TILE_OBJECT_TYPE tILE_OBJECT_TYPE2 = characterClass.craftableArmors[j];
				if (!EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(tILE_OBJECT_TYPE2.ToStringEnumWithSpace()).isLegendary)
				{
					list2.Add(tILE_OBJECT_TYPE2);
				}
			}
			if (list2.Count > 0)
			{
				TILE_OBJECT_TYPE randomElement2 = CollectionUtilities.GetRandomElement(characterClass.craftableArmors);
				EquipmentItem item2 = InnerMapManager.Instance.CreateNewTileObject<EquipmentItem>(randomElement2);
				p_character.ObtainItem(item2);
			}
			RuinarchListPool<TILE_OBJECT_TYPE>.Release(list2);
		}
		if (!GameUtilities.RollChance(accessoryEquipmentChance))
		{
			return;
		}
		List<TILE_OBJECT_TYPE> list3 = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
		for (int k = 0; k < characterClass.craftableAccessories.Count; k++)
		{
			TILE_OBJECT_TYPE tILE_OBJECT_TYPE3 = characterClass.craftableAccessories[k];
			if (!EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(tILE_OBJECT_TYPE3.ToStringEnumWithSpace()).isLegendary)
			{
				list3.Add(tILE_OBJECT_TYPE3);
			}
		}
		if (list3.Count > 0)
		{
			TILE_OBJECT_TYPE randomElement3 = CollectionUtilities.GetRandomElement(characterClass.craftableAccessories);
			EquipmentItem item3 = InnerMapManager.Instance.CreateNewTileObject<EquipmentItem>(randomElement3);
			p_character.ObtainItem(item3);
		}
		RuinarchListPool<TILE_OBJECT_TYPE>.Release(list3);
	}

	public void InitialCharacterPlacement(LocationGridTile tile)
	{
		if (needsComponent.HasNeeds())
		{
			needsComponent.InitialCharacterPlacement();
		}
		ConstructInitialGoapAdvertisementActions();
		marker.InitialPlaceMarkerAt(tile);
		SubscribeToSignals();
		SubscribeToPermanentSignals();
	}

	public void SubscribeToPermanentSignals()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		crimeComponent.SubscribeToPermanentSignals();
	}

	public void UnsubscribeFromPermanentSignals()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.RemoveListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		crimeComponent.UnsubscribeToPermanentSignals();
	}

	public virtual void SubscribeToSignals()
	{
		_ = minion;
		if (!hasSubscribedToSignals)
		{
			hasSubscribedToSignals = true;
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
			Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
			Messenger.AddListener(Signals.DAY_STARTED, DailyGoapProcesses);
			Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingPOI);
			Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelAllJobsOfTypeTargetingPOI);
			Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI_EXCEPT_SELF, ForceCancelAllJobsTargetingPOIExceptSelf);
			Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, ForceCancelAllActionsTargetingPOI);
			Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnActionPerformed);
			Messenger.AddListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnInterruptStarted);
			Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
			Messenger.AddListener<IPointOfInterest>(CharacterSignals.BEFORE_SEIZING_POI, OnBeforeSeizingPOI);
			Messenger.AddListener<TileObject>(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, OnStopCurrentActionTargetingPOI);
			Messenger.AddListener<TileObject, Character>(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, OnStopCurrentActionTargetingPOIExceptActor);
			Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
			Messenger.AddListener<LocationStructure>(StructureSignals.BEFORE_STRUCTURE_DESTROYED, OnBeforeStructureDestroyed);
			Messenger.AddListener<IPointOfInterest, int>(CharacterSignals.INCREASE_THREAT_THAT_SEES_POI, IncreaseThreatThatSeesPOI);
			Messenger.AddListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
			Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
			Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
			Messenger.AddListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnChangeFactionRelationship);
			Messenger.AddListener<Faction>(FactionSignals.FACTION_ACTIVE_CHANGED, OnChangeFactionActiveChanged);
			needsComponent.SubscribeToSignals();
			jobComponent.SubscribeToListeners();
			combatComponent.SubscribeToSignals();
			religionComponent.SubscribeListeners();
			movementComponent.SubscribeToSignals();
			previousCharacterDataComponent.SubscribeToListeners();
			moodComponent.SubscribeListeners();
			rumorComponent.SubscribeListeners();
		}
	}

	public virtual void UnsubscribeSignals()
	{
		if (hasSubscribedToSignals)
		{
			hasSubscribedToSignals = false;
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
			Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
			Messenger.RemoveListener(Signals.DAY_STARTED, DailyGoapProcesses);
			Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingPOI);
			Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelAllJobsOfTypeTargetingPOI);
			Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI_EXCEPT_SELF, ForceCancelAllJobsTargetingPOIExceptSelf);
			Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, ForceCancelAllActionsTargetingPOI);
			Messenger.RemoveListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnActionPerformed);
			Messenger.RemoveListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnInterruptStarted);
			Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
			Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.BEFORE_SEIZING_POI, OnBeforeSeizingPOI);
			Messenger.RemoveListener<TileObject>(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, OnStopCurrentActionTargetingPOI);
			Messenger.RemoveListener<TileObject, Character>(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, OnStopCurrentActionTargetingPOIExceptActor);
			Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
			Messenger.RemoveListener<LocationStructure>(StructureSignals.BEFORE_STRUCTURE_DESTROYED, OnBeforeStructureDestroyed);
			Messenger.RemoveListener<IPointOfInterest, int>(CharacterSignals.INCREASE_THREAT_THAT_SEES_POI, IncreaseThreatThatSeesPOI);
			Messenger.RemoveListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
			Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
			Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
			Messenger.RemoveListener<Faction>(FactionSignals.FACTION_ACTIVE_CHANGED, OnChangeFactionActiveChanged);
			needsComponent.UnsubscribeToSignals();
			jobComponent.UnsubscribeListeners();
			combatComponent.UnsubscribeToSignals();
			religionComponent.UnsubscribeListeners();
			movementComponent.UnsubscribeFromSignals();
			previousCharacterDataComponent.UnsubscribeToListeners();
			moodComponent.UnsubscribeListeners();
			rumorComponent.UnsubscribeListeners();
		}
	}

	public virtual void OnSetIsHidden()
	{
	}

	private void OnStopCurrentActionTargetingPOI(TileObject poi)
	{
		if (currentActionNode != null && currentActionNode.poiTarget == poi)
		{
			StopCurrentActionNode();
		}
	}

	private void OnStopCurrentActionTargetingPOIExceptActor(TileObject poi, Character actor)
	{
		if (currentActionNode != null && currentActionNode.poiTarget == poi && this != actor)
		{
			StopCurrentActionNode();
		}
	}

	private void IncreaseThreatThatSeesPOI(IPointOfInterest poi, int amount)
	{
		if (faction == null || !faction.isMajorNonPlayerOrVagrant || !marker)
		{
			return;
		}
		if (poi is Character poi2)
		{
			if (marker.IsPOIInVision(poi2))
			{
				PlayerManager.Instance.player.threatComponent.AdjustThreatAndApplyModification(amount);
			}
		}
		else if (poi is TileObject poi3 && marker.IsPOIInVision(poi3))
		{
			PlayerManager.Instance.player.threatComponent.AdjustThreatAndApplyModification(amount);
		}
	}

	private void ProcessBeforeDeath(string cause, Character responsibleCharacter)
	{
		if (cause == "attacked" && responsibleCharacter != null && responsibleCharacter.isNormalCharacter && responsibleCharacter.faction != null && responsibleCharacter.faction.isMajorNonPlayer && responsibleCharacter.faction != faction && faction != null && faction.isMajorNonPlayer && (faction.factionType.type == FACTION_TYPE.Human_Empire || faction.factionType.type == FACTION_TYPE.Elven_Kingdom) && IsInHomeSettlement() && !homeSettlement.HasAliveResident())
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
			LocationGridTile deathTile = gridTileLocation;
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				SpawnRevenant(responsibleCharacter, deathTile);
			}, this);
		}
	}

	private void SpawnRevenant(Character responsibleCharacter, LocationGridTile deathTile)
	{
		BaseSettlement settlement = null;
		deathTile.IsPartOfSettlement(out settlement);
		Region region = deathTile.structure.region;
		if (settlement == null)
		{
			return;
		}
		Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Revenant, FactionManager.Instance.undeadFaction, settlement, region);
		CharacterManager.Instance.PlaceSummonInitially(summon, deathTile);
		Revenant revenant = summon as Revenant;
		if (responsibleCharacter.partyComponent.hasParty)
		{
			for (int i = 0; i < responsibleCharacter.partyComponent.currentParty.members.Count; i++)
			{
				Character character = responsibleCharacter.partyComponent.currentParty.members[i];
				revenant.AddBetrayer(character);
			}
		}
		else
		{
			revenant.AddBetrayer(responsibleCharacter);
		}
		int num = UnityEngine.Random.Range(1, 4);
		for (int j = 0; j < num; j++)
		{
			Character randomBetrayer = revenant.GetRandomBetrayer();
			Summon summon2 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, FactionManager.Instance.undeadFaction, settlement, region);
			(summon2 as Ghost).SetBetrayedBy(randomBetrayer);
			Area randomArea = settlement.GetRandomArea();
			if (randomArea != null)
			{
				CharacterManager.Instance.PlaceSummonInitially(summon2, randomArea.gridTileComponent.GetRandomTile());
				Messenger.Broadcast(CharacterSignals.GHOST_SPAWNED_THAT_COUNTS_FOR_TASK, summon2);
			}
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "spawn_revenant", LOG_TAG.Major);
		log.AddToFillers(revenant, revenant.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
		LogPool.Release(log);
		Messenger.Broadcast(CharacterSignals.REVENANT_SPAWNED_THAT_COUNTS_FOR_TASK, revenant);
	}

	private void OnCharacterChangedName(Character p_character)
	{
		if (p_character != this)
		{
			UpdateCurrentLogsBasedOnUpdatedCharacter(p_character);
			moodComponent.UpdateMoodSummaryLogsOnCharacterChangedName(p_character);
		}
	}

	private void UpdateCurrentLogsBasedOnUpdatedCharacter(Character p_character)
	{
		if (interruptComponent.isInterrupted)
		{
			interruptComponent.thoughtBubbleLog?.TryUpdateLogAfterRename(p_character, force: true);
		}
		if (currentActionNode != null)
		{
			currentActionNode.thoughtBubbleLog?.TryUpdateLogAfterRename(p_character);
			currentActionNode.thoughtBubbleMovingLog?.TryUpdateLogAfterRename(p_character);
			currentActionNode.descriptionLog?.TryUpdateLogAfterRename(p_character);
		}
		if (deathLog != null)
		{
			deathLog.TryUpdateLogAfterRename(p_character);
		}
		stateComponent.currentState?.thoughtBubbleLog?.TryUpdateLogAfterRename(p_character, force: true);
	}

	private void OnChangeFactionActiveChanged(Faction p_faction)
	{
		crimeComponent.OnFactionActiveStateChanged(p_faction);
	}

	public void DisconnectFromSettlement(NPCSettlement p_settlement)
	{
		if (homeSettlement == p_settlement)
		{
			MigrateHomeTo(null);
		}
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
		list.AddRange(jobQueue.jobsInQueue);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "CancelReasons_Table", "Location_Destroyed", LOG_TAG.Life_Changes);
		log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		string logText = log.logText;
		LogPool.Release(log);
		for (int i = 0; i < list.Count; i++)
		{
			JobQueueItem jobQueueItem = list[i];
			if (jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.HasOtherDataRelatedTo(p_settlement))
			{
				jobQueueItem.ForceCancelJob(logText);
			}
		}
		RuinarchListPool<JobQueueItem>.Release(list);
		previousCharacterDataComponent?.DisconnectFromSettlement(p_settlement);
		behaviourComponent?.DisconnectFromSettlement(p_settlement);
	}

	public virtual void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (_currentStructure == p_structure)
		{
			if (gridTileLocation != null)
			{
				SetCurrentStructureLocation(gridTileLocation.structure);
			}
			else
			{
				SetCurrentStructureLocation(null);
			}
		}
		if (homeStructure == p_structure)
		{
			MigrateHomeStructureTo(null);
		}
		previousCharacterDataComponent.DisconnectFromStructure(p_structure);
		trapStructure.DisconnectFromStructure(p_structure);
		if (deployedAtStructure == p_structure)
		{
			deployedAtStructure = null;
		}
		necromancerTrait?.DisconnectFromStructure(p_structure);
		movementComponent?.DisconnectFromStructure(p_structure);
		behaviourComponent?.DisconnectFromStructure(p_structure);
		rumorComponent?.DisconnectFromStructure(p_structure);
	}

	protected virtual void DisconnectFromCharacter(Character p_character)
	{
		if (!hasBeenCleanedUp)
		{
			base.relationshipContainer?.DisconnectFromCharacter(p_character);
			traitContainer?.DisconnectFromCharacter(this, p_character);
			carryComponent?.DisconnectFromCharacter(p_character);
			combatComponent?.DisconnectFromCharacter(p_character);
			gatheringComponent?.DisconnectFromCharacter(p_character);
			interruptComponent?.DisconnectFromCharacter(p_character);
			reactionComponent?.DisconnectFromCharacter(p_character);
			rumorComponent?.DisconnectFromCharacter(p_character);
			lycanData?.DisconnectFromCharacter(p_character);
			traitComponent?.DisconnectFromCharacter(p_character);
			behaviourComponent?.DisconnectFromCharacter(p_character);
			crimeComponent?.DisconnectFromCharacter(p_character);
			if (hasMarker)
			{
				marker.DisconnectFromCharacter(p_character);
			}
		}
	}

	private void GenerateSexuality()
	{
		if (GameUtilities.IsRaceBeast(race))
		{
			sexuality = SEXUALITY.STRAIGHT;
			return;
		}
		int num = UnityEngine.Random.Range(0, 100);
		if (num < 80)
		{
			sexuality = SEXUALITY.STRAIGHT;
		}
		else if (num >= 80 && num < 90)
		{
			sexuality = SEXUALITY.BISEXUAL;
		}
		else
		{
			sexuality = SEXUALITY.GAY;
		}
	}

	public void SetSexuality(SEXUALITY sexuality)
	{
		this.sexuality = sexuality;
	}

	public void SetGridTilePosition(Vector2 p_anchoredPos)
	{
		int x = (int)p_anchoredPos.x;
		int y = (int)p_anchoredPos.y;
		gridTileLocalPosition = new Vector2Int(x, y);
	}

	public void SetGridTileWorldPosition(Vector3 p_pos)
	{
		gridTileWorldPosition = p_pos;
	}

	public void CreateMarker()
	{
		CharacterMarker component = InnerMapManager.Instance.mapObjectFactory.CreateCharacterMarker(race).GetComponent<CharacterMarker>();
		SetCharacterMarker(component);
		component.SetCharacter(this);
	}

	public void DestroyMarker(LocationGridTile destroyedAt = null, bool removeFromGame = true)
	{
		if (hasMarker)
		{
			LocationGridTile locationGridTile;
			if (destroyedAt == null)
			{
				locationGridTile = gridTileLocation;
				locationGridTile.RemoveCharacterHere(this);
				locationGridTile.structure.RemoveCharacterAtLocation(this);
				locationGridTile.area.locationCharacterTracker.RemoveCharacterFromLocation(this, locationGridTile.area);
			}
			else
			{
				locationGridTile = destroyedAt;
				destroyedAt.RemoveCharacterHere(this);
				destroyedAt.structure.RemoveCharacterAtLocation(this);
				destroyedAt.area.locationCharacterTracker.RemoveCharacterFromLocation(this, destroyedAt.area);
			}
			GridMap.Instance.mainRegion.RemoveCharacterFromLocation(this);
			GridMap.Instance.mainRegion.RemoveResident(this);
			ObjectPoolManager.Instance.DestroyObject(marker);
			SetCharacterMarker(null);
			CancelAllJobs();
			DropAllItems(locationGridTile);
			equipmentComponent?.RemoveAllEquipment(this);
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, "");
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)this, "");
			Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, (IPointOfInterest)this);
			if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.seizeComponent.seizedPOI == this)
			{
				throw new Exception(name + " is seized by the player but its marker was destroyed! Refer to call stack to find out what destroyed it.");
			}
			if (UIManager.Instance != null && UIManager.Instance.IsContextMenuShowingForTarget(this))
			{
				UIManager.Instance.HideContextMenu();
			}
			if (removeFromGame && !isNormalCharacter)
			{
				CharacterManager.Instance.AddCharacterToBeCleanedUp(this);
			}
		}
	}

	public void DisableMarker()
	{
		if (marker.gameObject.activeSelf)
		{
			marker.gameObject.SetActive(value: false);
			gridTileLocation.RemoveCharacterHere(this);
		}
	}

	public void EnableMarker()
	{
		if (!marker.gameObject.activeSelf)
		{
			marker.gameObject.SetActive(value: true);
		}
	}

	private void SetCharacterMarker(CharacterMarker p_marker)
	{
		if (p_marker == null && marker != null)
		{
			gridTileLocation?.RemoveCharacterHere(this);
			currentStructure?.RemoveCharacterAtLocation(this);
		}
		marker = p_marker;
		if (p_marker == null)
		{
			hasMarker = false;
			Messenger.Broadcast(CharacterSignals.CHARACTER_MARKER_DESTROYED, this);
		}
		else
		{
			hasMarker = true;
		}
	}

	public virtual void PerTickDuringMovement()
	{
	}

	public void SetCurrentJob(JobQueueItem job)
	{
		currentJob = job;
	}

	public void OnJobRemovedFromQueue(JobQueueItem job)
	{
		combatComponent.OnJobRemovedFromQueue(job);
		if (job.jobType.IsApprehendTypeJob() && job.poiTarget != null && carryComponent.IsPOICarried(job.poiTarget))
		{
			UncarryPOI(job.poiTarget);
		}
	}

	public void AddJobTargetingThis(JobQueueItem job)
	{
		allJobsTargetingThis.Add(job);
	}

	public bool RemoveJobTargetingThis(JobQueueItem job)
	{
		if (allJobsTargetingThis.Remove(job))
		{
			return true;
		}
		return false;
	}

	public void ForceCancelAllJobsTargetingThisCharacter(bool shouldDoAfterEffect)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i].ForceCancelJob())
			{
				i--;
			}
		}
	}

	public void ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE jobType)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem.ForceCancelJob())
			{
				i--;
			}
		}
	}

	public void ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE jobType, string conditionKey, Character otherCharacter)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i] is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = allJobsTargetingThis[i] as GoapPlanJob;
				if (goapPlanJob.jobType == jobType && goapPlanJob.assignedCharacter != otherCharacter && goapPlanJob.HasGoalConditionKey(conditionKey) && goapPlanJob.ForceCancelJob())
				{
					i--;
				}
			}
		}
	}

	public void ForceCancelAllJobsTargetingPOI(IPointOfInterest target, string reason)
	{
		for (int i = 0; i < jobQueue.jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobQueue.jobsInQueue[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI == target && (!(reason == GoapPlanJob.Target_Already_Dead_Reason) || goapPlanJob.shouldBeCancelledOnDeath) && goapPlanJob.ForceCancelJob(reason))
				{
					i--;
				}
			}
		}
	}

	private void ForceCancelAllActionsTargetingPOI(IPointOfInterest target, string reason)
	{
		if (currentActionNode != null && currentActionNode.associatedJob != null && currentActionNode.poiTarget == target && currentActionNode.associatedJob.poiTarget == target)
		{
			currentActionNode.associatedJob.ForceCancelJob(reason);
		}
	}

	private void ForceCancelAllJobsOfTypeTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType)
	{
		for (int i = 0; i < jobQueue.jobsInQueue.Count; i++)
		{
			if (jobQueue.jobsInQueue[i] is GoapPlanJob goapPlanJob && goapPlanJob.jobType == jobType && goapPlanJob.targetPOI == target && (!(reason == GoapPlanJob.Target_Already_Dead_Reason) || goapPlanJob.shouldBeCancelledOnDeath) && goapPlanJob.ForceCancelJob(reason))
			{
				i--;
			}
		}
	}

	private void ForceCancelAllJobsTargetingPOIExceptSelf(IPointOfInterest target, string reason)
	{
		for (int i = 0; i < jobQueue.jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobQueue.jobsInQueue[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI == target && this != target && (!(target is Character character) || !character.carryComponent.IsCurrentlyPartOf(this)) && goapPlanJob.ForceCancelJob(reason))
				{
					i--;
				}
			}
		}
	}

	public bool HasJobTargetingThis(JOB_TYPE jobType)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i].jobType == jobType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJobTargetingThis(JOB_TYPE jobType, Faction actorFaction)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem.assignedCharacter != null && jobQueueItem.assignedCharacter.faction == actorFaction)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJobTargetingThis(JOB_TYPE p_jobType, RACE p_assignedCharacterRace)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if (jobQueueItem.jobType == p_jobType && jobQueueItem.assignedCharacter != null && jobQueueItem.assignedCharacter.race == p_assignedCharacterRace)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJobTargetingThis(JOB_TYPE jobType1, JOB_TYPE jobType2)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if (jobQueueItem.jobType == jobType1 || jobQueueItem.jobType == jobType2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJobTargetingThis(JOB_TYPE jobType1, JOB_TYPE jobType2, JOB_TYPE jobType3)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if (jobQueueItem.jobType == jobType1 || jobQueueItem.jobType == jobType2 || jobQueueItem.jobType == jobType3)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCharacterInVisionWithJobAndTarget(JOB_TYPE p_jobType, IPointOfInterest p_target, Character p_exception = null)
	{
		if (hasMarker)
		{
			for (int i = 0; i < marker.inVisionCharacters.Count; i++)
			{
				Character character = marker.inVisionCharacters[i];
				if (character != this && (p_exception == null || character != p_exception) && character.currentJob != null && character.currentJob.jobType == p_jobType && character.currentJob.poiTarget == p_target)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetNumOfJobsTargettingThisCharacter(JOB_TYPE jobType)
	{
		int num = 0;
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i].jobType == jobType)
			{
				num++;
			}
		}
		return num;
	}

	public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType, string conditionKey)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i] is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = allJobsTargetingThis[i] as GoapPlanJob;
				if (goapPlanJob.jobType == jobType && goapPlanJob.HasGoalConditionKey(conditionKey))
				{
					return goapPlanJob;
				}
			}
		}
		return null;
	}

	public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i] is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = allJobsTargetingThis[i] as GoapPlanJob;
				if (goapPlanJob.jobType == jobType)
				{
					return goapPlanJob;
				}
			}
		}
		return null;
	}

	public void CancelOrUnassignRemoveTraitRelatedJobs()
	{
		ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.REMOVE_STATUS);
		jobQueue.CancelAllJobs(JOB_TYPE.REMOVE_STATUS);
	}

	public void CancelRemoveStatusFeedAndRepairJobsTargetingThis()
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if ((jobQueueItem.jobType == JOB_TYPE.REMOVE_STATUS || jobQueueItem.jobType == JOB_TYPE.REPAIR || jobQueueItem.jobType == JOB_TYPE.FEED) && jobQueueItem.CancelJob())
			{
				i--;
			}
		}
	}

	public bool CreateJobsOnTargetGainTrait(IPointOfInterest targetPOI, Trait traitGained)
	{
		if (!CanCharacterReact(targetPOI))
		{
			return true;
		}
		bool result = false;
		if (traitGained.CreateJobsOnEnterVisionBasedOnTrait(targetPOI, this))
		{
			result = true;
		}
		return result;
	}

	public void CancelAllJobsExceptForCurrent()
	{
		if (currentJob == null)
		{
			return;
		}
		for (int i = 0; i < jobQueue.jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobQueue.jobsInQueue[i];
			if (jobQueueItem != currentJob && jobQueueItem.CancelJob())
			{
				i--;
			}
		}
	}

	public void CancelAllJobs()
	{
		for (int i = 0; i < jobQueue.jobsInQueue.Count; i++)
		{
			if (jobQueue.jobsInQueue[i].CancelJob())
			{
				i--;
			}
		}
	}

	public void CancelAllJobsExceptForCurrent(params JOB_TYPE[] jobType)
	{
		if (currentJob == null)
		{
			return;
		}
		for (int i = 0; i < jobQueue.jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobQueue.jobsInQueue[i];
			if (jobQueueItem == currentJob)
			{
				continue;
			}
			for (int j = 0; j < jobType.Length; j++)
			{
				if (jobQueueItem.jobType == jobType[j])
				{
					if (jobQueueItem.CancelJob())
					{
						i--;
					}
					break;
				}
			}
		}
	}

	public void NoPathToDoJobOrAction(JobQueueItem job, ActualGoapNode action)
	{
		if (gridTileLocation != null && limiterComponent.canMove)
		{
			if (job.jobType == JOB_TYPE.RETURN_TERRITORY)
			{
				jobComponent.TriggerRoamAroundTile(JOB_TYPE.NO_PATH_IDLE);
			}
			else if (job.jobType == JOB_TYPE.ROAM_AROUND_TERRITORY || job.jobType == JOB_TYPE.ROAM_AROUND_CORRUPTION || job.jobType == JOB_TYPE.ROAM_AROUND_PORTAL)
			{
				jobComponent.TriggerRoamAroundTile(JOB_TYPE.NO_PATH_IDLE);
			}
			else if (action.goapType == INTERACTION_TYPE.RETURN_HOME)
			{
				jobComponent.TriggerRoamAroundTile(JOB_TYPE.NO_PATH_IDLE);
			}
			else
			{
				jobComponent.TriggerStand(JOB_TYPE.NO_PATH_IDLE);
			}
		}
	}

	public bool SetFaction(Faction newFaction)
	{
		if (_faction == newFaction)
		{
			return false;
		}
		if (_faction != null)
		{
			previousCharacterDataComponent.SetPreviousFaction(_faction);
		}
		_faction = newFaction;
		OnChangeFaction(prevFaction, newFaction);
		if (_faction != null)
		{
			bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
			Messenger.Broadcast(FactionSignals.FACTION_SET, this);
			eventDispatcher.ExecuteJoinedFaction(this, _faction);
		}
		return true;
	}

	public bool ChangeFactionTo(Faction newFaction, bool bypassIdeologyChecking = false)
	{
		if (faction == newFaction)
		{
			return false;
		}
		faction?.LeaveFaction(this);
		return newFaction.JoinFaction(this, broadcastSignal: true, bypassIdeologyChecking);
	}

	protected virtual void OnChangeFaction(Faction prevFaction, Faction newFaction)
	{
		if (prevFaction != null)
		{
			if (prevFaction.factionType.type == FACTION_TYPE.Undead)
			{
				behaviourComponent.RemoveBehaviourComponent(typeof(UndeadBehaviour));
			}
			if (prevFaction.factionType.type == FACTION_TYPE.Demons)
			{
				if (partyComponent.hasParty)
				{
					partyComponent.currentParty.RemoveMember(this);
				}
				if (minion != null)
				{
					ConstructDefaultPlayerActions();
				}
			}
		}
		if (newFaction != null)
		{
			if (newFaction.factionType.type == FACTION_TYPE.Undead)
			{
				behaviourComponent.AddBehaviourComponent(typeof(UndeadBehaviour));
			}
			if (newFaction.factionType.type == FACTION_TYPE.Demons && minion != null)
			{
				ConstructDefaultPlayerActions();
			}
		}
		if (!isDead && newFaction != null)
		{
			Faction vagrantFaction = FactionManager.Instance.vagrantFaction;
			if (prevFaction == vagrantFaction && newFaction != vagrantFaction)
			{
				if (crimeComponent.HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous))
				{
					PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<BanditsEvent>().AdjustNumberOfAliveVagrantsWithSeriousOrHeinousCrime(-1);
				}
			}
			else if (prevFaction != vagrantFaction && newFaction == vagrantFaction && crimeComponent.HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous))
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<BanditsEvent>().AdjustNumberOfAliveVagrantsWithSeriousOrHeinousCrime(1);
			}
		}
		movementComponent.OnChangeFactionTo(newFaction);
		movementComponent.RedetermineFactionsToAvoid(this);
		petComponent.OnComponentOwnerChangedFaction(newFaction);
		previousCharacterDataComponent.OnChangeFactionTo(newFaction);
		if (partyComponent.hasParty)
		{
			partyComponent.currentParty.UpdatePartyFaction();
		}
		ResetUIString();
	}

	private void OnChangeFactionRelationship(Faction p_faction1, Faction p_faction2, FACTION_RELATIONSHIP_STATUS p_newStatus, FACTION_RELATIONSHIP_STATUS p_oldStatus)
	{
		if (p_faction1 == _faction || p_faction2 == _faction)
		{
			movementComponent.RedetermineFactionsToAvoid(this);
		}
	}

	public Faction JoinFactionProcessing()
	{
		Faction chosenFaction = null;
		if ((this.faction == null || this.faction.factionType.type != FACTION_TYPE.Bandits) && !isFactionLeader && !isSettlementRuler && crimeComponent.HasWantedCrimeType(CRIME_TYPE.Theft, CRIME_TYPE.Murder, CRIME_TYPE.Assault, CRIME_TYPE.Arson) && characterClass.className != "Demon Cult Leader")
		{
			FactionManager.Instance.JoinOrCreateBanditFaction(this);
			if (this.faction == FactionManager.Instance.banditFaction && this.faction != null)
			{
				return FactionManager.Instance.banditFaction;
			}
		}
		ReligiousCultJoinFactionProcessing(RELIGION.Demon_Worship, out chosenFaction);
		if (chosenFaction == null)
		{
			ReligiousCultJoinFactionProcessing(RELIGION.Divine_Worship, out chosenFaction);
		}
		if (chosenFaction == null)
		{
			ReligiousCultJoinFactionProcessing(RELIGION.Nature_Worship, out chosenFaction);
		}
		List<Faction> list = RuinarchListPool<Faction>.Claim(10);
		if (chosenFaction == null)
		{
			list.Clear();
			if (currentRegion != null)
			{
				for (int i = 0; i < currentRegion.factionsHere.Count; i++)
				{
					Faction faction = currentRegion.factionsHere[i];
					if (faction.isMajorNonPlayer && !faction.isDisbanded && !faction.IsCharacterBannedFromJoining(this) && faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(this) && faction != prevFaction && faction.HasOwnedSettlementInRegion(currentRegion))
					{
						if (this.faction.HasMemberThatIsNotDeadAndIsFamilyOrLoverAndNotEnemyRivalWith(this))
						{
							chosenFaction = faction;
							break;
						}
						if (this.faction.HasMemberThatIsNotDeadAndIsCloseFriendWith(this))
						{
							chosenFaction = faction;
							break;
						}
						if (!this.faction.HasMemberThatIsNotDeadAndIsRivalWith(this))
						{
							chosenFaction = faction;
							break;
						}
						if (this.faction.HasMemberThatIsNotDeadAndIsFriendWith(this))
						{
							chosenFaction = faction;
							break;
						}
						if (!list.Contains(faction))
						{
							list.Add(faction);
						}
					}
				}
				if (chosenFaction == null && list.Count > 0)
				{
					chosenFaction = CollectionUtilities.GetRandomElement(list);
				}
			}
		}
		RuinarchListPool<Faction>.Release(list);
		if (chosenFaction != null)
		{
			if (chosenFaction.characters.Count > 0)
			{
				interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, chosenFaction.characters[0], "join_faction_normal");
			}
			else
			{
				interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, this, "join_faction_normal", null, chosenFaction.persistentID ?? "");
			}
			return chosenFaction;
		}
		return null;
	}

	private void ReligiousCultJoinFactionProcessing(RELIGION religion, out Faction chosenFaction)
	{
		string cultistTraitNameForReligion = religion.GetCultistTraitNameForReligion();
		FACTION_TYPE factionTypeForReligion = religion.GetFactionTypeForReligion();
		chosenFaction = null;
		if (traitContainer.HasTrait(cultistTraitNameForReligion))
		{
			Faction religiousCultFactionForReligion = FactionManager.Instance.GetReligiousCultFactionForReligion(religion);
			if (religiousCultFactionForReligion != null && religiousCultFactionForReligion.factionType.type == factionTypeForReligion && religiousCultFactionForReligion != prevFaction && !religiousCultFactionForReligion.isDisbanded && !religiousCultFactionForReligion.IsCharacterBannedFromJoining(this) && religiousCultFactionForReligion.ideologyComponent.DoesCharacterFitCurrentIdeologies(this))
			{
				chosenFaction = religiousCultFactionForReligion;
			}
		}
	}

	private void OnFactionCreated(Faction newFaction, Character creator)
	{
		if (this != creator && (faction == null || faction.isMajorNonPlayerOrBandits || faction == FactionManager.Instance.vagrantFaction))
		{
			JoinFactionProcessingSpecific(newFaction, creator);
		}
	}

	private void JoinFactionProcessingSpecific(Faction faction, Character factionLeader)
	{
		if (isFactionLeader || isSettlementRuler || partyComponent.isMemberThatJoinedQuest || (faction.factionType.type == FACTION_TYPE.Bandits && !crimeComponent.HasWantedCrimeType(CRIME_TYPE.Theft, CRIME_TYPE.Murder, CRIME_TYPE.Assault, CRIME_TYPE.Arson)) || !faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(this))
		{
			return;
		}
		Faction faction2 = this.faction;
		if (faction2 != faction)
		{
			Character firstCharacterWithRelationship = base.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
			bool flag = firstCharacterWithRelationship == null || !base.relationshipContainer.IsFriendsWith(firstCharacterWithRelationship);
			bool flag2 = base.relationshipContainer.IsFriendsWith(factionLeader);
			int num = 0;
			RELIGION p_religion;
			if (firstCharacterWithRelationship == factionLeader && flag2)
			{
				num += 90;
			}
			else if (traitContainer.IsReligiousCultist(out p_religion) && p_religion.GetFactionTypeForReligion() == faction.factionType.type)
			{
				num += 50;
			}
			else if (base.relationshipContainer.HasRelationshipWith(factionLeader, RELATIONSHIP_TYPE.AFFAIR) && flag2 && flag)
			{
				num += 60;
			}
			else if (base.relationshipContainer.IsFamilyMember(factionLeader) && flag2)
			{
				num += 30;
			}
			else if (flag2)
			{
				num += 10;
			}
			else if (isVagrantOrFactionless && race.IsSapient())
			{
				num += 15;
			}
			else if (faction.factionType.type == FACTION_TYPE.Bandits && crimeComponent.HasWantedCrimeType(CRIME_TYPE.Theft, CRIME_TYPE.Murder, CRIME_TYPE.Assault, CRIME_TYPE.Arson))
			{
				num += 50;
			}
			if (faction2 != null && faction2.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.Hostile, faction))
			{
				num = Mathf.RoundToInt((float)num * 0.5f);
			}
			if (GameUtilities.RollChance(num))
			{
				interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, factionLeader, "join_faction_decision");
			}
		}
	}

	public bool ChangeToDefaultFaction()
	{
		if (minion != null)
		{
			return ChangeFactionTo(PlayerManager.Instance.player.playerFaction, bypassIdeologyChecking: true);
		}
		if (IsUndead() || necromancerTrait != null)
		{
			return ChangeFactionTo(FactionManager.Instance.undeadFaction, bypassIdeologyChecking: true);
		}
		if (this is Summon summon)
		{
			return ChangeFactionTo(summon.defaultFaction, bypassIdeologyChecking: true);
		}
		return ChangeFactionTo(FactionManager.Instance.vagrantFaction, bypassIdeologyChecking: true);
	}

	private void UpdateFactionIdeologiesCrimesOnFactionLeaderUpdatedTraits(Trait p_trait)
	{
		if (!isFactionLeader)
		{
			return;
		}
		if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges)
		{
			FactionManager.Instance.RerollPeaceTypeIdeology(faction, this);
			FactionManager.Instance.RerollInclusiveTypeIdeology(faction, this, shouldRerollChanceBased: false);
			FactionManager.Instance.RerollReligionTypeIdeology(faction, this);
			FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, this, p_trait);
			List<Character> list = RuinarchListPool<Character>.Claim();
			list.AddRange(faction.characters);
			list.Remove(this);
			for (int i = 0; i < list.Count; i++)
			{
				Character character = list[i];
				faction.CheckIfCharacterStillFitsIdeology(character);
			}
			RuinarchListPool<Character>.Release(list);
			Messenger.Broadcast(FactionSignals.FACTION_IDEOLOGIES_CHANGED, faction);
		}
		FactionManager.Instance.RevalidateFactionCrimes(faction, this);
		Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, faction);
	}

	public void CarryPOI(IPointOfInterest poi, bool changeOwnership = false, bool setOwnership = true)
	{
		if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			carryComponent.CarryPOI(poi);
		}
		else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			PickUpItem(poi as TileObject, changeOwnership, setOwnership);
		}
	}

	public bool IsPOICarriedOrInInventory(IPointOfInterest poi)
	{
		if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			return HasItemOrEquipment(poi as TileObject);
		}
		return carryComponent.IsPOICarried(poi);
	}

	public bool IsPOICarriedOrInInventory(string poiName)
	{
		if (!HasItem(poiName))
		{
			return carryComponent.IsPOICarried(poiName);
		}
		return true;
	}

	public void UncarryPOI(IPointOfInterest poi, bool bringBackToInventory = false, bool addToLocation = true, LocationGridTile dropLocation = null, bool isUncarriedFromSeize = false)
	{
		if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			carryComponent.UncarryPOI(poi, addToLocation, dropLocation);
		}
		else
		{
			if (poi.poiType != POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				return;
			}
			TileObject item = poi as TileObject;
			carryComponent.UncarryPOI(poi, addToLocation: false);
			if (!bringBackToInventory)
			{
				if (addToLocation)
				{
					DropItem(item, dropLocation, isUncarriedFromSeize);
				}
				else
				{
					UnobtainItem(item);
				}
			}
		}
	}

	public void UncarryPOI(bool bringBackToInventory = false, bool addToLocation = true, LocationGridTile dropLocation = null, bool isUncarriedFromSeize = false)
	{
		if (carryComponent.isCarryingAnyPOI)
		{
			IPointOfInterest carriedPOI = carryComponent.carriedPOI;
			UncarryPOI(carriedPOI, bringBackToInventory, addToLocation, dropLocation, isUncarriedFromSeize);
		}
	}

	public void ShowItemVisualCarryingPOI(TileObject item)
	{
		if (HasItemOrEquipment(item) && item != carryComponent.carriedPOI && item != carryComponent.carriedPOI)
		{
			carryComponent.CarryPOI(item);
		}
	}

	public void SetCurrentStructureLocation(LocationStructure newStructure, bool broadcast = true)
	{
		if (newStructure == _currentStructure)
		{
			return;
		}
		LocationStructure locationStructure = _currentStructure;
		_currentStructure = newStructure;
		if (!broadcast)
		{
			return;
		}
		if (newStructure != null)
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, this, newStructure);
			eventDispatcher.ExecuteCharacterArrivedAtStructure(this, newStructure);
			LocationAwarenessUtility.RemoveFromAwarenessList(this);
			LocationAwarenessUtility.AddToAwarenessList(this, gridTileLocation);
		}
		if (locationStructure != null)
		{
			eventDispatcher.ExecuteCharacterLeftStructure(this, locationStructure);
			Messenger.Broadcast(CharacterSignals.CHARACTER_LEFT_STRUCTURE, this, locationStructure);
			if (newStructure == null && currentLocationAwareness == locationStructure.locationAwareness)
			{
				LocationAwarenessUtility.RemoveFromAwarenessList(this);
			}
		}
	}

	public void SetGridTileLocation(LocationGridTile tile)
	{
	}

	public LocationGridTile GetNearestUnoccupiedTileFromThis()
	{
		LocationGridTile result = null;
		if (!isDead && gridTileLocation != null)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			gridTileLocation.PopulateUnoccupiedNeighbours(list, sameStructureOnly: true);
			if (list.Count > 0)
			{
				result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		return result;
	}

	public LocationGridTile GetNearestUnoccupiedEdgeTileFromThis()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile.IsAtEdgeOfWalkableMap() && locationGridTile.structure != null)
		{
			return locationGridTile;
		}
		LocationGridTile locationGridTile2 = null;
		List<LocationGridTile> neighbourList = gridTileLocation.neighbourList;
		for (int i = 0; i < neighbourList.Count; i++)
		{
			if (neighbourList[i].IsAtEdgeOfWalkableMap() && neighbourList[i].structure != null)
			{
				locationGridTile2 = neighbourList[i];
				break;
			}
		}
		if (locationGridTile2 == null)
		{
			float num = -999f;
			for (int j = 0; j < gridTileLocation.parentMap.allEdgeTiles.Count; j++)
			{
				LocationGridTile locationGridTile3 = gridTileLocation.parentMap.allEdgeTiles[j];
				float num2 = Vector2.Distance(locationGridTile3.localLocation, locationGridTile.localLocation);
				if ((num == -999f || num2 < num) && locationGridTile3.structure != null)
				{
					locationGridTile2 = locationGridTile3;
					num = num2;
				}
			}
		}
		return locationGridTile2;
	}

	public void SetRegionLocation(Region region)
	{
		_currentRegion = region;
	}

	public bool IsInHomeSettlement()
	{
		if (homeSettlement != null)
		{
			return currentSettlement == homeSettlement;
		}
		return false;
	}

	public bool HasHome()
	{
		if (homeSettlement == null && (homeStructure == null || homeStructure.hasBeenDestroyed))
		{
			return HasTerritory();
		}
		return true;
	}

	public bool IsAtHome()
	{
		if (homeSettlement != null)
		{
			return IsInHomeSettlement();
		}
		if (homeStructure != null)
		{
			return isAtHomeStructure;
		}
		if (territory != null)
		{
			return IsInTerritory();
		}
		return false;
	}

	public int GetAliveResidentsCountInHome()
	{
		int result = 0;
		if (homeSettlement != null)
		{
			result = homeSettlement.residents.Count((Character x) => !x.isDead);
		}
		else if (homeStructure != null)
		{
			result = homeStructure.residents.Count((Character x) => !x.isDead);
		}
		else if (HasTerritory())
		{
			result = territory.region.GetCountOfAliveCharacterWithSameTerritory(this);
		}
		return result;
	}

	public bool IsUndead()
	{
		if (characterClass.IsZombie() || (this is Summon summon && (summon.summonType == SUMMON_TYPE.Ghost || summon.summonType == SUMMON_TYPE.Skeleton || summon.summonType == SUMMON_TYPE.Vengeful_Ghost || summon.summonType == SUMMON_TYPE.Revenant || summon.summonType == SUMMON_TYPE.Ghoul || summon.summonType == SUMMON_TYPE.Whisperer)))
		{
			return true;
		}
		return false;
	}

	public bool AssignRace(RACE race, bool isInitial = false)
	{
		if (_raceSetting == null || _raceSetting.race != race)
		{
			if (_raceSetting != null)
			{
				if (_raceSetting.race == race)
				{
					return false;
				}
				for (int i = 0; i < _raceSetting.traitNames.Length; i++)
				{
					traitContainer.RemoveTrait(this, _raceSetting.traitNames[i]);
				}
			}
			_raceSetting = RaceManager.Instance.GetRaceData(race);
			if (!isInitial)
			{
				OnUpdateRace();
				Messenger.Broadcast(CharacterSignals.CHARACTER_CHANGED_RACE, this);
			}
			return true;
		}
		return false;
	}

	protected void OnUpdateRace()
	{
		combatComponent.UpdateBasicData(resetHP: false);
		needsComponent.UpdateBaseStaminaDecreaseRate();
		for (int i = 0; i < _raceSetting.traitNames.Length; i++)
		{
			traitContainer.AddTrait(this, _raceSetting.traitNames[i]);
		}
		visuals.UpdateAllVisuals(this, regeneratePortrait: true);
		if (race == RACE.SKELETON)
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.DRINK_BLOOD);
		}
		else
		{
			AddAdvertisedAction(INTERACTION_TYPE.DRINK_BLOOD);
		}
		if (race.IsSapient())
		{
			AddAdvertisedAction(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE);
			AddAdvertisedAction(INTERACTION_TYPE.SHARE_INFORMATION);
			AddAdvertisedAction(INTERACTION_TYPE.REPORT_CRIME);
			AddAdvertisedAction(INTERACTION_TYPE.REPORT_MURDER);
			AddAdvertisedAction(INTERACTION_TYPE.REPORT_ABDUCT);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE);
			RemoveAdvertisedAction(INTERACTION_TYPE.SHARE_INFORMATION);
			RemoveAdvertisedAction(INTERACTION_TYPE.REPORT_CRIME);
			RemoveAdvertisedAction(INTERACTION_TYPE.REPORT_MURDER);
			RemoveAdvertisedAction(INTERACTION_TYPE.REPORT_ABDUCT);
		}
	}

	public void SetRandomName()
	{
		string firstName = RandomNameGenerator.GenerateRandomName(race, gender);
		SetFirstName(firstName);
	}

	public void SetFirstName(string p_firstName)
	{
		_name = p_firstName;
		RandomNameGenerator.RemoveNameAsAvailable(gender, race, _name);
	}

	public void RenameCharacter(string p_firstName)
	{
		ResetUIString();
		SetFirstName(p_firstName);
		UpdateCurrentLogsBasedOnUpdatedCharacter(this);
		bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		if (lycanData?.limboForm != null)
		{
			lycanData.limboForm.SetFirstName(_name);
			lycanData.limboForm.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(lycanData.limboForm);
		}
		Messenger.Broadcast(CharacterSignals.CHARACTER_CHANGED_NAME, this);
	}

	private string GetDefaultRaceClassName()
	{
		CharacterClass characterClass = this.characterClass;
		if (race == RACE.DEMON)
		{
			return characterClass.displayName + " " + GameUtilities.GetNormalizedRaceAdjective(race);
		}
		return characterClass.displayName;
	}

	public void CenterOnCharacter()
	{
		if (isInLimbo)
		{
			if (isLycanthrope && lycanData.activeForm != this)
			{
				lycanData.activeForm.CenterOnCharacter();
			}
		}
		else if (grave != null && (grave.gridTileLocation != null || grave.isBeingCarriedBy != null))
		{
			Region region = null;
			GameObject gameObject = null;
			if (grave.isBeingCarriedBy != null)
			{
				region = grave.isBeingCarriedBy.currentRegion;
				if (grave.isBeingCarriedBy.hasMarker)
				{
					gameObject = grave.isBeingCarriedBy.marker.gameObject;
				}
			}
			else if (grave.gridTileLocation != null)
			{
				region = grave.gridTileLocation.parentMap.region;
				if (grave.mapObjectVisual != null)
				{
					gameObject = grave.mapObjectVisual.gameObject;
				}
			}
			if (region != null && gameObject != null)
			{
				if (!InnerMapManager.Instance.IsShowingInnerMap(region))
				{
					InnerMapManager.Instance.ShowInnerMap(region, centerCameraOnMapCenter: false);
				}
				InnerMapCameraMove.Instance.CenterCameraOn(gameObject);
			}
		}
		else if ((bool)marker && carryComponent.masterCharacter.gridTileLocation != null)
		{
			bool flag = !InnerMapManager.Instance.IsShowingInnerMap(currentRegion);
			if (flag)
			{
				InnerMapManager.Instance.ShowInnerMap(carryComponent.masterCharacter.gridTileLocation.structure.region, centerCameraOnMapCenter: false);
			}
			InnerMapCameraMove.Instance.CenterCameraOn(marker.gameObject, flag);
		}
	}

	private void OnCharacterDied(Character characterThatDied)
	{
		if (!characterThatDied.persistentID.Equals(persistentID))
		{
			if (!isDead)
			{
				if (characterThatDied.currentRegion == homeRegion && IsHostileWith(characterThatDied))
				{
					needsComponent.AdjustHope(5f);
				}
				if ((bool)marker)
				{
					marker.OnOtherCharacterDied(characterThatDied);
				}
			}
		}
		else
		{
			crimeComponent.OnCharacterDied();
		}
	}

	private void OnBeforeSeizingPOI(IPointOfInterest poi)
	{
		if (poi is TileObject tileObject)
		{
			OnBeforeSeizingTileObject(tileObject);
		}
	}

	public void OnSeizePOI(IPointOfInterest poi)
	{
		if (poi != this && hasMarker && marker.IsTargetPOIInPathfinding(poi) && marker.hasFleePath)
		{
			marker.SetHasFleePath(state: false);
			marker.pathfindingAI.ClearAllCurrentPathData();
			combatComponent.SetWillProcessCombat(state: true);
		}
		if (poi is Character character)
		{
			OnSeizeCharacter(character);
		}
		else if (poi is TileObject tileObject)
		{
			OnSeizeTileObject(tileObject);
		}
	}

	private void OnSeizeCharacter(Character character)
	{
		if (character != this)
		{
			OnSeizeOtherCharacter(character);
		}
	}

	private void OnSeizeOtherCharacter(Character character)
	{
		combatComponent.RemoveHostileInRange(character);
		combatComponent.RemoveAvoidInRange(character);
	}

	private void OnBeforeSeizingTileObject(TileObject tileObject)
	{
	}

	private void OnSeizeTileObject(TileObject tileObject)
	{
		if (currentActionNode != null && currentActionNode.poiTarget == tileObject)
		{
			StopCurrentActionNode();
		}
		Character[] users = tileObject.users;
		if (users == null || users.Length == 0)
		{
			return;
		}
		for (int i = 0; i < users.Length; i++)
		{
			Character character = users[i];
			if (character != null)
			{
				character.StopCurrentActionNode();
				if (tileObject.RemoveUser(character))
				{
					i--;
				}
			}
		}
	}

	public void AdjustDoNotRecoverHP(int amount)
	{
		doNotRecoverPassiveHP += amount;
		doNotRecoverPassiveHP = Math.Max(doNotRecoverPassiveHP, 0);
	}

	public override string ToString()
	{
		return name;
	}

	private LocationGridTile GetLocationGridTileByXY(int x, int y, bool throwOnException = true)
	{
		if (currentRegion != null)
		{
			if (Utilities.IsInRange(x, 0, currentRegion.innerMap.width) && Utilities.IsInRange(y, 0, currentRegion.innerMap.height))
			{
				return currentRegion.innerMap.map[x, y];
			}
			int num = Mathf.Clamp(x, 0, currentRegion.innerMap.width - 1);
			int num2 = Mathf.Clamp(y, 0, currentRegion.innerMap.height - 1);
			return currentRegion.innerMap.map[num, num2];
		}
		return null;
	}

	public void UpdateCanCombatState()
	{
		bool flag = traitContainer.HasTrait("Combatant") && !traitContainer.HasTrait("Injured");
		if (canPersonalPatrol != flag)
		{
			canPersonalPatrol = flag;
			if (!canPersonalPatrol)
			{
				Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERSONAL_PATROL, this);
			}
		}
	}

	private bool CanCharacterReact(IPointOfInterest targetPOI = null)
	{
		if (!limiterComponent.canWitness || !limiterComponent.canPerform)
		{
			return false;
		}
		if (interruptComponent.isInterrupted)
		{
			return false;
		}
		if (!isNormalCharacter)
		{
			return false;
		}
		if (stateComponent.currentState != null && !stateComponent.currentState.isDone && stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT)
		{
			return false;
		}
		if (targetPOI != null && targetPOI is Character)
		{
			Character character = targetPOI as Character;
			if (character.faction != null && character.faction.IsHostileWith(faction))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsAble()
	{
		if (!isDead)
		{
			return !characterClass.IsZombie();
		}
		return false;
	}

	public void SetTileObjectLocation(TileObject tileObject)
	{
		tileObjectLocation = tileObject;
	}

	public void AdjustNumOfNonSecretActionsBeingPerformedOnThis(int amount)
	{
		numOfNonSecretActionsBeingPerformedOnThis += amount;
		numOfNonSecretActionsBeingPerformedOnThis = Mathf.Max(0, numOfNonSecretActionsBeingPerformedOnThis);
		if ((bool)marker)
		{
			marker.UpdateAnimation();
		}
	}

	public virtual bool IsValidCombatTargetFor(IPointOfInterest source)
	{
		if (!isDead && hasMarker && gridTileLocation != null && source.gridTileLocation != null)
		{
			if (source is Character character)
			{
				return character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation);
			}
			return false;
		}
		return false;
	}

	public void SetHasUnresolvedCrime(bool state)
	{
		hasUnresolvedCrime = state;
	}

	public void SetIsInLimbo(bool state)
	{
		isInLimbo = state;
	}

	public void SetIsLimboCharacter(bool state)
	{
		isLimboCharacter = state;
	}

	public void SetDestroyMarkerOnDeath(bool state)
	{
		destroyMarkerOnDeath = state;
	}

	public void SetIsWanderer(bool state)
	{
		if (isWanderer == state)
		{
			return;
		}
		isWanderer = state;
		if (isWanderer)
		{
			behaviourComponent.ChangeDefaultBehaviourSet("Default Wanderer Behaviour");
			return;
		}
		if (HasTerritory())
		{
			ClearTerritory();
		}
		behaviourComponent.ChangeDefaultBehaviourSet("Default Resident Behaviour");
	}

	public void SetHasBeenRaisedFromDead(bool state)
	{
		hasBeenRaisedFromDead = state;
	}

	public bool IsConsideredInDangerBy(Character character)
	{
		if (traitContainer.HasTrait("Enslaved") && faction != character.faction)
		{
			return true;
		}
		if (traitContainer.HasTrait("Restrained", "Ensnared", "Frozen"))
		{
			return !IsAtHome();
		}
		if (HasHome() && !IsAtHome())
		{
			return !movementComponent.CanReturnHome();
		}
		return false;
	}

	private string GetFirstNameWithColor()
	{
		if (CharacterManager.Instance != null)
		{
			string characterNameColorHex = CharacterManager.Instance.GetCharacterNameColorHex(this);
			return "<color=#" + characterNameColorHex + ">" + _name + "</color>";
		}
		return _name;
	}

	public bool IsRatmanThatIsPartOfMajorFaction()
	{
		if (race == RACE.RATMAN && faction != null)
		{
			return faction.isMajorNonPlayer;
		}
		return false;
	}

	public string GetCultistUnableToDoJobReason(GoapPlanJob job, Precondition failedPrecondition, INTERACTION_TYPE failedPreconditionActionType)
	{
		string text = job.GetJobDetailString();
		if (failedPrecondition != null && failedPrecondition.goapEffect.conditionType == GOAP_EFFECT_CONDITION.TAKE_POI)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "CancelReasons_Table", "Cultist_Unable");
			string value = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", failedPrecondition.goapEffect.conditionKey);
			if (string.IsNullOrEmpty(value))
			{
				value = failedPrecondition.goapEffect.conditionKey;
			}
			log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
			log.AddToFillers(null, failedPreconditionActionType.LocalizedActionName(), LOG_IDENTIFIER.STRING_2);
			string logText = log.logText;
			LogPool.Release(log);
			text = text + ", " + logText;
		}
		return text;
	}

	public void LogUnableToDoJob(string reason)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "cancel_job_no_plan", LOG_TAG.Work, null);
		log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
		logComponent.RegisterLog(log, releaseAfter: true);
	}

	public void SetDeployedAtStructure(LocationStructure p_structure)
	{
		deployedAtStructure = p_structure;
	}

	public bool IsTargetOfCurrentAction(IPointOfInterest poi)
	{
		if (currentActionNode != null)
		{
			return currentActionNode.poiTarget == poi;
		}
		return false;
	}

	public void SetIsRecruitedByAMajorFaction(bool p_state)
	{
		isRecruitedByAMajorFaction = p_state;
	}

	public virtual void OnActionPerformed(ActualGoapNode node)
	{
		if (!isDead && limiterComponent.canWitness && node.actor != this && (bool)marker)
		{
			if (marker.IsPOIInVision(node.actor))
			{
				marker.AddUnprocessedAction(node);
			}
			else if (marker.IsPOIInVision(node.poiTarget))
			{
				marker.AddUnprocessedAction(node);
			}
		}
	}

	public virtual void OnInterruptStarted(InterruptHolder interruptHolder)
	{
		if (!isDead && limiterComponent.canWitness && interruptHolder.actor != this && (bool)marker && marker.IsPOIInVision(interruptHolder.actor))
		{
			marker.AddUnprocessedPOI(interruptHolder.actor, reactToInterruptOnly: true);
		}
	}

	public void ThisCharacterSaw(IPointOfInterest target, bool reactToActionOnly = false)
	{
		IPointOfInterest targetPOI = target;
		Character character = null;
		Character character2 = null;
		if (target is Character character3)
		{
			character2 = character3;
			if (character2.reactionComponent.disguisedCharacter != null)
			{
				character = character2.reactionComponent.disguisedCharacter;
			}
		}
		if (character != null)
		{
			targetPOI = character;
		}
		List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions("See_Poi_Cannot_Witness_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].OnSeePOIEvenCannotWitness(targetPOI, this);
			}
		}
		if (!limiterComponent.canWitness)
		{
			return;
		}
		ActualGoapNode actualGoapNode = null;
		bool flag = false;
		if (character2 != null)
		{
			if (character2.interruptComponent.isInterrupted)
			{
				reactionComponent.ReactTo(character2.interruptComponent.currentInterrupt, REACTION_STATUS.WITNESSED);
			}
			else
			{
				actualGoapNode = character2.currentActionNode;
				if (actualGoapNode != null && actualGoapNode.actionStatus != ACTION_STATUS.STARTED && actualGoapNode.actionStatus != ACTION_STATUS.NONE && actualGoapNode.actor != this)
				{
					flag = true;
					actualGoapNode.IncreaseReactionCounter();
					reactionComponent.ReactTo(actualGoapNode, REACTION_STATUS.WITNESSED);
				}
			}
		}
		if (target.allJobsTargetingThis.Count > 0)
		{
			for (int j = 0; j < target.allJobsTargetingThis.Count; j++)
			{
				if (!(target.allJobsTargetingThis[j] is GoapPlanJob))
				{
					continue;
				}
				GoapPlan assignedPlan = (target.allJobsTargetingThis[j] as GoapPlanJob).assignedPlan;
				if (assignedPlan != null)
				{
					ActualGoapNode currentActualNode = assignedPlan.currentActualNode;
					if (currentActualNode.actionStatus != ACTION_STATUS.STARTED && currentActualNode.actionStatus != ACTION_STATUS.NONE && currentActualNode != actualGoapNode && currentActualNode.actor != this)
					{
						currentActualNode.IncreaseReactionCounter();
						reactionComponent.ReactTo(currentActualNode, REACTION_STATUS.WITNESSED);
						currentActualNode.DecreaseReactionCounter();
					}
				}
			}
		}
		if (flag)
		{
			actualGoapNode?.DecreaseReactionCounter();
		}
		if (!reactToActionOnly)
		{
			string debugLog = string.Empty;
			if (character != null)
			{
				reactionComponent.ReactToDisguised(target as Character, character, ref debugLog);
			}
			else
			{
				reactionComponent.ReactTo(target, ref debugLog);
			}
			string.IsNullOrEmpty(debugLog);
		}
	}

	public void ThisCharacterSawAction(ActualGoapNode action)
	{
		if (limiterComponent.canWitness)
		{
			reactionComponent.ReactTo(action, REACTION_STATUS.WITNESSED);
		}
	}

	public void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, ref string attackSummary)
	{
		if (characterThatAttacked != null)
		{
			OnHitByAttackFrom(characterThatAttacked, combatStateOfAttacker, characterThatAttacked.combatComponent.attack, characterThatAttacked.combatComponent.currentElement.type, ref attackSummary);
		}
	}

	public void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, int p_attackPower, ELEMENTAL_TYPE p_elementType, ref string attackSummary)
	{
		if (mountComponent.IsMounting())
		{
			mountComponent.mountedCharacter.OnHitByAttackFrom(characterThatAttacked, combatStateOfAttacker, ref attackSummary);
		}
		else
		{
			if (characterThatAttacked == null || !HasHealth())
			{
				return;
			}
			Character character = mountComponent.GetRider();
			if (character == null)
			{
				character = this;
			}
			CombatData combatData = characterThatAttacked.combatComponent.GetCombatData(character);
			bool flag = true;
			ActualGoapNode attackerActionTowardsTarget = null;
			if (combatData != null)
			{
				flag = combatData.isLethal;
				attackerActionTowardsTarget = combatData.connectedAction;
			}
			bool flag2 = characterThatAttacked.faction != null && characterThatAttacked.faction.isPlayerFaction;
			if (!flag2 && characterThatAttacked.partyComponent.isMemberThatJoinedQuest && characterThatAttacked.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Demon_Raid)
			{
				flag2 = true;
			}
			int num = 0;
			num = ((character.limiterComponent.canPerform && character.limiterComponent.canMove) ? GetChanceToBeKnockedOutBy(characterThatAttacked, ref attackSummary) : 50);
			if (characterThatAttacked.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Snatcher))
			{
				num += Mathf.CeilToInt((float)num * 0.5f);
			}
			if (race == RACE.ANGEL)
			{
				num = Mathf.RoundToInt((float)num * 0.35f);
			}
			num = ProcessKnockoutChanceThroughPiercingAndResistances(num, characterThatAttacked, this, ref attackSummary);
			if (minion != null)
			{
				num = Mathf.RoundToInt((float)num / 3f);
			}
			int num2 = currentHP;
			int attackWithCritAndModifications = characterThatAttacked.combatComponent.GetAttackWithCritAndModifications(this, p_attackPower);
			AdjustHP(-attackWithCritAndModifications, p_elementType, triggerDeath: false, characterThatAttacked, null, showHPBar: true, characterThatAttacked.piercingAndResistancesComponent.piercingPower, flag2);
			characterThatAttacked.combatComponent.ResetDamageDoneType();
			bool didMinimalDamage = num2 - currentHP == 1;
			if (!HasHealth())
			{
				AgitateData obj = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.AGITATE) as AgitateData;
				bool flag3 = obj != null && obj.currentLevel >= 3 && traitContainer.HasTrait("Agitated");
				if (!flag && !traitContainer.HasTrait("Sturdy") && !flag3)
				{
					if (character.traitContainer.HasTrait("Unconscious"))
					{
						AlreadyUnconsciousCharacterIsAttacked(character);
					}
					else
					{
						NotUnconsciousCharacterIsAttacked(characterThatAttacked, character);
					}
				}
				else if (!isDead)
				{
					ActualGoapNode actualGoapNode = ((!race.IsSapient()) ? InteractionManager.Instance.CreateNewIllusionAction(characterThatAttacked, this, INTERACTION_TYPE.MURDER, "Slay Success", shouldLog: true) : InteractionManager.Instance.CreateNewIllusionAction(characterThatAttacked, this, INTERACTION_TYPE.MURDER, shouldLog: true));
					Log descriptionLog = actualGoapNode.descriptionLog;
					string cause = "attacked";
					if (!flag && !traitContainer.HasTrait("Sturdy") && !flag3)
					{
						cause = "accidental_attacked";
					}
					if (characterThatAttacked.partyComponent.isMemberThatJoinedQuest && characterThatAttacked.partyComponent.currentParty.currentQuest is HuntBeastPartyQuest huntBeastPartyQuest && this is Summon p_monster && (huntBeastPartyQuest.targetStructure?.GetFirstTileWithObject())?.tileObjectComponent.objHere is AnimalBurrow animalBurrow && animalBurrow.HasDeadOrAliveSpawnedMonster(p_monster))
					{
						huntBeastPartyQuest.SetIsSuccessful(state: true);
					}
					Death(cause, null, characterThatAttacked, descriptionLog, null, null, null, flag2, characterThatAttacked, p_elementType);
					if (characterThatAttacked.classComponent.hunterKillingSpecialization == CHARACTER_CATEGORY.None && characterThatAttacked.characterClass.className == "Hunter" && !isNormalCharacter)
					{
						characterThatAttacked.interruptComponent.TriggerInterrupt(INTERRUPT.Hunter_Specialization, this);
					}
					actualGoapNode.ProcessReturnToPool();
				}
			}
			else if (character.traitContainer.HasTrait("Unconscious"))
			{
				AlreadyUnconsciousCharacterIsAttacked(character);
			}
			else if (GameUtilities.RollChance(num, ref attackSummary))
			{
				AgitateData obj2 = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.AGITATE) as AgitateData;
				bool flag4 = obj2 != null && obj2.currentLevel >= 3 && traitContainer.HasTrait("Agitated");
				if (!flag && !traitContainer.HasTrait("Sturdy") && !flag4)
				{
					NotUnconsciousCharacterIsAttacked(characterThatAttacked, character);
				}
			}
			if (characterThatAttacked.hasMarker && combatStateOfAttacker != null)
			{
				for (int i = 0; i < characterThatAttacked.marker.inVisionCharacters.Count; i++)
				{
					Character character2 = characterThatAttacked.marker.inVisionCharacters[i];
					character2.needsComponent.WakeUpFromNoise();
					if (character2.limiterComponent.canPerform && character2.limiterComponent.canMove)
					{
						character2.reactionComponent.ReactToCombat(combatStateOfAttacker, this);
					}
				}
			}
			if (characterThatAttacked.traitContainer.HasTrait("Plagued"))
			{
				Transmission<CombatRateTransmission>.Instance.Transmit(characterThatAttacked, this, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Combat));
			}
			if (characterThatAttacked.equipmentComponent.currentWeapon is WeaponItem weaponItem)
			{
				weaponItem.ApplyWeaponEffectsOnHit(this);
			}
			Messenger.Broadcast(CharacterSignals.CHARACTER_WAS_HIT, this, characterThatAttacked);
			AfterBeingHitBy(characterThatAttacked, character, didMinimalDamage, attackerActionTowardsTarget, flag);
		}
	}

	private void AlreadyUnconsciousCharacterIsAttacked(Character p_target)
	{
		if (!HasHealth())
		{
			int hP = Mathf.CeilToInt((float)maxHP * 0.1f);
			SetHP(hP);
		}
		if (p_target != this && !p_target.HasHealth())
		{
			int hP2 = Mathf.CeilToInt((float)p_target.maxHP * 0.1f);
			p_target.SetHP(hP2);
		}
	}

	private void NotUnconsciousCharacterIsAttacked(Character p_attacker, Character p_target)
	{
		p_target.traitContainer.AddTrait(p_target, "Unconscious", p_attacker);
		if (p_target.traitContainer.HasTrait("Unconscious") && p_attacker.partyComponent.hasParty && p_attacker.partyComponent.currentParty.currentQuest is DemonRaidPartyQuest { raidType: DEMON_RAID_TYPE.Harass_Villagers } && !p_target.characterClass.IsCombatant())
		{
			p_target.traitContainer.AddTrait(p_target, "Traumatized");
		}
		if (p_target != this && !HasHealth())
		{
			int hP = Mathf.CeilToInt((float)maxHP * 0.1f);
			SetHP(hP);
		}
	}

	private void AfterBeingHitBy(Character characterThatAttacked, Character p_target, bool didMinimalDamage, ActualGoapNode attackerActionTowardsTarget, bool isAttackLethal)
	{
		if (didMinimalDamage)
		{
			if (characterThatAttacked.stateComponent.currentState is CombatState combatState && combatState.currentClosestHostile == this)
			{
				Messenger.Broadcast(CharacterSignals.DETERMINE_COMBAT_REACTION, characterThatAttacked);
			}
		}
		else
		{
			if (traitContainer.HasTrait("Thorns"))
			{
				traitContainer.GetTraitOrStatus<Thorns>("Thorns").ReflectDamage(characterThatAttacked, this, isAttackLethal);
			}
			if (!isDead && characterClass.className == "Barbarian" && traitContainer.GetStacks("Enraged") < 5)
			{
				traitContainer.AddTrait(this, "Enraged");
			}
		}
		if (traitContainer.HasTrait("Unconscious"))
		{
			traitContainer.RemoveStatusAndStacks(this, "Burning");
			traitContainer.RemoveStatusAndStacks(this, "Poisoned");
		}
		if (p_target != this && p_target.traitContainer.HasTrait("Unconscious"))
		{
			p_target.traitContainer.RemoveStatusAndStacks(p_target, "Burning");
			p_target.traitContainer.RemoveStatusAndStacks(p_target, "Poisoned");
		}
	}

	private int GetChanceToBeKnockedOutBy(Character p_attacker, ref string p_attackSummary)
	{
		int num = 0;
		float num2 = (float)currentHP / (float)maxHP * 100f;
		p_attackSummary += $"\nDefender Percent HP: {num2}%";
		float num3 = (100f - num2) * 0.35f + 1f;
		if (!characterClass.IsCombatant())
		{
			num3 += 10f;
		}
		if (num3 > 0f)
		{
			num = Mathf.RoundToInt(num3);
		}
		p_attackSummary += $"\nKnockout Chance: {num}%";
		return num;
	}

	private int ProcessKnockoutChanceThroughPiercingAndResistances(int p_currentKnockoutChance, Character p_attacker, Character p_target, ref string attackSummary)
	{
		attackSummary = attackSummary + "\nWill process knockout chance of combat between attacker:" + p_attacker.name + " and defender:" + p_target.name + " through piercing";
		float resistanceValue = p_target.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Mental);
		float resistanceValue2 = p_target.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Physical);
		float num = resistanceValue + resistanceValue2;
		int num2 = Mathf.RoundToInt(num / 5f);
		attackSummary = attackSummary + "\n\tTotal resistance of " + p_target.name + " is " + num + ". Chance Reduction is " + num2;
		float piercingPower = p_attacker.piercingAndResistancesComponent.piercingPower;
		int num3 = Mathf.RoundToInt(piercingPower / 4f);
		attackSummary = attackSummary + "\n\tTotal piercing of " + p_attacker.name + " is " + piercingPower + ". Chance Increase is " + num3;
		int num4 = p_currentKnockoutChance;
		num4 += num3;
		num4 -= num2;
		attackSummary = $"{attackSummary}\n\tComputed chance is {num4}";
		num4 = Mathf.Clamp(num4, 1, 50);
		attackSummary = $"{attackSummary}\n\tFinal value after Min/Max is {num4}";
		return num4;
	}

	private Character GetCharacterResponsibleForUnconsciousness(Character characterThatAttacked, CombatState combatStateOfAttacker)
	{
		Character result = null;
		if (combatStateOfAttacker != null && combatStateOfAttacker.currentClosestHostile == this)
		{
			result = characterThatAttacked;
		}
		return result;
	}

	public void ResetToFullHP()
	{
		SetHP(maxHP);
	}

	public void SetHP(int amount)
	{
		currentHP = amount;
	}

	public void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		if (mountComponent.IsMounting() && amount < 0)
		{
			Character mountedCharacter = mountComponent.mountedCharacter;
			if (mountedCharacter != null)
			{
				mountedCharacter.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar, piercingPower, isPlayerSource, isTrueDamage);
				return;
			}
		}
		if (faction != null && faction.isPlayerFaction && CombatManager.Instance.IsDamageSourceFromPlayerSpell(source))
		{
			return;
		}
		bool flag = source is BeingDrained;
		Character character = source as Character;
		if (character != null && character.faction != null && character.faction.isPlayerFaction)
		{
			isPlayerSource = true;
		}
		int num = amount;
		int damage = amount;
		if (!isTrueDamage)
		{
			CombatManager.Instance.ModifyDamage(ref damage, elementalDamageType, piercingPower, this);
			float slayerBonusDamage = EquipmentBonusProcessor.GetSlayerBonusDamage(character, this, damage);
			float wardBonusDamage = EquipmentBonusProcessor.GetWardBonusDamage(character, this, damage);
			damage += (int)slayerBonusDamage;
			damage -= (int)wardBonusDamage;
			if (EquipmentBonusProcessor.GetDeadlyBonus(character, this))
			{
				damage += -maxHP;
			}
		}
		amount = damage;
		if (amount == 0 && num < 0)
		{
			amount = -1;
		}
		if ((amount < 0 && (flag || CanBeDamaged())) || amount > 0)
		{
			if (hasMarker && currentHP > 0)
			{
				if (character == null || character.combatComponent == null)
				{
					InnerMapManager.Instance.ShowHealthAdjustmentEffect(amount, null, worldPosition);
				}
				else
				{
					InnerMapManager.Instance.ShowHealthAdjustmentEffect(amount, character.combatComponent, worldPosition);
				}
			}
			int num2 = currentHP;
			currentHP += amount;
			currentHP = Mathf.Clamp(currentHP, 0, maxHP);
			Messenger.Broadcast(CharacterSignals.CHARACTER_ADJUSTED_HP, this, amount, source);
			if ((bool)marker && showHPBar)
			{
				if (marker.hasHPBarGO && marker.hpBarGO.activeSelf)
				{
					marker.UpdateHP(this);
				}
				else if (amount < 0 && HasHealth())
				{
					marker.QuickShowHPBar(this);
				}
			}
			if (amount < 0)
			{
				if (source is Character character2 && character2.partyComponent.hasParty && character2.partyComponent.currentParty.isActive && character2.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Demon_Raid)
				{
					int p_amount = amount;
					if (currentHP == 0)
					{
						p_amount = num2;
					}
					if (character2.partyComponent.currentParty.isPlayerParty)
					{
						character2.partyComponent.currentParty.damageAccumulator.AccumulateDamageFromDemonRaid(p_amount, character2);
					}
					else
					{
						character2.partyComponent.currentParty.damageAccumulator.AccumulateDamageMonsterSpawnerRaid(p_amount, character2);
					}
				}
				if (isPlayerSource && !isAlliedWithPlayer && (faction == null || !faction.isPlayerFaction))
				{
					int p_amount2 = amount;
					if (currentHP == 0)
					{
						p_amount2 = num2;
					}
					PlayerManager.Instance.player.damageAccumulator.AccumulateDamage(p_amount2, gridTileLocation, this);
				}
			}
		}
		if (amount < 0)
		{
			jobComponent.OnHPReduced();
			CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, character, elementalTraitProcessor, createHitEffect: true, isPlayerSource, piercingPower);
			if (character != null)
			{
				Character character3 = character;
				if (character3.race.IsSapient())
				{
					if (character3.characterClass.attackType == ATTACK_TYPE.PHYSICAL)
					{
						character?.talentComponent.GetTalent(CHARACTER_TALENT.Martial_Arts).AdjustExperience(1, character);
					}
					else
					{
						character?.talentComponent.GetTalent(CHARACTER_TALENT.Combat_Magic).AdjustExperience(1, character);
					}
				}
			}
		}
		else
		{
			Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RECOVER_HP, (IPointOfInterest)this);
		}
		if (!HasHealth())
		{
			if (character != null && character.race.IsSapient())
			{
				if (character.characterClass.attackType == ATTACK_TYPE.PHYSICAL)
				{
					character?.talentComponent.GetTalent(CHARACTER_TALENT.Martial_Arts).AdjustExperience(3, character);
				}
				else
				{
					character?.talentComponent.GetTalent(CHARACTER_TALENT.Combat_Magic).AdjustExperience(3, character);
				}
			}
			if (!isDead && (!isNormalCharacter || isConsideredRatman) && character != null && character.race.IsSapient())
			{
				tileObjectComponent.DropRandomItemBasedOnWeights(character);
			}
			if (triggerDeath)
			{
				if (source != null && source != this)
				{
					if (character != null)
					{
						Death("attacked", null, character, null, null, null, null, isPlayerSource, source, elementalDamageType);
					}
					else
					{
						string text = "attacked";
						if (source is SkillData skillData)
						{
							text += "_spell";
							LogFiller logFiller = ObjectPoolManager.Instance.CreateNewLogFiller(null, skillData.localizedName, LOG_IDENTIFIER.STRING_1);
							Death(text, null, null, null, null, logFiller, null, isPlayerSource, source, elementalDamageType);
							ObjectPoolManager.Instance.ReturnLogFillerToPool(logFiller);
						}
						else if (source is TileObject tileObject)
						{
							text += "_spell";
							LogFiller logFiller2 = ObjectPoolManager.Instance.CreateNewLogFiller(null, tileObject.name, LOG_IDENTIFIER.STRING_1);
							Death(text, null, null, null, null, logFiller2, null, isPlayerSource, source, elementalDamageType);
							ObjectPoolManager.Instance.ReturnLogFillerToPool(logFiller2);
						}
						else if (source is DefenseTower p_obj)
						{
							text += "_tower";
							LogFiller logFiller3 = ObjectPoolManager.Instance.CreateNewLogFiller(p_obj, LOG_IDENTIFIER.LANDMARK_1);
							Death(text, null, null, null, null, logFiller3, null, isPlayerSource, source, elementalDamageType);
							ObjectPoolManager.Instance.ReturnLogFillerToPool(logFiller3);
						}
						else
						{
							text = $"{text}_{source}";
							Death(text, null, null, null, null, null, null, isPlayerSource, source, elementalDamageType);
						}
					}
				}
				else
				{
					Death("normal", null, null, null, null, null, null, isPlayerSource, source, elementalDamageType);
				}
			}
		}
		else if (amount < 0 && IsHealthCriticallyLow())
		{
			Messenger.Broadcast(CharacterSignals.HEALTH_CRITICALLY_LOW, this);
			if (traitContainer.HasTrait("Coward", "Vampire") && !traitContainer.HasTrait("Berserked") && !characterClass.IsZombie())
			{
				bool flag2 = true;
				if (traitContainer.HasTrait("Vampire") && crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire))
				{
					flag2 = false;
				}
				if (flag2)
				{
					combatComponent.FlightAll("critically low health");
				}
			}
		}
		if (!isDead)
		{
			if (EquipmentBonusProcessor.GetFesteringBonus(character, this))
			{
				traitContainer.AddTrait(this, "Plagued", character);
			}
			if (EquipmentBonusProcessor.GetHauntedBonus(character, this))
			{
				traitContainer.AddTrait(this, "Coward", character);
			}
		}
		AfterAdjustHP(amount, elementalDamageType);
	}

	protected virtual void AfterAdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType)
	{
	}

	public void PassiveHPRecovery(float maxHPPercentage)
	{
		if (doNotRecoverPassiveHP <= 0 && !IsHealthFull() && HasHealth())
		{
			AdjustHP(Mathf.CeilToInt(maxHPPercentage * (float)maxHP), ELEMENTAL_TYPE.Normal);
		}
	}

	public void PassiveHPRecovery(int p_amount)
	{
		if (doNotRecoverPassiveHP <= 0 && !IsHealthFull() && HasHealth())
		{
			AdjustHP(p_amount, ELEMENTAL_TYPE.Normal);
		}
	}

	public bool IsHealthFull()
	{
		return currentHP >= maxHP;
	}

	public bool HasHealth()
	{
		return currentHP > 0;
	}

	public bool IsHealthCriticallyLow()
	{
		return (float)currentHP < (float)maxHP * 0.2f;
	}

	protected void PerTickOutsideCombatHPRecovery()
	{
		if (!combatComponent.isInCombat)
		{
			if (characterClass.IsZombie())
			{
				PassiveHPRecovery(0.01f);
			}
			else if (combatComponent.hpRecoveryPerTickOutsideCombat > 0)
			{
				PassiveHPRecovery(combatComponent.hpRecoveryPerTickOutsideCombat);
			}
		}
	}

	public void SetHomeRegion(Region newHome)
	{
		Region region = homeRegion;
		homeRegion = newHome;
		if (faction == null)
		{
			return;
		}
		if (newHome == null)
		{
			if (region == null)
			{
				return;
			}
			bool flag = true;
			for (int i = 0; i < region.residents.Count; i++)
			{
				Character character = region.residents[i];
				if (character != this && character.faction == faction)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				region.RemoveFactionHere(faction);
			}
		}
		else
		{
			newHome.AddFactionHere(faction);
		}
	}

	public void SetHomeStructure(LocationStructure p_homeStructure)
	{
		if (homeStructure != null)
		{
			previousCharacterDataComponent.SetPreviousHomeStructure(homeStructure);
		}
		homeStructure = p_homeStructure;
		if (p_homeStructure != null)
		{
			if (tileObjectComponent.primaryBed != null)
			{
				if (tileObjectComponent.primaryBed.gridTileLocation == null || tileObjectComponent.primaryBed.gridTileLocation.structure != p_homeStructure)
				{
					tileObjectComponent.SetPrimaryBed(p_homeStructure.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuilt<Bed>());
				}
			}
			else
			{
				tileObjectComponent.SetPrimaryBed(p_homeStructure.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuilt<Bed>());
			}
		}
		eventDispatcher.ExecuteCharacterSetHomeStructure(this, p_homeStructure);
		petComponent.OnComponentOwnerChangedHomeStructure(p_homeStructure);
	}

	public bool MigrateHomeTo(BaseSettlement newHomeSettlement, LocationStructure homeStructure = null, bool broadcast = true, bool addToRegionResidents = true)
	{
		BaseSettlement baseSettlement = null;
		bool flag = false;
		if (homeSettlement != null)
		{
			baseSettlement = homeSettlement;
			if (baseSettlement == newHomeSettlement)
			{
				if (this.homeStructure != homeStructure)
				{
					newHomeSettlement.AssignCharacterToDwellingInArea(this, homeStructure);
				}
				return true;
			}
			if (baseSettlement.RemoveResident(this) && baseSettlement is NPCSettlement nPCSettlement && nPCSettlement.ruler == this)
			{
				nPCSettlement.SetRuler(null);
			}
		}
		if (homeRegion != null)
		{
			if (newHomeSettlement != null && newHomeSettlement is NPCSettlement nPCSettlement2 && homeRegion == nPCSettlement2.region)
			{
				flag = true;
			}
			else if (newHomeSettlement != null)
			{
				homeRegion.RemoveResident(this);
			}
		}
		if (newHomeSettlement != null && newHomeSettlement.AddResident(this, homeStructure))
		{
			if (addToRegionResidents && newHomeSettlement is NPCSettlement nPCSettlement3 && !flag)
			{
				nPCSettlement3.region.AddResident(this);
			}
			if (broadcast)
			{
				Messenger.Broadcast(CharacterSignals.CHARACTER_MIGRATED_HOME, this, baseSettlement, newHomeSettlement);
			}
			return true;
		}
		return false;
	}

	public void ClearTerritoryAndMigrateHomeStructureTo(LocationStructure dwelling, bool broadcast = true, bool addToRegionResidents = true, bool affectSettlement = true)
	{
		MigrateHomeStructureTo(dwelling, broadcast, addToRegionResidents, affectSettlement);
		ClearTerritory();
	}

	public void ClearTerritoryAndMigrateHomeSettlementTo(BaseSettlement newHomeSettlement, LocationStructure homeStructure = null, bool broadcast = true, bool addToRegionResidents = true)
	{
		MigrateHomeTo(newHomeSettlement, homeStructure, broadcast, addToRegionResidents);
		ClearTerritory();
	}

	public void MigrateHomeStructureTo(LocationStructure dwelling, bool broadcast = true, bool addToRegionResidents = true, bool affectSettlement = true)
	{
		if (dwelling == null)
		{
			if (affectSettlement)
			{
				if (homeSettlement != null)
				{
					MigrateHomeTo(null);
				}
				else
				{
					ChangeHomeStructure(dwelling);
				}
			}
			else
			{
				ChangeHomeStructure(dwelling);
			}
			return;
		}
		if (dwelling.settlementLocation != null)
		{
			if (affectSettlement)
			{
				MigrateHomeTo(dwelling.settlementLocation, dwelling, broadcast, addToRegionResidents);
			}
			else
			{
				ChangeHomeStructure(dwelling);
			}
			return;
		}
		bool flag = false;
		if (homeStructure != null && homeStructure.region != null)
		{
			if (homeStructure.region == dwelling.region)
			{
				flag = true;
			}
			else
			{
				homeStructure.region.RemoveResident(this);
			}
		}
		if (!flag)
		{
			dwelling.region.AddResident(this);
		}
		ChangeHomeStructure(dwelling);
	}

	public bool ChangeHomeStructure(LocationStructure dwelling)
	{
		bool flag = true;
		if (homeStructure != null)
		{
			if (homeStructure == dwelling)
			{
				return true;
			}
			if (dwelling != null && homeStructure.settlementLocation != null && dwelling.settlementLocation != null && homeStructure.settlementLocation.owner == dwelling.settlementLocation.owner)
			{
				flag = false;
			}
			homeStructure.RemoveResident(this);
		}
		if (dwelling != null && dwelling.AddResident(this) && GameManager.Instance.gameHasStarted)
		{
			if (isNormalCharacter && flag)
			{
				jobComponent.PlanReturnHome(JOB_TYPE.RETURN_HOME_URGENT);
			}
			return true;
		}
		return false;
	}

	public void SetHomeSettlement(NPCSettlement settlement)
	{
		if (homeSettlement != settlement)
		{
			if (settlement == null && partyComponent.hasParty && !isDead)
			{
				interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, this, "", null, "Left_Party_Village");
			}
			if (homeSettlement != null)
			{
				previousCharacterDataComponent.SetPreviousHomeSettlement(homeSettlement);
			}
			homeSettlement = settlement;
			if (isNormalCharacter)
			{
				behaviourComponent.UpdateDefaultBehaviourSet();
			}
			if (faction != null)
			{
				faction.ProcessFactionLeaderAsSettlementRuler();
			}
			if (partyComponent.hasParty)
			{
				partyComponent.currentParty.UpdatePartySettlement();
			}
		}
	}

	private void OnStructureDestroyed(LocationStructure structure)
	{
		if (structure == homeStructure)
		{
			bool affectSettlement = homeSettlement != null && homeSettlement.structures.Count == 0;
			MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: true, affectSettlement);
		}
	}

	public void OnBeforeStructureDestroyed(LocationStructure structure)
	{
		if (hasMarker && partyComponent.hasParty && partyComponent.currentParty.currentQuest is DemonRaidPartyQuest)
		{
			for (int i = 0; i < structure.tiles.Count; i++)
			{
				TileObject genericTileObject = structure.tiles.ElementAt(i).tileObjectComponent.genericTileObject;
				marker.OnTileObjectRemovedFromTile(genericTileObject, null, null);
			}
		}
	}

	public bool HasSameHomeAs(Character otherCharacter)
	{
		if (homeSettlement != null)
		{
			return otherCharacter.homeSettlement == homeSettlement;
		}
		if (homeStructure != null)
		{
			return otherCharacter.homeStructure == homeStructure;
		}
		if (territory != null && otherCharacter.territory != null)
		{
			return territory == otherCharacter.territory;
		}
		return false;
	}

	public void CreateTraitContainer()
	{
		traitContainer = new TraitContainer();
	}

	public void CreateDefaultTraits()
	{
		traitContainer.AddTrait(this, "Character Trait");
		traitContainer.AddTrait(this, "Flammable");
		defaultCharacterTrait = traitContainer.GetTraitOrStatus<CharacterTrait>("Character Trait");
	}

	public void CreateRandomInitialTraits(List<string> buffPool = null, List<string> neutralPool = null, List<string> flawPool = null)
	{
		if (minion != null || race == RACE.DEMON || this is Summon || race == RACE.RATMAN)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		List<string> list;
		if (buffPool != null)
		{
			list = buffPool;
		}
		else
		{
			list = RuinarchListPool<string>.Claim();
			list.AddRange(TraitManager.Instance.buffTraitPool);
			flag = true;
		}
		List<string> list2;
		if (neutralPool != null)
		{
			list2 = neutralPool;
		}
		else
		{
			list2 = RuinarchListPool<string>.Claim();
			list2.AddRange(TraitManager.Instance.neutralTraitPool);
			flag2 = true;
		}
		List<string> list3;
		if (flawPool != null)
		{
			list3 = flawPool;
		}
		else
		{
			list3 = RuinarchListPool<string>.Claim();
			list3.AddRange(TraitManager.Instance.flawTraitPool);
			flag3 = true;
		}
		for (int i = 0; i < traitContainer.traits.Count; i++)
		{
			Trait trait = traitContainer.traits[i];
			list.Remove(trait.name);
			list2.Remove(trait.name);
			list3.Remove(trait.name);
			if (trait.mutuallyExclusive != null)
			{
				CollectionUtilities.RemoveElements(list, trait.mutuallyExclusive);
				CollectionUtilities.RemoveElements(list2, trait.mutuallyExclusive);
				CollectionUtilities.RemoveElements(list3, trait.mutuallyExclusive);
			}
		}
		if (list.Count > 0)
		{
			string randomElement = CollectionUtilities.GetRandomElement(list);
			list.Remove(randomElement);
			traitContainer.AddTrait(this, randomElement);
			Trait traitOrStatus = traitContainer.GetTraitOrStatus<Trait>(randomElement);
			if (traitOrStatus.mutuallyExclusive != null)
			{
				CollectionUtilities.RemoveElements(list, traitOrStatus.mutuallyExclusive);
				CollectionUtilities.RemoveElements(list2, traitOrStatus.mutuallyExclusive);
				CollectionUtilities.RemoveElements(list3, traitOrStatus.mutuallyExclusive);
			}
			List<string> list4 = RuinarchListPool<string>.Claim();
			if (GameUtilities.RollChance(80))
			{
				list4.AddRange(list);
				list4.AddRange(list2);
				if (list4.Count <= 0)
				{
					throw new Exception("No more buff or neutral traits!");
				}
				string randomElement2 = CollectionUtilities.GetRandomElement(list4);
				list.Remove(randomElement2);
				list2.Remove(randomElement2);
				traitContainer.AddTrait(this, randomElement2);
				Trait traitOrStatus2 = traitContainer.GetTraitOrStatus<Trait>(randomElement2);
				if (traitOrStatus2.mutuallyExclusive != null)
				{
					CollectionUtilities.RemoveElements(list, traitOrStatus2.mutuallyExclusive);
					CollectionUtilities.RemoveElements(list2, traitOrStatus2.mutuallyExclusive);
					CollectionUtilities.RemoveElements(list3, traitOrStatus2.mutuallyExclusive);
				}
			}
			if (GameUtilities.RollChance(40))
			{
				list4.Clear();
				list4.AddRange(list);
				list4.AddRange(list2);
				list4.AddRange(list3);
				if (list4.Count <= 0)
				{
					throw new Exception("No more buff, neutral or flaw traits!");
				}
				string randomElement3 = CollectionUtilities.GetRandomElement(list4);
				traitContainer.AddTrait(this, randomElement3);
			}
			RuinarchListPool<string>.Release(list4);
			if (flag)
			{
				RuinarchListPool<string>.Release(list);
			}
			if (flag2)
			{
				RuinarchListPool<string>.Release(list2);
			}
			if (flag3)
			{
				RuinarchListPool<string>.Release(list3);
			}
			return;
		}
		throw new Exception("There are no buff traits!");
	}

	public void ProcessTraitsOnTickStarted()
	{
		if (!interruptComponent.isInterrupted)
		{
			traitContainer.ProcessOnTickStarted(this);
		}
	}

	public void ProcessTraitsOnTickEnded()
	{
		if (!interruptComponent.isInterrupted)
		{
			traitContainer.ProcessOnTickEnded(this);
		}
	}

	public void ProcessTraitsOnHourStarted()
	{
		if (!interruptComponent.isInterrupted)
		{
			traitContainer.ProcessOnHourStarted(this);
		}
	}

	public void TryProcessTraitsOnTickEndedWhileStationaryOrUnoccupied()
	{
		if (interruptComponent.isInterrupted || (!(currentActionNode == null) && currentActionNode.actionStatus == ACTION_STATUS.PERFORMING) || traitContainer.HasTrait("Unconscious") || traitContainer.HasTrait("Resting") || traitContainer.HasTrait("Frozen"))
		{
			return;
		}
		List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions("Per_Tick_While_Stationary_Unoccupied");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count && !traitOverrideFunctions[i].PerTickWhileStationaryOrUnoccupied(this); i++)
			{
			}
		}
	}

	private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_trait)
	{
		if (p_trait is Burning burning && currentActionNode != null && currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && currentActionNode.poiTarget == p_traitable && currentActionNode.action.actionCategory == ACTION_CATEGORY.CONSUME)
		{
			StopCurrentActionNode("Burning_Object");
			if (traitContainer.HasTrait("Flammable"))
			{
				traitContainer.AddTrait(this, "Burning", out var trait, null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Fire);
				(trait as Burning)?.SetSourceOfBurning(burning.sourceOfBurning, this);
				traitContainer.GetTraitOrStatus<Burning>("Burning")?.SetIsPlayerSource(burning.isPlayerSource);
			}
		}
		if (p_traitable == this && isFactionLeader && !(p_trait is Status))
		{
			UpdateFactionIdeologiesCrimesOnFactionLeaderUpdatedTraits(p_trait);
		}
		jobComponent.OnTraitableGainedTrait(p_traitable, p_trait);
	}

	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy)
	{
		jobComponent.OnTraitableLostTrait(traitable, trait, removedBy);
		if (traitable == this && isFactionLeader && !(trait is Status))
		{
			UpdateFactionIdeologiesCrimesOnFactionLeaderUpdatedTraits(trait);
		}
	}

	public void SetMinion(Minion minion)
	{
		if (_minion != null && minion == null)
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_BECOMES_NON_MINION_OR_SUMMON, this);
			moodComponent.OnCharacterNoLongerMinionOrSummon();
		}
		else if (_minion == null && minion != null)
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_BECOMES_MINION_OR_SUMMON, this);
			moodComponent.OnCharacterBecomeMinionOrSummon();
		}
		_minion = minion;
		ApplyAllPrimordialBonus();
	}

	private void DailyGoapProcesses()
	{
		needsComponent.DailyGoapProcesses();
		behaviourComponent.DailyGoapProcesses();
	}

	public void TickStarted()
	{
		OnTickStarted();
	}

	public void TickEnded()
	{
		OnTickEnded();
	}

	protected virtual void OnTickStarted()
	{
		trapStructure.IncrementCurrentDuration(1);
		needsComponent.PerTick();
		ProcessTraitsOnTickStarted();
		StartTickGoapPlanGeneration();
	}

	protected virtual void OnTickEnded()
	{
		ProcessForcedCancelJobsOnTickEnded();
		moodComponent.OnTickEnded();
		interruptComponent.OnTickEnded();
		stateComponent.OnTickEnded();
		combatComponent.OnTickEnded();
		ProcessTraitsOnTickEnded();
		TryProcessTraitsOnTickEndedWhileStationaryOrUnoccupied();
		EndTickPerformJobs();
		PerTickOutsideCombatHPRecovery();
	}

	protected virtual void OnHourStarted()
	{
		ProcessTraitsOnHourStarted();
		dailyScheduleComponent.OnHourStarted(this);
		needsComponent.PerHour();
		classComponent.PerHour();
	}

	protected void StartTickGoapPlanGeneration()
	{
		if (isNormalCharacter && CanTryToTakeSettlementJobInVision(out var _))
		{
			_ = string.Empty;
			JobQueueItem jobQueueItem = homeSettlement?.GetFirstJobBasedOnVisionExcept(this, JOB_TYPE.CRAFT_OBJECT);
			if (jobQueueItem != null && ((jobQueue.jobsInQueue.Count <= 0 && behaviourComponent.GetHighestBehaviourPriority() < jobQueueItem.priority) || (jobQueue.jobsInQueue.Count > 0 && jobQueueItem.priority > jobQueue.jobsInQueue[0].priority)))
			{
				jobQueue.AddJobInQueue(jobQueueItem);
			}
		}
		if (CanPlanGoap())
		{
			PerStartTickActionPlanning();
		}
		else if (currentActionNode != null && currentActionNode.hasStartedPerTickEffect)
		{
			currentActionNode.PerTickEffect();
		}
		behaviourComponent.SetSubterraneanJustExitedCombat(state: false);
	}

	public void GoToArea()
	{
		marker.GoTo(areaLocation.GetRandomPassableTile());
	}

	private bool CanTryToTakeSettlementJobInVision(out string invalidReason)
	{
		if (isDead)
		{
			invalidReason = "Character is dead.";
			return false;
		}
		if (numOfNonSecretActionsBeingPerformedOnThis > 0)
		{
			invalidReason = "Actions being performed on this is " + numOfNonSecretActionsBeingPerformedOnThis + ".";
			return false;
		}
		if (!limiterComponent.canPerform)
		{
			invalidReason = "Character cannot perform";
			return false;
		}
		if (currentActionNode != null)
		{
			invalidReason = "Character has current action";
			return false;
		}
		if (currentJob != null)
		{
			invalidReason = "Character is in the middle of a job";
			return false;
		}
		if (marker == null || marker.hasFleePath)
		{
			invalidReason = "Character has no marker or is fleeing";
			return false;
		}
		if (stateComponent.currentState != null)
		{
			invalidReason = "Character is in a state";
			return false;
		}
		if (!carryComponent.IsNotBeingCarried())
		{
			invalidReason = "Character is being carried";
			return false;
		}
		if (interruptComponent.isInterrupted)
		{
			invalidReason = "Character is interrupted";
			return false;
		}
		if (partyComponent.isActiveMember)
		{
			invalidReason = "Character is in an active party";
			return false;
		}
		if (homeSettlement != null && homeSettlement.owner != faction)
		{
			invalidReason = "Character is not part of settlement faction!";
			return false;
		}
		invalidReason = "No reason";
		return true;
	}

	public void EndTickPerformJobs()
	{
		if (shouldDoActionOnFirstTickUponLoadGame)
		{
			shouldDoActionOnFirstTickUponLoadGame = false;
			if (currentActionNode != null)
			{
				currentActionNode.DoActionUponLoadingSavedGame();
			}
			return;
		}
		if (jobQueue.pendingTopPriorityJobs.Count > 0 && planner.status == GOAP_PLANNING_STATUS.NONE)
		{
			jobQueue.pendingTopPriorityJobs[0].ProcessJob();
		}
		if (CanPerformEndTickJobs() && HasSameOrHigherPriorityJobThanBehaviour())
		{
			JobQueueItem jobQueueItem = null;
			if (jobQueue.jobsInQueue.Count > 0)
			{
				jobQueueItem = jobQueue.jobsInQueue[0];
			}
			if (jobQueueItem != null && !jobQueueItem.ProcessJob())
			{
				PerformJob(jobQueueItem);
			}
		}
	}

	public bool CanPlanGoap()
	{
		if (!isDead && numOfNonSecretActionsBeingPerformedOnThis <= 0 && limiterComponent.canPerform && currentActionNode == null && planner.status == GOAP_PLANNING_STATUS.NONE && (jobQueue.jobsInQueue.Count <= 0 || behaviourComponent.GetHighestBehaviourPriority() > jobQueue.jobsInQueue[0].priority) && hasMarker && !marker.hasFleePath && stateComponent.currentState == null && carryComponent.IsNotBeingCarried() && !interruptComponent.isInterrupted)
		{
			return !partyComponent.isFollowingBeacon;
		}
		return false;
	}

	public bool CanPerformEndTickJobs()
	{
		if (!isDead && numOfNonSecretActionsBeingPerformedOnThis <= 0 && currentActionNode == null && planner.status == GOAP_PLANNING_STATUS.NONE && jobQueue.HasJobInAnyQueue() && hasMarker && !marker.hasFleePath && stateComponent.currentState == null && carryComponent.IsNotBeingCarried() && !interruptComponent.isInterrupted)
		{
			return !partyComponent.isFollowingBeacon;
		}
		return false;
	}

	public bool HasSameOrHigherPriorityJobThanBehaviour()
	{
		JobQueueItem jobQueueItem = null;
		if (jobQueue.pendingTopPriorityJobs.Count > 0)
		{
			jobQueueItem = jobQueue.pendingTopPriorityJobs[0];
		}
		else if (jobQueue.jobsInQueue.Count > 0)
		{
			jobQueueItem = jobQueue.jobsInQueue[0];
		}
		if (jobQueueItem != null)
		{
			return jobQueueItem.priority >= behaviourComponent.GetHighestBehaviourPriority();
		}
		return false;
	}

	public void PerStartTickActionPlanning()
	{
		if (!interruptComponent.NecromanticTransform())
		{
			OtherIdlePlans();
		}
	}

	private string OtherIdlePlans()
	{
		string empty = string.Empty;
		if (isDead)
		{
			return empty;
		}
		behaviourComponent.RunBehaviour();
		return empty;
	}

	public void PlanIdle(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, OtherData[] otherData = null)
	{
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, target);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		jobQueue.AddJobInQueue(goapPlanJob);
	}

	public void PlanFixedJob(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, out JobQueueItem producedJob, OtherData[] otherData = null)
	{
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, target);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		producedJob = goapPlanJob;
	}

	public void PlanFixedJob(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, OtherData[] otherData = null)
	{
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, target);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		jobQueue.AddJobInQueue(goapPlanJob);
	}

	public void SetIsConversing(bool state)
	{
		if (isConversing != state)
		{
			isConversing = state;
			if ((bool)marker)
			{
				marker.UpdateActionIcon();
			}
			if (isConversing)
			{
				nonActionEventsComponent.SetLastConversationDate(GameManager.Instance.Today());
			}
		}
	}

	public void AddAdvertisedAction(INTERACTION_TYPE type, bool allowDuplicates = false)
	{
		if (allowDuplicates || !advertisedActions.Contains(type))
		{
			advertisedActions.Add(type);
		}
	}

	public void RemoveAdvertisedAction(INTERACTION_TYPE type)
	{
		advertisedActions.Remove(type);
	}

	public List<IPointOfInterest> GetListOfInventoryItemsBasedOnActionType(INTERACTION_TYPE actionType)
	{
		List<IPointOfInterest> list = RuinarchListPool<IPointOfInterest>.Claim();
		for (int i = 0; i < items.Count; i++)
		{
			TileObject tileObject = items[i];
			if (tileObject.Advertises(actionType))
			{
				list.Add(tileObject);
			}
		}
		if (list.Count > 0)
		{
			return list;
		}
		RuinarchListPool<IPointOfInterest>.Release(list);
		return null;
	}

	public bool AddOwnedItem(TileObject item)
	{
		if (!HasOwnedItem(item))
		{
			ownedItems.Add(item);
			return true;
		}
		return false;
	}

	public bool RemoveOwnedItem(TileObject item)
	{
		return ownedItems.Remove(item);
	}

	public bool HasOwnedItem(TileObject p_item)
	{
		return ownedItems.Contains(p_item);
	}

	public TileObject GetOwnedItemOnTheGround(TILE_OBJECT_TYPE p_itemType)
	{
		for (int i = 0; i < ownedItems.Count; i++)
		{
			TileObject tileObject = ownedItems[i];
			if (p_itemType == tileObject.tileObjectType && tileObject.gridTileLocation != null)
			{
				return tileObject;
			}
		}
		return null;
	}

	public bool HasOwnedItemThatIsOnGroundInSameRegion()
	{
		for (int i = 0; i < ownedItems.Count; i++)
		{
			TileObject tileObject = ownedItems[i];
			if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure.region == currentRegion)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOwnedItemThatIsOnGroundThatAdvertises(INTERACTION_TYPE p_advertisedAction)
	{
		for (int i = 0; i < ownedItems.Count; i++)
		{
			TileObject tileObject = ownedItems[i];
			if (tileObject.gridTileLocation != null && tileObject.Advertises(p_advertisedAction))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOwnedItemInHomeStructure()
	{
		if (homeStructure == null)
		{
			return false;
		}
		for (int i = 0; i < ownedItems.Count; i++)
		{
			TileObject tileObject = ownedItems[i];
			if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure == homeStructure)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOwnedItemInHomeStructure(string itemName)
	{
		if (homeStructure == null)
		{
			return false;
		}
		for (int i = 0; i < ownedItems.Count; i++)
		{
			TileObject tileObject = ownedItems[i];
			if (itemName == tileObject.name && tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure == homeStructure)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumOfOwnedItemsInHomeStructure(string itemName)
	{
		int num = 0;
		if (homeStructure == null)
		{
			return num;
		}
		for (int i = 0; i < ownedItems.Count; i++)
		{
			TileObject tileObject = ownedItems[i];
			if (itemName == tileObject.name && tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure == homeStructure)
			{
				num++;
			}
		}
		return num;
	}

	public TileObject GetRandomOwnedItemInHomeStructure()
	{
		TileObject result = null;
		if (homeStructure == null)
		{
			return result;
		}
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < ownedItems.Count; i++)
		{
			TileObject tileObject = ownedItems[i];
			if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure == homeStructure)
			{
				list.Add(tileObject);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public bool ObtainItem(TileObject item, bool changeCharacterOwnership = false, bool setOwnership = true, bool equipItem = true)
	{
		if (AddItem(item, equipItem))
		{
			item.SetInventoryOwner(this);
			if (changeCharacterOwnership)
			{
				item.SetCharacterOwner(this);
			}
			else if (setOwnership && item.characterOwner == null)
			{
				item.SetCharacterOwner(this);
			}
			EquipmentItem equipmentItem = item as EquipmentItem;
			if (equipmentItem != null && equipItem)
			{
				if (equipmentComponent.HasEquipmentInSlotFor(equipmentItem))
				{
					equipmentComponent.RemoveEquipmentInSlotFor(equipmentItem, this);
				}
				equipmentComponent.SetEquipment(equipmentItem, this);
			}
			return true;
		}
		return false;
	}

	public bool UnobtainItem(TileObject item)
	{
		if (RemoveItem(item))
		{
			item.SetInventoryOwner(null);
			return true;
		}
		return false;
	}

	public bool UnobtainItem(TILE_OBJECT_TYPE itemType)
	{
		TileObject tileObject = RemoveItem(itemType);
		if (tileObject != null)
		{
			tileObject.SetInventoryOwner(null);
			return true;
		}
		return false;
	}

	public bool UnobtainItem(string name)
	{
		TileObject tileObject = RemoveItem(name);
		if (tileObject != null)
		{
			tileObject.SetInventoryOwner(null);
			return true;
		}
		return false;
	}

	private bool AddItem(TileObject item, bool equipEquipment = true)
	{
		if (item is EquipmentItem equipmentItem)
		{
			if (equipEquipment)
			{
				if (!equipmentComponent.allEquipments.Contains(item))
				{
					equipmentComponent.allEquipments.Add(equipmentItem);
					Messenger.Broadcast(CharacterSignals.CHARACTER_OBTAINED_ITEM, item, this);
					eventDispatcher.ExecuteItemObtained(this, item);
					equipmentItem.OnEquipmentPickedUp(this);
					return true;
				}
			}
			else if (!items.Contains(item))
			{
				items.Add(item);
				Messenger.Broadcast(CharacterSignals.CHARACTER_OBTAINED_ITEM, item, this);
				eventDispatcher.ExecuteItemObtained(this, item);
				equipmentItem.OnEquipmentPickedUp(this);
				return true;
			}
		}
		else if (!items.Contains(item))
		{
			items.Add(item);
			Messenger.Broadcast(CharacterSignals.CHARACTER_OBTAINED_ITEM, item, this);
			eventDispatcher.ExecuteItemObtained(this, item);
			return true;
		}
		return false;
	}

	private bool RemoveItem(TileObject item)
	{
		if (item is EquipmentItem equipmentItem && equipmentComponent.HasEquipment(equipmentItem))
		{
			if (equipmentComponent.RemoveEquipment(equipmentItem, this))
			{
				Messenger.Broadcast(CharacterSignals.CHARACTER_LOST_ITEM, item, this);
				eventDispatcher.ExecuteItemLost(this, item);
				return true;
			}
		}
		else if (items.Remove(item))
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_LOST_ITEM, item, this);
			eventDispatcher.ExecuteItemLost(this, item);
			return true;
		}
		return false;
	}

	private TileObject RemoveItem(TILE_OBJECT_TYPE itemType)
	{
		TileObject tileObject = GetItem(itemType);
		if (tileObject == null)
		{
			tileObject = equipmentComponent.GetEquipment(itemType);
		}
		if (tileObject != null)
		{
			RemoveItem(tileObject);
		}
		return tileObject;
	}

	private TileObject RemoveItem(string name)
	{
		TileObject tileObject = GetItem(name);
		if (tileObject == null)
		{
			tileObject = equipmentComponent.GetEquipment(name);
		}
		if (tileObject != null)
		{
			RemoveItem(tileObject);
		}
		return tileObject;
	}

	public bool DropItem(TileObject item, LocationGridTile gridTile = null, bool isUncarriedFromSeize = false)
	{
		if (DropItemBase(item, gridTile))
		{
			if (!isUncarriedFromSeize)
			{
				LocationGridTile locationGridTile = item.gridTileLocation;
				if (locationGridTile != null && item is ResourcePile resourcePile)
				{
					if (resourcePile.characterOwner == this && homeSettlement != null && !locationGridTile.IsPartOfSettlement(homeSettlement))
					{
						resourcePile.SetCharacterOwner(null);
					}
					if (locationGridTile.structure is Dwelling dwelling)
					{
						ResourcePile firstBuiltTileObjectOfType = dwelling.GetFirstBuiltTileObjectOfType<ResourcePile>(item.tileObjectType, item);
						if (firstBuiltTileObjectOfType != null)
						{
							int resourceInPile = resourcePile.resourceInPile;
							firstBuiltTileObjectOfType.AdjustResourceInPile(resourceInPile);
							InnerMapManager.Instance.ShowAreaMapTextPopup($"+{resourceInPile}", firstBuiltTileObjectOfType.worldPosition, Color.green);
							TraitManager.Instance.CopyStatuses(resourcePile, firstBuiltTileObjectOfType);
							dwelling.RemovePOI(resourcePile);
						}
					}
				}
			}
			return true;
		}
		return false;
	}

	private bool DropItemBase(TileObject item, LocationGridTile gridTile)
	{
		if (UnobtainItem(item))
		{
			LocationGridTile locationGridTile = gridTile;
			if (locationGridTile == null)
			{
				locationGridTile = gridTileLocation;
			}
			if (locationGridTile == null)
			{
				return true;
			}
			if (locationGridTile.tileObjectComponent.objHere != null)
			{
				LocationGridTile locationGridTile2 = gridTileLocation;
				if (locationGridTile2 != null)
				{
					locationGridTile = locationGridTile2.GetFirstNoObjectNeighbor();
					if (locationGridTile == null)
					{
						locationGridTile = locationGridTile2.GetFirstNearestTileFromThisWithNoObject(thisStructureOnly: true);
					}
					if (locationGridTile == null)
					{
						locationGridTile = locationGridTile2.GetFirstNearestTileFromThisWithNoObject();
					}
				}
			}
			if (locationGridTile == null)
			{
				return true;
			}
			locationGridTile.structure.AddPOI(item, locationGridTile);
			item.OnTileObjectDroppedBy(this, locationGridTile);
			return true;
		}
		return false;
	}

	public void DropAllItems(LocationGridTile tile)
	{
		for (int i = 0; i < items.Count; i++)
		{
			TileObject item = items[i];
			if (DropItem(item, tile))
			{
				i--;
			}
		}
	}

	public void UnownOrTransferOwnershipOfAllItems()
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		if (homeStructure != null)
		{
			for (int i = 0; i < homeStructure.residents.Count; i++)
			{
				Character character = homeStructure.residents[i];
				if (character != this && character.faction != null && character.faction.isMajorFaction)
				{
					list.Add(character);
				}
			}
		}
		int num;
		for (num = 0; num < ownedItems.Count; num++)
		{
			TileObject tileObject = ownedItems[num];
			if (tileObject.gridTileLocation == null || tileObject.gridTileLocation.structure != homeStructure)
			{
				tileObject.SetCharacterOwner(null);
			}
			else if (list != null && list.Count > 0)
			{
				Character randomElement = CollectionUtilities.GetRandomElement(list);
				tileObject.SetCharacterOwner(randomElement);
			}
			else
			{
				tileObject.SetCharacterOwner(null);
			}
			num--;
		}
		RuinarchListPool<Character>.Release(list);
	}

	public void UnownOrTransferOwnershipOfItemsIn(LocationStructure structure)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		if (structure != null)
		{
			for (int i = 0; i < structure.residents.Count; i++)
			{
				Character character = structure.residents[i];
				if (character != this && character.faction != null && character.faction.isMajorFaction)
				{
					list.Add(character);
				}
			}
		}
		for (int j = 0; j < ownedItems.Count; j++)
		{
			TileObject tileObject = ownedItems[j];
			if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure == structure)
			{
				if (list != null && list.Count > 0)
				{
					Character randomElement = CollectionUtilities.GetRandomElement(list);
					tileObject.SetCharacterOwner(randomElement);
				}
				else
				{
					tileObject.SetCharacterOwner(null);
				}
				j--;
			}
		}
		RuinarchListPool<Character>.Release(list);
	}

	public void TransferAllInventoryItemsAndEquippableEquipmentsTo(Character character, bool transferOwnership)
	{
		while (items.Count > 0)
		{
			TileObject item = items[0];
			UnobtainItem(item);
			character.ObtainItem(item, transferOwnership);
		}
		while (equipmentComponent.allEquipments.Count > 0)
		{
			EquipmentItem equipmentItem = equipmentComponent.allEquipments[0];
			UnobtainItem(equipmentItem);
			if (character.equipmentComponent.CanEquipItem(equipmentItem, character))
			{
				character.ObtainItem(equipmentItem, transferOwnership);
			}
		}
	}

	public void PickUpItem(TileObject item, bool changeCharacterOwnership = false, bool setOwnership = true, bool equipItem = true)
	{
		item.isBeingCarriedBy?.UnobtainItem(item);
		if (ObtainItem(item, changeCharacterOwnership, setOwnership, equipItem))
		{
			item.gridTileLocation?.structure.RemovePOIDestroyVisualOnly(item, this);
			item.SetPOIState(POI_STATE.ACTIVE);
		}
	}

	public void DestroyItem(TileObject item)
	{
		item.structureLocation.RemovePOI(item);
	}

	public bool HasResourcePileWithAmount<T>(TILE_OBJECT_TYPE p_type, int p_amount) where T : ResourcePile
	{
		for (int i = 0; i < items.Count; i++)
		{
			TileObject tileObject = items[i];
			if (tileObject.tileObjectType == p_type && tileObject is T val && val.resourceInPile >= p_amount)
			{
				return true;
			}
		}
		return false;
	}

	public TileObject GetItem(TileObject item)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] == item)
			{
				return items[i];
			}
		}
		return null;
	}

	public TileObject GetItem(TILE_OBJECT_TYPE itemType)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].tileObjectType == itemType)
			{
				return items[i];
			}
		}
		return null;
	}

	public TileObject GetItem(string itemName)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].name == itemName || items[i].internalName == itemName)
			{
				return items[i];
			}
		}
		return null;
	}

	public T GetItem<T>() where T : TileObject
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public void PopulateItemsOfType<T>(List<T> p_list) where T : TileObject
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] is T item)
			{
				p_list.Add(item);
			}
		}
	}

	public TileObject GetRandomItem()
	{
		if (items.Count > 0)
		{
			return items[UnityEngine.Random.Range(0, items.Count)];
		}
		return null;
	}

	public TileObject GetRandomItemThatIsNotOfType(TILE_OBJECT_TYPE p_type)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < items.Count; i++)
		{
			TileObject tileObject = items[i];
			if (tileObject.tileObjectType != p_type)
			{
				list.Add(tileObject);
			}
		}
		TileObject result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public TileObject GetRandomItemThatIsNotOfType(TILE_OBJECT_TYPE[] p_type)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < items.Count; i++)
		{
			TileObject tileObject = items[i];
			if (!p_type.Contains(tileObject.tileObjectType))
			{
				list.Add(tileObject);
			}
		}
		TileObject result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public bool HasItemOrEquipment(TileObject item)
	{
		if (item is EquipmentItem equipment && equipmentComponent.HasEquipment(equipment))
		{
			return true;
		}
		return GetItem(item) != null;
	}

	public bool HasItem(TILE_OBJECT_TYPE itemType)
	{
		return GetItem(itemType) != null;
	}

	public bool HasItem(string itemName)
	{
		return GetItem(itemName) != null;
	}

	public bool HasItem<T>() where T : TileObject
	{
		return GetItem<T>() != null;
	}

	public bool HasItem()
	{
		return items.Count > 0;
	}

	public bool HasItemOtherThan(TILE_OBJECT_TYPE p_type)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].tileObjectType != p_type)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasItemOtherThan(TILE_OBJECT_TYPE[] p_type)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (!p_type.Contains(items[i].tileObjectType))
			{
				return true;
			}
		}
		return false;
	}

	public int GetItemCount(string name)
	{
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].internalName == name || items[i].name == name)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsInventoryAtFullCapacity()
	{
		return items.Count >= characterClass.inventoryCapacity;
	}

	public void LogAwarenessList()
	{
		if (currentLocationAwareness == null)
		{
			return;
		}
		string text = "--------------AWARENESS LIST OF " + name + "-----------------";
		foreach (KeyValuePair<INTERACTION_TYPE, List<IPointOfInterest>> item in currentLocationAwareness.awareness)
		{
			text += $"\n{item.Key}: ";
			for (int i = 0; i < item.Value.Count; i++)
			{
				if (i > 0)
				{
					text += ", ";
				}
				text += item.Value[i].ToString();
			}
		}
	}

	public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, GoapPlanJob job, ref int cost, ref string log)
	{
		GoapAction result = null;
		if (advertisedActions != null && advertisedActions.Count > 0)
		{
			bool flag = IsAvailable();
			GoapAction goapAction = null;
			int num = 0;
			log += $"\n--Choices for {precondition}";
			log += "\n--";
			for (int i = 0; i < advertisedActions.Count; i++)
			{
				INTERACTION_TYPE iNTERACTION_TYPE = advertisedActions[i];
				GoapAction goapAction2 = InteractionManager.Instance.goapActionData[iNTERACTION_TYPE];
				if ((!flag && !goapAction2.canBeAdvertisedEvenIfTargetIsUnavailable) || (!goapAction2.canBePerformedEvenIfPathImpossible && !actor.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation)) || !RaceManager.Instance.CanCharacterDoGoapAction(actor, iNTERACTION_TYPE))
				{
					continue;
				}
				OtherData[] otherDataFor = job.GetOtherDataFor(iNTERACTION_TYPE);
				if (goapAction2.CanSatisfyRequirements(actor, this, otherDataFor, job) && goapAction2.WillEffectsSatisfyPrecondition(precondition, actor, this, job))
				{
					int cost2 = goapAction2.GetCost(actor, this, job);
					log += $"({cost2}){goapAction2.goapName}-{nameWithID}, ";
					if (goapAction == null || cost2 < num)
					{
						goapAction = goapAction2;
						num = cost2;
					}
				}
			}
			cost = num;
			result = goapAction;
		}
		return result;
	}

	public bool CanAdvertiseActionToActor(Character actor, GoapAction action, GoapPlanJob job)
	{
		if ((IsAvailable() || action.canBeAdvertisedEvenIfTargetIsUnavailable) && actor.trapStructure.SatisfiesForcedStructure(this) && actor.trapStructure.SatisfiesForcedArea(this) && RaceManager.Instance.CanCharacterDoGoapAction(actor, action.goapType) && (action.canBePerformedEvenIfPathImpossible || actor.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation)))
		{
			OtherData[] otherDataFor = job.GetOtherDataFor(action.goapType);
			if (action.CanSatisfyRequirements(actor, this, otherDataFor, job))
			{
				return true;
			}
		}
		return false;
	}

	public void SetPOIState(POI_STATE state)
	{
		this.state = state;
	}

	public bool IsAvailable()
	{
		return state != POI_STATE.INACTIVE;
	}

	public void OnPlacePOI()
	{
	}

	public void OnLoadPlacePOI()
	{
	}

	public void OnDestroyPOI(Character p_destroyer = null)
	{
	}

	public virtual bool IsStillConsideredPartOfAwarenessByCharacter(Character character)
	{
		if (character.currentRegion == currentRegion && !isBeingSeized)
		{
			if (!isDead && carryComponent.masterCharacter.movementComponent.isTravellingInWorld)
			{
				return false;
			}
			if (isDead && !marker)
			{
				return false;
			}
			if (isInVampireBatForm)
			{
				if (!traitContainer.GetTraitOrStatus<Vampire>("Vampire").DoesCharacterKnowThisVampire(character))
				{
					return false;
				}
			}
			else if (isInWerewolfForm && !lycanData.DoesCharacterKnowThisLycan(character))
			{
				return false;
			}
			if (character.isInVampireBatForm)
			{
				if (!character.traitContainer.GetTraitOrStatus<Vampire>("Vampire").DoesCharacterKnowThisVampire(this))
				{
					return false;
				}
			}
			else if (character.isInWerewolfForm && !character.lycanData.DoesCharacterKnowThisLycan(this))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsOwnedBy(Character character)
	{
		return false;
	}

	public bool Advertises(INTERACTION_TYPE type)
	{
		if (advertisedActions != null)
		{
			return advertisedActions.Contains(type);
		}
		return false;
	}

	public virtual void ConstructInitialGoapAdvertisementActions()
	{
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.SLEEP_OUTSIDE);
		AddAdvertisedAction(INTERACTION_TYPE.GO_TO);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.RETURN_HOME);
		AddAdvertisedAction(INTERACTION_TYPE.CARRY);
		AddAdvertisedAction(INTERACTION_TYPE.DROP);
		AddAdvertisedAction(INTERACTION_TYPE.CARRY_CORPSE);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_CORPSE);
		AddAdvertisedAction(INTERACTION_TYPE.CARRY_RESTRAINED);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_RESTRAINED);
		AddAdvertisedAction(INTERACTION_TYPE.KNOCKOUT_CHARACTER);
		AddAdvertisedAction(INTERACTION_TYPE.RESTRAIN_CHARACTER);
		AddAdvertisedAction(INTERACTION_TYPE.JUDGE_CHARACTER);
		AddAdvertisedAction(INTERACTION_TYPE.SLAY_CHARACTER);
		AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
		AddAdvertisedAction(INTERACTION_TYPE.BURY_CHARACTER);
		AddAdvertisedAction(INTERACTION_TYPE.POISON);
		AddAdvertisedAction(INTERACTION_TYPE.EXILE);
		AddAdvertisedAction(INTERACTION_TYPE.WHIP);
		AddAdvertisedAction(INTERACTION_TYPE.EXECUTE);
		AddAdvertisedAction(INTERACTION_TYPE.ABSOLVE);
		AddAdvertisedAction(INTERACTION_TYPE.BURN_AT_STAKE);
		AddAdvertisedAction(INTERACTION_TYPE.START_TEND);
		AddAdvertisedAction(INTERACTION_TYPE.START_DOUSE);
		AddAdvertisedAction(INTERACTION_TYPE.START_CLEANSE);
		AddAdvertisedAction(INTERACTION_TYPE.START_DRY);
		AddAdvertisedAction(INTERACTION_TYPE.START_PATROL);
		AddAdvertisedAction(INTERACTION_TYPE.PATROL);
		AddAdvertisedAction(INTERACTION_TYPE.EAT_ALIVE);
		AddAdvertisedAction(INTERACTION_TYPE.DECREASE_MOOD);
		AddAdvertisedAction(INTERACTION_TYPE.DISABLE);
		AddAdvertisedAction(INTERACTION_TYPE.TORTURE);
		AddAdvertisedAction(INTERACTION_TYPE.BIRTH_RATMAN);
		AddAdvertisedAction(INTERACTION_TYPE.CARRY_PATIENT);
		AddAdvertisedAction(INTERACTION_TYPE.QUARANTINE);
		AddAdvertisedAction(INTERACTION_TYPE.START_PLAGUE_CARE);
		AddAdvertisedAction(INTERACTION_TYPE.CARE);
		AddAdvertisedAction(INTERACTION_TYPE.LONG_STAND_STILL);
		AddAdvertisedAction(INTERACTION_TYPE.COOK);
		AddAdvertisedAction(INTERACTION_TYPE.MAKE_LOVE);
		AddAdvertisedAction(INTERACTION_TYPE.INVITE);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_TRAIT);
		AddAdvertisedAction(INTERACTION_TYPE.GIVE_TRAIT);
		AddAdvertisedAction(INTERACTION_TYPE.FOLLOW_ACTION);
		AddAdvertisedAction(INTERACTION_TYPE.BUILD_BANDIT_CAMP);
		AddAdvertisedAction(INTERACTION_TYPE.ENHANCE_RELATIONSHIP);
		AddAdvertisedAction(INTERACTION_TYPE.PURIFY);
		AddAdvertisedAction(INTERACTION_TYPE.SACRIFICE_FOR_CHAOS_ORBS);
		AddAdvertisedAction(INTERACTION_TYPE.SEDUCE);
		AddAdvertisedAction(INTERACTION_TYPE.SUMMON_EPHEMERAL_BEASTS);
		AddAdvertisedAction(INTERACTION_TYPE.START_PURIFYING_GROUND);
		AddAdvertisedAction(INTERACTION_TYPE.CREATE_GOLEM);
		AddAdvertisedAction(INTERACTION_TYPE.BLOOD_SACRIFICE);
		AddAdvertisedAction(INTERACTION_TYPE.IMPREGNATE);
		if (this is Summon summon)
		{
			if (summon is GiantSpider)
			{
				AddAdvertisedAction(INTERACTION_TYPE.LAY_EGG);
			}
			else if (summon is Wurm)
			{
				AddAdvertisedAction(INTERACTION_TYPE.BURROW);
			}
			else if (summon is Revenant)
			{
				AddAdvertisedAction(INTERACTION_TYPE.SPAWN_GHOST);
			}
			else if (summon is Mothman)
			{
				AddAdvertisedAction(INTERACTION_TYPE.PRAY);
			}
			else if (summon is Broodmother)
			{
				AddAdvertisedAction(INTERACTION_TYPE.LAY_EGG);
				AddAdvertisedAction(INTERACTION_TYPE.BROODMOTHER_ORDER_ATTACK);
			}
			else if (summon is Sludge)
			{
				AddAdvertisedAction(INTERACTION_TYPE.SPAWN_POISON_CLOUD);
			}
			else if (summon is Wisp)
			{
				AddAdvertisedAction(INTERACTION_TYPE.ABSORB_WISP);
			}
		}
		if (this is Animal)
		{
			AddAdvertisedAction(INTERACTION_TYPE.EAT_CORPSE);
			AddAdvertisedAction(INTERACTION_TYPE.DRINK_BLOOD);
		}
		if (race.IsShearable())
		{
			AddAdvertisedAction(INTERACTION_TYPE.SHEAR_ANIMAL);
		}
		if (race.IsSkinnable())
		{
			AddAdvertisedAction(INTERACTION_TYPE.SKIN_ANIMAL);
		}
		if (raceSetting.category == CHARACTER_CATEGORY.Humanoid || race.IsButcherableWhenDead() || race.IsButcherableWhenDeadOrAlive())
		{
			AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
		}
		if (isNormalCharacter)
		{
			AddAdvertisedAction(INTERACTION_TYPE.DAYDREAM);
			AddAdvertisedAction(INTERACTION_TYPE.SNIFF_CLOTHES);
			AddAdvertisedAction(INTERACTION_TYPE.SMELL_HAIR);
			AddAdvertisedAction(INTERACTION_TYPE.WATCH_SLEEP);
			AddAdvertisedAction(INTERACTION_TYPE.PRAY);
			AddAdvertisedAction(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER);
			AddAdvertisedAction(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE);
			AddAdvertisedAction(INTERACTION_TYPE.ASK_TO_STOP_JOB);
			AddAdvertisedAction(INTERACTION_TYPE.STRANGLE);
			AddAdvertisedAction(INTERACTION_TYPE.CRY);
			AddAdvertisedAction(INTERACTION_TYPE.DANCE);
			AddAdvertisedAction(INTERACTION_TYPE.SING);
			AddAdvertisedAction(INTERACTION_TYPE.SCREAM_FOR_HELP);
			AddAdvertisedAction(INTERACTION_TYPE.PICKPOCKET);
			AddAdvertisedAction(INTERACTION_TYPE.STEAL_COINS);
			AddAdvertisedAction(INTERACTION_TYPE.CHANGE_CLASS);
			AddAdvertisedAction(INTERACTION_TYPE.STOCKPILE_FOOD);
			AddAdvertisedAction(INTERACTION_TYPE.SHARE_INFORMATION);
			AddAdvertisedAction(INTERACTION_TYPE.REPORT_CRIME);
			AddAdvertisedAction(INTERACTION_TYPE.REPORT_MURDER);
			AddAdvertisedAction(INTERACTION_TYPE.REPORT_ABDUCT);
			AddAdvertisedAction(INTERACTION_TYPE.DRINK_BLOOD);
			AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
			AddAdvertisedAction(INTERACTION_TYPE.HAVE_AFFAIR);
			AddAdvertisedAction(INTERACTION_TYPE.REMOVE_BUFF);
			AddAdvertisedAction(INTERACTION_TYPE.EVANGELIZE);
			AddAdvertisedAction(INTERACTION_TYPE.LIBERATE);
			AddAdvertisedAction(INTERACTION_TYPE.BUILD_CAMPFIRE);
			AddAdvertisedAction(INTERACTION_TYPE.VAMPIRIC_EMBRACE);
			AddAdvertisedAction(INTERACTION_TYPE.EAT_CORPSE);
			AddAdvertisedAction(INTERACTION_TYPE.CAST_MESMERIZED);
		}
		if (race.IsSapient())
		{
			AddAdvertisedAction(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE);
			AddAdvertisedAction(INTERACTION_TYPE.HEAL_SELF);
			AddAdvertisedAction(INTERACTION_TYPE.TRANSFORM_CENTAUR);
		}
	}

	public void PerformJob(JobQueueItem job)
	{
		string log = string.Empty;
		if (currentActionNode != null)
		{
			return;
		}
		GoapPlanJob goapPlanJob = job as GoapPlanJob;
		if (goapPlanJob?.assignedPlan == null)
		{
			return;
		}
		GoapPlan assignedPlan = goapPlanJob.assignedPlan;
		ActualGoapNode currentActualNode = assignedPlan.currentActualNode;
		if (currentActualNode.hasBeenReset)
		{
			return;
		}
		bool num = RaceManager.Instance.CanCharacterDoGoapAction(this, currentActualNode.action.goapType);
		bool flag = InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentActualNode.action.goapType, currentActualNode.actor, currentActualNode.poiTarget, currentActualNode.otherData, goapPlanJob);
		if (num && flag)
		{
			if (!currentActualNode.action.CanSatisfyAllPreconditions(currentActualNode.actor, currentActualNode.poiTarget, currentActualNode.otherData, goapPlanJob.jobType, out var failedPrecondition))
			{
				if (assignedPlan.doNotRecalculate)
				{
					string reason = string.Empty;
					if (goapPlanJob.jobType.IsCultistJob())
					{
						reason = GetCultistUnableToDoJobReason(goapPlanJob, failedPrecondition, currentActualNode.goapType);
					}
					currentActualNode.action.OnStopWhileStarted(currentActualNode);
					goapPlanJob.CancelJob(reason);
				}
				else
				{
					planner.RecalculateJob(goapPlanJob);
				}
				return;
			}
			if (this is Troll && goapPlanJob.jobType == JOB_TYPE.CAPTURE_CHARACTER)
			{
				bool flag2 = false;
				if (!marker || (!marker.IsPOIInVision(goapPlanJob.targetPOI as Character) && !carryComponent.IsPOICarried(goapPlanJob.targetPOI) && !isAtHomeStructure && !IsInHomeSettlement()))
				{
					flag2 = true;
				}
				if (!flag2)
				{
					if (goapPlanJob.targetPOI.gridTileLocation == null)
					{
						flag2 = true;
					}
					else
					{
						TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
						if (currentTimeInWordsOfTick != TIME_IN_WORDS.EARLY_NIGHT && currentTimeInWordsOfTick != TIME_IN_WORDS.LATE_NIGHT && currentTimeInWordsOfTick != TIME_IN_WORDS.AFTER_MIDNIGHT && !goapPlanJob.targetPOI.gridTileLocation.structure.isInterior)
						{
							flag2 = true;
						}
					}
				}
				if (flag2)
				{
					currentActualNode.action.OnStopWhileStarted(currentActualNode);
					goapPlanJob.CancelJob();
					return;
				}
			}
			if (currentActualNode.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				Character character = currentActualNode.poiTarget as Character;
				if (!character.carryComponent.IsNotBeingCarried() && character.isBeingCarriedBy != this)
				{
					bool flag3 = true;
					if ((goapPlanJob.jobType == JOB_TYPE.SNATCH || goapPlanJob.jobType == JOB_TYPE.SNATCH_RESTRAIN) && partyComponent.hasParty && partyComponent.currentParty.currentQuest is DemonSnatchPartyQuest demonSnatchPartyQuest && demonSnatchPartyQuest.targetCharacter == character && character.isBeingCarriedBy.partyComponent.currentParty != partyComponent.currentParty)
					{
						if (IsHostileWith(character.isBeingCarriedBy))
						{
							currentActualNode.action.OnStopWhileStarted(currentActualNode);
							goapPlanJob.CancelJob();
							return;
						}
						flag3 = false;
					}
					if (flag3)
					{
						return;
					}
				}
			}
			if (traitContainer.HasTrait("Lazy"))
			{
				Lazy traitOrStatus = traitContainer.GetTraitOrStatus<Lazy>("Lazy");
				float triggerChance = traitOrStatus.GetTriggerChance(this);
				if (goapPlanJob.originalOwner != null && goapPlanJob.originalOwner.ownerType != JOB_OWNER.CHARACTER && GameUtilities.RollChance(triggerChance, ref log) && traitOrStatus.TriggerLazy())
				{
					if (currentActualNode != null && currentActualNode.action != null)
					{
						currentActualNode.action.OnStopWhileStarted(currentActualNode);
					}
					goapPlanJob.CancelJob();
					return;
				}
			}
			if (goapPlanJob.jobType.ShouldOnlyBeDoneIfTargetIsNearby())
			{
				LocationGridTile locationGridTile = gridTileLocation;
				LocationGridTile locationGridTile2 = goapPlanJob.poiTarget?.gridTileLocation;
				if (locationGridTile == null || locationGridTile2 == null || !locationGridTile.area.IsNearbyTo(locationGridTile2.area))
				{
					goapPlanJob.CancelJob();
					return;
				}
			}
			Messenger.Broadcast(JobSignals.CHARACTER_WILL_DO_JOB, this, goapPlanJob);
			currentActualNode.DoAction(goapPlanJob, assignedPlan);
			return;
		}
		string reason2 = string.Empty;
		if (currentActualNode.associatedJob != null && currentActualNode.associatedJob.jobType.IsCultistJob())
		{
			bool isOverridden;
			Precondition precondition = currentActualNode.action.GetPrecondition(this, currentActualNode.poiTarget, currentActualNode.otherData, goapPlanJob.jobType, out isOverridden);
			if (precondition != null)
			{
				reason2 = GetCultistUnableToDoJobReason(goapPlanJob, precondition, currentActualNode.action.goapType);
			}
		}
		currentActualNode.action.OnStopWhileStarted(currentActualNode);
		goapPlanJob.CancelJob(reason2);
	}

	public void PerformGoapAction()
	{
		_ = string.Empty;
		if (currentActionNode == null || (currentActionNode.associatedJob != null && currentActionNode.associatedJob.hasBeenReset))
		{
			return;
		}
		InnerMapManager.Instance.FaceTarget(this, currentActionNode.poiTarget);
		bool willStillContinueAction = true;
		OnStartPerformGoapAction(currentActionNode, ref willStillContinueAction);
		if (willStillContinueAction)
		{
			if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentActionNode.action.goapType, currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData, currentActionNode.associatedJob) && currentActionNode.action.CanSatisfyAllPreconditions(currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData, currentActionNode.associatedJobType))
			{
				currentActionNode.PerformAction();
				return;
			}
			if (currentPlan.doNotRecalculate)
			{
				currentJob.CancelJob();
				return;
			}
			planner.RecalculateJob(currentJob as GoapPlanJob);
			SetCurrentActionNode(null, null, null);
		}
	}

	public void GoapActionResult(string result, ActualGoapNode actionNode)
	{
		_ = string.Empty;
		GoapPlan goapPlan = currentPlan;
		GoapPlanJob goapPlanJob = currentJob as GoapPlanJob;
		if (actionNode == currentActionNode)
		{
			SetCurrentActionNode(null, null, null);
		}
		if (isDead || !limiterComponent.canPerform)
		{
			goapPlanJob.CancelJob();
		}
		else if (result == "Success")
		{
			OnCharacterFinishedActionSuccessfully(actionNode);
			Messenger.Broadcast(JobSignals.CHARACTER_DID_ACTION_SUCCESSFULLY, this, actionNode);
			goapPlan.SetNextNode();
			if (goapPlan.currentNode == null)
			{
				goapPlanJob.SetFinishedSuccessfully(state: true);
				OnCharacterFinishedJobSuccessfully(goapPlanJob);
				Messenger.Broadcast(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, this, goapPlanJob);
				goapPlanJob.ForceCancelJob();
			}
			else
			{
				SetCurrentJob(goapPlanJob);
			}
		}
		else if (result == "Fail")
		{
			if (goapPlan.doNotRecalculate)
			{
				goapPlanJob.CancelJob();
			}
			else
			{
				planner.RecalculateJob(goapPlanJob);
			}
		}
	}

	public void SetCurrentActionNode(ActualGoapNode actionNode, JobQueueItem job, GoapPlan plan)
	{
		if (currentActionNode != null && !currentActionNode.hasBeenReset)
		{
			previousCharacterDataComponent.SetPreviousActionType(currentActionNode.goapType);
			previousCharacterDataComponent.SetPreviousJobType(currentActionNode.associatedJobType);
		}
		currentActionNode = actionNode;
		SetCurrentJob(job);
		SetCurrentPlan(plan);
		if (hasMarker)
		{
			marker?.UpdateActionIcon();
		}
	}

	private void SetCurrentPlan(GoapPlan plan)
	{
		currentPlan = plan;
	}

	public bool StopCurrentActionNode(string reason = "")
	{
		if (currentActionNode == null)
		{
			return false;
		}
		if ((reason != "" && currentActionNode.poiTarget != this) ? true : false)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "current_action_abandoned_reason", LOG_TAG.Social);
			log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string localizedName = currentActionNode.action.localizedName;
			log.AddToFillers(null, localizedName, LOG_IDENTIFIER.STRING_1);
			string value = LocalizationManager.Instance.GetLocalizedValue("CancelReasons_Table", reason);
			if (string.IsNullOrEmpty(value))
			{
				value = reason;
			}
			log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_2);
			logComponent.RegisterLog(log, releaseAfter: true);
		}
		if ((object)marker != null)
		{
			marker.StopMovement();
		}
		if (currentActionNode.avoidCombat && (bool)marker)
		{
			marker.SetVisionColliderSize(8);
		}
		currentActionNode.StopActionNode();
		SetCurrentActionNode(null, null, null);
		if (carryComponent.IsNotBeingCarried() && carryComponent.isCarryingAnyPOI)
		{
			UncarryPOI();
		}
		if (UIManager.Instance.characterInfoUI.isShowing)
		{
			UIManager.Instance.characterInfoUI.UpdateBasicInfo();
		}
		return true;
	}

	private void OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction)
	{
		bool willStillContinueAction2 = true;
		List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions("Start_Perform_Trait");
		if (traitOverrideFunctions == null)
		{
			return;
		}
		for (int i = 0; i < traitOverrideFunctions.Count; i++)
		{
			if (traitOverrideFunctions[i].OnStartPerformGoapAction(node, ref willStillContinueAction2))
			{
				willStillContinueAction = willStillContinueAction2;
				break;
			}
			willStillContinueAction2 = true;
		}
	}

	protected void OnCharacterFinishedActionSuccessfully(ActualGoapNode p_action)
	{
		moneyComponent.GainCoinsAfterDoingAction(p_action);
	}

	protected void OnCharacterFinishedJobSuccessfully(JobQueueItem p_job)
	{
		needsComponent.OnCharacterFinishedJob(p_job);
		behaviourComponent.OnCharacterFinishedJob(p_job);
		traitComponent.OnCharacterFinishedJob(p_job);
		moneyComponent.GainCoinsAfterDoingJob(p_job);
		moodComponent.OnCharacterFinishedJob(p_job);
	}

	private void HeardAScream(Character characterThatScreamed)
	{
		if (!limiterComponent.canPerform || !limiterComponent.canWitness || currentRegion != characterThatScreamed.currentRegion)
		{
			return;
		}
		if (gridTileLocation != null && characterThatScreamed.gridTileLocation != null)
		{
			float distanceTo = gridTileLocation.GetDistanceTo(characterThatScreamed.gridTileLocation);
			float num = 10f;
			if (distanceTo > num)
			{
				return;
			}
		}
		if (!jobQueue.HasJob(JOB_TYPE.GO_TO, characterThatScreamed) && CanCharacterReact(characterThatScreamed))
		{
			ReactToScream(characterThatScreamed);
		}
	}

	private void ReactToScream(Character characterThatScreamed)
	{
		_ = string.Empty;
		bool flag = true;
		int jobTypePriority = JOB_TYPE.GO_TO.GetJobTypePriority();
		if (stateComponent.currentState != null && stateComponent.currentState.job != null && stateComponent.currentState.job.priority >= jobTypePriority)
		{
			flag = false;
		}
		else if (currentJob != null && currentJob.priority >= jobTypePriority)
		{
			flag = false;
		}
		if (flag)
		{
			jobComponent.CreateGoToJob(characterThatScreamed);
		}
	}

	public virtual void OnSeizePOI(bool wasUnseizedFromCharacter)
	{
		if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this)
		{
			UIManager.Instance.characterInfoUI.CloseMenu();
		}
		else if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == this)
		{
			UIManager.Instance.monsterInfoUI.CloseMenu();
		}
		if (trapStructure.IsTrapped())
		{
			trapStructure.ResetAllTrapStructures();
		}
		if (trapStructure.IsTrappedInArea())
		{
			trapStructure.ResetTrapArea();
		}
		RevertFromVampireBatForm();
		RevertFromWerewolfForm();
		minion?.OnSeize();
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, "");
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)this, "");
		jobQueue.CancelAllJobs();
		interruptComponent.OnSeizedOwner();
		tileObjectLocation?.RemoveUser(this);
		if (carryComponent.isCarryingAnyPOI)
		{
			carryComponent.UncarryPOI(carryComponent.carriedPOI);
		}
		UnsubscribeSignals();
		SetIsConversing(state: false);
		SetPOIState(POI_STATE.INACTIVE);
		partyComponent.UnfollowBeacon();
		if (hasMarker)
		{
			marker.StopMovement();
			marker.OnSeize();
			DisableMarker();
			Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, (IPointOfInterest)this);
		}
	}

	public virtual void OnUnseizePOI(LocationGridTile tileLocation)
	{
		if (minion == null && !isDead)
		{
			SubscribeToSignals();
		}
		SetPOIState(POI_STATE.ACTIVE);
		if (!marker)
		{
			CreateMarker();
		}
		else
		{
			marker.SetCharacter(this);
		}
		marker.UpdatePauseAnimationSpeed();
		EnableMarker();
		marker.OnUnseize();
		minion?.OnUnseize();
		if (tileLocation.structure.region != currentRegion)
		{
			currentRegion.RemoveCharacterFromLocation(this);
		}
		marker.InitialPlaceMarkerAt(tileLocation);
		tileLocation.structure.OnCharacterUnSeizedHere(this);
		needsComponent.CheckExtremeNeeds();
		if (isDead)
		{
			Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.BURY, (IPointOfInterest)this);
			jobComponent.TriggerBuryMe();
		}
		if (traitContainer.HasTrait("Berserked") && (bool)marker)
		{
			marker.BerserkedMarker();
		}
		if (isNormalCharacter && !traitContainer.HasTrait("Burning") && traitContainer.HasTrait("Flammable"))
		{
			if (tileLocation.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Burning"))
			{
				Burning traitOrStatus = tileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Burning>("Burning");
				traitContainer.AddTrait(this, "Burning", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Fire);
				traitContainer.GetTraitOrStatus<Burning>("Burning")?.SetIsPlayerSource(traitOrStatus.isPlayerSource);
			}
			else if (tileLocation.tileObjectComponent.objHere != null && tileLocation.tileObjectComponent.objHere.traitContainer.HasTrait("Burning"))
			{
				Burning traitOrStatus2 = tileLocation.tileObjectComponent.objHere.traitContainer.GetTraitOrStatus<Burning>("Burning");
				traitContainer.AddTrait(this, "Burning", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Fire);
				traitContainer.GetTraitOrStatus<Burning>("Burning")?.SetIsPlayerSource(traitOrStatus2.isPlayerSource);
			}
		}
		if (!tileLocation.structure.structureType.IsPlayerStructure())
		{
			Prisoner traitOrStatus3 = traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus3 != null && traitOrStatus3.IsFactionPrisonerOf(PlayerManager.Instance.player.playerFaction))
			{
				traitContainer.RemoveRestrainAndImprison(this);
				if (isLycanthrope)
				{
					lycanData.limboForm.traitContainer.RemoveRestrainAndImprison(lycanData.limboForm);
				}
			}
		}
		if ((race == RACE.RATMAN || this is Summon) && faction != null && !faction.isMajorFaction && tileLocation.structure != null && CharacterManager.Instance.ShouldMonsterRelocateHomeToStructureOnPlace(tileLocation.structure, this))
		{
			ClearTerritoryAndMigrateHomeStructureTo(tileLocation.structure);
		}
		if (!tileLocation.charactersHere.Contains(this))
		{
			tileLocation.AddCharacterHere(this);
		}
		marker.UpdateAnimation();
	}

	public bool IsHostileWith(Character otherCharacter)
	{
		return IsHostileWithCheckingForInformed(otherCharacter);
	}

	public bool IsHostileWithCheckingForInformed(Character otherCharacter)
	{
		if (faction != null && faction.isPlayerFaction && otherCharacter.isAlliedWithPlayer)
		{
			return false;
		}
		if (isAlliedWithPlayer && otherCharacter.faction != null && otherCharacter.faction.isPlayerFaction)
		{
			return false;
		}
		if (CharacterManager.Instance.IsCultistOfSameReligion(this, otherCharacter))
		{
			return false;
		}
		Faction obj = faction;
		if ((obj == null || obj.factionType.type != FACTION_TYPE.Undead) && hasMarker && marker.IsPOIInVision(otherCharacter))
		{
			if (otherCharacter.isInVampireBatForm)
			{
				if (!traitContainer.HasTrait("Vampire") && !otherCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire").DoesCharacterKnowThisVampire(this))
				{
					return true;
				}
			}
			else if (otherCharacter.isInWerewolfForm && !isLycanthrope && !otherCharacter.lycanData.DoesCharacterKnowThisLycan(this))
			{
				return true;
			}
		}
		if (faction == null || otherCharacter.faction == null)
		{
			return false;
		}
		if (((race == RACE.RATMAN || faction.factionType.type == FACTION_TYPE.Ratmen) && otherCharacter.race == RACE.RAT) || (race == RACE.RAT && (otherCharacter.race == RACE.RATMAN || otherCharacter.faction.factionType.type == FACTION_TYPE.Ratmen)))
		{
			return false;
		}
		if (faction != otherCharacter.faction)
		{
			if (faction != null && faction.isMajorOrVagrant && otherCharacter.traitContainer.HasTrait("Transitioning"))
			{
				Faction obj2 = otherCharacter.faction;
				if (obj2 == null || obj2.factionType.type != FACTION_TYPE.Wild_Monsters)
				{
					return false;
				}
			}
			if (otherCharacter.faction != null && otherCharacter.faction.isMajorOrVagrant && traitContainer.HasTrait("Transitioning"))
			{
				return false;
			}
		}
		return faction.IsHostileWith(otherCharacter.faction);
	}

	public bool IsLycanHostileWith(Character targetCharacter)
	{
		if ((isLycanthrope && targetCharacter.race == RACE.WOLF) || (targetCharacter.isLycanthrope && race == RACE.WOLF))
		{
			return false;
		}
		if (race == RACE.WOLF && targetCharacter.faction != null && targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves))
		{
			return false;
		}
		if ((targetCharacter.race == RACE.WOLF || (targetCharacter.isLycanthrope && targetCharacter.lycanData.isInWerewolfForm)) && faction != null && faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves))
		{
			return false;
		}
		return true;
	}

	public void PopulateTilesInRadius(List<LocationGridTile> tiles, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false)
	{
		if (currentRegion == null)
		{
			return;
		}
		int upperBound = currentRegion.innerMap.map.GetUpperBound(0);
		int upperBound2 = currentRegion.innerMap.map.GetUpperBound(1);
		int x = gridTileLocation.localPlace.x;
		int y = gridTileLocation.localPlace.y;
		if (includeCenterTile)
		{
			tiles.Add(gridTileLocation);
		}
		int num = x - radiusLimit;
		int num2 = x + radiusLimit;
		int num3 = y - radiusLimit;
		int num4 = y + radiusLimit;
		for (int i = x - radius; i <= x + radius; i++)
		{
			for (int j = y - radius; j <= y + radius; j++)
			{
				if (i >= 0 && i <= upperBound && j >= 0 && j <= upperBound2 && (i != x || j != y) && (radiusLimit <= 0 || i <= num || i >= num2 || j <= num3 || j >= num4))
				{
					LocationGridTile locationGridTile = currentRegion.innerMap.map[i, j];
					if (includeTilesInDifferentStructure || locationGridTile.structure == gridTileLocation.structure)
					{
						tiles.Add(locationGridTile);
					}
				}
			}
		}
	}

	private bool IsAlliedWithPlayer()
	{
		if (traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
		{
			return true;
		}
		if (this.faction != null)
		{
			if (this.faction.isPlayerFaction)
			{
				return true;
			}
			Faction faction = null;
			if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
			{
				faction = PlayerManager.Instance.player.playerFaction;
			}
			if (faction != null && this.faction.IsFriendlyWith(faction))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsNotHostileWithPlayer()
	{
		if (traitContainer.HasTrait("Demon Cultist"))
		{
			return true;
		}
		if (this.faction != null)
		{
			if (this.faction.isPlayerFaction)
			{
				return true;
			}
			Faction faction = null;
			if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
			{
				faction = PlayerManager.Instance.player.playerFaction;
			}
			if (faction != null && (this.faction == faction || !this.faction.IsHostileWith(faction)))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
	}

	public virtual void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		if (character == this && job is GoapPlanJob { isAgitateJob: not false })
		{
			behaviourComponent.SetIsAgitated(state: false);
			behaviourComponent.SetIsAgitated(state: true, GameManager.Instance.GetTicksBasedOnHour(2));
		}
		if (job.originalOwner == this)
		{
			JobManager.Instance.ReleaseJob(job);
		}
	}

	public bool ForceCancelJob(JobQueueItem job)
	{
		return true;
	}

	public void AddForcedCancelJobsOnTickEnded(JobQueueItem job)
	{
		if (!forcedCancelJobsOnTickEnded.Contains(job))
		{
			forcedCancelJobsOnTickEnded.Add(job);
		}
	}

	public bool WillCancelJobOnTickEnded(JobQueueItem job)
	{
		return forcedCancelJobsOnTickEnded.Contains(job);
	}

	public void ProcessForcedCancelJobsOnTickEnded()
	{
		if (forcedCancelJobsOnTickEnded.Count > 0)
		{
			for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++)
			{
				forcedCancelJobsOnTickEnded[i].ForceCancelJob();
			}
			forcedCancelJobsOnTickEnded.Clear();
		}
	}

	public void ForceCancelJobTypesTargetingPOI(JOB_TYPE jobType, IPointOfInterest target)
	{
		ForceCancelAllJobsOfTypeTargetingPOI(target, string.Empty, jobType);
	}

	public bool CanBeDamaged()
	{
		return !traitContainer.HasTrait("Indestructible");
	}

	public Vector3 GetProjectileTargetPosition()
	{
		Vector3 result = gridTileLocation.centeredWorldLocation;
		if (projectileReceiver != null)
		{
			result = projectileReceiver.transform.position;
		}
		else if (mapObjectVisual != null)
		{
			result = mapObjectVisual.transform.position;
		}
		return result;
	}

	public void SetLycanthropeData(LycanthropeData data)
	{
		lycanData = data;
	}

	public void SetIsInWerewolfForm(bool state)
	{
		if (isLycanthrope)
		{
			lycanData.SetIsInWerewolfForm(state);
		}
	}

	public void TransformToWerewolfForm()
	{
		if (isLycanthrope && !lycanData.isInWerewolfForm)
		{
			lycanData.SetIsInWerewolfForm(state: true);
			classComponent.AssignClass("Werewolf");
			if (visuals != null)
			{
				visuals.UpdateAllVisuals(this);
			}
		}
	}

	public void RevertFromWerewolfForm()
	{
		if (isLycanthrope && lycanData.isInWerewolfForm)
		{
			lycanData.SetIsInWerewolfForm(state: false);
			if (classComponent.previousClassName != "Werewolf" && !string.IsNullOrEmpty(classComponent.previousClassName))
			{
				classComponent.AssignClass(classComponent.previousClassName);
			}
			if (visuals != null)
			{
				visuals.UpdateAllVisuals(this);
			}
		}
	}

	public virtual void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		if (actions == null)
		{
			actions = new List<PLAYER_SKILL_TYPE>();
		}
		else
		{
			actions.Clear();
		}
		if (race == RACE.DEMON)
		{
			if (faction != null && faction.factionType.type != FACTION_TYPE.Demons)
			{
				AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_CHARACTER, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.SCHEME, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.ZAP, broadcastSignal);
			}
		}
		else
		{
			if (isNormalCharacter)
			{
				AddPlayerAction(PLAYER_SKILL_TYPE.AFFLICT, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.ZAP, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.TRIGGER_FLAW, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.RAISE_DEAD, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.CRITICAL_BREAK, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.REFRESH, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.FOUND_FACTION, broadcastSignal);
				AddPlayerAction(PLAYER_SKILL_TYPE.GLOOM, broadcastSignal);
			}
			AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_CHARACTER, broadcastSignal);
			AddPlayerAction(PLAYER_SKILL_TYPE.SCHEME, broadcastSignal);
			AddPlayerAction(PLAYER_SKILL_TYPE.TORTURE, broadcastSignal);
			AddPlayerAction(PLAYER_SKILL_TYPE.BRAINWASH, broadcastSignal);
			AddPlayerAction(PLAYER_SKILL_TYPE.EXPEL, broadcastSignal);
		}
		AddPlayerAction(PLAYER_SKILL_TYPE.RELEASE, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.HEAL, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.REMOVE_BUFF, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.REMOVE_FLAW, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.DRAIN_SPIRIT, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.LET_GO, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.FULL_HEAL, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.CREATE_BLACKMAIL, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.EMPOWER, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_VILLAGER, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.KILL_VILLAGER, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.FINGER_OF_DEATH, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.HELLSPAWN, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.TRIGGER_AROUSAL, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.TRIGGER_GRUDGE, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.UNDEPLOY_PARTY, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_MONSTER, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.CREATE_CAPTIVE_INTEL, broadcastSignal);
	}

	public void AddPlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (!actions.Contains(action))
		{
			actions.Add(action);
			if (broadcastSignal)
			{
				Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, (IPlayerActionTarget)this);
			}
		}
	}

	public void RemovePlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (actions.Remove(action) && broadcastSignal)
		{
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, (IPlayerActionTarget)this);
		}
	}

	public void ClearPlayerActions()
	{
		actions.Clear();
	}

	public virtual bool IsCurrentlySelected()
	{
		Character character = this;
		if (isLycanthrope)
		{
			character = lycanData.activeForm;
		}
		if (character.isNormalCharacter)
		{
			if (UIManager.Instance.characterInfoUI.isShowing)
			{
				return UIManager.Instance.characterInfoUI.activeCharacter == character;
			}
			return false;
		}
		if (UIManager.Instance.monsterInfoUI.isShowing)
		{
			return UIManager.Instance.monsterInfoUI.activeMonster == character;
		}
		return false;
	}

	public void LeftSelectAction()
	{
		if (mapObjectVisual != null)
		{
			mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Left);
		}
		else
		{
			UIManager.Instance.ShowCharacterInfo(this, centerOnCharacter: true);
		}
	}

	public void RightSelectAction()
	{
		mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Right);
	}

	public void MiddleSelectAction()
	{
		mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Middle);
	}

	public bool CanBeSelected()
	{
		if (hasMarker && !marker.IsShowingVisuals())
		{
			return false;
		}
		return true;
	}

	public void SetTerritory([NotNull] Area p_area, bool returnHome = true)
	{
		if (territory == p_area)
		{
			return;
		}
		territory = p_area;
		if (territory.region != homeRegion)
		{
			if (homeRegion != null)
			{
				homeRegion.RemoveResident(this);
			}
			territory.region.AddResident(this);
		}
		if (homeStructure != null && homeStructure.hasBeenDestroyed)
		{
			MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: true, affectSettlement: false);
		}
		if (returnHome)
		{
			jobComponent.PlanReturnHome(JOB_TYPE.RETURN_HOME_URGENT);
		}
	}

	public void ClearTerritory()
	{
		territory = null;
	}

	public bool HasTerritory()
	{
		return territory != null;
	}

	public bool IsTerritory(Area p_area)
	{
		if (HasTerritory())
		{
			return territory == p_area;
		}
		return false;
	}

	public bool IsInTerritory()
	{
		Area area = areaLocation;
		if (area != null)
		{
			return IsTerritory(area);
		}
		return false;
	}

	public bool IsInTerritoryOf(Character character)
	{
		Area area = areaLocation;
		if (area != null)
		{
			return character.IsTerritory(area);
		}
		return false;
	}

	public LocationGridTile GetRandomLocationGridTileWithPath()
	{
		LocationGridTile result = null;
		if (HasTerritory())
		{
			LocationGridTile locationGridTile = territory.gridTileComponent.gridTiles[UnityEngine.Random.Range(0, territory.gridTileComponent.gridTiles.Count)];
			if (movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
			{
				result = locationGridTile;
			}
		}
		return result;
	}

	public void SetIsDead(bool isDead)
	{
		if (_isDead == isDead)
		{
			return;
		}
		_isDead = isDead;
		if (_isDead)
		{
			if (faction == FactionManager.Instance.vagrantFaction && crimeComponent.HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous))
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<BanditsEvent>().AdjustNumberOfAliveVagrantsWithSeriousOrHeinousCrime(-1);
			}
			if (traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<CultLeaderEvent>().AdjustNumberOfAliveDemonCultists(-1);
			}
			if (classComponent.characterClass.IsReligiousCultLeaderClass(RELIGION.Demon_Worship))
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<CultLeaderEvent>().AdjustNumberOfAliveCultLeaders(-1);
			}
			if (traitContainer.HasTrait("Plagued") && isVillager)
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<RatmenEvent>().AdjustNumberOfPlaguedVillagers(-1);
			}
		}
	}

	public void ReturnToLife(Faction faction, RACE race, string className)
	{
		if (_isDead)
		{
			classComponent.AssignClass(className);
			AssignRace(race);
			ReturnToLife();
			ChangeFactionTo(faction);
		}
	}

	public bool ReturnToLife()
	{
		if (_isDead)
		{
			SetIsDead(isDead: false);
			SubscribeToSignals();
			ResetToFullHP();
			SetPOIState(POI_STATE.ACTIVE);
			needsComponent.ResetFullnessMeter();
			needsComponent.ResetTirednessMeter();
			needsComponent.ResetHappinessMeter();
			marker.OnReturnToLife();
			if (grave != null)
			{
				Tombstone tombstone = grave;
				grave.gridTileLocation.structure.RemovePOI(grave);
				SetGrave(null);
				marker.PlaceMarkerAt(tombstone.previousTile);
			}
			traitContainer.RemoveTrait(this, "Dead");
			visuals.UpdateAllVisuals(this);
			ConstructDefaultPlayerActions();
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, "");
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)this, "");
			Messenger.Broadcast(CharacterSignals.CHARACTER_RETURNED_TO_LIFE, this);
			return true;
		}
		return false;
	}

	public virtual void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] multipleDeathLogFillers = null, LogFiller p_singleDeathLogFiller = null, Interrupt interrupt = null, bool isPlayerSource = false, object deathSource = null, ELEMENTAL_TYPE elementType = ELEMENTAL_TYPE.Normal)
	{
		if (mountComponent.IsBeingMounted())
		{
			mountComponent.GetRider()?.mountComponent.Dismount();
		}
		else if (mountComponent.IsMounting())
		{
			mountComponent.Dismount();
		}
		if (GameManager.Instance.gameHasStarted && (gridTileLocation == null || !gridTileLocation.IsPassable()))
		{
			SetDestroyMarkerOnDeath(state: true);
		}
		if (minion != null)
		{
			minion.Death(cause, deathFromAction, responsibleCharacter, _deathLog, multipleDeathLogFillers, p_singleDeathLogFiller);
		}
		else
		{
			if (_isDead)
			{
				return;
			}
			if (race.IsSapient() && hasMarker && GameManager.Instance.gameHasStarted && !(deathSource is LocationStructureObject))
			{
				AkSoundEngine.PostEvent("Play_Death", marker.gameObject);
			}
			if (isBeingSeized)
			{
				PlayerManager.Instance.player.seizeComponent.UnseizePOIOnCharacterDeath();
			}
			SetDeathLocation(gridTileLocation);
			SetIsConversing(state: false);
			_ = currentRegion;
			_ = currentStructure;
			if (deathFromAction != null && deathFromAction.action != null)
			{
				causeOfDeath = deathFromAction.action.goapType;
			}
			if (isPlayerSource)
			{
				PlayerManager.Instance?.player?.retaliationComponent.CharacterDeathRetaliation(this);
				if (isNormalCharacter)
				{
					Messenger.Broadcast(CharacterSignals.CHARACTER_DIED_FROM_PLAYER_SOURCE, this);
				}
			}
			if (isNormalCharacter && (isPlayerSource || (responsibleCharacter?.faction != null && responsibleCharacter.faction.factionType.type == FACTION_TYPE.Demons)))
			{
				bool flag = true;
				if (isPlayerSource && deathSource is SkillData { type: PLAYER_SKILL_TYPE.LANDMINE })
				{
					flag = false;
				}
				if (flag)
				{
					bool flag2 = faction.factionType is CultFaction;
					faction.factionType.GetCrimeSeverity(CRIME_TYPE.Demon_Worship);
					if (faction != null && faction.isMajorNonPlayer && !flag2)
					{
						bool flag3 = false;
						if (homeSettlement != null && currentSettlement == homeSettlement)
						{
							flag3 = true;
						}
						else if (currentSettlement != null && currentSettlement.owner == faction)
						{
							flag3 = true;
						}
						if (flag3)
						{
							faction.TrySetDemonWorshipAsHeinousCrimeBecauseOfPlayer();
						}
					}
				}
			}
			List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions("Death_Trait");
			if (traitOverrideFunctions != null)
			{
				for (int i = 0; i < traitOverrideFunctions.Count; i++)
				{
					if (traitOverrideFunctions[i].OnDeath(this))
					{
						i--;
					}
				}
			}
			if (isLycanthrope)
			{
				lycanData.LycanDies(this, cause, deathFromAction, responsibleCharacter, _deathLog, multipleDeathLogFillers, p_singleDeathLogFiller, deathSource);
			}
			SetIsDead(isDead: true);
			petComponent.OnComponentOwnerDied();
			moodComponent.OnOwnerDied();
			behaviourComponent.OnOwnerDied();
			base.relationshipContainer.OnOwnerDied(this);
			if (isLimboCharacter && isInLimbo)
			{
				CharacterManager.Instance.RemoveLimboCharacter(this);
				return;
			}
			reactionComponent.SetDisguisedCharacter(null);
			if (responsibleCharacter != null)
			{
				reactionComponent.AddCharacterThatSawThisDead(responsibleCharacter);
			}
			UnsubscribeSignals();
			SetPOIState(POI_STATE.INACTIVE);
			ProcessBeforeDeath(cause, responsibleCharacter);
			traitContainer.RemoveTrait(this, "Necromancer");
			if (currentRegion == null)
			{
				throw new Exception("Current Region Location of " + name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
			}
			if (stateComponent.currentState != null)
			{
				stateComponent.ExitCurrentState();
			}
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, GoapPlanJob.Target_Already_Dead_Reason);
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)this, GoapPlanJob.Target_Already_Dead_Reason);
			jobQueue.CancelAllJobs();
			DropAllItems(deathTilePosition);
			UnownOrTransferOwnershipOfAllItems();
			reactionComponent.SetIsHidden(state: false);
			UncarryPOI();
			isBeingCarriedBy?.UncarryPOI(this);
			previousCharacterDataComponent.SetHomeSettlementOnDeath(homeSettlement);
			if (homeRegion != null)
			{
				Region region = homeRegion;
				_ = homeStructure;
				homeRegion.RemoveResident(this);
				MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: false);
				SetHomeRegion(region);
			}
			if (partyComponent.hasParty)
			{
				partyComponent.currentParty.RemoveMember(this);
			}
			SetHP(0);
			if (structureComponent.HasWorkPlaceStructure() && structureComponent.workPlaceStructure.DoesCharacterWorkHere(this))
			{
				structureComponent.workPlaceStructure.RemoveAssignedWorker(this);
			}
			if (interruptComponent.isInterrupted && interruptComponent.currentInterrupt.interrupt != interrupt)
			{
				interruptComponent.ForceEndNonSimultaneousInterrupt();
			}
			traitContainer.AddTrait(this, "Dead", responsibleCharacter);
			if (deathFromAction != null)
			{
				traitContainer.GetTraitOrStatus<Trait>("Dead")?.SetGainedFromDoingAction(deathFromAction.action.goapType, deathFromAction.isStealth);
			}
			else if (cause == "burn_at_stake")
			{
				traitContainer.GetTraitOrStatus<Trait>("Dead")?.SetGainedFromDoingAction(INTERACTION_TYPE.BURN_AT_STAKE, p_actionStealth: false);
			}
			if (cause == "attacked" && responsibleCharacter != null)
			{
				if (responsibleCharacter.isInWerewolfForm)
				{
					traitContainer.AddTrait(this, "Mangled", responsibleCharacter);
					if (deathFromAction != null)
					{
						traitContainer.GetTraitOrStatus<Trait>("Mangled")?.SetGainedFromDoingAction(deathFromAction.action.goapType, deathFromAction.isStealth);
					}
				}
				FactionType factionType = faction?.factionType;
				if (factionType != null && (factionType.type == FACTION_TYPE.Undead || factionType.type == FACTION_TYPE.Wild_Monsters) && responsibleCharacter.partyComponent.isMemberThatJoinedQuest && !responsibleCharacter.partyComponent.currentParty.isPlayerParty)
				{
					responsibleCharacter.partyComponent.currentParty.AllMembersThatJoinedQuestGainsRandomCoinAmount(5, 10);
				}
			}
			if (_deathLog == null)
			{
				if (cause == "attacked")
				{
				}
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterDeath_Table", "death_" + cause, LOG_TAG.Life_Changes);
				log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				if (responsibleCharacter != null)
				{
					log.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				}
				if (p_singleDeathLogFiller != null)
				{
					log.AddToFillers(p_singleDeathLogFiller);
				}
				if (multipleDeathLogFillers != null)
				{
					for (int j = 0; j < multipleDeathLogFillers.Length; j++)
					{
						log.AddToFillers(multipleDeathLogFillers[j]);
					}
				}
				log.AddLogToDatabase();
				PlayerManager.Instance.player?.ShowNotificationFrom(this, log);
				SetDeathLog(log);
				LogPool.Release(log);
			}
			else
			{
				SetDeathLog(_deathLog);
			}
			Messenger.Broadcast(CharacterSignals.CHARACTER_DEATH, this);
			eventDispatcher.ExecuteCharacterDied(this);
			List<Trait> traitOverrideFunctions2 = traitContainer.GetTraitOverrideFunctions("After_Death");
			if (traitOverrideFunctions2 != null)
			{
				for (int k = 0; k < traitOverrideFunctions2.Count; k++)
				{
					traitOverrideFunctions2[k].AfterDeath(this);
				}
			}
			if (hasMarker)
			{
				marker.UpdateName();
			}
			wasKilledByPlayerSource = isPlayerSource;
			if (isPlayerSource)
			{
				if (race == RACE.HUMANS)
				{
					PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_KILL_HUMAN);
				}
				else if (race == RACE.ELVES)
				{
					PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_KILL_ELF);
				}
			}
			marker?.OnDeath(deathTilePosition);
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
			if (faction != null)
			{
				faction.ideologyComponent.OnFactionMemberDied(this);
			}
		}
	}

	public void SetDeathLog(Log log)
	{
		if (deathLog != null)
		{
			LogPool.Release(deathLog);
		}
		deathLog = GameManager.CreateNewLog();
		deathLog.Copy(log);
		if (!deathLog.hasBeenFinalized)
		{
			deathLog.FinalizeText();
		}
	}

	public void SetGrave(Tombstone grave)
	{
		this.grave = grave;
	}

	public void SetNecromancerTrait(Necromancer necromancer)
	{
		necromancerTrait = necromancer;
	}

	public bool CanFeelEmotion(EMOTION emotion)
	{
		if (emotion == EMOTION.Anger || emotion == EMOTION.Rage)
		{
			return characterClass.className != "Hero";
		}
		return true;
	}

	public bool IsInPrison()
	{
		BaseSettlement baseSettlement = currentSettlement;
		if (baseSettlement != null && baseSettlement is NPCSettlement nPCSettlement)
		{
			return currentStructure == nPCSettlement.prison;
		}
		return false;
	}

	public bool IsInPrisonOf(NPCSettlement settlement)
	{
		BaseSettlement baseSettlement = currentSettlement;
		if (baseSettlement != null && baseSettlement == settlement)
		{
			return currentStructure == settlement.prison;
		}
		return false;
	}

	public bool IsInPrisonOf(BaseSettlement settlement)
	{
		if (settlement is NPCSettlement settlement2)
		{
			return IsInPrisonOf(settlement2);
		}
		return false;
	}

	public LocationStructure GetSettlementPrisonFor(Character character)
	{
		NPCSettlement nPCSettlement = null;
		if (homeSettlement != null)
		{
			nPCSettlement = ((currentSettlement == null) ? homeSettlement : ((currentSettlement == homeSettlement) ? homeSettlement : ((!(currentSettlement is NPCSettlement { owner: not null } nPCSettlement2) || faction == null || nPCSettlement2.owner != faction) ? homeSettlement : nPCSettlement2)));
		}
		return nPCSettlement?.prison;
	}

	private bool IsInVampireBatForm()
	{
		return traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.isInVampireBatForm ?? false;
	}

	public void TransformToVampireBatForm()
	{
		Vampire traitOrStatus = traitContainer.GetTraitOrStatus<Vampire>("Vampire");
		if (traitOrStatus != null && !traitOrStatus.isInVampireBatForm)
		{
			traitOrStatus.SetIsInVampireBatForm(state: true);
			movementComponent.AdjustSpeedModifier(0.2f);
			movementComponent.SetToFlying();
			if (visuals != null)
			{
				visuals.UpdateAllVisuals(this);
			}
			AkSoundEngine.PostEvent("Play_Vampire_Transform", marker.gameObject);
		}
	}

	public void RevertFromVampireBatForm()
	{
		Vampire traitOrStatus = traitContainer.GetTraitOrStatus<Vampire>("Vampire");
		if (traitOrStatus != null && traitOrStatus.isInVampireBatForm)
		{
			traitOrStatus.RevertFromVampireBatForm(this);
		}
	}

	public void PopulateListOfCultistTargets(List<Character> choices, Func<Character, bool> criteria)
	{
		for (int i = 0; i < base.relationshipContainer.charactersWithOpinion.Count; i++)
		{
			Character character = base.relationshipContainer.charactersWithOpinion[i];
			if (criteria(character))
			{
				choices.Add(character);
			}
		}
	}

	public bool IsPOICurrentlyTargetedByOtherCharacterPerformingAction()
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i] is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = allJobsTargetingThis[i] as GoapPlanJob;
				if (goapPlanJob.assignedPlan != null && goapPlanJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING && goapPlanJob.assignedPlan.currentActualNode.actor != this)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsPOICurrentlyTargetedByAPerformingAction(params JOB_TYPE[] jobType)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			for (int j = 0; j < jobType.Length; j++)
			{
				if (jobType[j] == jobQueueItem.jobType && jobQueueItem is GoapPlanJob { assignedPlan: not null } goapPlanJob && goapPlanJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE p_jobType, GoapEffect p_goapEffect)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i] is GoapPlanJob { assignedPlan: not null } goapPlanJob && goapPlanJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING && goapPlanJob.goal != null && goapPlanJob.goal.IsSameGoal(p_goapEffect))
			{
				return true;
			}
		}
		return false;
	}

	public void SetCurrentLocationAwareness(ILocationAwareness locationAwareness)
	{
		currentLocationAwareness = locationAwareness;
	}

	public bool IsUnpassable()
	{
		return false;
	}

	public virtual bool CanBeSeenBy(Character p_character)
	{
		if (p_character.hasMarker)
		{
			return p_character.marker.inVisionCharacters.Contains(this);
		}
		return false;
	}

	public void OnAddedAsUnprocessedPOI(Character p_characterThatAddedPOI)
	{
	}

	private string GetUIString()
	{
		if (string.IsNullOrEmpty(_uiString))
		{
			string text = GetType().ToString() + "|" + persistentID;
			_uiString = visuals.GetCharacterStringIcon() + "<link=" + text + ">" + Utilities.ColorizeName(name, CharacterManager.Instance.GetCharacterNameColorHex(this)) + "</link>";
		}
		return _uiString;
	}

	public void ResetUIString()
	{
		_uiString = string.Empty;
	}

	public bool IsValidForStoreTarget()
	{
		if (!isDead && PlayerManager.Instance.player != null)
		{
			return faction != PlayerManager.Instance.player.playerFaction;
		}
		return false;
	}

	public bool CanBeStoredAsTarget()
	{
		if (traitContainer.HasTrait("Temporal"))
		{
			return false;
		}
		if (traitContainer.HasTrait("Slick"))
		{
			return false;
		}
		return true;
	}

	public void SetAsStoredTarget(bool p_state)
	{
		isStoredAsTarget = p_state;
	}

	public bool IsValidTargetForPartyStructure(LocationStructure p_structure)
	{
		if (isTargetted || (faction != null && faction.isPlayerFaction) || isTrappedInsideDemonicRoom)
		{
			return false;
		}
		if (p_structure.partyStructureComponent.structurePartyType == STRUCTURE_PARTY_TYPE.Snatch_Villager || p_structure.partyStructureComponent.structurePartyType == STRUCTURE_PARTY_TYPE.Snatch_Monster)
		{
			if (traitContainer.HasTrait("Sturdy"))
			{
				return false;
			}
			if (traitContainer.HasTrait("Temporal"))
			{
				return false;
			}
			return true;
		}
		if (p_structure.partyStructureComponent.structurePartyType == STRUCTURE_PARTY_TYPE.Kill)
		{
			return !isDead;
		}
		return true;
	}

	public Sprite GetPortraitSprite()
	{
		Sprite sprite = null;
		string classToUseForPortrait = visuals.portraitSettings.GetClassToUseForPortrait();
		if (!string.IsNullOrEmpty(classToUseForPortrait))
		{
			sprite = CharacterManager.Instance.GetCharacterClass(classToUseForPortrait)?.portraitSprite;
		}
		if (sprite == null)
		{
			sprite = CharacterManager.Instance.portraitCollection.GetPortraitAsset(visuals.portraitSettings);
		}
		return sprite;
	}

	public bool HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE p_afflictionType)
	{
		return afflictionsSkillsInflictedByPlayer.Contains(p_afflictionType);
	}

	public bool HasAfflictedByPlayerWith(string p_traitName)
	{
		PLAYER_SKILL_TYPE afflictionTypeByTraitName = PlayerSkillManager.Instance.GetAfflictionTypeByTraitName(p_traitName);
		if (afflictionTypeByTraitName != PLAYER_SKILL_TYPE.NONE)
		{
			return afflictionsSkillsInflictedByPlayer.Contains(afflictionTypeByTraitName);
		}
		return false;
	}

	public bool HasAfflictedByPlayerWith(Trait p_trait)
	{
		return HasAfflictedByPlayerWith(p_trait.name);
	}

	public void AddAfflictionByPlayer(string p_traitName)
	{
		PLAYER_SKILL_TYPE afflictionTypeByTraitName = PlayerSkillManager.Instance.GetAfflictionTypeByTraitName(p_traitName);
		if (afflictionTypeByTraitName != PLAYER_SKILL_TYPE.NONE && !afflictionsSkillsInflictedByPlayer.Contains(afflictionTypeByTraitName))
		{
			afflictionsSkillsInflictedByPlayer.Add(afflictionTypeByTraitName);
		}
	}

	public virtual void LoadReferences(SaveDataCharacter data)
	{
		isInfoUnlocked = data.isInfoUnlocked;
		ConstructDefaultPlayerActions(broadcastSignal: false);
		if (data.hasLycan && lycanData == null)
		{
			data.lycanData.Load();
		}
		if (!string.IsNullOrEmpty(data.grave))
		{
			grave = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.grave) as Tombstone;
		}
		if (data.deathLog != null)
		{
			deathLog = data.deathLog;
		}
		homeRegion = DatabaseManager.Instance.regionDatabase.mainRegion;
		if (!string.IsNullOrEmpty(data.homeSettlement))
		{
			homeSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.homeSettlement) as NPCSettlement;
		}
		if (!string.IsNullOrEmpty(data.homeStructure))
		{
			homeStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.homeStructure);
		}
		_currentRegion = DatabaseManager.Instance.regionDatabase.mainRegion;
		if (!string.IsNullOrEmpty(data.currentStructure))
		{
			_currentStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.currentStructure);
			_ = _currentStructure;
		}
		if (!string.IsNullOrEmpty(data.faction))
		{
			_faction = FactionManager.Instance.GetFactionByPersistentID(data.faction);
		}
		if (!string.IsNullOrEmpty(data.currentJob))
		{
			currentJob = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.currentJob);
		}
		if (!string.IsNullOrEmpty(data.currentActionNode))
		{
			currentActionNode = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.currentActionNode);
		}
		if (!string.IsNullOrEmpty(data.territory))
		{
			territory = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.territory);
		}
		if (!string.IsNullOrEmpty(data.deployedAtStructure))
		{
			deployedAtStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.deployedAtStructure);
		}
		for (int i = 0; i < data.items.Count; i++)
		{
			TileObject tileObjectByPersistentID = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.items[i]);
			if (tileObjectByPersistentID != null)
			{
				items.Add(tileObjectByPersistentID);
			}
		}
		for (int j = 0; j < data.ownedItems.Count; j++)
		{
			TileObject tileObjectByPersistentID2 = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.ownedItems[j]);
			if (tileObjectByPersistentID2 != null)
			{
				ownedItems.Add(tileObjectByPersistentID2);
			}
		}
		jobQueue.LoadReferences(data);
		for (int k = 0; k < data.forceCancelJobsOnTickEnded.Count; k++)
		{
			string text = data.forceCancelJobsOnTickEnded[k];
			JobQueueItem jobWithPersistentID = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(text);
			if (jobWithPersistentID != null && !forcedCancelJobsOnTickEnded.Contains(jobWithPersistentID))
			{
				forcedCancelJobsOnTickEnded.Add(jobWithPersistentID);
			}
		}
		trapStructure.LoadReferences(data.trapStructure);
		needsComponent.LoadReferences(data.needsComponent);
		structureComponent.LoadReferences(data.structureComponent);
		stateComponent.LoadReferences(data.stateComponent);
		nonActionEventsComponent.LoadReferences(data.nonActionEventsComponent);
		interruptComponent.LoadReferences(data.interruptComponent);
		behaviourComponent.LoadReferences(data.behaviourComponent);
		moodComponent.LoadReferences(data.moodComponent);
		jobComponent.LoadReferences(data.jobComponent);
		reactionComponent.LoadReferences(data.reactionComponent);
		combatComponent.LoadReferences(data.combatComponent);
		rumorComponent.LoadReferences(data.rumorComponent);
		assumptionComponent.LoadReferences(data.assumptionComponent);
		movementComponent.LoadReferences(data.movementComponent);
		stateAwarenessComponent.LoadReferences(data.stateAwarenessComponent);
		carryComponent.LoadReferences(data.carryComponent);
		partyComponent.LoadReferences(data.partyComponent);
		gatheringComponent.LoadReferences(data.gatheringComponent);
		tileObjectComponent.LoadReferences(data.tileObjectComponent);
		crimeComponent.LoadReferences(data.crimeComponent);
		previousCharacterDataComponent.LoadReferences(data.previousCharacterDataComponent);
		traitComponent.LoadReferences(data.traitComponent);
		dailyScheduleComponent.LoadReferences(data.dailyScheduleComponent);
		villagerWantsComponent?.LoadReferences(data.villagerWantsComponent, this);
		petComponent.LoadReferences(data.petComponent);
		mountComponent.LoadReferences(data.mountComponent);
		classComponent.LoadReferences(data.classComponent);
		if (data.deathTileLocation.hasValue)
		{
			deathTilePosition = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(data.deathTileLocation);
		}
		LoadCharacterTraitsFromSave(data);
		equipmentComponent = new EquipmentComponent();
		equipmentComponent.LoadReferences(data.equipmentComponent);
		SetRelationshipContainer(data.saveDataBaseRelationshipContainer.Load());
	}

	public virtual void LoadReferencesMainThread(SaveDataCharacter data)
	{
		if (currentJob != null && currentJob is GoapPlanJob { assignedPlan: not null } goapPlanJob)
		{
			currentPlan = goapPlanJob.assignedPlan;
		}
		crimeComponent.LoadReferencesMainThread(data.crimeComponent);
		interruptComponent.LoadReferencesInMainThread(data.interruptComponent);
		visuals = new CharacterVisuals(this, data);
		if (data.hasMinion)
		{
			_minion = data.minion.Load(this);
			DatabaseManager.Instance.characterDatabase.RemoveFromAliveVillagersList(this);
		}
		if (data.hasMarker)
		{
			if (!marker)
			{
				CreateMarker();
			}
			if (SaveManager.Instance.currentSaveDataProgress.playerSave.seizedPOIID == persistentID)
			{
				LocationGridTile firstPassableGridTile = GridMap.Instance.GetFirstPassableGridTile();
				if (firstPassableGridTile == null)
				{
					throw new Exception("Trying to place a loaded seized character " + nameWithID + " but there is no available grid tile");
				}
				data.worldPos = firstPassableGridTile.centeredWorldLocation;
			}
			marker.LoadMarkerPlacement(data, _currentRegion);
			carryComponent.LoadCarryReference(data.carryComponent);
		}
		visuals.UpdateAllVisuals(this);
		OnSetIsHidden();
		reactionComponent.UpdateHiddenState();
		if ((bool)marker)
		{
			marker.UpdateAnimation();
		}
		if (!isDead && minion == null && !isInLimbo)
		{
			SubscribeToSignals();
		}
		SubscribeToPermanentSignals();
		if (minion != null)
		{
			ConstructDefaultPlayerActions();
		}
	}

	public void LoadCurrentlyDoingAction()
	{
		if (hasMarker)
		{
			marker.UpdatePosition();
		}
		if (currentJob != null && currentJob is GoapPlanJob { assignedPlan: not null } goapPlanJob)
		{
			currentPlan = goapPlanJob.assignedPlan;
		}
		if (currentActionNode != null)
		{
			if (currentActionNode.action.goapType == INTERACTION_TYPE.DIG && currentActionNode.poiTarget is ThinWall)
			{
				SetCurrentActionNode(null, null, null);
			}
			else if (currentActionNode.actionStatus == ACTION_STATUS.STARTED)
			{
				SetCurrentActionNode(null, null, null);
			}
			else if (currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
			{
				if (currentActionNode.goapType == INTERACTION_TYPE.MAKE_LOVE)
				{
					Character actor = currentActionNode.actor;
					Character character = currentActionNode.poiTarget as Character;
					Bed bed = null;
					if (actor.tileObjectComponent.primaryBed != null)
					{
						if (actor.tileObjectComponent.primaryBed.gridTileLocation != null && (actor.gridTileLocation == actor.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(actor.tileObjectComponent.primaryBed.gridTileLocation, sameStructureOnly: true)))
						{
							bed = actor.tileObjectComponent.primaryBed;
						}
					}
					else if (character.tileObjectComponent.primaryBed != null && character.tileObjectComponent.primaryBed.gridTileLocation != null && (actor.gridTileLocation == character.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(character.tileObjectComponent.primaryBed.gridTileLocation, sameStructureOnly: true)))
					{
						bed = character.tileObjectComponent.primaryBed;
					}
					if (bed != null)
					{
						bed.OnDoActionToObject(currentActionNode);
					}
					else
					{
						SetCurrentActionNode(null, null, null);
					}
				}
				else if (currentActionNode.poiTarget is TileObject tileObject)
				{
					tileObject.OnDoActionToObject(currentActionNode);
				}
			}
		}
		if (CanPerformEndTickJobs())
		{
			JobQueueItem jobQueueItem = null;
			if (jobQueue.jobsInQueue.Count > 0)
			{
				jobQueueItem = jobQueue.jobsInQueue[0];
			}
			if (jobQueueItem != null && jobQueueItem is GoapPlanJob { assignedPlan: not null } goapPlanJob2 && goapPlanJob2.assignedPlan.currentNode == null)
			{
				jobQueueItem.ForceCancelJob();
				jobQueueItem = null;
				if (jobQueue.jobsInQueue.Count > 0)
				{
					jobQueueItem = jobQueue.jobsInQueue[0];
				}
			}
			if (jobQueueItem != null && !jobQueueItem.ProcessJob())
			{
				PerformJob(jobQueueItem);
			}
		}
		if ((combatComponent.hostilesInRange.Count > 0 || combatComponent.avoidInRange.Count > 0) && !jobQueue.HasJob<CharacterStateJob>())
		{
			combatComponent.SetWillProcessCombat(state: true);
		}
		visuals?.UpdateAllVisuals(this);
		if (hasMarker)
		{
			marker.UpdateAnimation();
		}
	}

	protected void LoadCharacterTraitsFromSave(SaveDataCharacter data)
	{
		traitContainer.Load(this, data.saveDataTraitContainer);
		limiterComponent.ApplyDataFromSave(data.limiterComponent);
		moodComponent.SetSaveDataMoodComponent(data.moodComponent);
		if (traitContainer.HasTrait("Character Trait"))
		{
			defaultCharacterTrait = traitContainer.GetTraitOrStatus<CharacterTrait>("Character Trait");
		}
		if (traitContainer.HasTrait("Necromancer"))
		{
			necromancerTrait = traitContainer.GetTraitOrStatus<Necromancer>("Necromancer");
		}
	}

	public void OnSelectBookmark()
	{
		LeftSelectAction();
	}

	public void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		UIManager.Instance.ShowCharacterNameplateTooltip(this, p_pos);
	}

	public void OnHoverOutBookmarkItem()
	{
		UIManager.Instance.HideCharacterNameplateTooltip();
	}

	public void SetDeathLocation(LocationGridTile p_tile)
	{
		deathTilePosition = p_tile;
	}

	public void RecomputeResistanceInitialChangeClass(Character p_character, string p_previousClass)
	{
		if (!(p_previousClass != string.Empty) || talentComponent == null || p_character.race != RACE.HUMANS)
		{
			return;
		}
		int getLevelCount = 0;
		talentComponent.allTalents.ForEach(delegate(CharacterTalent eachTalent)
		{
			if (eachTalent.level > 1)
			{
				getLevelCount += eachTalent.level - 1;
			}
		});
		for (int num = 0; num < getLevelCount; num++)
		{
			ApplyClassBonusOnLevelUp(p_previousClass, -1f);
		}
		for (int num2 = 0; num2 < getLevelCount; num2++)
		{
			ApplyClassBonusOnLevelUp(classComponent.characterClass.className);
		}
	}

	public void RecomputePiercingAndResistanceForGameStart(Character p_character, string p_className)
	{
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_className);
		float initialVillagerPiercing = characterClass.initialVillagerPiercing;
		float p_value = 0f;
		float p_value2 = 0f;
		if (characterClass.initialVillagerPhysicalResistances != null && characterClass.initialVillagerPhysicalResistances.Length != 0)
		{
			p_value = CollectionUtilities.GetRandomElement(characterClass.initialVillagerPhysicalResistances);
		}
		if (characterClass.initialVillagerMentalResistances != null && characterClass.initialVillagerMentalResistances.Length != 0)
		{
			p_value2 = CollectionUtilities.GetRandomElement(characterClass.initialVillagerMentalResistances);
		}
		p_character.piercingAndResistancesComponent.SetBasePiercing(initialVillagerPiercing);
		p_character.piercingAndResistancesComponent.SetResistance(RESISTANCE.Physical, p_value);
		p_character.piercingAndResistancesComponent.SetResistance(RESISTANCE.Mental, p_value2);
		RESISTANCE[] enumValues = CollectionUtilities.GetEnumValues<RESISTANCE>();
		foreach (RESISTANCE rESISTANCE in enumValues)
		{
			if (rESISTANCE == RESISTANCE.None)
			{
				continue;
			}
			float p_value3 = 0f;
			if (rESISTANCE.IsElemental())
			{
				if (characterClass.initialVillagerElementalResistances != null && characterClass.initialVillagerElementalResistances.Length != 0)
				{
					p_value3 = CollectionUtilities.GetRandomElement(characterClass.initialVillagerElementalResistances);
				}
				p_character.piercingAndResistancesComponent.SetResistance(rESISTANCE, p_value3);
			}
			else if (rESISTANCE.IsSecondary())
			{
				if (characterClass.initialVillagerSecondaryResistances != null && characterClass.initialVillagerSecondaryResistances.Length != 0)
				{
					p_value3 = CollectionUtilities.GetRandomElement(characterClass.initialVillagerSecondaryResistances);
				}
				p_character.piercingAndResistancesComponent.SetResistance(rESISTANCE, p_value3);
			}
		}
	}

	public void ApplyClassBonusOnLevelUp(string p_className, float factor = 1f)
	{
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_className);
		piercingAndResistancesComponent.AdjustBasePiercing(characterClass.characterSkillUpdateData.GetPiercingBonus() * factor);
		RESISTANCE[] enumValues = CollectionUtilities.GetEnumValues<RESISTANCE>();
		foreach (RESISTANCE rESISTANCE in enumValues)
		{
			if (rESISTANCE != RESISTANCE.None)
			{
				if (rESISTANCE.IsElemental())
				{
					piercingAndResistancesComponent.AdjustResistance(rESISTANCE, characterClass.characterSkillUpdateData.GetAllElementalResistanceBonus() * factor);
				}
				else if (rESISTANCE.IsSecondary())
				{
					piercingAndResistancesComponent.AdjustResistance(rESISTANCE, characterClass.characterSkillUpdateData.GetAllSecondaryResistanceBonus() * factor);
				}
				piercingAndResistancesComponent.AdjustResistance(rESISTANCE, characterClass.characterSkillUpdateData.GetBonusBaseOnElement(rESISTANCE) * factor);
			}
		}
	}

	public void AbsorbCrystal(PowerCrystal p_crystal)
	{
		ApplyCrystalBonus(p_crystal);
		string value;
		string value2;
		Log log;
		if (homeSettlement == null || homeSettlement.locationType != LOCATION_TYPE.VILLAGE)
		{
			if (p_crystal.amountBonusPiercing > 0f)
			{
				value = p_crystal.amountBonusPiercing.ToString(CultureInfo.InvariantCulture) + "%";
				value2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Piercing") + ".";
			}
			else
			{
				value = p_crystal.amountBonusResistance.ToString(CultureInfo.InvariantCulture) + "%";
				value2 = p_crystal.resistanceBonuses[0].LocalizedName();
			}
			log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "absorb_crystal_homeless", LOG_TAG.Major);
			log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
			log.AddToFillers(null, value2, LOG_IDENTIFIER.STRING_2);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
			return;
		}
		if (p_crystal.amountBonusPiercing > 0f)
		{
			value = p_crystal.amountBonusPiercing.ToString(CultureInfo.InvariantCulture) + "%";
			value2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Piercing") + ".";
		}
		else
		{
			value = p_crystal.amountBonusResistance.ToString(CultureInfo.InvariantCulture) + "%";
			value2 = p_crystal.resistanceBonuses[0].LocalizedName();
		}
		log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "absorb_crystal_villager", LOG_TAG.Major);
		log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(homeSettlement, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		log.AddToFillers(null, value2, LOG_IDENTIFIER.STRING_2);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
		for (int i = 0; i < homeSettlement.residents.Count; i++)
		{
			Character character = homeSettlement.residents[i];
			if (this != character && !character.isDead && character.race == RACE.ELVES)
			{
				character.ApplyCrystalBonus(p_crystal);
			}
		}
	}

	private void ApplyCrystalBonus(PowerCrystal p_crystal)
	{
		for (int i = 0; i < p_crystal.resistanceBonuses.Count; i++)
		{
			RESISTANCE p_resistance = p_crystal.resistanceBonuses[i];
			piercingAndResistancesComponent.AdjustResistance(p_resistance, p_crystal.amountBonusResistance);
		}
		piercingAndResistancesComponent.AdjustBasePiercing(p_crystal.amountBonusPiercing);
	}

	public bool HasTalents()
	{
		return talentComponent != null;
	}

	public int TryGetTalentLevel(CHARACTER_TALENT talent)
	{
		if (HasTalents())
		{
			return talentComponent.GetTalent(talent).level;
		}
		return 1;
	}

	public void OnThisCharactersTalentLeveledUp(CharacterTalent p_talent)
	{
		if (p_talent.talentType == CHARACTER_TALENT.Combat_Magic || p_talent.talentType == CHARACTER_TALENT.Healing_Magic)
		{
			UpdatePetCapacityBasedOnTalentLevel();
		}
	}

	private void UpdatePetCapacityBasedOnTalentLevel()
	{
		int num = TryGetTalentLevel(CHARACTER_TALENT.Combat_Magic) + TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic);
		if (num >= 1 && num <= 3)
		{
			petComponent.ownedPetsData.SetBaseMaximumPetCapacity(1);
		}
		else if (num >= 4 && num <= 6)
		{
			petComponent.ownedPetsData.SetBaseMaximumPetCapacity(2);
		}
		else if (num >= 7 && num <= 9)
		{
			petComponent.ownedPetsData.SetBaseMaximumPetCapacity(3);
		}
		else if (num >= 10)
		{
			petComponent.ownedPetsData.SetBaseMaximumPetCapacity(4);
		}
	}

	public void ApplyAllPrimordialBonus()
	{
		RaceData raceData = RaceManager.Instance.GetRaceData(race);
		combatComponent.LevelUpBaseOnInitializeAndApplyAllLevelBonus(raceData.category, PRIMORDIAL_STATS_BONUS.Str);
		combatComponent.LevelUpBaseOnInitializeAndApplyAllLevelBonus(raceData.category, PRIMORDIAL_STATS_BONUS.Int);
		combatComponent.LevelUpBaseOnInitializeAndApplyAllLevelBonus(raceData.category, PRIMORDIAL_STATS_BONUS.Piercing);
		combatComponent.LevelUpBaseOnInitializeAndApplyAllLevelBonus(raceData.category, PRIMORDIAL_STATS_BONUS.Mental_Res);
		combatComponent.LevelUpBaseOnInitializeAndApplyAllLevelBonus(raceData.category, PRIMORDIAL_STATS_BONUS.Physical_Res);
		combatComponent.LevelUpBaseOnInitializeAndApplyAllLevelBonus(raceData.category, PRIMORDIAL_STATS_BONUS.Elemental_Res);
		combatComponent.LevelUpBaseOnInitializeAndApplyAllLevelBonus(raceData.category, PRIMORDIAL_STATS_BONUS.Secondary_Res);
	}

	public void OnCharacterHitByPlayerSpell(int p_amount)
	{
		classComponent.OnCharacterHitByPlayerSpell(p_amount);
	}

	public void SetIsDeadReference(bool p_state)
	{
		isDeadReference = p_state;
	}

	public void OnPickAction()
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character p_actor)
		{
			if (UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is TriggerGrudgeData triggerGrudgeData)
			{
				triggerGrudgeData.ShowGrudgeChoices(p_actor, this);
				return;
			}
		}
		else if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is TileObject tileObject && UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is LureData lureData)
		{
			UIManager.Instance.HideContextMenu();
			lureData.TriggerLure(this, tileObject);
			return;
		}
		string hoverText = string.Empty;
		if (PlayerManager.Instance.player.CanShareIntelTo(this, ref hoverText, PlayerManager.Instance.shareIntelContextMenuItem.currentIntelForContextMenu))
		{
			PlayerManager.Instance.player.TryExecuteShareIntel(this, PlayerManager.Instance.shareIntelContextMenuItem.currentIntelForContextMenu);
			UIManager.Instance.HideContextMenu();
		}
	}

	public bool CanBePickedRegardlessOfCooldown()
	{
		string hoverText = string.Empty;
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character p_actor)
		{
			if (UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is PlayerAction { type: PLAYER_SKILL_TYPE.TRIGGER_GRUDGE })
			{
				return CanTriggerGrudge(p_actor, ref hoverText);
			}
		}
		else if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is TileObject tileObject && UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is LureData lureData)
		{
			return lureData.CanTriggerLure(this, tileObject);
		}
		if (PlayerManager.Instance.player.CanShareIntelTo(this, ref hoverText, PlayerManager.Instance.shareIntelContextMenuItem.currentIntelForContextMenu))
		{
			return true;
		}
		return false;
	}

	public bool IsInCooldown()
	{
		return false;
	}

	public float GetCoverFillAmount()
	{
		return 0f;
	}

	public int GetCurrentRemainingCooldownTicks()
	{
		return 0;
	}

	public int GetManaCost()
	{
		return 0;
	}

	public bool CanTriggerGrudge(Character p_actor, ref string hoverText)
	{
		if (isDead)
		{
			hoverText += Utilities.ColorizeInvalidText("Cannot target dead characters.");
			return false;
		}
		if (!p_actor.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation))
		{
			hoverText += Utilities.ColorizeInvalidText("No path towards target.");
			return false;
		}
		return true;
	}

	public virtual void OnLocaleChanged(Locale locale)
	{
		currentActionNode?.thoughtBubbleLog?.ReevaluateTextGivenNewLanguage(locale);
		currentActionNode?.thoughtBubbleMovingLog?.ReevaluateTextGivenNewLanguage(locale);
		currentActionNode?.descriptionLog?.ReevaluateTextGivenNewLanguage(locale);
		interruptComponent?.thoughtBubbleLog?.ReevaluateTextGivenNewLanguage(locale);
	}

	public virtual void CleanUp()
	{
		string text = GameManager.Instance.TodayLogString() + "Character " + name + " is being cleaned up...";
		Stopwatch stopwatch = new Stopwatch();
		Stopwatch stopwatch2 = new Stopwatch();
		hasBeenCleanedUp = true;
		stopwatch.Start();
		stopwatch2.Start();
		crimeComponent?.CleanUpAllCrimesByThisCharacter();
		stopwatch2.Stop();
		string text2 = text + "\n\t-crimeComponent.CleanUpAllCrimesByThisCharacter() took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		traitContainer?.RemoveAllTraitsAndStatuses(this);
		stopwatch2.Stop();
		string text3 = text2 + "\n\t-traitContainer RemoveAllTraitsAndStatuses took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		Messenger.Broadcast(CharacterSignals.DISCONNECT_FROM_CHARACTER, this);
		stopwatch2.Stop();
		string text4 = text3 + "\n\t-Disconnect From Character Signal took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		UnsubscribeSignals();
		UnsubscribeFromPermanentSignals();
		stopwatch2.Start();
		base.relationshipContainer?.CleanUp();
		stopwatch2.Stop();
		string text5 = text4 + "\n\t-relationshipContainer Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		_minion?.CleanUp();
		stopwatch2.Stop();
		string text6 = text5 + "\n\t-Minion Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		visuals?.CleanUp();
		stopwatch2.Stop();
		string text7 = text6 + "\n\t-visuals Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		traitContainer?.CleanUp();
		stopwatch2.Stop();
		string text8 = text7 + "\n\t-traitContainer Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		classComponent?.CleanUp();
		stopwatch2.Stop();
		string text9 = text8 + "\n\t-classComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		needsComponent?.CleanUp();
		stopwatch2.Stop();
		string text10 = text9 + "\n\t-needsComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		structureComponent?.CleanUp();
		stopwatch2.Stop();
		string text11 = text10 + "\n\t-structureComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		stateComponent?.CleanUp();
		stopwatch2.Stop();
		string text12 = text11 + "\n\t-stateComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		nonActionEventsComponent?.CleanUp();
		stopwatch2.Stop();
		string text13 = text12 + "\n\t-nonActionEventsComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		interruptComponent?.CleanUp();
		stopwatch2.Stop();
		string text14 = text13 + "\n\t-interruptComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		behaviourComponent?.CleanUp();
		stopwatch2.Stop();
		string text15 = text14 + "\n\t-behaviourComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		moodComponent?.CleanUp();
		stopwatch2.Stop();
		string text16 = text15 + "\n\t-moodComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		reactionComponent?.CleanUp();
		stopwatch2.Stop();
		string text17 = text16 + "\n\t-reactionComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		combatComponent?.CleanUp();
		stopwatch2.Stop();
		string text18 = text17 + "\n\t-combatComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		assumptionComponent?.CleanUp();
		stopwatch2.Stop();
		string text19 = text18 + "\n\t-assumptionComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		movementComponent?.CleanUp();
		stopwatch2.Stop();
		string text20 = text19 + "\n\t-movementComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		stateAwarenessComponent?.CleanUp();
		stopwatch2.Stop();
		string text21 = text20 + "\n\t-stateAwarenessComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		carryComponent?.CleanUp();
		stopwatch2.Stop();
		string text22 = text21 + "\n\t-carryComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		partyComponent?.CleanUp();
		stopwatch2.Stop();
		string text23 = text22 + "\n\t-partyComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		gatheringComponent?.CleanUp();
		stopwatch2.Stop();
		string text24 = text23 + "\n\t-gatheringComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		tileObjectComponent?.CleanUp();
		stopwatch2.Stop();
		string text25 = text24 + "\n\t-tileObjectComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		crimeComponent?.CleanUp();
		stopwatch2.Stop();
		string text26 = text25 + "\n\t-crimeComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		religionComponent?.CleanUp();
		stopwatch2.Stop();
		string text27 = text26 + "\n\t-religionComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		limiterComponent?.CleanUp();
		stopwatch2.Stop();
		string text28 = text27 + "\n\t-limiterComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		piercingAndResistancesComponent?.CleanUp();
		stopwatch2.Stop();
		string text29 = text28 + "\n\t-piercingAndResistancesComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		previousCharacterDataComponent?.CleanUp();
		stopwatch2.Stop();
		string text30 = text29 + "\n\t-previousCharacterDataComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		traitComponent?.CleanUp();
		stopwatch2.Stop();
		string text31 = text30 + "\n\t-traitComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		moneyComponent?.CleanUp();
		stopwatch2.Stop();
		string text32 = text31 + "\n\t-moneyComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		dailyScheduleComponent?.CleanUp();
		stopwatch2.Stop();
		string text33 = text32 + "\n\t-dailyScheduleComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		talentComponent?.CleanUp();
		stopwatch2.Stop();
		string text34 = text33 + "\n\t-talentComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		villagerWantsComponent?.CleanUp();
		stopwatch2.Stop();
		string text35 = text34 + "\n\t-wantsComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch2.Start();
		jobComponent?.CleanUp();
		stopwatch2.Stop();
		string text36 = text35 + "\n\t-jobComponent Clean Up took " + stopwatch2.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch2.Reset();
		stopwatch.Stop();
		_ = text36 + "\n-Whole Clean Up took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
		stopwatch.Reset();
	}

	public virtual void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = _currentStructure;
		_ = homeStructure;
		_ = deployedAtStructure;
		trapStructure?.CheckIfStructureIsStillReferenced(p_structure);
		planner?.CheckIfStructureIsStillReferenced(p_structure);
		classComponent?.CheckIfStructureIsStillReferenced(p_structure);
		needsComponent?.CheckIfStructureIsStillReferenced(p_structure);
		structureComponent?.CheckIfStructureIsStillReferenced(p_structure);
		stateComponent?.CheckIfStructureIsStillReferenced(p_structure);
		nonActionEventsComponent?.CheckIfStructureIsStillReferenced(p_structure);
		interruptComponent?.CheckIfStructureIsStillReferenced(p_structure);
		behaviourComponent?.CheckIfStructureIsStillReferenced(p_structure);
		moodComponent?.CheckIfStructureIsStillReferenced(p_structure);
		jobComponent?.CheckIfStructureIsStillReferenced(p_structure);
		reactionComponent?.CheckIfStructureIsStillReferenced(p_structure);
		logComponent?.CheckIfStructureIsStillReferenced(p_structure);
		combatComponent?.CheckIfStructureIsStillReferenced(p_structure);
		rumorComponent?.CheckIfStructureIsStillReferenced(p_structure);
		assumptionComponent?.CheckIfStructureIsStillReferenced(p_structure);
		movementComponent?.CheckIfStructureIsStillReferenced(p_structure);
		stateAwarenessComponent?.CheckIfStructureIsStillReferenced(p_structure);
		carryComponent?.CheckIfStructureIsStillReferenced(p_structure);
		partyComponent?.CheckIfStructureIsStillReferenced(p_structure);
		gatheringComponent?.CheckIfStructureIsStillReferenced(p_structure);
		tileObjectComponent?.CheckIfStructureIsStillReferenced(p_structure);
		crimeComponent?.CheckIfStructureIsStillReferenced(p_structure);
		religionComponent?.CheckIfStructureIsStillReferenced(p_structure);
		limiterComponent?.CheckIfStructureIsStillReferenced(p_structure);
		piercingAndResistancesComponent?.CheckIfStructureIsStillReferenced(p_structure);
		eventDispatcher?.CheckIfStructureIsStillReferenced(p_structure);
		previousCharacterDataComponent?.CheckIfStructureIsStillReferenced(p_structure);
		traitComponent?.CheckIfStructureIsStillReferenced(p_structure);
		bookmarkEventDispatcher?.CheckIfStructureIsStillReferenced(p_structure);
		equipmentComponent?.CheckIfStructureIsStillReferenced(p_structure);
		moneyComponent?.CheckIfStructureIsStillReferenced(p_structure);
		resourceStorageComponent?.CheckIfStructureIsStillReferenced(p_structure);
		dailyScheduleComponent?.CheckIfStructureIsStillReferenced(p_structure);
		talentComponent?.CheckIfStructureIsStillReferenced(p_structure);
		villagerWantsComponent?.CheckIfStructureIsStillReferenced(p_structure);
		petComponent?.CheckIfStructureIsStillReferenced(p_structure);
		mountComponent?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.relationshipContainer?.CheckIfCharacterIsStillReferenced(p_character);
		lycanData?.CheckIfCharacterIsStillReferenced(p_character);
		currentLocationAwareness?.CheckIfCharacterIsStillReferenced(p_character);
		trapStructure?.CheckIfCharacterIsStillReferenced(p_character);
		planner?.CheckIfCharacterIsStillReferenced(p_character);
		classComponent?.CheckIfCharacterIsStillReferenced(p_character);
		needsComponent?.CheckIfCharacterIsStillReferenced(p_character);
		structureComponent?.CheckIfCharacterIsStillReferenced(p_character);
		stateComponent?.CheckIfCharacterIsStillReferenced(p_character);
		nonActionEventsComponent?.CheckIfCharacterIsStillReferenced(p_character);
		interruptComponent?.CheckIfCharacterIsStillReferenced(p_character);
		behaviourComponent?.CheckIfCharacterIsStillReferenced(p_character);
		moodComponent?.CheckIfCharacterIsStillReferenced(p_character);
		jobComponent?.CheckIfCharacterIsStillReferenced(p_character);
		reactionComponent?.CheckIfCharacterIsStillReferenced(p_character);
		logComponent?.CheckIfCharacterIsStillReferenced(p_character);
		combatComponent?.CheckIfCharacterIsStillReferenced(p_character);
		rumorComponent?.CheckIfCharacterIsStillReferenced(p_character);
		assumptionComponent?.CheckIfCharacterIsStillReferenced(p_character);
		movementComponent?.CheckIfCharacterIsStillReferenced(p_character);
		stateAwarenessComponent?.CheckIfCharacterIsStillReferenced(p_character);
		carryComponent?.CheckIfCharacterIsStillReferenced(p_character);
		partyComponent?.CheckIfCharacterIsStillReferenced(p_character);
		gatheringComponent?.CheckIfCharacterIsStillReferenced(p_character);
		tileObjectComponent?.CheckIfCharacterIsStillReferenced(p_character);
		crimeComponent?.CheckIfCharacterIsStillReferenced(p_character);
		religionComponent?.CheckIfCharacterIsStillReferenced(p_character);
		limiterComponent?.CheckIfCharacterIsStillReferenced(p_character);
		piercingAndResistancesComponent?.CheckIfCharacterIsStillReferenced(p_character);
		eventDispatcher?.CheckIfCharacterIsStillReferenced(p_character);
		previousCharacterDataComponent?.CheckIfCharacterIsStillReferenced(p_character);
		traitComponent?.CheckIfCharacterIsStillReferenced(p_character);
		bookmarkEventDispatcher?.CheckIfCharacterIsStillReferenced(p_character);
		equipmentComponent?.CheckIfCharacterIsStillReferenced(p_character);
		moneyComponent?.CheckIfCharacterIsStillReferenced(p_character);
		resourceStorageComponent?.CheckIfCharacterIsStillReferenced(p_character);
		dailyScheduleComponent?.CheckIfCharacterIsStillReferenced(p_character);
		talentComponent?.CheckIfCharacterIsStillReferenced(p_character);
		villagerWantsComponent?.CheckIfCharacterIsStillReferenced(p_character);
		petComponent?.CheckIfCharacterIsStillReferenced(p_character);
		mountComponent?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
