using Inner_Maps.Location_Structures;

public class BrainwashData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BRAINWASH;

	public override string name => "Brainwash";

	public override string description => "This Ability will attempt to turn the target into a Cultist. The target's Mood as well as some of its Traits will affect the success rate. Leaders are also much more difficult to brainwash.";

	public BrainwashData()
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
			prisonCell.StartBrainwash();
			AkSoundEngine.PostEvent("Play_Brainwash", InnerMapCameraMove.Instance.gameObject);
		}
		base.ActivateAbility(room);
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		if (targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell)
		{
			prisonCell.StartBrainwash();
			AkSoundEngine.PostEvent("Play_Brainwash", InnerMapCameraMove.Instance.gameObject);
			base.ActivateAbility(targetStructure);
		}
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character { gridTileLocation: not null } character && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room is PrisonCell prisonCell)
		{
			prisonCell.StartBrainwash(character);
			AkSoundEngine.PostEvent("Play_Brainwash", InnerMapCameraMove.Instance.gameObject);
		}
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(StructureRoom room)
	{
		if (base.CanPerformAbilityTowards(room))
		{
			if (room is PrisonCell prisonCell)
			{
				if (prisonCell.currentBrainwashTarget == null && prisonCell.currentTortureTarget == null)
				{
					return prisonCell.HasValidBrainwashTarget();
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
				if (tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell { currentBrainwashTarget: null, currentTortureTarget: null } prisonCell && prisonCell.HasValidBrainwashTarget())
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
				if (prisonCell.currentBrainwashTarget == null && prisonCell.currentTortureTarget == null)
				{
					return prisonCell.IsValidBrainwashTarget(targetCharacter);
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
			else if (!prisonCell.HasValidBrainwashTarget())
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Structure_No_Brainwash_Target") + "|";
			}
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target))
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
		return false;
	}
}
