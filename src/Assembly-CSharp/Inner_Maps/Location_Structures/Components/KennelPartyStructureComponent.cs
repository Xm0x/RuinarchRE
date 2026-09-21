using System;

namespace Inner_Maps.Location_Structures.Components;

public class KennelPartyStructureComponent : PartyStructureComponent
{
	private Kennel _kennel;

	public override Type serializedData => typeof(SaveDataKennelPartyStructureComponent);

	public KennelPartyStructureComponent(LocationStructure p_owner)
		: base(p_owner)
	{
		_kennel = p_owner as Kennel;
	}

	public KennelPartyStructureComponent(SaveDataPartyStructureComponent p_data)
		: base(p_data)
	{
	}

	public override void LoadReferences(LocationStructure p_owner, SaveDataPartyStructureComponent p_saveDataPartyStructureComponent)
	{
		base.LoadReferences(p_owner, p_saveDataPartyStructureComponent);
		_kennel = base.owner as Kennel;
	}

	public override bool IsAvailable()
	{
		return !_kennel.HasValidCharacterInside();
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.owner.persistentID))
		{
			_kennel = null;
			base.CleanUp();
		}
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = _kennel;
	}
}
