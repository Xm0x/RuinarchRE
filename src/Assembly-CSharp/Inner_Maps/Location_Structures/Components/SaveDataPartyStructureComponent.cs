namespace Inner_Maps.Location_Structures.Components;

public class SaveDataPartyStructureComponent : SaveData<PartyStructureComponent>
{
	public string ownerID;

	public string partyID;

	public override void Save(PartyStructureComponent data)
	{
		base.Save(data);
		ownerID = data.owner.persistentID;
		if (data.party != null)
		{
			partyID = data.party.persistentID;
		}
	}

	public override PartyStructureComponent Load()
	{
		return new PartyStructureComponent(this);
	}
}
