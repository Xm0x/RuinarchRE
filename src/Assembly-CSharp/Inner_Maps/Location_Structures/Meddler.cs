using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class Meddler : DemonicStructure
{
	public Meddler(Region location)
		: base(STRUCTURE_TYPE.MEDDLER, location)
	{
		SetMaxHPAndReset(5000);
	}

	public Meddler(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		Vector3 position = structureObj.transform.position;
		worldPosition = position;
	}
}
