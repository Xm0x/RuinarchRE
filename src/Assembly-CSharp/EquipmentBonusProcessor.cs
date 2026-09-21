using System.Collections.Generic;
using Traits;
using UtilityScripts;

public static class EquipmentBonusProcessor
{
	private static Dictionary<EQUIPMENT_SLAYER_BONUS, string> traitDictionaryForSlayer = new Dictionary<EQUIPMENT_SLAYER_BONUS, string>
	{
		{
			EQUIPMENT_SLAYER_BONUS.Monster_Slayer,
			"Monster Slayer"
		},
		{
			EQUIPMENT_SLAYER_BONUS.Elf_Slayer,
			"Elf Slayer"
		},
		{
			EQUIPMENT_SLAYER_BONUS.Human_Slayer,
			"Human Slayer"
		},
		{
			EQUIPMENT_SLAYER_BONUS.Demon_Slayer,
			"Demon Slayer"
		},
		{
			EQUIPMENT_SLAYER_BONUS.Undead_SLayer,
			"Undead Slayer"
		}
	};

	private static Dictionary<EQUIPMENT_WARD_BONUS, string> traitDictionaryForWard = new Dictionary<EQUIPMENT_WARD_BONUS, string>
	{
		{
			EQUIPMENT_WARD_BONUS.Monster_Ward,
			"Monster Ward"
		},
		{
			EQUIPMENT_WARD_BONUS.Elf_Ward,
			"Elf Ward"
		},
		{
			EQUIPMENT_WARD_BONUS.Human_Ward,
			"Human Ward"
		},
		{
			EQUIPMENT_WARD_BONUS.Demon_Ward,
			"Demon Ward"
		},
		{
			EQUIPMENT_WARD_BONUS.Undead_Ward,
			"Undead Ward"
		}
	};

