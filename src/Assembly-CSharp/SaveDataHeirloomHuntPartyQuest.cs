using System;

[Serializable]
public class SaveDataHeirloomHuntPartyQuest : SaveDataPartyQuest
{
	public string targetHeirloom;

	public string targetArea;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is HeirloomHuntPartyQuest heirloomHuntPartyQuest)
		{
			if (heirloomHuntPartyQuest.targetHeirloom != null)
			{
				targetHeirloom = heirloomHuntPartyQuest.targetHeirloom.persistentID;
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(heirloomHuntPartyQuest.targetHeirloom);
			}
			if (heirloomHuntPartyQuest.targetArea != null)
			{
				targetArea = heirloomHuntPartyQuest.targetArea.persistentID;
			}
		}
	}
}
