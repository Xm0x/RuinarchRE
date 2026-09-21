using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class LetGoData : PlayerAction
{
	public override bool canBeCastOnBlessed => true;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LET_GO;

	public override string name => "Let It Go";

	public override string description => "This Ability will move the target out of its cold and bothersome Prison or Kennel.";

	public LetGoData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target))
		{
			if (target is Character character)
			{
				if (character.currentStructure is Kennel)
				{
					if (character.movementComponent.isFlying && !character.traitContainer.HasTrait("Restrained"))
					{
						return false;
					}
					return true;
				}
				if (character.gridTileLocation != null && character.currentStructure is TortureChambers tortureChambers && character.currentStructure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room.parentStructure == tortureChambers)
				{
					if (character.movementComponent.isFlying && !character.traitContainer.HasTrait("Restrained"))
					{
						return false;
					}
					return true;
				}
			}
			else if (target is DemonicStructure demonicStructure)
			{
				if (demonicStructure is Kennel kennel && kennel.HasValidCharacterInside())
				{
					return true;
				}
				if (demonicStructure is TortureChambers tortureChambers2 && tortureChambers2.rooms.ElementAtOrDefault(0) is PrisonCell prisonCell && prisonCell.HasOccupants())
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			return false;
		}
		if (targetCharacter.interruptComponent.isInterrupted && (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed || targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override bool CanPerformAbilityTowards(LocationStructure targetStructure)
	{
		if (targetStructure is Kennel kennel)
		{
			if (!kennel.HasValidCharacterInside())
			{
				return false;
			}
			if (!HasCharacterInKennelThatIsValid(kennel))
			{
				return false;
			}
		}
		else if (targetStructure is TortureChambers tortureChambers)
		{
			if (tortureChambers.rooms.Length == 0)
			{
				return false;
			}
			if (tortureChambers.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell))
			{
				return false;
			}
		}
		return base.CanPerformAbilityTowards(targetStructure);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Being_Drained_Cannot_Let_Go") + "|";
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
			text = (kennel.HasValidCharacterInside() ? (text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Let_Go_No_Target") + "|") : (text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Let_Go_No_Character_Kennel") + "|"));
		}
		else if (p_targetStructure is TortureChambers tortureChambers)
		{
			if (tortureChambers.rooms.Length == 0)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Let_Go_No_Cell") + "|";
			}
			else if (tortureChambers.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell))
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Let_Go_No_Target") + "|";
			}
		}
		return text;
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		if (targetStructure is Kennel kennel)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			list.AddRange(kennel.charactersHere);
			for (int i = 0; i < list.Count; i++)
			{
				Character targetCharacter = list[i];
				if (CanPerformAbilityTowards(targetCharacter))
				{
					LetGo(targetCharacter);
				}
			}
			RuinarchListPool<Character>.Release(list);
			base.ActivateAbility(targetStructure);
		}
		else
		{
			if (!(targetStructure is TortureChambers tortureChambers))
			{
				return;
			}
			PrisonCell obj = tortureChambers.rooms[0] as PrisonCell;
			List<Character> list2 = RuinarchListPool<Character>.Claim();
			obj.PopulateCharactersInRoom(list2);
			for (int j = 0; j < list2.Count; j++)
			{
				Character targetCharacter2 = list2[j];
				if (CanPerformAbilityTowards(targetCharacter2))
				{
					LetGo(targetCharacter2);
				}
			}
			RuinarchListPool<Character>.Release(list2);
			base.ActivateAbility(targetStructure);
		}
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character targetCharacter)
		{
			LetGo(targetCharacter);
			base.ActivateAbility(targetPOI);
		}
	}

	private void LetGo(Character targetCharacter)
	{
		LocationStructure currentStructure = targetCharacter.currentStructure;
		bool becomeDazed = true;
		if (targetCharacter.faction != null)
		{
			becomeDazed = targetCharacter.faction.IsHostileWith(PlayerManager.Instance.player.playerFaction) && !targetCharacter.isAlliedWithPlayer;
		}
		targetCharacter.movementComponent.LetGo(becomeDazed);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", "Let Go activated", LOG_TAG.Player);
		log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(currentStructure, currentStructure?.GetNameRelativeTo(targetCharacter), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	private bool HasCharacterInPrisonCellThatIsValid(PrisonCell prisonCell)
	{
		return GetFirstCharacterInPrisonCellThatIsValid(prisonCell) != null;
	}

	private Character GetFirstCharacterInPrisonCellThatIsValid(PrisonCell prisonCell)
	{
		for (int i = 0; i < prisonCell.parentStructure.charactersHere.Count; i++)
		{
			Character character = prisonCell.parentStructure.charactersHere[i];
			if (character.gridTileLocation != null && prisonCell.parentStructure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room == prisonCell && CanPerformAbilityTowards(character))
			{
				return character;
			}
		}
		return null;
	}

	private bool HasCharacterInKennelThatIsValid(Kennel kennel)
	{
		return GetFirstCharacterInKennelThatIsValid(kennel) != null;
	}

	private Character GetFirstCharacterInKennelThatIsValid(Kennel kennel)
	{
		for (int i = 0; i < kennel.charactersHere.Count; i++)
		{
			Character character = kennel.charactersHere[i];
			if (CanPerformAbilityTowards(character))
			{
				return character;
			}
		}
		return null;
	}
}
