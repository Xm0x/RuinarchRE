using System;
using System.Collections.Generic;
using System.Diagnostics;
using EZObjectPools;
using Interrupts;
using Logs;
using Threads;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
	public static ObjectPoolManager Instance;

	private Dictionary<string, EZObjectPool> allObjectPools;

	private Dictionary<ELEMENTAL_TYPE, List<Projectile>> _projectilePool;

	private List<Projectile> _dragonProjectilePool;

	[SerializeField]
	private GameObject[] UIPrefabs;

	[SerializeField]
	internal GameObject[] otherPrefabs;

	[SerializeField]
	private GameObject UIObjectPoolParent;

	[SerializeField]
	private Transform _projectileObjectPoolParent;

	[SerializeField]
	private GameObject _dragonProjectilePrefab;

	[SerializeField]
	private ProjectileDictionary _projectilePrefabDictionary;

	private Queue<GoapNode> _goapNodesPool;

	private Queue<OpinionData> _opinionDataPool;

	private Queue<TraitRemoveSchedule> _traitRemoveSchedulePool;

	private Queue<CombatData> _combatDataPool;

	private Queue<InterruptHolder> _interruptPool;

	private Queue<Party> _partyPool;

	private Queue<GoapThread> _goapThreadPool;

	private Queue<UpdateCharacterNameThread> _updateCharacterNameThreadPool;

	private Queue<SQLLogInsertThread> _sqlInsertThreadPool;

	private Queue<GoapPlanJob> _goapJobPool;

	private Queue<CharacterStateJob> _stateJobPool;

	private Queue<ConversationData> _conversationDataPool;

	private Queue<ScheduledAction> _scheduledActionPool;

	private Queue<SingleJobNode> _jobNodePool;

	private Queue<GoapPlan> _goapPlanPool;

	private Queue<ActualGoapNode> _actionPool;

	private Queue<MoodModification> _moodModificationPool;

	private Queue<LogFiller> _logFillerPool;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			InitializeObjectPools();
			if (EZObjectPool.Marker != null)
			{
				UnityEngine.Object.DontDestroyOnLoad(EZObjectPool.Marker);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void InitializeObjectPools()
	{
		allObjectPools = new Dictionary<string, EZObjectPool>();
		for (int i = 0; i < UIPrefabs.Length; i++)
		{
			GameObject gameObject = UIPrefabs[i];
			int size = 0;
			if (gameObject.name == "LogHistoryItem")
			{
				size = 200;
			}
			CreateNewPool(gameObject, gameObject.name, size, autoResize: true, instantiateImmediate: true, shared: false).transform.SetParent(UIObjectPoolParent.transform, worldPositionStays: false);
		}
		for (int j = 0; j < otherPrefabs.Length; j++)
		{
			GameObject gameObject2 = otherPrefabs[j];
			int size2 = 0;
			CreateNewPool(gameObject2, gameObject2.name, size2, autoResize: true, instantiateImmediate: true, shared: false);
		}
		ConstructProjectilePool();
		ConstructOpinionDataPool();
		ConstructTraitRemoveSchedulePool();
		ConstructCombatDataPool();
		ConstructInterruptPool();
		ConstructPartyPool();
		ConstructGoapThreadPool();
		ConstructLogDatabaseThreadPool();
		ConstructSQLInsertThreadPool();
		ConstructGoapNodes();
		ConstructJobPool();
		ConstructConversationPool();
		ConstructScheduledActionPool();
		ConstructSingleJobNodePool();
		ConstructGoapPlanPool();
		ConstructActionPool();
		ConstructMoodModificationPool();
		ConstructLogFillerPool();
		InitialPoolObjectCreation("TILEOBJECTGAMEOBJECT", 2000);
	}

	private void InitialPoolObjectCreation(string p_poolName, int p_objectCount)
	{
		if (!allObjectPools.ContainsKey(p_poolName))
		{
			throw new Exception("Object Pool does not have key " + p_poolName);
		}
		EZObjectPool eZObjectPool = allObjectPools[p_poolName];
		Stopwatch p_stopwatch = new Stopwatch();
		StartCoroutine(eZObjectPool.InstantiatePoolCoroutine(p_objectCount, p_stopwatch));
	}

	public GameObject InstantiateObjectFromPool(string poolName, Vector3 position, Quaternion rotation, Transform parent = null, bool isWorldPosition = false)
	{
		poolName = poolName.ToUpperInvariant();
		if (!allObjectPools.ContainsKey(poolName))
		{
			throw new Exception("Object Pool does not have key " + poolName);
		}
		GameObject obj = null;
		EZObjectPool eZObjectPool = allObjectPools[poolName];
		if ((object)eZObjectPool == null)
		{
			throw new Exception("Cannot find an object pool with name " + poolName);
		}
		if (eZObjectPool.TryGetNextObject(Vector3.zero, rotation, out obj))
		{
			if ((object)parent != null)
			{
				obj.transform.SetParent(parent, worldPositionStays: false);
			}
			obj.transform.localScale = eZObjectPool.Template.transform.localScale;
			if (isWorldPosition)
			{
				obj.transform.position = position;
			}
			else
			{
				obj.transform.localPosition = position;
			}
		}
		obj.SetActive(value: true);
		return obj;
	}

	public GameObject GetOriginalObjectFromPool(string poolName)
	{
		poolName = poolName.ToUpperInvariant();
		if (!allObjectPools.ContainsKey(poolName))
		{
			throw new Exception("Object Pool does not have key " + poolName);
		}
		return allObjectPools[poolName].Template;
	}

	public void DestroyObject(PooledObject pooledObject)
	{
		PooledObject[] components = pooledObject.GetComponents<PooledObject>();
		Messenger.Broadcast(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, pooledObject.gameObject);
		pooledObject.BeforeDestroyActions();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].BeforeDestroyActions();
		}
		for (int j = 0; j < components.Length; j++)
		{
			components[j].Reset();
		}
		pooledObject.SendObjectBackToPool();
	}

	public void DestroyObjectWithoutCheckingChildren(PooledObject pooledObject)
	{
		Messenger.Broadcast(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, pooledObject.gameObject);
		pooledObject.BeforeDestroyActions();
		pooledObject.Reset();
		pooledObject.SendObjectBackToPool();
	}

	public void DestroyObject(GameObject gameObject)
	{
		PooledObject[] components = gameObject.GetComponents<PooledObject>();
		Messenger.Broadcast(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, gameObject);
		for (int i = 0; i < components.Length; i++)
		{
			components[i].BeforeDestroyActions();
		}
		for (int j = 0; j < components.Length; j++)
		{
			components[j].Reset();
		}
		components[0].SendObjectBackToPool();
	}

	private EZObjectPool CreateNewPool(GameObject template, string poolName, int size, bool autoResize, bool instantiateImmediate, bool shared)
	{
		poolName = poolName.ToUpperInvariant();
		EZObjectPool eZObjectPool = EZObjectPool.CreateObjectPool(template, poolName, size, autoResize, instantiateImmediate, shared);
		allObjectPools.Add(poolName, eZObjectPool);
		return eZObjectPool;
	}

	public bool HasPool(string key)
	{
		if (allObjectPools.ContainsKey(key))
		{
			return true;
		}
		return false;
	}

	private void ConstructGoapNodes()
	{
		_goapNodesPool = new Queue<GoapNode>(60);
	}

	public GoapNode CreateNewGoapNode(int cost, int level, GoapAction action, IPointOfInterest target)
	{
		GoapNode goapNodeFromPool = GetGoapNodeFromPool();
		goapNodeFromPool.Initialize(cost, level, action, target);
		return goapNodeFromPool;
	}

	public GoapNode CreateNewGoapNode()
	{
		return GetGoapNodeFromPool();
	}

	public void ReturnGoapNodeToPool(GoapNode node)
	{
		node.Reset();
		_goapNodesPool.Enqueue(node);
	}

	private GoapNode GetGoapNodeFromPool()
	{
		if (_goapNodesPool.Count > 0)
		{
			return _goapNodesPool.Dequeue();
		}
		return new GoapNode();
	}

	private void ConstructProjectilePool()
	{
		_projectilePool = new Dictionary<ELEMENTAL_TYPE, List<Projectile>>(_projectilePrefabDictionary.Count);
		foreach (KeyValuePair<ELEMENTAL_TYPE, GameObject> item3 in _projectilePrefabDictionary)
		{
			_projectilePool.Add(item3.Key, new List<Projectile>(40));
			List<Projectile> list = _projectilePool[item3.Key];
			for (int i = 0; i < 10; i++)
			{
				Projectile item = CreateNewProjectileGameObject(item3.Key, Vector3.zero, _projectileObjectPoolParent);
				list.Add(item);
			}
		}
		_dragonProjectilePool = new List<Projectile>(20);
		for (int j = 0; j < 10; j++)
		{
			Projectile item2 = CreateNewDragonProjectileGameObject(Vector3.zero, _projectileObjectPoolParent);
			_dragonProjectilePool.Add(item2);
		}
	}

	public Projectile CreateNewProjectile(ELEMENTAL_TYPE p_element, Vector3 p_worldPosition, Transform p_parent)
	{
		List<Projectile> list = _projectilePool[p_element];
		if (list.Count > 0)
		{
			int index = list.Count - 1;
			if ((object)list[index] == null)
			{
				throw new Exception($"Projectile Pool {p_element} has missing objects in its pool! Are you accidentally destroying any GameObjects retrieved from the pool?");
			}
			Projectile projectile = list[index];
			projectile.transform.position = p_worldPosition;
			projectile.transform.rotation = Quaternion.identity;
			projectile.transform.SetParent(p_parent);
			projectile.gameObject.SetActive(value: true);
			list.RemoveAt(index);
			return projectile;
		}
		Projectile projectile2 = CreateNewProjectileGameObject(p_element, p_worldPosition, p_parent);
		projectile2.gameObject.SetActive(value: true);
		return projectile2;
	}

	public Projectile CreateNewDragonProjectile(Vector3 p_worldPosition, Transform p_parent)
	{
		List<Projectile> dragonProjectilePool = _dragonProjectilePool;
		if (dragonProjectilePool.Count > 0)
		{
			int index = dragonProjectilePool.Count - 1;
			if ((object)dragonProjectilePool[index] == null)
			{
				throw new Exception("Projectile Pool DRAGON has missing objects in its pool! Are you accidentally destroying any GameObjects retrieved from the pool?");
			}
			Projectile projectile = dragonProjectilePool[index];
			projectile.transform.position = p_worldPosition;
			projectile.transform.rotation = Quaternion.identity;
			projectile.transform.SetParent(p_parent);
			projectile.gameObject.SetActive(value: true);
			dragonProjectilePool.RemoveAt(index);
			return projectile;
		}
		Projectile projectile2 = CreateNewDragonProjectileGameObject(p_worldPosition, p_parent);
		projectile2.gameObject.SetActive(value: true);
		return projectile2;
	}

	public void ReturnProjectileToPool(Projectile p_projectile)
	{
		List<Projectile> list = ((!p_projectile.isDragonProjectile) ? _projectilePool[p_projectile.poolElement] : _dragonProjectilePool);
		p_projectile.gameObject.SetActive(value: false);
		p_projectile.transform.SetParent(_projectileObjectPoolParent);
		p_projectile.transform.position = Vector3.zero;
		p_projectile.transform.localScale = Vector3.one;
		p_projectile.Reset();
		list.Add(p_projectile);
	}

	private Projectile CreateNewDragonProjectileGameObject(Vector3 p_worldPosition, Transform p_parent)
	{
		GameObject obj = UnityEngine.Object.Instantiate(_dragonProjectilePrefab, p_parent);
		obj.gameObject.SetActive(value: false);
		obj.transform.localScale = Vector3.one;
		obj.transform.position = p_worldPosition;
		Projectile component = obj.GetComponent<Projectile>();
		component.isDragonProjectile = true;
		return component;
	}

	private Projectile CreateNewProjectileGameObject(ELEMENTAL_TYPE p_element, Vector3 p_worldPosition, Transform p_parent)
	{
		GameObject obj = UnityEngine.Object.Instantiate(_projectilePrefabDictionary[p_element], p_worldPosition, Quaternion.identity, p_parent);
		obj.gameObject.SetActive(value: false);
		obj.transform.localScale = Vector3.one;
		Projectile component = obj.GetComponent<Projectile>();
		component.poolElement = p_element;
		return component;
	}

	private void ConstructOpinionDataPool()
	{
		_opinionDataPool = new Queue<OpinionData>(50);
	}

	public OpinionData CreateNewOpinionData()
	{
		OpinionData opinionDataFromPool = GetOpinionDataFromPool();
		opinionDataFromPool.Initialize();
		return opinionDataFromPool;
	}

	public void ReturnOpinionDataToPool(OpinionData data)
	{
		data.Reset();
		_opinionDataPool.Enqueue(data);
	}

	private OpinionData GetOpinionDataFromPool()
	{
		if (_opinionDataPool.Count > 0)
		{
			return _opinionDataPool.Dequeue();
		}
		return new OpinionData();
	}

	private void ConstructTraitRemoveSchedulePool()
	{
		_traitRemoveSchedulePool = new Queue<TraitRemoveSchedule>(50);
	}

	public TraitRemoveSchedule CreateNewTraitRemoveSchedule()
	{
		TraitRemoveSchedule traitRemoveScheduleFromPool = GetTraitRemoveScheduleFromPool();
		traitRemoveScheduleFromPool.Initialize();
		return traitRemoveScheduleFromPool;
	}

	public void ReturnTraitRemoveScheduleToPool(TraitRemoveSchedule data)
	{
		data.Reset();
		_traitRemoveSchedulePool.Enqueue(data);
	}

	private TraitRemoveSchedule GetTraitRemoveScheduleFromPool()
	{
		if (_traitRemoveSchedulePool.Count > 0)
		{
			return _traitRemoveSchedulePool.Dequeue();
		}
		return new TraitRemoveSchedule();
	}

	private void ConstructCombatDataPool()
	{
		_combatDataPool = new Queue<CombatData>(50);
	}

	public CombatData CreateNewCombatData()
	{
		CombatData combatDataFromPool = GetCombatDataFromPool();
		combatDataFromPool.Initialize();
		return combatDataFromPool;
	}

	public void ReturnCombatDataToPool(CombatData data)
	{
		data.Reset();
		_combatDataPool.Enqueue(data);
	}

	private CombatData GetCombatDataFromPool()
	{
		if (_combatDataPool.Count > 0)
		{
			return _combatDataPool.Dequeue();
		}
		return new CombatData();
	}

	private void ConstructInterruptPool()
	{
		_interruptPool = new Queue<InterruptHolder>(50);
	}

	public InterruptHolder CreateNewInterrupt()
	{
		if (_interruptPool.Count > 0)
		{
			return _interruptPool.Dequeue();
		}
		return new InterruptHolder();
	}

	public void TryReturnInterruptToPool(InterruptHolder data)
	{
		if (!data.isSupposedToBeInPool)
		{
			data.SetIsSupposedToBeInPool(p_state: true);
		}
		if (!data.shouldNotBeObjectPooled && data.reactionProcessCounter <= 0)
		{
			data.Reset();
			_interruptPool.Enqueue(data);
		}
	}

	private void ConstructPartyPool()
	{
		_partyPool = new Queue<Party>(50);
	}

	public Party CreateNewParty()
	{
		if (_partyPool.Count > 0)
		{
			return _partyPool.Dequeue();
		}
		return new Party();
	}

	public void ReturnPartyToPool(Party data)
	{
		data.Reset();
		_partyPool.Enqueue(data);
	}

	private void ConstructLogDatabaseThreadPool()
	{
		_updateCharacterNameThreadPool = new Queue<UpdateCharacterNameThread>(10);
	}

	public UpdateCharacterNameThread CreateNewLogDatabaseThread()
	{
		if (_updateCharacterNameThreadPool.Count > 0)
		{
			return _updateCharacterNameThreadPool.Dequeue();
		}
		return new UpdateCharacterNameThread();
	}

	public void ReturnLogDatabaseThreadToPool(SQLWorkerItem data)
	{
		data.Reset();
		if (data is UpdateCharacterNameThread item)
		{
			_updateCharacterNameThreadPool.Enqueue(item);
		}
		else if (data is SQLLogInsertThread item2)
		{
			_sqlInsertThreadPool.Enqueue(item2);
		}
	}

	private void ConstructSQLInsertThreadPool()
	{
		_sqlInsertThreadPool = new Queue<SQLLogInsertThread>(100);
	}

	public SQLLogInsertThread CreateNewSQLInsertThread()
	{
		if (_sqlInsertThreadPool.Count > 0)
		{
			return _sqlInsertThreadPool.Dequeue();
		}
		return new SQLLogInsertThread();
	}

	private void ConstructGoapThreadPool()
	{
		_goapThreadPool = new Queue<GoapThread>(100);
	}

	public GoapThread CreateNewGoapThread()
	{
		if (_goapThreadPool.Count > 0)
		{
			return _goapThreadPool.Dequeue();
		}
		return new GoapThread();
	}

	public void ReturnGoapThreadToPool(GoapThread data)
	{
		data.Reset();
		_goapThreadPool.Enqueue(data);
	}

	private void ConstructJobPool()
	{
		_goapJobPool = new Queue<GoapPlanJob>(100);
		_stateJobPool = new Queue<CharacterStateJob>(50);
	}

	public GoapPlanJob CreateNewGoapPlanJob()
	{
		if (_goapJobPool.Count > 0)
		{
			GoapPlanJob goapPlanJob = _goapJobPool.Dequeue();
			if (!goapPlanJob.isAssigned)
			{
				goapPlanJob.isAssigned = true;
			}
			return goapPlanJob;
		}
		return new GoapPlanJob();
	}

	public void ReturnGoapPlanJobToPool(GoapPlanJob job)
	{
		job.Reset();
		job.isAssigned = false;
		_goapJobPool.Enqueue(job);
	}

	public CharacterStateJob CreateNewCharacterStateJob()
	{
		if (_stateJobPool.Count > 0)
		{
			return _stateJobPool.Dequeue();
		}
		return new CharacterStateJob();
	}

	public void ReturnCharacterStateJobToPool(CharacterStateJob job)
	{
		job.Reset();
		_stateJobPool.Enqueue(job);
	}

	private void ConstructConversationPool()
	{
		_conversationDataPool = new Queue<ConversationData>(5);
	}

	public ConversationData CreateNewConversationData(string text, Character character, DialogItem.Position position)
	{
		ConversationData conversationData = CreateNewConversationData();
		conversationData.text = text;
		conversationData.character = character;
		conversationData.position = position;
		return conversationData;
	}

	public ConversationData CreateNewConversationData()
	{
		if (_conversationDataPool.Count > 0)
		{
			return _conversationDataPool.Dequeue();
		}
		return new ConversationData();
	}

	public void ReturnConversationDataToPool(ConversationData data)
	{
		data.Reset();
		_conversationDataPool.Enqueue(data);
	}

	private void ConstructScheduledActionPool()
	{
		_scheduledActionPool = new Queue<ScheduledAction>(100);
	}

	public ScheduledAction CreateNewScheduledAction()
	{
		if (_scheduledActionPool.Count > 0)
		{
			ScheduledAction scheduledAction = _scheduledActionPool.Dequeue();
			if (scheduledAction == null)
			{
				return new ScheduledAction();
			}
			return scheduledAction;
		}
		return new ScheduledAction();
	}

	public void ReturnScheduledActionToPool(ScheduledAction data)
	{
		if (data != null)
		{
			data.Reset();
			_scheduledActionPool.Enqueue(data);
		}
	}

	private void ConstructGoapPlanPool()
	{
		_goapPlanPool = new Queue<GoapPlan>(100);
	}

	public GoapPlan CreateNewGoapPlanForInitialGoapThread()
	{
		return CreateNewGoapPlan();
	}

	public GoapPlan CreateNewGoapPlan(List<JobNode> p_nodes, IPointOfInterest p_target)
	{
		GoapPlan goapPlan = CreateNewGoapPlan();
		goapPlan.SetNodes(p_nodes);
		goapPlan.SetTarget(p_target);
		return goapPlan;
	}

	public GoapPlan CreateNewGoapPlan(ActualGoapNode p_action, IPointOfInterest p_target)
	{
		GoapPlan goapPlan = CreateNewGoapPlan();
		goapPlan.SetActionNodes(p_action);
		goapPlan.SetTarget(p_target);
		return goapPlan;
	}

	public GoapPlan CreateNewGoapPlan(ActualGoapNode p_action1, ActualGoapNode p_action2, IPointOfInterest p_target)
	{
		GoapPlan goapPlan = CreateNewGoapPlan();
		goapPlan.SetActionNodes(p_action1, p_action2);
		goapPlan.SetTarget(p_target);
		return goapPlan;
	}

	public GoapPlan CreateNewGoapPlan(ActualGoapNode p_action1, ActualGoapNode p_action2, ActualGoapNode p_action3, ActualGoapNode p_action4, ActualGoapNode p_action5, IPointOfInterest p_target)
	{
		GoapPlan goapPlan = CreateNewGoapPlan();
		goapPlan.SetActionNodes(p_action1, p_action2, p_action3, p_action4, p_action5);
		goapPlan.SetTarget(p_target);
		return goapPlan;
	}

	private GoapPlan CreateNewGoapPlan()
	{
		if (_goapPlanPool.Count > 0)
		{
			GoapPlan goapPlan = _goapPlanPool.Dequeue();
			if (!goapPlan.isAssigned)
			{
				goapPlan.isAssigned = true;
			}
			return goapPlan;
		}
		return new GoapPlan();
	}

	public void ReturnGoapPlanToPool(GoapPlan data)
	{
		if (data != null)
		{
			data.Reset();
			data.isAssigned = false;
			_goapPlanPool.Enqueue(data);
		}
	}

	private void ConstructSingleJobNodePool()
	{
		_jobNodePool = new Queue<SingleJobNode>();
	}

	public SingleJobNode CreateNewSingleJobNode()
	{
		lock (_jobNodePool)
		{
			if (_jobNodePool.Count > 0)
			{
				SingleJobNode singleJobNode = _jobNodePool.Dequeue();
				if (singleJobNode != null)
				{
					return singleJobNode;
				}
			}
		}
		return new SingleJobNode();
	}

	public void ReturnSingleJobNodeToPool(SingleJobNode data)
	{
		lock (_jobNodePool)
		{
			if (data != null)
			{
				_jobNodePool.Enqueue(data);
				data.Reset();
			}
		}
	}

	private void ConstructActionPool()
	{
		_actionPool = new Queue<ActualGoapNode>(100);
	}

	public ActualGoapNode CreateNewAction(GoapAction action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, int cost)
	{
		ActualGoapNode actualGoapNode = CreateNewAction();
		if (!actualGoapNode.hasBeenReset)
		{
			UnityEngine.Debug.LogError("Action is still assigned to: " + actualGoapNode.actor.name + ", " + actualGoapNode.action.name + ", " + actualGoapNode.poiTarget.name);
		}
		else
		{
			actualGoapNode.SetActionData(action, actor, poiTarget, otherData, cost);
		}
		return actualGoapNode;
	}

	private ActualGoapNode CreateNewAction()
	{
		if (_actionPool.Count > 0)
		{
			ActualGoapNode actualGoapNode = _actionPool.Dequeue();
			if (!actualGoapNode.isAssigned)
			{
				actualGoapNode.isAssigned = true;
			}
			return actualGoapNode;
		}
		ActualGoapNode actualGoapNode2 = new ActualGoapNode();
		actualGoapNode2.SetHasBeenReset(p_state: true);
		return actualGoapNode2;
	}

	public void ReturnActionToPool(ActualGoapNode data)
	{
		if (data != null && !data.hasBeenReset)
		{
			_actionPool.Enqueue(data);
			data.isAssigned = false;
			data.Reset();
			Messenger.Broadcast(ObjectPoolSignals.ACTUAL_GOAP_NODE_OBJECT_POOLED, data);
		}
	}

	private void ConstructMoodModificationPool()
	{
		_moodModificationPool = new Queue<MoodModification>(50);
	}

	public MoodModification CreateNewMoodModification()
	{
		if (_moodModificationPool.Count > 0)
		{
			return _moodModificationPool.Dequeue();
		}
		return new MoodModification();
	}

	public void ReturnMoodModificationToPool(MoodModification data)
	{
		data.Reset();
		_moodModificationPool.Enqueue(data);
	}

	private void ConstructLogFillerPool()
	{
		_logFillerPool = new Queue<LogFiller>(50);
	}

	public LogFiller CreateNewLogFiller(ILogFiller p_obj, string p_value, LOG_IDENTIFIER p_identifier)
	{
		LogFiller logFiller = CreateNewLogFiller();
		logFiller.Initialize(p_obj, p_value, p_identifier);
		return logFiller;
	}

	public LogFiller CreateNewLogFiller(ILogFiller p_obj, LOG_IDENTIFIER p_identifier)
	{
		LogFiller logFiller = CreateNewLogFiller();
		logFiller.Initialize(p_obj, p_identifier);
		return logFiller;
	}

	public LogFiller CreateNewLogFiller(string p_formattedLogFillerString, LOG_IDENTIFIER p_identifier)
	{
		LogFiller logFiller = CreateNewLogFiller();
		logFiller.Initialize(p_formattedLogFillerString, p_identifier);
		return logFiller;
	}

	public LogFiller CreateNewLogFiller()
	{
		if (_logFillerPool.Count > 0)
		{
			return _logFillerPool.Dequeue();
		}
		return new LogFiller();
	}

	public void ReturnLogFillerToPool(LogFiller data)
	{
		data.Reset();
		_logFillerPool.Enqueue(data);
	}
}
