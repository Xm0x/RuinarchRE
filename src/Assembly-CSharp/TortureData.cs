using Inner_Maps.Location_Structures;

public class TortureData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TORTURE;

	public override string name => "Torture";

	public override string description => "This Ability will commit unspeakable suffering to the target. After the deed, the Villager will gain a negative Trait.";

	public override bool canBeCastOnBlessed => true;

	public TortureData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.ROOM,
			SPELL_TARGET.CHARACTER
		};
	}

	public override void ActivateAbility(StructureRoom room)
	{
		if (room is PrisonCell prisonCell)
		{
			prisonCell.BeginTorture();
			AkSoundEngine.PostEvent("Play_Torture", InnerMapCameraMove.Instance.gameObject);
		}
		base.ActivateAbility(room);
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character { gridTileLocation: not null } character && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room is PrisonCell prisonCell)
		{
			prisonCell.BeginTorture(character);
			AkSoundEngine.PostEvent("Play_Torture", InnerMapCameraMove.Instance.gameObject);
		}
		base.ActivateAbility(targetPOI);
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		if (targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell)
		{
			prisonCell.BeginTorture();
			AkSoundEngine.PostEvent("Play_Torture", InnerMapCameraMove.Instance.gameObject);
			base.ActivateAbility(targetStructure);
		}
	}

	public override bool CanPerformAbilityTowards(StructureRoom room)
	{
		if (base.CanPerformAbilityTowards(room))
		{
			if (room is PrisonCell prisonCell)
			{
				if (prisonCell.currentTortureTarget == null && prisonCell.currentBrainwashTarget == null)
				{
					return prisonCell.HasValidTortureTarget();
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(LocationStructure targetStructure)
	{
		if (base.CanPerformAbilityTowards(targetStructure))
		{
			if (targetStructure is TortureChambers tortureChambers)
			{
				if (tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell { currentBrainwashTarget: null, currentTortureTarget: null } prisonCell && prisonCell.HasValidTortureTarget())
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell prisonCell)
			{
				if (prisonCell.currentTortureTarget == null && prisonCell.currentBrainwashTarget == null)
				{
					return prisonCell.IsValidTortureTarget(targetCharacter);
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
		if (p_targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell)
		{
			if (prisonCell.currentBrainwashTarget != null)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Structure_Being_Brainwashed") + "|";
			}
			else if (prisonCell.currentTortureTarget != null)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Structure_Being_Tortured") + "|";
			}
			else if (!prisonCell.HasValidTortureTarget())
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Structure_No_Torture_Target") + "|";
			}
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (flag)
		{
			if (target is Character character)
			{
				if (character.gridTileLocation != null && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room is PrisonCell)
				{
					return true;
				}
				return false;
			}
			if (target is TortureChambers tortureChambers)
			{
				if (tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && prisonCell.HasValidOccupant())
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return flag;
	}
}
