using System;
using System.Collections.Generic;
using UnityEngine;

namespace Traits;

public class Narcoleptic : Trait
{
	public List<Character> witnessedNarcolepticAttack { get; private set; }

	public override Type serializedData => typeof(SaveDataNarcoleptic);

	public Narcoleptic()
	{
		name = "Narcoleptic";
		description = "Randomly plops down to sleep. If afflicted by the player, will produce a Chaos Orb each time it enters narcoleptic sleep.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		witnessedNarcolepticAttack = new List<Character>();
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		if (p_saveDataTrait is SaveDataNarcoleptic { witnessedNarcolepticAttack: not null } saveDataNarcoleptic)
		{
			witnessedNarcolepticAttack = SaveUtilities.ConvertIDListToCharacters(saveDataNarcoleptic.witnessedNarcolepticAttack);
		}
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		return base.GetTestingData(traitable) + "\nNarcoleptic Attack Witnesses: " + witnessedNarcolepticAttack.ComafyList();
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		int p_chance = 4;
		INTERRUPT p_interruptType = INTERRUPT.Narcoleptic_Nap;
		GetChanceAndInterruptType(p_character, ref p_chance, ref p_interruptType);
		if (UnityEngine.Random.Range(0, 100) < p_chance)
		{
			return DoNarcolepticNap(p_character, p_interruptType);
		}
		return false;
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		int p_chance = 4;
		INTERRUPT p_interruptType = INTERRUPT.Narcoleptic_Nap;
		GetChanceAndInterruptType(character, ref p_chance, ref p_interruptType);
		DoNarcolepticNap(character, p_interruptType);
		return base.TriggerFlaw(character);
	}

	private bool DoNarcolepticNap(Character p_owner, INTERRUPT p_interruptType)
	{
		if (p_owner.interruptComponent.TriggerInterrupt(p_interruptType, p_owner))
		{
			if (p_owner.HasAfflictedByPlayerWith(name))
			{
				DispenseChaosOrbsForAffliction(p_owner, PLAYER_SKILL_TYPE.NARCOLEPSY, 1);
			}
			return true;
		}
		return false;
	}

	private void GetChanceAndInterruptType(Character p_owner, ref int p_chance, ref INTERRUPT p_interruptType)
	{
		int num = 0;
		if (p_owner.HasAfflictedByPlayerWith(name))
		{
			num = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.NARCOLEPSY).currentLevel;
		}
		switch (num)
		{
		case 0:
			p_chance = 3;
			p_interruptType = INTERRUPT.Narcoleptic_Nap;
			break;
		case 1:
			p_chance = 4;
			p_interruptType = INTERRUPT.Narcoleptic_Nap_Short;
			break;
		case 2:
			p_chance = 5;
			p_interruptType = INTERRUPT.Narcoleptic_Nap_Medium;
			break;
		case 3:
			p_chance = 6;
			p_interruptType = INTERRUPT.Narcoleptic_Nap_Long;
			break;
		}
	}

	public void AddNarcolepticAttackWitness(Character p_character)
	{
		if (!witnessedNarcolepticAttack.Contains(p_character))
		{
			witnessedNarcolepticAttack.Add(p_character);
		}
	}

	public bool HasWitnessedNarcolepticAttack(Character p_character)
	{
		return witnessedNarcolepticAttack.Contains(p_character);
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		witnessedNarcolepticAttack.Remove(p_character);
	}
}
