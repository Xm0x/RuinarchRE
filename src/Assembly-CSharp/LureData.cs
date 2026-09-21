using System.Collections.Generic;
using System.Text;
using UtilityScripts;

public class LureData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LURE;

	public override string name => "Lure";

	public LureData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE_OBJECT };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		TileObject tileObject = targetPOI as TileObject;
		if (tileObject == null)
		{
			return;
		}
		Area area = tileObject.gridTileLocation.area;
		List<Area> list = RuinarchListPool<Area>.Claim();
		area.PopulateAreasInRange(list, 6, includeCenterTile: true);
		List<Character> list2 = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < PlayerManager.Instance.player.storedTargetsComponent.storedVillagers.Count; i++)
		{
			if (PlayerManager.Instance.player.storedTargetsComponent.storedVillagers[i] is Character character && IsCharacterNearEnough(list, character))
			{
				list2.Add(character);
			}
		}
		UIManager.Instance.ShowClickableObjectPicker(list2, delegate(object o)
		{
			OnChooseCharacter(o, tileObject);
		}, null, (Character t) => CanTriggerLure(t, tileObject), "", delegate(Character t)
		{
			OnHoverEnter(tileObject, t);
		}, OnHoverExit, "", showCover: true, 25);
		RuinarchListPool<Character>.Release(list2);
		RuinarchListPool<Area>.Release(list);
	}

	public override bool IsValid(IPlayerActionTarget p_target)
	{
		if (base.IsValid(p_target))
		{
			return PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.MANIFEST_FOOD).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Unlock_Lure);
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		bool flag = base.CanPerformAbilityTowards(tileObject);
		if (flag)
		{
			if (tileObject.gridTileLocation == null)
			{
				return false;
			}
			if (tileObject.HasJobTargetingThis(JOB_TYPE.MANIFEST_FOOD_EAT))
			{
				return false;
			}
			if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null)
			{
				if (base.subMenus == null || base.subMenus.Count <= 0)
				{
					return false;
				}
			}
			else if (!HasValidStoredCharacter(tileObject))
			{
				return false;
			}
		}
		return flag;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag && targetCharacter.jobQueue.HasJob(JOB_TYPE.MANIFEST_FOOD_EAT))
		{
			return false;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetTileObject);
		if (targetTileObject.gridTileLocation == null && targetTileObject.isBeingCarriedBy != null)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Being_Carried") + "|";
		}
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null)
		{
			if (base.subMenus != null && base.subMenus.Count <= 0)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("No_Valid_Stored_Characters") + "|";
			}
		}
		else if (!HasValidStoredCharacter(targetTileObject))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("No_Valid_Stored_Characters") + "|";
		}
		return text;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!targetCharacter.limiterComponent.canMove || !targetCharacter.limiterComponent.canPerform)
		{
			text += GetLocalizedReasonWhyCannotPerformAbilityTowards("Lure_Cannot_Perform");
		}
		return text;
	}

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is TileObject { gridTileLocation: not null } tileObject)
		{
			p_contextMenuItems.Clear();
			Area area = tileObject.gridTileLocation.area;
			List<Area> list = RuinarchListPool<Area>.Claim();
			area.PopulateAreasInRange(list, 6, includeCenterTile: true);
			for (int i = 0; i < PlayerManager.Instance.player.storedTargetsComponent.storedVillagers.Count; i++)
			{
				Character character = PlayerManager.Instance.player.storedTargetsComponent.storedVillagers[i] as Character;
				if (IsCharacterNearEnough(list, character))
				{
					p_contextMenuItems.Add(character);
				}
			}
			RuinarchListPool<Area>.Release(list);
			return p_contextMenuItems;
		}
		return null;
	}

	private bool IsCharacterNearEnough(List<Area> areasInRange, Character p_character)
	{
		if (p_character.gridTileLocation != null && areasInRange.Contains(p_character.gridTileLocation.area))
		{
			return true;
		}
		return false;
	}

	public bool CanTriggerLure(Character p_character, TileObject tileObject)
	{
		bool flag = CanPerformAbilityTowards(p_character);
		if (flag)
		{
			if (tileObject.gridTileLocation == null)
			{
				return false;
			}
			if (!p_character.limiterComponent.canMove || !p_character.limiterComponent.canPerform)
			{
				return false;
			}
			Area area = tileObject.gridTileLocation.area;
			List<Area> list = RuinarchListPool<Area>.Claim();
			area.PopulateAreasInRange(list, 6, includeCenterTile: true);
			if (!IsCharacterNearEnough(list, p_character))
			{
				RuinarchListPool<Area>.Release(list);
				return false;
			}
		}
		return flag;
	}

	public void TriggerLure(Character p_character, TileObject tileObject)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MANIFEST_FOOD_EAT, INTERACTION_TYPE.EAT, tileObject, p_character);
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.EAT], p_character, tileObject, null, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, p_character);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		p_character.jobQueue.AddJobInQueue(goapPlanJob);
	}

	private void OnHoverEnter(TileObject owner, Character target)
	{
		string reasonsWhyCannotPerformAbilityTowards = GetReasonsWhyCannotPerformAbilityTowards(target);
		if (!string.IsNullOrEmpty(reasonsWhyCannotPerformAbilityTowards))
		{
			StringBuilder stringBuilder = new StringBuilder();
			Utilities.SplitStringIntoNewLines(reasonsWhyCannotPerformAbilityTowards, '|', stringBuilder);
			reasonsWhyCannotPerformAbilityTowards = stringBuilder.ToString().TrimEnd();
			reasonsWhyCannotPerformAbilityTowards = Utilities.ColorizeInvalidText(reasonsWhyCannotPerformAbilityTowards);
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, reasonsWhyCannotPerformAbilityTowards);
		}
	}

	private void OnHoverExit(Character target)
	{
		PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
	}

	private void OnChooseCharacter(object obj, TileObject actor)
	{
		if (obj is Character p_character)
		{
			UIManager.Instance.HideObjectPicker();
			TriggerLure(p_character, actor);
			base.ActivateAbility((IPointOfInterest)actor);
		}
	}

	private bool HasValidStoredCharacter(TileObject tileObject)
	{
		Area area = tileObject.gridTileLocation.area;
		List<Area> list = RuinarchListPool<Area>.Claim();
		area.PopulateAreasInRange(list, 6, includeCenterTile: true);
		for (int i = 0; i < PlayerManager.Instance.player.storedTargetsComponent.storedVillagers.Count; i++)
		{
			if (PlayerManager.Instance.player.storedTargetsComponent.storedVillagers[i] is Character p_character && IsCharacterNearEnough(list, p_character))
			{
				RuinarchListPool<Area>.Release(list);
				return true;
			}
		}
		RuinarchListPool<Area>.Release(list);
		return false;
	}
}
