using EZObjectPools;

namespace Inner_Maps.Location_Structures;

public class StructureTemplate : PooledObject
{
	public LocationStructureObject[] structureObjects;

	public void CheckForDestroy()
	{
		bool flag = true;
		for (int i = 0; i < structureObjects.Length; i++)
		{
			if (structureObjects[i].gameObject.activeSelf)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			ObjectPoolManager.Instance.DestroyObject(this);
		}
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < structureObjects.Length; i++)
		{
			structureObjects[i].Reset();
		}
	}
}
