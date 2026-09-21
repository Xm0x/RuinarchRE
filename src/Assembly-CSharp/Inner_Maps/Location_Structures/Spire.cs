using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class Spire : DemonicStructure
{
	public Spire(Region location)
		: base(STRUCTURE_TYPE.SPIRE, location)
	{
		SetMaxHPAndReset(5000);
	}

	public Spire(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		Vector3 position = structureObj.transform.position;
		position.x -= 0.5f;
		worldPosition = position;
	}
}
