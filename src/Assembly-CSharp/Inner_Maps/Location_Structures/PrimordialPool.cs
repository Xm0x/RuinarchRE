using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class PrimordialPool : DemonicStructure
{
	public override Vector3 worldPosition => base.structureObj.transform.position;

	public PrimordialPool(Region location)
		: base(STRUCTURE_TYPE.PRIMORDIAL_POOL, location)
	{
		SetMaxHPAndReset(5000);
	}

	public PrimordialPool(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
	}
}
