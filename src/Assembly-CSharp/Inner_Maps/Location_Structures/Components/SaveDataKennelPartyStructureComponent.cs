namespace Inner_Maps.Location_Structures.Components;

public class SaveDataKennelPartyStructureComponent : SaveDataPartyStructureComponent
{
	public override PartyStructureComponent Load()
	{
		return new KennelPartyStructureComponent(this);
	}
}
