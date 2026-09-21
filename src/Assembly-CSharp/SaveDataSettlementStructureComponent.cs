using System.Collections.Generic;

public class SaveDataSettlementStructureComponent : SaveData<SettlementStructureComponent>
{
	public List<string> linkedStructures;

	public override void Save(SettlementStructureComponent data)
	{
		if (data.linkedStructures.Count > 0)
		{
			linkedStructures = new List<string>();
			for (int i = 0; i < data.linkedStructures.Count; i++)
			{
				linkedStructures.Add(data.linkedStructures[i].persistentID);
			}
		}
	}

	public override SettlementStructureComponent Load()
	{
		return new SettlementStructureComponent(this);
	}
}
