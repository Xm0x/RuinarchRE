namespace Inner_Maps.Location_Structures.Components;

public class SaveDataTortureChambersPartyStructureComponent : SaveDataPartyStructureComponent
{
	public override PartyStructureComponent Load()
	{
		return new TortureChambersStructureComponent(this);
	}
}
