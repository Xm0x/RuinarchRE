using System;
using UnityEngine;

namespace EZObjectPools;

[AddComponentMenu("EZ Object Pools/Pooled Object")]
public class PooledObject : BaseMonoBehaviour
{
	[HideInInspector]
	public EZObjectPool ParentPool;

	public virtual void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SendObjectBackToPool()
	{
		base.gameObject.SetActive(value: false);
		base.transform.position = Vector3.zero;
		if ((bool)ParentPool)
		{
			if (ParentPool.transform != base.transform.parent)
			{
				base.transform.SetParent(ParentPool.transform);
				ParentPool.AddToAvailableObjects(base.gameObject);
			}
			return;
		}
		throw new Exception("PooledObject " + base.gameObject.name + " does not have a parent pool. If this occurred during a scene transition, ignore this. Otherwise report to developer.");
	}

	public virtual void Reset()
	{
	}

	public virtual void BeforeDestroyActions()
	{
	}
}
