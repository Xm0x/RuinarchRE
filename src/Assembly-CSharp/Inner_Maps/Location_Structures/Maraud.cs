using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class Maraud : DemonicStructure
{
	public Maraud(Region location)
		: base(STRUCTURE_TYPE.MARAUD, location)
	{
		SetMaxHPAndReset(5000);
	}

	public Maraud(Region location, SaveDataDemonicStructure data)
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
