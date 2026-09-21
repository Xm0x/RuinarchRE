using EZObjectPools;
using UnityEngine;
using UtilityScripts;

public class SnatchObjectItemsPage : PooledObject
{
	[SerializeField]
	private int maxItemsInPage;

	public bool HasMaximumChildren()
	{
		return base.transform.childCount >= maxItemsInPage;
	}

	public override void Reset()
	{
		base.Reset();
		Utilities.DestroyChildrenObjectPool(base.transform);
	}
}
