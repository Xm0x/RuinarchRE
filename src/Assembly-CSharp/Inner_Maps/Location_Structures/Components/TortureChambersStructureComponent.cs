using System;

namespace Inner_Maps.Location_Structures.Components;

public class TortureChambersStructureComponent : PartyStructureComponent
{
	private TortureChambers _tortureChambers;

	public override Type serializedData => typeof(SaveDataTortureChambersPartyStructureComponent);

	public TortureChambersStructureComponent(LocationStructure p_owner)
		: base(p_owner)
	{
		_tortureChambers = p_owner as TortureChambers;
	}

	public TortureChambersStructureComponent(SaveDataPartyStructureComponent p_data)
		: base(p_data)
	{
	}

	public override void LoadReferences(LocationStructure p_owner, SaveDataPartyStructureComponent p_saveDataPartyStructureComponent)
	{
		base.LoadReferences(p_owner, p_saveDataPartyStructureComponent);
		_tortureChambers = base.owner as TortureChambers;
	}

	public override bool IsAvailable()
	{
		if (_tortureChambers.HasRoomAvailableForSnatchVillager() && _tortureChambers.rooms.Length != 0 && _tortureChambers.rooms[0] is PrisonCell prisonCell)
		{
			return !prisonCell.HasOccupants();
		}
		return false;
	}

	protected override void OnCharacterDied(Character p_deadCharacter)
	{
		base.OnCharacterDied(p_deadCharacter);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.owner.persistentID))
		{
			_tortureChambers = null;
			base.CleanUp();
		}
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = _tortureChambers;
	}
}
