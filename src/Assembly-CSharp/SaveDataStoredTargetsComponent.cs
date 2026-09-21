using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataStoredTargetsComponent : SaveData<StoredTargetsComponent>
{
	public List<string> allStoredTargets;

	public List<STORED_TARGET_TYPE> allStoredTargetTypes;

	public string[] validRaidTargets;

	public override void Save(StoredTargetsComponent component)
	{
		allStoredTargets = new List<string>();
		allStoredTargetTypes = new List<STORED_TARGET_TYPE>();
		for (int i = 0; i < component.allStoredTargets.Count; i++)
		{
			IStoredTarget storedTarget = component.allStoredTargets[i];
			allStoredTargets.Add(storedTarget.persistentID);
			allStoredTargetTypes.Add(storedTarget.storedTargetType);
		}
		if (component.validRaidTargets.Count > 0)
		{
			validRaidTargets = new string[component.validRaidTargets.Count];
			for (int j = 0; j < component.validRaidTargets.Count; j++)
			{
				IStoredTarget storedTarget2 = component.validRaidTargets[j];
				validRaidTargets[j] = storedTarget2.persistentID;
			}
		}
	}

	public override StoredTargetsComponent Load()
	{
		return new StoredTargetsComponent();
	}
}
