using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class CreateCaptiveIntelData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CREATE_CAPTIVE_INTEL;

	public override string name => "Create Captive Intel";

	public override string description => "This Ability creates captive intel.";

	public override bool canBeCastOnBlessed => true;

	public CreateCaptiveIntelData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.IS_CAPTIVE], character, character, new OtherData[1]
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

	private void OnReachIntelTab(GameObject effectGO)
	{
		PlayerUI.Instance.DoIntelTabPunchEffect();
		ObjectPoolManager.Instance.DestroyObject(effectGO);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target) && target is Character character)
		{
			if (character.gridTileLocation != null && character.currentStructure is TortureChambers && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room is PrisonCell)
			{
				return false;
			}
			if (character.isNormalCharacter && !character.isDead && character.gridTileLocation != null)
			{
				return character.traitContainer.HasTrait("Restrained");
			}
			return false;
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (PlayerManager.Instance.player.HasIsCaptiveIntel(targetCharacter))
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
		if (PlayerManager.Instance.player.HasIsCaptiveIntel(targetCharacter))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Has_Captive_Intel", targetCharacter) + "|";
		}
		return text;
	}
}
