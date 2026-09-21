using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UnityEngine;
using UtilityScripts;

public class PlayerAction : SkillData, IContextMenuItem
{
	private List<IContextMenuItem> _contextMenuItems;

	private string _cachedContextMenuNameWithChaosOrbs;

	private string _cachedContextMenuNameWithoutChaosOrbs;

	public virtual bool canBeCastOnBlessed => false;

	public virtual bool canBeCastOnTemporal => false;

	public virtual bool shouldShowOnContextMenu => true;

	public virtual Sprite contextMenuIcon => PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(type)?.contextMenuIcon;

	public string contextMenuName => GetContextMenuName();

	public virtual int contextMenuColumn => PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(type)?.contextMenuColumn ?? 0;

	public List<IContextMenuItem> subMenus => GetSubMenus(_contextMenuItems);

	public override string localizedName => LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", name);

	public override string localizedDescription => LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", name + "_Description") ?? "";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public PlayerAction()
	{
		_contextMenuItems = new List<IContextMenuItem>();
	}

	private string GetContextMenuName()
	{
		if (_cachedContextMenuNameWithChaosOrbs == null)
		{
			_cachedContextMenuNameWithChaosOrbs = Utilities.ColorizeName(localizedName, PlayerSkillManager.Instance.withChaosOrbsTextColor);
		}
		if (_cachedContextMenuNameWithoutChaosOrbs == null)
		{
			_cachedContextMenuNameWithoutChaosOrbs = Utilities.ColorizeName(localizedName, PlayerSkillManager.Instance.withoutChaosOrbsTextColor);
		}
		if (!base.hasRemainingOrUnliChaosOrbs || type == PLAYER_SKILL_TYPE.BRAINWASH)
		{
			return _cachedContextMenuNameWithoutChaosOrbs;
		}
		return _cachedContextMenuNameWithChaosOrbs;
	}

