namespace Inner_Maps.Location_Structures.Components;

public class SaveDataMaraudPartyStructureComponent : SaveDataPartyStructureComponent
{
	public override PartyStructureComponent Load()
	{
		return new MaraudPartyStructureComponent(this);
	}
}
