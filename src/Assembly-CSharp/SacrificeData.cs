using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SacrificeData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SACRIFICE;

	public override string name => "Sacrifice";

	public override string description => "This Action allows you to sacrifice an imprisoned Monster for Chaotic Orbs.";

	public SacrificeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Summon summon)
		{
			int p_amount = GameUtilities.RandomBetweenTwoNumbers(2, 5);
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SACRIFICE).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, summon.gridTileLocation.centeredWorldLocation, p_amount, summon.gridTileLocation.parentMap);
			}
			summon.SetDestroyMarkerOnDeath(state: true);
			summon.Death("sacrifice");
			base.ActivateAbility(targetPOI);
		}
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		if (targetStructure is Kennel kennel)
		{
			ActivateAbility(kennel.occupyingSummon);
		}
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Sacrifice_Being_Drained") + "|";
		}
		return text;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter.traitContainer.HasTrait("Being Drained"))
			{
				return false;
			}
			if (targetCharacter is Summon && !targetCharacter.isDead && targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null)
			{
				if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL)
				{
					return true;
				}
				StructureRoom room;
				if (targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS)
				{
					return targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out room);
				}
			}
			return false;
		}
		return false;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target))
		{
			if (target is Summon summon)
			{
				if (summon.gridTileLocation != null && summon.gridTileLocation.structure != null)
				{
					if (summon.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL)
					{
						if (summon.movementComponent.isFlying && !summon.traitContainer.HasTrait("Restrained"))
						{
							return false;
						}
						return true;
					}
					if (summon.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS)
					{
						if (summon.movementComponent.isFlying && !summon.traitContainer.HasTrait("Restrained"))
						{
							return false;
						}
						StructureRoom room;
						return summon.gridTileLocation.structure.IsTilePartOfARoom(summon.gridTileLocation, out room);
					}
				}
				return false;
			}
			if (target is DemonicStructure demonicStructure && demonicStructure is Kennel { occupyingSummon: not null })
			{
				return true;
			}
		}
		return false;
	}
}
