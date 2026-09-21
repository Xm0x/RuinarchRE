using System;

public class HeirloomHuntPartyQuest : PartyQuest
{
	private bool isSearching;

	private int currentChance;

	public Heirloom targetHeirloom { get; private set; }

	public Area targetArea { get; private set; }

	public bool foundHeirloom { get; private set; }

	public override IPartyQuestTarget target => targetHeirloom;

	public override Type serializedData => typeof(SaveDataHeirloomHuntPartyQuest);

	public HeirloomHuntPartyQuest()
		: base(PARTY_QUEST_TYPE.Heirloom_Hunt)
	{
		base.minimumPartySize = 3;
		base.priority = 1;
		base.relatedBehaviour = typeof(HeirloomHuntBehaviour);
	}

	public HeirloomHuntPartyQuest(SaveDataHeirloomHuntPartyQuest data)
		: base(data)
	{
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetArea;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName;
	}

	public void SetTargetHeirloom(Heirloom heirloom)
	{
		targetHeirloom = heirloom;
	}

	public void SetFoundHeirloom(bool state)
	{
		foundHeirloom = state;
		if (foundHeirloom)
		{
			currentChance = 100;
		}
	}

	private void ProcessSettingTargetHex()
	{
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataHeirloomHuntPartyQuest saveDataHeirloomHuntPartyQuest)
		{
			if (!string.IsNullOrEmpty(saveDataHeirloomHuntPartyQuest.targetHeirloom))
			{
				targetHeirloom = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataHeirloomHuntPartyQuest.targetHeirloom) as Heirloom;
			}
			if (!string.IsNullOrEmpty(saveDataHeirloomHuntPartyQuest.targetArea))
			{
				targetArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataHeirloomHuntPartyQuest.targetArea);
			}
		}
	}
}
