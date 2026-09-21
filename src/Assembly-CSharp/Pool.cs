using UnityEngine;

public class Pool : MonoBehaviour
{
	private GameObject[] ObjectPool;

	private GameObject ObjectToPool;

	public void CreatePool(GameObject ObjectToPool, int numberOfObjects)
	{
		ObjectPool = new GameObject[numberOfObjects];
		this.ObjectToPool = ObjectToPool;
		for (int i = 0; i < ObjectPool.Length; i++)
		{
			ObjectPool[i] = Object.Instantiate(ObjectToPool);
			ObjectPool[i].SetActive(value: false);
		}
	}

	public GameObject GetObject()
	{
		for (int i = 0; i < ObjectPool.Length; i++)
		{
			if ((bool)ObjectPool[i])
			{
				if (!ObjectPool[i].activeSelf)
				{
					ObjectPool[i].SetActive(value: true);
					return ObjectPool[i];
				}
			}
			else
			{
				ObjectPool[i] = Object.Instantiate(ObjectToPool);
				ObjectPool[i].SetActive(value: false);
			}
		}
		return null;
	}
}
