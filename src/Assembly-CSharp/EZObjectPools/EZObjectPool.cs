using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace EZObjectPools;

[AddComponentMenu("EZ Object Pool/Object Pool")]
public class EZObjectPool : MonoBehaviour
{
	private static Dictionary<string, EZObjectPool> SharedPools = new Dictionary<string, EZObjectPool>();

	public static GameObject Marker;

	public GameObject Template;

	public string PoolName;

	public List<GameObject> ObjectList;

	public bool AutoResize;

	public int PoolSize = 100;

	public bool InstantiateOnAwake;

	public bool Shared;

	private List<GameObject> AvailableObjects;

	public static EZObjectPool CreateObjectPool(GameObject template, string name, int size, bool autoResize, bool instantiateImmediate, bool shared)
	{
		if (!Marker)
		{
			Marker = new GameObject("EZ Object Pools Container");
			SharedPools.Clear();
		}
		if (shared)
		{
			if (SharedPools.ContainsKey(name))
			{
				return SharedPools[name];
			}
			GameObject obj = new GameObject(name);
			EZObjectPool eZObjectPool = obj.AddComponent<EZObjectPool>();
			eZObjectPool.InstantiateOnAwake = false;
			eZObjectPool.SetProperties(template, size, name, autoResize);
			SharedPools.Add(name, eZObjectPool);
			if (instantiateImmediate)
			{
				eZObjectPool.InstantiatePool();
			}
			obj.transform.parent = Marker.transform;
			return eZObjectPool;
		}
		GameObject obj2 = new GameObject(name);
		EZObjectPool eZObjectPool2 = obj2.AddComponent<EZObjectPool>();
		eZObjectPool2.InstantiateOnAwake = false;
		eZObjectPool2.SetProperties(template, size, name, autoResize);
		if (instantiateImmediate)
		{
			eZObjectPool2.InstantiatePool();
		}
		obj2.transform.SetParent(Marker.transform);
		return eZObjectPool2;
	}

	private void Awake()
	{
		if (!Marker)
		{
			Marker = new GameObject("EZ Object Pools Container");
			SharedPools.Clear();
		}
		if (InstantiateOnAwake)
		{
			ObjectList = new List<GameObject>(PoolSize);
			AvailableObjects = new List<GameObject>(PoolSize);
			InstantiatePool();
		}
		if (Shared)
		{
			SharedPools.Add(PoolName, this);
		}
	}

	public void SetProperties(GameObject objectTemplate, int size, string name, bool autoResize)
	{
		Template = objectTemplate;
		PoolSize = size;
		ObjectList = new List<GameObject>(size);
		AvailableObjects = new List<GameObject>(size);
		PoolName = name;
		AutoResize = autoResize;
	}

	public void InstantiatePool()
	{
		if (Template == null)
		{
			UnityEngine.Debug.LogError("EZ Object Pool: " + base.name + ": Template GameObject is null! Make sure you assigned a template either in the inspector or in your scripts.");
			return;
		}
		ClearPool();
		for (int i = 0; i < PoolSize; i++)
		{
			GameObject gameObject = NewActiveObject();
			gameObject.GetComponent<PooledObject>().SendObjectBackToPool();
			ObjectList.Add(gameObject);
		}
	}

	public IEnumerator InstantiatePoolCoroutine(int p_poolSize, Stopwatch p_stopwatch)
	{
		p_stopwatch.Reset();
		p_stopwatch.Start();
		int count = 0;
		for (int i = 0; i < p_poolSize; i++)
		{
			GameObject gameObject = NewActiveObject();
			gameObject.GetComponent<PooledObject>().SendObjectBackToPool();
			ObjectList.Add(gameObject);
			count++;
			if (count > 20)
			{
				count = 0;
				yield return null;
			}
		}
		p_stopwatch.Stop();
	}

	public bool TryGetNextObject(Vector3 pos, Quaternion rot, out GameObject obj)
	{
		if (ObjectList == null)
		{
			throw new Exception("EZ Object Pool " + PoolName + ", the pool has not been instantiated but you are trying to retrieve an object!");
		}
		int index = AvailableObjects.Count - 1;
		if (AvailableObjects.Count > 0)
		{
			if ((object)AvailableObjects[index] == null)
			{
				obj = null;
				throw new Exception("EZObjectPool " + PoolName + " has missing objects in its pool! Are you accidentally destroying any GameObjects retrieved from the pool?");
			}
			AvailableObjects[index].transform.position = pos;
			AvailableObjects[index].transform.rotation = rot;
			AvailableObjects[index].SetActive(value: true);
			obj = AvailableObjects[index];
			AvailableObjects.RemoveAt(index);
			return true;
		}
		if (AutoResize)
		{
			GameObject gameObject = NewActiveObject();
			gameObject.transform.position = pos;
			gameObject.transform.rotation = rot;
			ObjectList.Add(gameObject);
			obj = gameObject;
			return true;
		}
		obj = null;
		return false;
	}

	public void TryGetNextObject(Vector3 pos, Quaternion rot)
	{
		if (ObjectList.Count == 0)
		{
			UnityEngine.Debug.LogError("EZ Object Pool " + PoolName + ", the pool has not been instantiated but you are trying to retrieve an object!");
		}
		int index = AvailableObjects.Count - 1;
		if (AvailableObjects.Count > 0)
		{
			if (AvailableObjects[index] == null)
			{
				UnityEngine.Debug.LogError("EZObjectPool " + PoolName + " has missing objects in its pool! Are you accidentally destroying any GameObjects retrieved from the pool?");
				return;
			}
			AvailableObjects[index].transform.position = pos;
			AvailableObjects[index].transform.rotation = rot;
			AvailableObjects[index].SetActive(value: true);
			AvailableObjects.RemoveAt(index);
		}
		else if (AutoResize)
		{
			GameObject gameObject = NewActiveObject();
			gameObject.transform.position = pos;
			gameObject.transform.rotation = rot;
			ObjectList.Add(gameObject);
		}
	}

	private GameObject NewActiveObject()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Template);
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localScale = Template.transform.localScale;
		PooledObject[] components = gameObject.GetComponents<PooledObject>();
		if (components != null && components.Length != 0)
		{
			for (int i = 0; i < components.Length; i++)
			{
				components[i].ParentPool = this;
			}
		}
		else
		{
			gameObject.AddComponent<PooledObject>().ParentPool = this;
		}
		return gameObject;
	}

	public void ClearPool()
	{
		for (int i = 0; i < ObjectList.Count; i++)
		{
			GameObject gameObject = ObjectList[i];
			if (!(gameObject == null))
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		ObjectList.Clear();
		AvailableObjects.Clear();
	}

	public void DeletePool(bool deleteActiveObjects)
	{
		for (int i = 0; i < ObjectList.Count; i++)
		{
			GameObject gameObject = ObjectList[i];
			if (!(gameObject == null) && (!gameObject.activeInHierarchy || (gameObject.activeInHierarchy && deleteActiveObjects)))
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	public void AddToAvailableObjects(GameObject obj)
	{
		AvailableObjects.Add(obj);
	}

	public int ActiveObjectCount()
	{
		return ObjectList.Count - AvailableObjects.Count;
	}

	public int AvailableObjectCount()
	{
		return AvailableObjects.Count;
	}
}