	public static void ApplyEquipBonusToTarget(EquipmentItem p_equipItem, Character p_targetCharacter, bool p_initializedStackCountOnly = false)
	{
		if (p_equipItem.equipmentData == null)
		{
			p_equipItem.AssignData();
		}
		p_equipItem.equipmentData.equipmentUpgradeData.bonuses.ForEach(delegate(EQUIPMENT_BONUS eachBonus)
		{
			if (eachBonus == EQUIPMENT_BONUS.Attack_Element && !p_equipItem.addedBonus.Contains(EQUIPMENT_BONUS.Attack_Element))
			{
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly);
			}
			else if (eachBonus == EQUIPMENT_BONUS.Flight && !p_equipItem.addedBonus.Contains(EQUIPMENT_BONUS.Flight))
			{
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly);
			}
			else
			{
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly);
			}
		});
		p_equipItem.addedBonus.ForEach(delegate(EQUIPMENT_BONUS eachBonus)
		{
			switch (eachBonus)
			{
			case EQUIPMENT_BONUS.Slayer_Bonus:
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly, EQUIPMENT_WARD_BONUS.None, p_equipItem.addedSlayerBonus);
				break;
			case EQUIPMENT_BONUS.Ward_Bonus:
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly, p_equipItem.addedWardBonus);
				break;
			case EQUIPMENT_BONUS.Int_Percentage:
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly, EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS.None, 0f, p_equipItem.intPercentageReduced);
				break;
			case EQUIPMENT_BONUS.Str_Percentage:
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly, EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS.None, p_equipItem.strPercentageReduced);
				break;
			case EQUIPMENT_BONUS.Crit_Rate_Actual:
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly, EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS.None, 0f, 0f, p_equipItem.addedCritRate);
				break;
			case EQUIPMENT_BONUS.Attack_Element:
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly, EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS.None, 0f, 0f, 0, p_equipItem.addedElementalBonus);
				break;
			default:
				ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly);
				break;
			}
		});
	}

	public static void RemoveEquipBonusToTarget(EquipmentItem p_equipItem, Character p_targetCharacter)
	{
		if (p_equipItem.equipmentData == null)
		{
			p_equipItem.AssignData();
		}
		p_equipItem.equipmentData.equipmentUpgradeData.bonuses.ForEach(delegate(EQUIPMENT_BONUS eachBonus)
		{
			if (eachBonus == EQUIPMENT_BONUS.Attack_Element && !p_equipItem.addedBonus.Contains(EQUIPMENT_BONUS.Attack_Element))
			{
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter);
			}
			else if (eachBonus == EQUIPMENT_BONUS.Flight && !p_equipItem.addedBonus.Contains(EQUIPMENT_BONUS.Flight))
			{
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter);
			}
			else
			{
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter);
			}
		});
		p_equipItem.addedBonus.ForEach(delegate(EQUIPMENT_BONUS eachBonus)
		{
			switch (eachBonus)
			{
			case EQUIPMENT_BONUS.Slayer_Bonus:
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, EQUIPMENT_WARD_BONUS.None, p_equipItem.addedSlayerBonus);
				break;
			case EQUIPMENT_BONUS.Ward_Bonus:
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_equipItem.addedWardBonus);
				break;
			case EQUIPMENT_BONUS.Int_Percentage:
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS.None, 0f, p_equipItem.intPercentageReduced);
				break;
			case EQUIPMENT_BONUS.Str_Percentage:
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS.None, p_equipItem.strPercentageReduced);
				break;
			case EQUIPMENT_BONUS.Crit_Rate_Actual:
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS.None, 0f, 0f, p_equipItem.addedCritRate);
				break;
			case EQUIPMENT_BONUS.Attack_Element:
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS.None, 0f, 0f, 0, p_equipItem.addedElementalBonus);
				break;
			default:
				RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter);
				break;
			}
		});
	}

	private static void ApplyEachBonusToTarget(EquipmentItem p_equipItem, EQUIPMENT_BONUS p_equipBonus, Character p_targetCharacter, bool p_initializedStackCountOnly = false, EQUIPMENT_WARD_BONUS p_wardBonus = EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS p_slayerBonus = EQUIPMENT_SLAYER_BONUS.None, float p_strPercentageBonus = 0f, float p_intPercentageBonus = 0f, int p_critRate = 0, ELEMENTAL_TYPE p_element = ELEMENTAL_TYPE.Normal)
	{
		switch (p_equipBonus)
		{
		case EQUIPMENT_BONUS.Str_Actual:
			p_targetCharacter.combatComponent.AdjustStrengthModifier(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalAttack(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Str_Percentage:
			if (p_strPercentageBonus != 0f)
			{
				p_targetCharacter.combatComponent.AdjustStrengthPercentModifier(p_strPercentageBonus);
			}
			else
			{
				p_targetCharacter.combatComponent.AdjustStrengthPercentModifier(p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage);
			}
			break;
		case EQUIPMENT_BONUS.Int_Actual:
			p_targetCharacter.combatComponent.AdjustIntelligenceModifier(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalInt(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Int_Percentage:
			if (p_intPercentageBonus != 0f)
			{
				p_targetCharacter.combatComponent.AdjustIntelligencePercentModifier(p_intPercentageBonus);
			}
			else
			{
				p_targetCharacter.combatComponent.AdjustIntelligencePercentModifier(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalIntPercentage(p_equipItem.quality));
			}
			break;
		case EQUIPMENT_BONUS.Crit_Rate_Actual:
			if ((float)p_critRate != 0f)
			{
				p_targetCharacter.combatComponent.AdjustCritRate(p_critRate);
			}
			else
			{
				p_targetCharacter.combatComponent.AdjustCritRate(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalCritRate(p_equipItem.quality));
			}
			break;
		case EQUIPMENT_BONUS.Max_HP_Actual:
			p_targetCharacter.combatComponent.AdjustMaxHPModifier(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalmaxHP(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Max_HP_Percentage:
			p_targetCharacter.combatComponent.AdjustMaxHPPercentModifier(p_equipItem.equipmentData.equipmentUpgradeData.AdditionalMaxHPPercentage);
			break;
		case EQUIPMENT_BONUS.Increased_Piercing:
			p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalPiercing(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Attack_Element:
			if (p_element != ELEMENTAL_TYPE.Normal)
			{
				p_targetCharacter.combatComponent.SetElementalType(p_element);
			}
			else
			{
				p_targetCharacter.combatComponent.SetElementalType(p_equipItem.equipmentData.equipmentUpgradeData.elementAttackBonus);
			}
			break;
		case EQUIPMENT_BONUS.Increased_3_Random_Resistance:
		case EQUIPMENT_BONUS.Increased_4_Random_Resistance:
		case EQUIPMENT_BONUS.Increased_5_Random_Resistance:
			ApplyResistanceBonusOnCharacter(p_equipItem, p_targetCharacter);
			break;
		case EQUIPMENT_BONUS.Mental_Resistance:
			p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Normal_Resistance:
			p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Secondary_Resistances:
			p_targetCharacter.piercingAndResistancesComponent.AdjustSecondaryResistances(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Slayer_Bonus:
		{
			EQUIPMENT_SLAYER_BONUS eQUIPMENT_SLAYER_BONUS = p_equipItem.equipmentData.equipmentUpgradeData.slayerBonus;
			if (p_slayerBonus != EQUIPMENT_SLAYER_BONUS.None)
			{
				eQUIPMENT_SLAYER_BONUS = p_slayerBonus;
			}
			if (eQUIPMENT_SLAYER_BONUS != EQUIPMENT_SLAYER_BONUS.None)
			{
				if (p_targetCharacter.traitContainer.HasTrait(traitDictionaryForSlayer[eQUIPMENT_SLAYER_BONUS]))
				{
					(p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForSlayer[eQUIPMENT_SLAYER_BONUS]) as Slayer).stackCount++;
					break;
				}
				p_targetCharacter.traitContainer.AddTrait(p_targetCharacter, traitDictionaryForSlayer[eQUIPMENT_SLAYER_BONUS]);
				(p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForSlayer[eQUIPMENT_SLAYER_BONUS]) as Slayer).stackCount++;
			}
			break;
		}
		case EQUIPMENT_BONUS.Ward_Bonus:
		{
			EQUIPMENT_WARD_BONUS eQUIPMENT_WARD_BONUS = p_equipItem.equipmentData.equipmentUpgradeData.wardBonus;
			if (p_wardBonus != EQUIPMENT_WARD_BONUS.None)
			{
				eQUIPMENT_WARD_BONUS = p_wardBonus;
			}
			if (eQUIPMENT_WARD_BONUS != EQUIPMENT_WARD_BONUS.None)
			{
				if (p_targetCharacter.traitContainer.HasTrait(traitDictionaryForWard[eQUIPMENT_WARD_BONUS]))
				{
					(p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForWard[eQUIPMENT_WARD_BONUS]) as Ward).stackCount++;
					break;
				}
				p_targetCharacter.traitContainer.AddTrait(p_targetCharacter, traitDictionaryForWard[eQUIPMENT_WARD_BONUS]);
				(p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForWard[eQUIPMENT_WARD_BONUS]) as Ward).stackCount++;
			}
			break;
		}
		case EQUIPMENT_BONUS.Flight:
			if (p_targetCharacter.traitContainer.HasTrait("Flying"))
			{
				(p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Flying") as Flying).stackCount++;
				break;
			}
			p_targetCharacter.movementComponent.SetToFlying();
			(p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Flying") as Flying).stackCount++;
			break;
		case EQUIPMENT_BONUS.Random_Ward_Bonus:
		case EQUIPMENT_BONUS.Random_Slayer_Bonus:
		case EQUIPMENT_BONUS.None:
			break;
		}
	}

	private static void RemoveEachBonusToTarget(EquipmentItem p_equipItem, EQUIPMENT_BONUS p_equipBonus, Character p_targetCharacter, EQUIPMENT_WARD_BONUS p_wardBonus = EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS p_slayerBonus = EQUIPMENT_SLAYER_BONUS.None, float p_strPercentageBonus = 0f, float p_intPercentageBonus = 0f, int p_critRate = 0, ELEMENTAL_TYPE p_element = ELEMENTAL_TYPE.Normal)
	{
		switch (p_equipBonus)
		{
		case EQUIPMENT_BONUS.Str_Actual:
			p_targetCharacter.combatComponent.AdjustStrengthModifier(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalAttack(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Str_Percentage:
			if (p_strPercentageBonus != 0f)
			{
				p_targetCharacter.combatComponent.AdjustStrengthPercentModifier(0f - p_strPercentageBonus);
			}
			else
			{
				p_targetCharacter.combatComponent.AdjustStrengthPercentModifier(0f - p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage);
			}
			break;
		case EQUIPMENT_BONUS.Int_Actual:
			p_targetCharacter.combatComponent.AdjustIntelligenceModifier(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalInt(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Int_Percentage:
			if (p_intPercentageBonus != 0f)
			{
				p_targetCharacter.combatComponent.AdjustIntelligencePercentModifier(0f - p_intPercentageBonus);
			}
			else
			{
				p_targetCharacter.combatComponent.AdjustIntelligencePercentModifier(0f - p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalIntPercentage(p_equipItem.quality));
			}
			break;
		case EQUIPMENT_BONUS.Crit_Rate_Actual:
			if (p_critRate != 0)
			{
				p_targetCharacter.combatComponent.AdjustCritRate(-p_critRate);
			}
			else
			{
				p_targetCharacter.combatComponent.AdjustCritRate(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalCritRate(p_equipItem.quality));
			}
			break;
		case EQUIPMENT_BONUS.Max_HP_Actual:
			p_targetCharacter.combatComponent.AdjustMaxHPModifier(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalmaxHP(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Max_HP_Percentage:
			p_targetCharacter.combatComponent.AdjustMaxHPPercentModifier(0f - p_equipItem.equipmentData.equipmentUpgradeData.AdditionalMaxHPPercentage);
			break;
		case EQUIPMENT_BONUS.Increased_Piercing:
			p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(0f - p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalPiercing(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Attack_Element:
		{
			EquipmentComponent equipmentComponent = p_targetCharacter.equipmentComponent;
			if (equipmentComponent.allEquipments.Count > 0 && equipmentComponent.allEquipments[equipmentComponent.allEquipments.Count - 1] == p_equipItem)
			{
				EquipmentItem randomRemainingEquipment = equipmentComponent.GetRandomRemainingEquipment(p_equipItem);
				ProcessElementAfterRemovingSomeItem(equipmentComponent, randomRemainingEquipment, p_targetCharacter);
			}
			break;
		}
		case EQUIPMENT_BONUS.Increased_3_Random_Resistance:
		case EQUIPMENT_BONUS.Increased_4_Random_Resistance:
		case EQUIPMENT_BONUS.Increased_5_Random_Resistance:
			RemoveResistanceBonusOnCharacter(p_equipItem, p_targetCharacter);
			break;
		case EQUIPMENT_BONUS.Mental_Resistance:
			p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, 0f - p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Normal_Resistance:
			p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 0f - p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Secondary_Resistances:
			p_targetCharacter.piercingAndResistancesComponent.AdjustSecondaryResistances(0f - p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
			break;
		case EQUIPMENT_BONUS.Slayer_Bonus:
		{
			EQUIPMENT_SLAYER_BONUS eQUIPMENT_SLAYER_BONUS = p_equipItem.equipmentData.equipmentUpgradeData.slayerBonus;
			if (p_slayerBonus != EQUIPMENT_SLAYER_BONUS.None)
			{
				eQUIPMENT_SLAYER_BONUS = p_slayerBonus;
			}
			if (eQUIPMENT_SLAYER_BONUS != EQUIPMENT_SLAYER_BONUS.None && p_targetCharacter.traitContainer.HasTrait(traitDictionaryForSlayer[eQUIPMENT_SLAYER_BONUS]))
			{
				Slayer obj3 = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForSlayer[eQUIPMENT_SLAYER_BONUS]) as Slayer;
				obj3.stackCount--;
				if (obj3.stackCount <= 0)
				{
					p_targetCharacter.traitContainer.RemoveTrait(p_targetCharacter, traitDictionaryForSlayer[eQUIPMENT_SLAYER_BONUS]);
				}
			}
			break;
		}
		case EQUIPMENT_BONUS.Ward_Bonus:
		{
			EQUIPMENT_WARD_BONUS eQUIPMENT_WARD_BONUS = p_equipItem.equipmentData.equipmentUpgradeData.wardBonus;
			if (p_wardBonus != EQUIPMENT_WARD_BONUS.None)
			{
				eQUIPMENT_WARD_BONUS = p_wardBonus;
			}
			if (eQUIPMENT_WARD_BONUS != EQUIPMENT_WARD_BONUS.None && p_targetCharacter.traitContainer.HasTrait(traitDictionaryForWard[eQUIPMENT_WARD_BONUS]))
			{
				Ward obj2 = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForWard[eQUIPMENT_WARD_BONUS]) as Ward;
				obj2.stackCount--;
				if (obj2.stackCount <= 0)
				{
					p_targetCharacter.traitContainer.RemoveTrait(p_targetCharacter, traitDictionaryForWard[eQUIPMENT_WARD_BONUS]);
				}
			}
			break;
		}
		case EQUIPMENT_BONUS.Flight:
			if (p_targetCharacter.traitContainer.HasTrait("Flying"))
			{
				Flying obj = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Flying") as Flying;
				obj.stackCount--;
				if (obj.stackCount <= 0)
				{
					p_targetCharacter.movementComponent.SetToNonFlying();
				}
			}
			break;
		case EQUIPMENT_BONUS.Random_Ward_Bonus:
		case EQUIPMENT_BONUS.Random_Slayer_Bonus:
		case EQUIPMENT_BONUS.None:
			break;
		}
	}

	private static void ProcessElementAfterRemovingSomeItem(EquipmentComponent ec, EquipmentItem ei, Character p_targetCharacter)
	{
		if (ei != null)
		{
			if (ei.addedBonus.Contains(EQUIPMENT_BONUS.Attack_Element))
			{
				p_targetCharacter.combatComponent.SetElementalType(ei.addedElementalBonus);
			}
			else if (ei.equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Attack_Element))
			{
				if (ei.addedElementalBonus != ELEMENTAL_TYPE.Normal)
				{
					p_targetCharacter.combatComponent.SetElementalType(ei.addedElementalBonus);
				}
				else
				{
					p_targetCharacter.combatComponent.SetElementalType(ei.equipmentData.equipmentUpgradeData.elementAttackBonus);
				}
			}
		}
		else if (p_targetCharacter.combatComponent.elementalStatusWaitingList.Count > 0)
		{
			p_targetCharacter.combatComponent.UpdateElementalType();
		}
		else
		{
			p_targetCharacter.combatComponent.SetElementalType(p_targetCharacter.characterClass.elementalType);
		}
	}

	private static void ApplyResistanceBonusOnCharacter(EquipmentItem p_equipItem, Character p_targetCharacter)
	{
		for (int i = 0; i < p_equipItem.resistanceBonuses.Count; i++)
		{
			p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(p_equipItem.resistanceBonuses[i], p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
		}
	}

	private static void RemoveResistanceBonusOnCharacter(EquipmentItem p_equipItem, Character p_targetCharacter)
	{
		for (int i = 0; i < p_equipItem.resistanceBonuses.Count; i++)
		{
			p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(p_equipItem.resistanceBonuses[i], 0f - p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
		}
	}

	public static void SetBonusResistanceOnWeapon(EquipmentItem p_equipItem)
	{
		int p_count = 0;
		if (p_equipItem.equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_3_Random_Resistance))
		{
			p_count = 3;
		}
		else if (p_equipItem.equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_4_Random_Resistance))
		{
			p_count = 4;
		}
		else if (p_equipItem.equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_5_Random_Resistance))
		{
			p_count = 5;
		}
		foreach (int item in GameUtilities.GetUniqueRandomNumbersInBetween(2, 10, p_count))
		{
			p_equipItem.resistanceBonuses.Add((RESISTANCE)item);
		}
	}

	public static void SetBonusResistanceOnPowerCrystal(PowerCrystal p_crystal)
	{
		List<RESISTANCE> list = RuinarchListPool<RESISTANCE>.Claim();
		list.AddRange(CollectionUtilities.GetEnumValues<RESISTANCE>());
		list.Remove(RESISTANCE.None);
		RESISTANCE randomElement = CollectionUtilities.GetRandomElement(list);
		RuinarchListPool<RESISTANCE>.Release(list);
		p_crystal.resistanceBonuses.Add(randomElement);
	}

	public static float GetSlayerBonusDamage(Character p_damager, Character p_damageReceiver, float p_currentAmountDamage)
	{
		float result = 0f;
		if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damageReceiver.faction != null && p_damageReceiver.faction.factionType != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Monster_Slayer]) && p_damageReceiver.isWildMonster)
		{
			result = p_currentAmountDamage * 0.5f;
		}
		if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Human_Slayer]) && p_damageReceiver.race == RACE.HUMANS)
		{
			result = p_currentAmountDamage * 0.5f;
		}
		if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Elf_Slayer]) && p_damageReceiver.race == RACE.ELVES)
		{
			result = p_currentAmountDamage * 0.5f;
		}
		if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Demon_Slayer]) && RaceManager.Instance.GetRaceData(p_damageReceiver.race).category == CHARACTER_CATEGORY.Demonic)
		{
			result = p_currentAmountDamage * 0.5f;
		}
		if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Undead_SLayer]) && p_damageReceiver.IsUndead())
		{
			result = p_currentAmountDamage * 0.5f;
		}
		return result;
	}

	public static float GetWardBonusDamage(Character p_damager, Character p_damageReceiver, float p_currentAmountDagame)
	{
		float result = 0f;
		if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damager.faction != null && p_damageReceiver.faction.factionType != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Monster_Ward]) && p_damager.isWildMonster)
		{
			result = p_currentAmountDagame * 0.5f;
		}
		if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Human_Ward]) && p_damager.race == RACE.HUMANS)
		{
			result = p_currentAmountDagame * 0.5f;
		}
		if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Elf_Ward]) && p_damager.race == RACE.ELVES)
		{
			result = p_currentAmountDagame * 0.5f;
		}
		if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Demon_Ward]) && RaceManager.Instance.GetRaceData(p_damageReceiver.race).category == CHARACTER_CATEGORY.Demonic)
		{
			result = p_currentAmountDagame * 0.5f;
		}
		if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Undead_Ward]) && p_damager.IsUndead())
		{
			result = p_currentAmountDagame * 0.5f;
		}
		return result;
	}

	public static bool GetDeadlyBonus(Character p_damager, Character p_damageReceiver)
	{
		if (p_damageReceiver != null && p_damager != null && p_damager.equipmentComponent.HasDeadlyEquipment() && GameUtilities.RollChance(2))
		{
			return true;
		}
		return false;
	}

	public static bool GetFesteringBonus(Character p_damager, Character p_damageReceiver)
	{
		if (p_damageReceiver != null && p_damager != null && p_damager.equipmentComponent.HasFesteringEquipment() && GameUtilities.RollChance(5))
		{
			return true;
		}
		return false;
	}

	public static bool GetHauntedBonus(Character p_damager, Character p_damageReceiver)
	{
		if (p_damageReceiver != null && p_damager != null && p_damager.equipmentComponent.HasHauntedEquipment() && GameUtilities.RollChance(5))
		{
			return true;
		}
		return false;
	}
}
