using System;

namespace Traits;

public class Polymorphed : Status
{
	private Character owner;

	public SUMMON_TYPE animalType { get; private set; }

	public ELEMENTAL_TYPE originalElement { get; private set; }

	public int strengthReduction { get; private set; }

	public int intelligenceReduction { get; private set; }

	public float piercingPowerReduction { get; private set; }

	public float fireResistanceReduction { get; private set; }

	public float poisonResistanceReduction { get; private set; }

	public float waterResistanceReduction { get; private set; }

	public float iceResistanceReduction { get; private set; }

	public float electricResistanceReduction { get; private set; }

	public float earthResistanceReduction { get; private set; }

	public float windResistanceReduction { get; private set; }

	public float mentalResistanceReduction { get; private set; }

	public float physicalResistanceReduction { get; private set; }

	public override Type serializedData => typeof(SaveDataPolymorphed);

	public override bool shouldBeLoadedInMainThread => true;

	public Polymorphed()
	{
		name = "Polymorphed";
		description = "Transformed into a weak animal!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		SetAnimalClass();
		if (addedTo is Character character)
		{
			owner = character;
			character.behaviourComponent.AddBehaviourComponent(typeof(PolymorphedBehaviour));
			GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Minion_Dissipate, allowRotation: false);
			character.visuals?.UpdateAllVisuals(character);
			ProcessStatsModification(character);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(PolymorphedBehaviour));
			if (character.gridTileLocation != null && !character.isBeingSeized)
			{
				GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Minion_Dissipate, allowRotation: false);
			}
			character.visuals?.UpdateAllVisuals(character);
			BringBackStats(character);
			owner = null;
		}
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
		}
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataPolymorphed saveDataPolymorphed = saveDataTrait as SaveDataPolymorphed;
		animalType = saveDataPolymorphed.animalType;
		strengthReduction = saveDataPolymorphed.strengthReduction;
		intelligenceReduction = saveDataPolymorphed.intelligenceReduction;
		piercingPowerReduction = saveDataPolymorphed.piercingPowerReduction;
		fireResistanceReduction = saveDataPolymorphed.fireResistanceReduction;
		poisonResistanceReduction = saveDataPolymorphed.poisonResistanceReduction;
		waterResistanceReduction = saveDataPolymorphed.waterResistanceReduction;
		iceResistanceReduction = saveDataPolymorphed.iceResistanceReduction;
		electricResistanceReduction = saveDataPolymorphed.electricResistanceReduction;
		earthResistanceReduction = saveDataPolymorphed.earthResistanceReduction;
		windResistanceReduction = saveDataPolymorphed.windResistanceReduction;
		mentalResistanceReduction = saveDataPolymorphed.mentalResistanceReduction;
		physicalResistanceReduction = saveDataPolymorphed.physicalResistanceReduction;
		originalElement = saveDataPolymorphed.originalElement;
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		owner.visuals?.UpdateAllVisuals(owner);
	}

	private void SetAnimalClass()
	{
		animalType = CharacterManager.Instance.GetRandomWeakAnimalType();
	}

	private void ProcessStatsModification(Character p_character)
	{
		if (p_character.characterClass.attackType == ATTACK_TYPE.PHYSICAL)
		{
			int baseAttackPower = p_character.characterClass.baseAttackPower;
			if (baseAttackPower > 5)
			{
				strengthReduction = baseAttackPower - 5;
			}
		}
		else
		{
			int baseAttackPower2 = p_character.characterClass.baseAttackPower;
			if (baseAttackPower2 > 5)
			{
				intelligenceReduction = baseAttackPower2 - 5;
			}
		}
		piercingPowerReduction = p_character.piercingAndResistancesComponent.basePiercing;
		fireResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Fire);
		poisonResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Poison);
		waterResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Water);
		iceResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Ice);
		electricResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Electric);
		earthResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Earth);
		windResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Wind);
		mentalResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Mental);
		physicalResistanceReduction = p_character.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Physical);
		originalElement = p_character.combatComponent.currentElement.type;
		p_character.combatComponent.AdjustStrengthModifier(-strengthReduction);
		p_character.combatComponent.AdjustIntelligenceModifier(-intelligenceReduction);
		p_character.piercingAndResistancesComponent.AdjustBasePiercing(0f - piercingPowerReduction);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Fire, 0f - fireResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Poison, 0f - poisonResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Water, 0f - waterResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Ice, 0f - iceResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Electric, 0f - electricResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Earth, 0f - earthResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Wind, 0f - windResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, 0f - mentalResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 0f - physicalResistanceReduction);
		p_character.combatComponent.SetElementalType(ELEMENTAL_TYPE.Normal);
	}

	private void BringBackStats(Character p_character)
	{
		p_character.combatComponent.AdjustStrengthModifier(strengthReduction);
		p_character.combatComponent.AdjustIntelligenceModifier(intelligenceReduction);
		p_character.piercingAndResistancesComponent.AdjustBasePiercing(piercingPowerReduction);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Fire, fireResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Poison, poisonResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Water, waterResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Ice, iceResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Electric, electricResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Earth, earthResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Wind, windResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, mentalResistanceReduction, shouldBroadcastSignal: false);
		p_character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, physicalResistanceReduction);
		p_character.combatComponent.SetElementalType(originalElement);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
