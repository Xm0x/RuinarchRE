using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataAssumptionComponent : SaveData<AssumptionComponent>
{
	public List<AssumptionData> assumptionData;

	public override void Save(AssumptionComponent data)
	{
		assumptionData = new List<AssumptionData>(data.assumptionData);
	}

	public override AssumptionComponent Load()
	{
		return new AssumptionComponent(this);
	}
}
