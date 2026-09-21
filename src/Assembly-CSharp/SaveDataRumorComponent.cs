using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataRumorComponent : SaveData<RumorComponent>
{
	public List<string> negativeInfoIDs;

	public override void Save(RumorComponent data)
	{
		negativeInfoIDs = RuinarchListPool<string>.Claim();
		for (int i = 0; i < data.negativeInfoPool.Count; i++)
		{
			ActualGoapNode actualGoapNode = data.negativeInfoPool[i];
			if (!actualGoapNode.hasBeenReset)
			{
				negativeInfoIDs.Add(actualGoapNode.persistentID);
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(actualGoapNode);
			}
		}
	}

	public override RumorComponent Load()
	{
		return new RumorComponent(this);
	}

	public override void CleanUp()
	{
		if (negativeInfoIDs != null)
		{
			RuinarchListPool<string>.Release(negativeInfoIDs);
			negativeInfoIDs = null;
		}
	}
}