	public void ResetCachedTexts()
	{
		_cachedContextMenuNameWithChaosOrbs = null;
		_cachedContextMenuNameWithoutChaosOrbs = null;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			return character.hasMarker;
		}
		if (target is TileObject tileObject)
		{
			if (tileObject.mapObjectVisual != null)
			{
				return tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT;
			}
			return false;
		}
		return true;
	}

	public string GetLabelName(IPlayerActionTarget target)
	{
		return localizedName;
	}

	protected virtual List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		return null;
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is TileObject)
		{
			IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
		}
		else if (targetPOI is Character)
		{
			if (targetPOI.traitContainer.HasTrait("Grounded"))
			{
				targetPOI.traitContainer.GetTraitOrStatus<Grounded>("Grounded").DrainManaFromGrounded(this);
			}
			if (targetPOI.traitContainer.HasTrait("Nullchild"))
			{
				targetPOI.traitContainer.GetTraitOrStatus<Nullchild>("Nullchild").TryLockSkillUsedOnCharacter(this);
			}
		}
		base.ActivateAbility(targetPOI);
		Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_EXECUTED_TOWARDS_POI, this, targetPOI);
	}

	public virtual void Activate(IPlayerActionTarget target, bool bypassChance = false)
	{
		if (target is Character character && character.traitContainer.HasTrait("Mummified") && PlayerManager.Instance.player.playerSkillComponent.allPowersCooldownMultiplier != 2 && ChanceData.RollChance(CHANCE_TYPE.Release_Mummy_Player))
		{
			character.traitComponent.MummifiedReleaseIncantationByPlayer();
		}
		else if (bypassChance || RollSuccessChance(target) || category == PLAYER_SKILL_CATEGORY.SCHEME || type == PLAYER_SKILL_TYPE.AFFLICT || category == PLAYER_SKILL_CATEGORY.RAID || type == PLAYER_SKILL_TYPE.UNDEPLOY_PARTY)
		{
			if (target is IPointOfInterest targetPOI)
			{
				ActivateAbility(targetPOI);
			}
			else if (target is Area targetArea)
			{
				ActivateAbility(targetArea);
			}
			else if (target is LocationStructure targetStructure)
			{
				ActivateAbility(targetStructure);
			}
			else if (target is StructureRoom room)
			{
				ActivateAbility(room);
			}
			else if (target is BaseSettlement targetSettlement)
			{
				ActivateAbility(targetSettlement);
			}
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ACTIVATED, this);
		}
		else
		{
			OnExecutePlayerSkill();
			if (target is Character character2)
			{
				character2.reactionComponent.ResistRuinarchPower();
			}
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_RESISTED, this);
		}
	}

	public bool CanPerformAbilityTo(IPlayerActionTarget target)
	{
		if (target is IPointOfInterest poi)
		{
			return CanPerformAbilityTowards(poi);
		}
		if (target is Area targetArea)
		{
			return CanPerformAbilityTowards(targetArea);
		}
		if (target is LocationStructure targetStructure)
		{
			return CanPerformAbilityTowards(targetStructure);
		}
		if (target is StructureRoom room)
		{
			return CanPerformAbilityTowards(room);
		}
		if (target is BaseSettlement targetSettlement)
		{
			return CanPerformAbilityTowards(targetSettlement);
		}
		return CanPerformAbility();
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (!canBeCastOnBlessed && targetCharacter.traitContainer.IsBlessed())
		{
			return false;
		}
		if (!canBeCastOnTemporal && targetCharacter.traitContainer.HasTrait("Temporal"))
		{
			return false;
		}
		LocationGridTile gridTileLocation = targetCharacter.gridTileLocation;
		if (gridTileLocation != null && gridTileLocation.IsTileConsideredProtected(radius) && type != PLAYER_SKILL_TYPE.SNATCH_VILLAGER && type != PLAYER_SKILL_TYPE.SNATCH_OBJECT)
		{
			return false;
		}
		if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.action.goapType == INTERACTION_TYPE.SACRIFICE_SELF)
		{
			return false;
		}
		return CanPerformAbility();
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!canBeCastOnBlessed && targetCharacter.traitContainer.IsBlessed())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Target_Blessed") + "|";
		}
		if (!canBeCastOnTemporal && targetCharacter.traitContainer.HasTrait("Temporal"))
		{
			text = ((type != PLAYER_SKILL_TYPE.SNATCH_MONSTER) ? (text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Target_Temporal") + "|") : (text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_Temporal") + "|"));
		}
		if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.action.goapType == INTERACTION_TYPE.SACRIFICE_SELF)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Being_Absorbed") + "|";
		}
		return text;
	}

	protected bool RollSuccessChance(IPlayerActionTarget p_target)
	{
		int p_value = 100;
		if (p_target is Character { isDead: false } character)
		{
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(type);
			if (scriptableObjPlayerSkillData.resistanceType != RESISTANCE.None)
			{
				float resistanceValue = character.piercingAndResistancesComponent.GetResistanceValue(scriptableObjPlayerSkillData.resistanceType);
				float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(type);
				CombatManager.ModifyValueByPiercingAndResistance(ref p_value, pierceBasedOnCurrentLevel, resistanceValue);
			}
		}
		string log = string.Empty;
		return GameUtilities.RollChance(p_value, ref log);
	}

	public void OnPickAction()
	{
		OnPickAction(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget);
	}

	public void OnPickAction(IPlayerActionTarget p_target)
	{
		if (p_target != null)
		{
			Activate(p_target, (p_target as ITraitable)?.traitContainer.HasTrait("Demon Cultist") ?? false);
		}
	}

	public bool CanBePickedRegardlessOfCooldown()
	{
		return CanBePickedRegardlessOfCooldown(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget);
	}

	public bool CanBePickedRegardlessOfCooldown(IPlayerActionTarget p_target)
	{
		if (p_target == null)
		{
			return false;
		}
		if (!CanPerformAbilityTo(p_target))
		{
			return false;
		}
		return true;
	}

	public virtual bool IsInCooldown()
	{
		return isInCooldown;
	}

	public virtual float GetCoverFillAmount()
	{
		if (isInCooldown)
		{
			return 1f - (float)base.currentCooldownTick / (float)base.cooldown;
		}
		return 1f;
	}

	public virtual int GetCurrentRemainingCooldownTicks()
	{
		return base.cooldown - base.currentCooldownTick;
	}

	public int GetManaCost()
	{
		return base.manaCost;
	}
}
