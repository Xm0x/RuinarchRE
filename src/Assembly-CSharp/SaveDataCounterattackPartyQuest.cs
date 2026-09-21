using System;

[Serializable]
public class SaveDataCounterattackPartyQuest : SaveDataPartyQuest
{
	public int numberOfStructureToBeDestroyed;

	public int numberOfDestroyedStructures;

	public override void Save(PartyQuest data)
	{
		base.Save(data);
		if (data is CounterattackPartyQuest counterattackPartyQuest)
		{
			numberOfStructureToBeDestroyed = counterattackPartyQuest.numberOfStructureToBeDestroyed;
			numberOfDestroyedStructures = counterattackPartyQuest.numberOfDestroyedStructures;
		}
	}
}
