using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class TileObjectDatabase
{
	public Dictionary<TILE_OBJECT_TYPE, List<TileObject>> allTileObjects { get; private set; }

	public Dictionary<string, TileObject> tileObjectsByGUID { get; private set; }

	public HashSet<TileObject> allTileObjectsList { get; private set; }

	public List<TileObject> tempHolderOfDeadTileObjects { get; private set; }

	public List<WeakReference> destroyedTileObjects { get; private set; }

	public Dictionary<string, WeakReference> destroyedTileObjectsDictionary { get; private set; }

	public List<WeakReference> pendingDestroyedTileObjects { get; private set; }

	public CleanUpTileObjectsThread cleanUpThread { get; private set; }

	public TileObjectDatabase()
	{
		allTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>();
		tileObjectsByGUID = new Dictionary<string, TileObject>();
		allTileObjectsList = new HashSet<TileObject>();
		destroyedTileObjects = new List<WeakReference>();
		pendingDestroyedTileObjects = new List<WeakReference>();
		destroyedTileObjectsDictionary = new Dictionary<string, WeakReference>();
		tempHolderOfDeadTileObjects = new List<TileObject>();
		cleanUpThread = new CleanUpTileObjectsThread();
	}

	public void RegisterTileObject(TileObject tileObject)
	{
		if (!allTileObjects.ContainsKey(tileObject.tileObjectType))
		{
			allTileObjects.Add(tileObject.tileObjectType, new List<TileObject>());
		}
		allTileObjects[tileObject.tileObjectType].Add(tileObject);
		tileObjectsByGUID.Add(tileObject.persistentID, tileObject);
		allTileObjectsList.Add(tileObject);
	}

	public void UnRegisterTileObject(TileObject tileObject, bool processCleanUp = true)
	{
		allTileObjects[tileObject.tileObjectType].Remove(tileObject);
		tileObjectsByGUID.Remove(tileObject.persistentID);
		allTileObjectsList.Remove(tileObject);
		tileObject.SetIsDeadReference(p_state: true);
		AddDestroyedTileObject(tileObject, processCleanUp);
	}

	public TileObject GetTileObject(TILE_OBJECT_TYPE type, int id)
	{
		if (allTileObjects.ContainsKey(type))
		{
			for (int i = 0; i < allTileObjects[type].Count; i++)
			{
				TileObject tileObject = allTileObjects[type][i];
				if (tileObject.id == id)
				{
					return tileObject;
				}
			}
		}
		return null;
	}

	public TileObject GetFirstTileObject(TILE_OBJECT_TYPE type)
	{
		if (allTileObjects.ContainsKey(type))
		{
			int num = 0;
			if (num < allTileObjects[type].Count)
			{
				return allTileObjects[type][num];
			}
		}
		return null;
	}

	public TileObject GetTileObjectByPersistentID(string id)
	{
		if (tileObjectsByGUID.ContainsKey(id))
		{
			return tileObjectsByGUID[id];
		}
		if (destroyedTileObjectsDictionary.ContainsKey(id))
		{
			WeakReference weakReference = destroyedTileObjectsDictionary[id];
			if (weakReference.IsAlive)
			{
				return weakReference.Target as TileObject;
			}
		}
		throw new Exception("Could not find tile object with id " + id);
	}

	public TileObject GetTileObjectByPersistentIDSafe(string id)
	{
		if (tileObjectsByGUID.ContainsKey(id))
		{
			return tileObjectsByGUID[id];
		}
		if (destroyedTileObjectsDictionary.ContainsKey(id))
		{
			WeakReference weakReference = destroyedTileObjectsDictionary[id];
			if (weakReference.IsAlive)
			{
				return weakReference.Target as TileObject;
			}
		}
		return null;
	}

	public TileObject GetFirstArtifact(ARTIFACT_TYPE artifactType)
	{
		if (allTileObjects.ContainsKey(TILE_OBJECT_TYPE.ARTIFACT))
		{
			for (int i = 0; i < allTileObjects[TILE_OBJECT_TYPE.ARTIFACT].Count; i++)
			{
				TileObject tileObject = allTileObjects[TILE_OBJECT_TYPE.ARTIFACT][i];
				if (tileObject is Artifact artifact && artifact.type == artifactType)
				{
					return tileObject;
				}
			}
		}
		return null;
	}

	private void AddDestroyedTileObject(TileObject tileObject, bool processCleanUp)
	{
		WeakReference weakReference = new WeakReference(tileObject);
		if (cleanUpThread.isProcessing)
		{
			pendingDestroyedTileObjects.Add(weakReference);
			return;
		}
		AddDestroyedTileObject(tileObject.persistentID, weakReference);
		if (processCleanUp)
		{
			ProcessCleanUpDestroyedTileObjects();
		}
	}

	private void AddDestroyedTileObject(string id, WeakReference wr)
	{
		if (!destroyedTileObjectsDictionary.ContainsKey(id))
		{
			destroyedTileObjectsDictionary.Add(id, wr);
			destroyedTileObjects.Add(wr);
		}
	}

	private void ProcessCleanUpDestroyedTileObjects()
	{
	}

	public void DoneProcessCleanUpDestroyedTileObjects(Dictionary<string, WeakReference> cleanDictionary)
	{
		RuinarchCleanUpDictionaryPool.Release(destroyedTileObjectsDictionary);
		destroyedTileObjectsDictionary = cleanDictionary;
	}

	public void AfterDone()
	{
		if (pendingDestroyedTileObjects.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < pendingDestroyedTileObjects.Count; i++)
		{
			WeakReference weakReference = pendingDestroyedTileObjects[i];
			if (weakReference.IsAlive)
			{
				AddDestroyedTileObject((weakReference.Target as TileObject).persistentID, weakReference);
			}
		}
		pendingDestroyedTileObjects.Clear();
		ProcessCleanUpDestroyedTileObjects();
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < allTileObjectsList.Count; i++)
		{
			allTileObjectsList.ElementAt(i).CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < allTileObjectsList.Count; i++)
		{
			allTileObjectsList.ElementAt(i).CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
