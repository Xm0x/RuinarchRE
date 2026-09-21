using System;
using System.Diagnostics;
using Databases;
using Databases.SQLDatabase;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DatabaseManager : MonoBehaviour
{
	public static DatabaseManager Instance;

	public AreaDatabase areaDatabase { get; private set; }

	public RegionDatabase regionDatabase { get; private set; }

	public CharacterDatabase characterDatabase { get; private set; }

	public FactionDatabase factionDatabase { get; private set; }

	public TileObjectDatabase tileObjectDatabase { get; private set; }

	public LocationGridTileDatabase locationGridTileDatabase { get; private set; }

	public SettlementDatabase settlementDatabase { get; private set; }

	public LocationStructureDatabase structureDatabase { get; private set; }

	public TraitDatabase traitDatabase { get; private set; }

	public BurningSourceDatabase burningSourceDatabase { get; private set; }

	public JobDatabase jobDatabase { get; private set; }

	public FamilyTreeDatabase familyTreeDatabase { get; private set; }

	public PartyDatabase partyDatabase { get; private set; }

	public SharedOpinionDatabase sharedOpinionDatabase { get; private set; }

	public CrimeDatabase crimeDatabase { get; private set; }

	public ActionDatabase actionDatabase { get; private set; }

	public PartyQuestDatabase partyQuestDatabase { get; private set; }

	public GatheringDatabase gatheringDatabase { get; private set; }

	public InterruptDatabase interruptDatabase { get; private set; }

	public RuinarchSQLDatabase mainSQLDatabase { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			SceneManager.sceneUnloaded += OnSceneUnloaded;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void Initialize()
	{
		areaDatabase = new AreaDatabase();
		regionDatabase = new RegionDatabase();
		characterDatabase = new CharacterDatabase();
		factionDatabase = new FactionDatabase();
		tileObjectDatabase = new TileObjectDatabase();
		locationGridTileDatabase = new LocationGridTileDatabase();
		settlementDatabase = new SettlementDatabase();
		structureDatabase = new LocationStructureDatabase();
		traitDatabase = new TraitDatabase();
		burningSourceDatabase = new BurningSourceDatabase();
		jobDatabase = new JobDatabase();
		familyTreeDatabase = new FamilyTreeDatabase();
		actionDatabase = new ActionDatabase();
		interruptDatabase = new InterruptDatabase();
		partyDatabase = new PartyDatabase();
		partyQuestDatabase = new PartyQuestDatabase();
		crimeDatabase = new CrimeDatabase();
		sharedOpinionDatabase = new SharedOpinionDatabase();
		gatheringDatabase = new GatheringDatabase();
		mainSQLDatabase = new RuinarchSQLDatabase();
	}

	public void InitializeDatabases()
	{
		mainSQLDatabase.InitializeDatabase();
		traitDatabase.ClearTraitsToBeLoadedOnMainThread();
		structureDatabase.ClearStructuresToBeLoadedOnMainThread();
	}

	public object GetObjectFromDatabase(Type type, string persistentID)
	{
		if (type == typeof(Character) || type.IsSubclassOf(typeof(Character)))
		{
			return characterDatabase.GetCharacterByPersistentID(persistentID);
		}
		if (type == typeof(TileObject) || type.IsSubclassOf(typeof(TileObject)))
		{
			return tileObjectDatabase.GetTileObjectByPersistentIDSafe(persistentID);
		}
		if (type == typeof(LocationStructure) || type.IsSubclassOf(typeof(LocationStructure)))
		{
			return structureDatabase.GetStructureByPersistentIDSafe(persistentID);
		}
		if (type == typeof(Region))
		{
			return regionDatabase.mainRegion;
		}
		if (type == typeof(BaseSettlement) || type.IsSubclassOf(typeof(BaseSettlement)))
		{
			return settlementDatabase.GetSettlementByPersistentIDSafe(persistentID);
		}
		if (type == typeof(Faction))
		{
			return factionDatabase.GetFactionBasedOnPersistentID(persistentID);
		}
		if (type == typeof(Party))
		{
			return partyDatabase.GetPartyByPersistentIDSafe(persistentID);
		}
		if (type == typeof(Area))
		{
			return areaDatabase.GetAreaByPersistentID(persistentID);
		}
		return null;
	}

	public void ClearVolatileDatabases()
	{
		actionDatabase.allActions.Clear();
		interruptDatabase.allInterrupts.Clear();
		partyQuestDatabase.allPartyQuests.Clear();
		crimeDatabase.allCrimes.Clear();
		locationGridTileDatabase.tileByGUID.Clear();
		locationGridTileDatabase.locationGridTiles.Clear();
		gatheringDatabase.allGatherings.Clear();
		traitDatabase.ClearTraitsToBeLoadedOnMainThread();
		structureDatabase.ClearStructuresToBeLoadedOnMainThread();
		GC.Collect();
	}

	private void OnSceneUnloaded(Scene unloaded)
	{
		if (unloaded.name == "Game")
		{
			DisposeDatabases();
		}
	}

	private void DisposeDatabases()
	{
		mainSQLDatabase?.Dispose();
	}

	private void OnDestroy()
	{
		DisposeDatabases();
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		areaDatabase.CheckIfStructureIsStillReferenced(p_structure);
		regionDatabase.CheckIfStructureIsStillReferenced(p_structure);
		characterDatabase.CheckIfStructureIsStillReferenced(p_structure);
		factionDatabase.CheckIfStructureIsStillReferenced(p_structure);
		tileObjectDatabase.CheckIfStructureIsStillReferenced(p_structure);
		locationGridTileDatabase.CheckIfStructureIsStillReferenced(p_structure);
		settlementDatabase.CheckIfStructureIsStillReferenced(p_structure);
		structureDatabase.CheckIfStructureIsStillReferenced(p_structure);
		traitDatabase.CheckIfStructureIsStillReferenced(p_structure);
		burningSourceDatabase.CheckIfStructureIsStillReferenced(p_structure);
		jobDatabase.CheckIfStructureIsStillReferenced(p_structure);
		familyTreeDatabase.CheckIfStructureIsStillReferenced(p_structure);
		partyDatabase.CheckIfStructureIsStillReferenced(p_structure);
		sharedOpinionDatabase.CheckIfStructureIsStillReferenced(p_structure);
		crimeDatabase.CheckIfStructureIsStillReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		areaDatabase.CheckIfCharacterIsStillReferenced(p_character);
		regionDatabase.CheckIfCharacterIsStillReferenced(p_character);
		characterDatabase.CheckIfCharacterIsStillReferenced(p_character);
		factionDatabase.CheckIfCharacterIsStillReferenced(p_character);
		tileObjectDatabase.CheckIfCharacterIsStillReferenced(p_character);
		locationGridTileDatabase.CheckIfCharacterIsStillReferenced(p_character);
		settlementDatabase.CheckIfCharacterIsStillReferenced(p_character);
		structureDatabase.CheckIfCharacterIsStillReferenced(p_character);
		traitDatabase.CheckIfCharacterIsStillReferenced(p_character);
		burningSourceDatabase.CheckIfCharacterIsStillReferenced(p_character);
		jobDatabase.CheckIfCharacterIsStillReferenced(p_character);
		familyTreeDatabase.CheckIfCharacterIsStillReferenced(p_character);
		partyDatabase.CheckIfCharacterIsStillReferenced(p_character);
		sharedOpinionDatabase.CheckIfCharacterIsStillReferenced(p_character);
		crimeDatabase.CheckIfCharacterIsStillReferenced(p_character);
	}
}
