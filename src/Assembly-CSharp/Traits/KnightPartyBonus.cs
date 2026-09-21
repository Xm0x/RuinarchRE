using System;

namespace Traits;

public class KnightPartyBonus : Status
{
	public int level { get; private set; }

	public override Type serializedData => typeof(SaveDataKnightPartyBonus);

	public KnightPartyBonus()
	{
		name = "Knight Party Bonus";
		description = "Bonus buffs when in party with a knight.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = false;
		ticksDuration = 0;
		isHidden = true;
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataKnightPartyBonus saveDataKnightPartyBonus = saveDataTrait as SaveDataKnightPartyBonus;
		level = saveDataKnightPartyBonus.level;
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character p_character)
		{
			UnapplyBonuses(p_character);
		}
	}

	public void SetLevel(int level, Character p_character)
	{
		this.level = level;
		ApplyBonuses(p_character);
	}

	private void ApplyBonuses(Character p_character)
	{
		switch (level)
		{
		case 1:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 20f);
			break;
		case 2:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 20f);
			p_character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(3);
			break;
		case 3:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 20f);
			p_character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(3);
			p_character.traitContainer.AddTrait(p_character, "Knight Mood Bonus");
			break;
		case 4:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 20f);
			p_character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(3);
			p_character.traitContainer.AddTrait(p_character, "Knight Mood Bonus");
			p_character.traitContainer.AddTrait(p_character, "Hermit");
			break;
		case 5:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 20f);
			p_character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(3);
			p_character.traitContainer.AddTrait(p_character, "Knight Mood Bonus");
			p_character.traitContainer.AddTrait(p_character, "Hermit");
			p_character.needsComponent.AdjustFullnessDecreaseRate(-0.02f);
			p_character.needsComponent.AdjustHappinessDecreaseRate(-0.02f);
			p_character.needsComponent.AdjustTirednessDecreaseRate(-0.02f);
			break;
		}
	}

	private void UnapplyBonuses(Character p_character)
	{
		switch (level)
		{
		case 1:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, -20f);
			break;
		case 2:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, -20f);
			p_character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(-3);
			break;
		case 3:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, -20f);
			p_character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(-3);
			p_character.traitContainer.RemoveTrait(p_character, "Knight Mood Bonus");
			break;
		case 4:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, -20f);
			p_character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(-3);
			p_character.traitContainer.RemoveTrait(p_character, "Knight Mood Bonus");
			p_character.traitContainer.RemoveTrait(p_character, "Hermit");
			break;
		case 5:
			p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, -20f);
			p_character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(-3);
			p_character.traitContainer.RemoveTrait(p_character, "Knight Mood Bonus");
			p_character.traitContainer.RemoveTrait(p_character, "Hermit");
			p_character.needsComponent.AdjustFullnessDecreaseRate(0.02f);
			p_character.needsComponent.AdjustHappinessDecreaseRate(0.02f);
			p_character.needsComponent.AdjustTirednessDecreaseRate(0.02f);
			break;
		}
	}
}
