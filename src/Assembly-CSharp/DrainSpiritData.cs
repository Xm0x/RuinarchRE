using Inner_Maps;
using Inner_Maps.Location_Structures;

public class DrainSpiritData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DRAIN_SPIRIT;

	public override string name => "Drain Spirit";

	public override string description => "This Ability slowly kills the target to produce Chaos Orbs.";

	public DrainSpiritData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			character.traitContainer.AddTrait(character, "Being Drained");
			AkSoundEngine.PostEvent("Play_Drain_Spirit", InnerMapCameraMove.Instance.gameObject);
			base.ActivateAbility(targetPOI);
		}
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		if (targetStructure is Kennel kennel)
		{
			ActivateAbility(kennel.occupyingSummon);
		}
		else if (targetStructure is TortureChambers tortureChambers)
		{
			PrisonCell prisonCell = tortureChambers.rooms[0] as PrisonCell;
			Character firstCharacterInPrisonCellThatIsValid = GetFirstCharacterInPrisonCellThatIsValid(prisonCell);
			ActivateAbility(firstCharacterInPrisonCellThatIsValid);
		}
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target))
		{
			if (target is Character character)
			{
				if (character.isDead)
				{
					return false;
				}
				StructureRoom room;
				if (character is Summon)
				{
					if (character.currentStructure is Kennel kennel && kennel.occupyingSummon == character)
					{
						return true;
					}
				}
				else if (character.gridTileLocation != null && character.currentStructure is TortureChambers tortureChambers && character.currentStructure.IsTilePartOfARoom(character.gridTileLocation, out room) && room.parentStructure == tortureChambers)
				{
					return true;
				}
			}
			else if (target is DemonicStructure demonicStructure)
			{
				if (demonicStructure is Kennel { occupyingSummon: not null })
				{
					return true;
				}
				if (demonicStructure is TortureChambers tortureChambers2 && tortureChambers2.rooms.Length != 0 && tortureChambers2.rooms[0] is PrisonCell prisonCell && prisonCell.HasValidOccupant())
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter.traitContainer.HasTrait("Being Drained"))
			{
				return false;
			}
			if (targetCharacter.interruptComponent.isInterrupted && (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed || targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(LocationStructure targetStructure)
	{
		if (base.CanPerformAbilityTowards(targetStructure))
		{
			if (targetStructure is Kennel kennel)
			{
				if (kennel.occupyingSummon != null && !CanPerformAbilityTowards(kennel.occupyingSummon))
				{
					return false;
				}
			}
			else if (targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Drained") + "|";
		}
		if (targetCharacter.interruptComponent.isInterrupted)
		{
			if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Being_Brainwashed") + "|";
			}
			else if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Being_Tortured") + "|";
			}
		}
		return text;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
		if (p_targetStructure is Kennel kennel)
		{
			if (kennel.occupyingSummon != null && !CanPerformAbilityTowards(kennel.occupyingSummon))
			{
				text += GetReasonsWhyCannotPerformAbilityTowards(kennel.occupyingSummon);
			}
		}
		else if (p_targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Drain_Spirit_No_Target") + "|";
		}
		return text;
	}

	private bool HasCharacterInPrisonCellThatIsValid(PrisonCell prisonCell)
	{
		return GetFirstCharacterInPrisonCellThatIsValid(prisonCell) != null;
	}

	private Character GetFirstCharacterInPrisonCellThatIsValid(PrisonCell prisonCell)
	{
		for (int i = 0; i < prisonCell.tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = prisonCell.tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if (CanPerformAbilityTowards(character) && IsValid(character))
				{
					return character;
				}
			}
		}
		return null;
	}
}
