using System;

namespace Inner_Maps.Location_Structures.Components;

public class MaraudPartyStructureComponent : PartyStructureComponent
{
	public override Type serializedData => typeof(SaveDataMaraudPartyStructureComponent);

	public MaraudPartyStructureComponent(LocationStructure p_owner)
		: base(p_owner)
	{
	}

	public MaraudPartyStructureComponent(SaveDataPartyStructureComponent p_data)
		: base(p_data)
	{
	}

	public override bool IsAvailable()
	{
		return true;
	}
}
