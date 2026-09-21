using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class CreateBlackmailData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CREATE_BLACKMAIL;

	public override string name => "Create Blackmail";

	public override string description => "This Ability produces an Intel regarding the state of a Player's Prisoner. It may be used as a blackmail material when shared with the Prisoner's acquaintances.";

	public override bool canBeCastOnBlessed => true;

	public CreateBlackmailData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.IS_IMPRISONED], character, character, new OtherData[1]
			{
				new LocationStructureOtherData(character.currentStructure)
			}, 0);
			actualGoapNode.SetAsIllusion();
			ActionIntel newIntel = InteractionManager.Instance.CreateNewIntel(actualGoapNode);
			PlayerManager.Instance.player.AddIntel(newIntel);
			Vector3 worldPosition = character.worldPosition;
			worldPosition.z = 0f;
			GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("StoreIntelEffect", worldPosition, Quaternion.identity, InnerMapManager.Instance.transform, isWorldPosition: true);
			effectGO.transform.position = worldPosition;
			Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(PlayerUI.Instance.intelToggle.transform.position);
			Vector3 worldPosition2 = character.worldPosition;
			worldPosition2.x += 5f;
			Vector3 vector2 = vector;
			vector2.y -= 5f;
			effectGO.transform.DOPath(new Vector3[3] { vector, worldPosition2, vector2 }, 0.7f, PathType.CubicBezier).SetEase(Ease.InSine).OnComplete(delegate
			{
				OnReachIntelTab(effectGO);
			});
			base.ActivateAbility(targetPOI);
		}
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		if (targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell)
		{
			Character firstCharacterInPrisonCellThatIsValid = GetFirstCharacterInPrisonCellThatIsValid(prisonCell);
			if (firstCharacterInPrisonCellThatIsValid != null)
			{
				ActivateAbility(firstCharacterInPrisonCellThatIsValid);
			}
		}
	}

	private void OnReachIntelTab(GameObject effectGO)
	{
		PlayerUI.Instance.DoIntelTabPunchEffect();
		ObjectPoolManager.Instance.DestroyObject(effectGO);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target))
		{
			if (target is Character character)
			{
				if (character.isNormalCharacter && !character.isDead && character.gridTileLocation != null && character.currentStructure is TortureChambers && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room))
				{
					return room is PrisonCell;
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
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (PlayerManager.Instance.player.playerSkillComponent.AlreadyHasBlackmail(targetCharacter))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(LocationStructure targetStructure)
	{
		if (base.CanPerformAbilityTowards(targetStructure) && targetStructure is TortureChambers tortureChambers)
		{
			if (tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && HasCharacterInPrisonCellThatIsValid(prisonCell))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (PlayerManager.Instance.player.playerSkillComponent.AlreadyHasBlackmail(targetCharacter))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Has_Blackmail", targetCharacter) + "|";
		}
		return text;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
		if (p_targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Has_Blackmail_All") + "|";
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
