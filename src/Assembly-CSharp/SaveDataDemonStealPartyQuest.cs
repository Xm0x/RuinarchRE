using System;

[Serializable]
public class SaveDataDemonStealPartyQuest : SaveDataPartyQuest
{
	public string targetItem;

	public string dropStructure;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is DemonStealPartyQuest demonStealPartyQuest)
		{
			if (demonStealPartyQuest.targetItem != null)
			{
				targetItem = demonStealPartyQuest.targetItem.persistentID;
			}
			if (demonStealPartyQuest.dropStructure != null)
			{
				dropStructure = demonStealPartyQuest.dropStructure.persistentID;
			}
		}
	}
}
